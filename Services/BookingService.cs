// Серверная логика заказа: проверяет сеанс и кресла, затем меняет бронь в транзакции SQLite.
using Cinema.Data;
using Cinema.Models;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Services;
public class BookingException(string message) : Exception(message);
public class BookingService(CinemaDbContext db, CinemaClock clock)
{
    // Microsoft.Data.Sqlite starts an IMMEDIATE transaction by default: only one
    // writer can enter. Every booking mutation begins it BEFORE checking seats.
    public async Task<Session> GetSessionAsync(int sessionId)
    {
        // sessionId передаёт контроллер; ищем именно этот сеанс в таблице Sessions.
        var session = await db.Sessions.SingleOrDefaultAsync(x => x.Id == sessionId);
        return session ?? throw new BookingException("Сеанс не найден.");
    }

    // Находит неоплаченные заказы этого сеанса с истёкшим временем и освобождает их кресла.
    private async Task ExpireSessionAsync(int sessionId)
    {
        // Берём Pending-заказы данного сеанса, у которых ExpiresAtUtc уже прошёл; Include загружает их кресла.
        var expired = await db.Bookings.Where(x => x.SessionId == sessionId && x.Status == BookingStatus.Pending && x.ExpiresAtUtc <= DateTime.UtcNow).Include(x => x.BookingSeats).ToListAsync();
        foreach (var booking in expired)
        {
            booking.Status = BookingStatus.Expired;
            foreach (var seat in booking.BookingSeats)
                seat.Active = null;
        }

        // Записываем накопленные изменения сущностей в SQLite.
        await db.SaveChangesAsync();
    }

    // Открывает транзакцию и очищает просроченные брони одного сеанса.
    public async Task CleanupSessionAsync(int sessionId)
    {
        // Начинаем транзакцию SQLite до проверки и изменения мест, чтобы параллельные запросы не продали одно кресло дважды.
        await using var transaction = await db.Database.BeginTransactionAsync();
        await GetSessionAsync(sessionId);
        await ExpireSessionAsync(sessionId);
        // Подтверждаем транзакцию: связанные изменения заказа и мест сохраняются вместе.
        await transaction.CommitAsync();
    }

    // Получает id пользователя из входа, id сеанса и номера кресел из формы; в транзакции проверяет доступность, берёт цену сеанса из SQLite и создаёт заказ.
    public async Task<int> CreateAsync(string userId, int sessionId, int[] seatIds)
    {
        var ids = seatIds.Distinct().ToArray();
        if (ids.Length is < 1 or > 12)
            throw new BookingException("Выберите от 1 до 12 мест.");
        // Начинаем транзакцию SQLite до проверки и изменения мест, чтобы параллельные запросы не продали одно кресло дважды.
        await using var transaction = await db.Database.BeginTransactionAsync();
        var session = await GetSessionAsync(sessionId);
        await ExpireSessionAsync(sessionId);
        if (session.IsCancelled || session.StartsAt <= clock.Now)
            throw new BookingException("Сеанс уже начался или отменён.");
        // Принимаем только существующие доступные кресла зала, в котором пройдёт этот сеанс.
        var seats = await db.Seats.Where(x => ids.Contains(x.Id) && x.HallId == session.HallId && x.IsAccessible).ToListAsync();
        if (seats.Count != ids.Length)
            throw new BookingException("В выборе есть недоступные места.");
        // Active=1 означает место с действующей бронью или билетом; повторно продать его нельзя.
        if (await db.BookingSeats.AnyAsync(x => x.SessionId == sessionId && ids.Contains(x.SeatId) && x.Active == 1))
            throw new BookingException("Одно из мест только что заняли. Выберите другие места.");
        var booking = new Booking
        {
            UserId = userId,
            SessionId = sessionId,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10),
            Status = BookingStatus.Pending,
            Total = session.Price * ids.Length,
            BookingSeats = ids.Select(id => new BookingSeat { SeatId = id, SessionId = sessionId, Price = session.Price, Active = 1 }).ToList()
        };
        db.Bookings.Add(booking);
        // Записываем накопленные изменения сущностей в SQLite.
        await db.SaveChangesAsync(); // Unique index is a second line of defence.
        // Подтверждаем транзакцию: связанные изменения заказа и мест сохраняются вместе.
        await transaction.CommitAsync();
        return booking.Id;
    }

    // Проверяет владельца и состояние заказа, затем записывает подтверждение оплаты и оставляет места занятыми.
    public async Task PayAsync(int id, string userId)
    {
        // Выбираем из SQLite заказы текущего пользователя, чей id получен из cookie входа.
        var sessionId = await db.Bookings.AsNoTracking().Where(x => x.Id == id && x.UserId == userId).Select(x => (int? )x.SessionId).SingleOrDefaultAsync() ?? throw new BookingException("Заказ не найден.");
        // Начинаем транзакцию SQLite до проверки и изменения мест, чтобы параллельные запросы не продали одно кресло дважды.
        await using var transaction = await db.Database.BeginTransactionAsync();
        var session = await GetSessionAsync(sessionId);
        await ExpireSessionAsync(sessionId);
        // Выбираем из SQLite заказы текущего пользователя, чей id получен из cookie входа.
        var booking = await db.Bookings.SingleAsync(x => x.Id == id && x.UserId == userId);
        if (booking.Status == BookingStatus.Paid)
        {
            // Подтверждаем транзакцию: связанные изменения заказа и мест сохраняются вместе.
            await transaction.CommitAsync();
            return;
        }

        if (booking.Status != BookingStatus.Pending || booking.ExpiresAtUtc <= DateTime.UtcNow || session.StartsAt <= clock.Now || session.IsCancelled)
            throw new BookingException("Срок брони истёк или сеанс недоступен. Создайте новый заказ.");
        booking.Status = BookingStatus.Paid;
        db.Payments.Add(new Payment { BookingId = id, Amount = booking.Total, PaidAtUtc = DateTime.UtcNow });
        // Записываем накопленные изменения сущностей в SQLite.
        await db.SaveChangesAsync();
        // Подтверждаем транзакцию: связанные изменения заказа и мест сохраняются вместе.
        await transaction.CommitAsync();
    }

    // Проверяет право на отмену, меняет статус заказа и освобождает места; при возврате отмечает платёж.
    public async Task CancelAsync(int id, string? ownerId, bool refund = false)
    {
        // Находим заказ по id; для посетителя дополнительно сверяем владельца, а администратор может действовать без ownerId.
        var sessionId = await db.Bookings.AsNoTracking().Where(x => x.Id == id && (ownerId == null || x.UserId == ownerId)).Select(x => (int? )x.SessionId).SingleOrDefaultAsync() ?? throw new BookingException("Заказ не найден.");
        // Начинаем транзакцию SQLite до проверки и изменения мест, чтобы параллельные запросы не продали одно кресло дважды.
        await using var transaction = await db.Database.BeginTransactionAsync();
        var session = await GetSessionAsync(sessionId);
        await ExpireSessionAsync(sessionId);
        // Загружаем заказ вместе с его креслами и платежом, чтобы освободить места и при необходимости отметить возврат.
        var booking = await db.Bookings.Include(x => x.BookingSeats).Include(x => x.Payment).SingleAsync(x => x.Id == id);
        if (session.StartsAt <= clock.Now)
            throw new BookingException("После начала сеанса отмена и возврат недоступны.");
        if (refund && booking.Status == BookingStatus.Paid && booking.Payment != null)
        {
            booking.Status = BookingStatus.Refunded;
            booking.Payment.IsRefunded = true;
        }
        else if (!refund && booking.Status == BookingStatus.Pending)
            booking.Status = BookingStatus.Cancelled;
        else
            throw new BookingException("Операция недоступна для текущего статуса заказа.");
        foreach (var seat in booking.BookingSeats)
            seat.Active = null;
        // Записываем накопленные изменения сущностей в SQLite.
        await db.SaveChangesAsync();
        // Подтверждаем транзакцию: связанные изменения заказа и мест сохраняются вместе.
        await transaction.CommitAsync();
    }
}
