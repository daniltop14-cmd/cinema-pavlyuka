// Проверяет места, сохраняет брони и изменяет состояние заказов в транзакциях.
// Пояснения ниже относятся к следующей строке или смысловому шагу.
// Подключаем пространство имён Cinema.Data для типов ниже.
using Cinema.Data;
// Подключаем пространство имён Cinema.Models для типов ниже.
using Cinema.Models;
// Подключаем пространство имён Microsoft.EntityFrameworkCore для типов ниже.
using Microsoft.EntityFrameworkCore;

// Помещаем объявленные типы в указанное пространство имён.
namespace Cinema.Services;
// Проверяет места, сохраняет брони и изменяет состояние заказов в транзакциях.
public class BookingException(string message) : Exception(message);
// Проверяет места, сохраняет брони и изменяет состояние заказов в транзакциях.
public class BookingService(CinemaDbContext db, CinemaClock clock)
{
    // Microsoft.Data.Sqlite starts an IMMEDIATE transaction by default: only one
    // writer can enter. Every booking mutation begins it BEFORE checking seats.
    // Объявляем метод GetSessionAsync: параметры приходят из запроса или вызова сервиса.
    public async Task<Session> GetSessionAsync(int sessionId)
    {
        // Получаем session из базы или подготавливаем запрос к ней.
        var session = await db.Sessions.SingleOrDefaultAsync(x => x.Id == sessionId);
        // Возвращаем результат вызывающему коду.
        return session ?? throw new BookingException("Сеанс не найден.");
    }

    // Объявляем метод ExpireSessionAsync: параметры приходят из запроса или вызова сервиса.
    private async Task ExpireSessionAsync(int sessionId)
    {
        // Получаем expired из базы или подготавливаем запрос к ней.
        var expired = await db.Bookings.Where(x => x.SessionId == sessionId && x.Status == BookingStatus.Pending && x.ExpiresAtUtc <= DateTime.UtcNow).Include(x => x.BookingSeats).ToListAsync();
        // Последовательно обрабатываем каждый элемент коллекции.
        foreach (var booking in expired)
        {
            // Записываем вычисленное значение в status.
            booking.Status = BookingStatus.Expired;
            // Последовательно обрабатываем каждый элемент коллекции.
            foreach (var seat in booking.BookingSeats)
                // Записываем вычисленное значение в active.
                seat.Active = null;
        }

        // Сохраняем накопленные изменения в SQLite.
        await db.SaveChangesAsync();
    }

    // Объявляем метод CleanupSessionAsync: параметры приходят из запроса или вызова сервиса.
    public async Task CleanupSessionAsync(int sessionId)
    {
        // Начинаем транзакцию до чтения и изменения связанных данных.
        await using var transaction = await db.Database.BeginTransactionAsync();
        // Дожидаемся завершения асинхронной операции.
        await GetSessionAsync(sessionId);
        // Дожидаемся завершения асинхронной операции.
        await ExpireSessionAsync(sessionId);
        // Дожидаемся завершения асинхронной операции.
        await transaction.CommitAsync();
    }

    // Создаём запись после проверки входных данных и ограничений.
    public async Task<int> CreateAsync(string userId, int sessionId, int[] seatIds)
    {
        // Вычисляем ids для дальнейших проверок или отображения.
        var ids = seatIds.Distinct().ToArray();
        // Выполняем следующую ветку только при истинности условия.
        if (ids.Length is < 1 or > 12)
            // Прерываем выполнение при нарушении обязательного условия.
            throw new BookingException("Выберите от 1 до 12 мест.");
        // Начинаем транзакцию до чтения и изменения связанных данных.
        await using var transaction = await db.Database.BeginTransactionAsync();
        // Асинхронно получаем session, не блокируя поток сервера.
        var session = await GetSessionAsync(sessionId);
        // Дожидаемся завершения асинхронной операции.
        await ExpireSessionAsync(sessionId);
        // Выполняем следующую ветку только при истинности условия.
        if (session.IsCancelled || session.StartsAt <= clock.Now)
            // Прерываем выполнение при нарушении обязательного условия.
            throw new BookingException("Сеанс уже начался или отменён.");
        // Получаем seats из базы или подготавливаем запрос к ней.
        var seats = await db.Seats.Where(x => ids.Contains(x.Id) && x.HallId == session.HallId && x.IsAccessible).ToListAsync();
        // Выполняем следующую ветку только при истинности условия.
        if (seats.Count != ids.Length)
            // Прерываем выполнение при нарушении обязательного условия.
            throw new BookingException("В выборе есть недоступные места.");
        // Выполняем следующую ветку только при истинности условия.
        if (await db.BookingSeats.AnyAsync(x => x.SessionId == sessionId && ids.Contains(x.SeatId) && x.Active == 1))
            // Прерываем выполнение при нарушении обязательного условия.
            throw new BookingException("Одно из мест только что заняли. Выберите другие места.");
        // Вычисляем booking для дальнейших проверок или отображения.
        var booking = new Booking
        {
            // Записываем вычисленное значение в user id.
            UserId = userId,
            // Записываем вычисленное значение в session id.
            SessionId = sessionId,
            // Записываем вычисленное значение в created at utc.
            CreatedAtUtc = DateTime.UtcNow,
            // Записываем вычисленное значение в expires at utc.
            ExpiresAtUtc = DateTime.UtcNow.AddMinutes(10),
            // Записываем вычисленное значение в status.
            Status = BookingStatus.Pending,
            // Записываем вычисленное значение в total.
            Total = session.Price * ids.Length,
            // Записываем вычисленное значение в booking seats.
            BookingSeats = ids.Select(id => new BookingSeat { SeatId = id, SessionId = sessionId, Price = session.Price, Active = 1 }).ToList()
        };
        // Добавляем новую сущность в контекст для последующего сохранения.
        db.Bookings.Add(booking);
        // Сохраняем накопленные изменения в SQLite.
        await db.SaveChangesAsync(); // Unique index is a second line of defence.
        // Дожидаемся завершения асинхронной операции.
        await transaction.CommitAsync();
        // Возвращаем результат вызывающему коду.
        return booking.Id;
    }

    // Объявляем метод PayAsync: параметры приходят из запроса или вызова сервиса.
    public async Task PayAsync(int id, string userId)
    {
        // Получаем session id из базы или подготавливаем запрос к ней.
        var sessionId = await db.Bookings.AsNoTracking().Where(x => x.Id == id && x.UserId == userId).Select(x => (int? )x.SessionId).SingleOrDefaultAsync() ?? throw new BookingException("Заказ не найден.");
        // Начинаем транзакцию до чтения и изменения связанных данных.
        await using var transaction = await db.Database.BeginTransactionAsync();
        // Асинхронно получаем session, не блокируя поток сервера.
        var session = await GetSessionAsync(sessionId);
        // Дожидаемся завершения асинхронной операции.
        await ExpireSessionAsync(sessionId);
        // Получаем booking из базы или подготавливаем запрос к ней.
        var booking = await db.Bookings.SingleAsync(x => x.Id == id && x.UserId == userId);
        // Выполняем следующую ветку только при истинности условия.
        if (booking.Status == BookingStatus.Paid)
        {
            // Дожидаемся завершения асинхронной операции.
            await transaction.CommitAsync();
            // Завершаем действие или объявление текущей строки.
            return;
        }

        // Выполняем следующую ветку только при истинности условия.
        if (booking.Status != BookingStatus.Pending || booking.ExpiresAtUtc <= DateTime.UtcNow || session.StartsAt <= clock.Now || session.IsCancelled)
            // Прерываем выполнение при нарушении обязательного условия.
            throw new BookingException("Срок брони истёк или сеанс недоступен. Создайте новый заказ.");
        // Записываем вычисленное значение в status.
        booking.Status = BookingStatus.Paid;
        // Добавляем новую сущность в контекст для последующего сохранения.
        db.Payments.Add(new Payment { BookingId = id, Amount = booking.Total, PaidAtUtc = DateTime.UtcNow });
        // Сохраняем накопленные изменения в SQLite.
        await db.SaveChangesAsync();
        // Дожидаемся завершения асинхронной операции.
        await transaction.CommitAsync();
    }

    // Объявляем метод CancelAsync: параметры приходят из запроса или вызова сервиса.
    public async Task CancelAsync(int id, string? ownerId, bool refund = false)
    {
        // Получаем session id из базы или подготавливаем запрос к ней.
        var sessionId = await db.Bookings.AsNoTracking().Where(x => x.Id == id && (ownerId == null || x.UserId == ownerId)).Select(x => (int? )x.SessionId).SingleOrDefaultAsync() ?? throw new BookingException("Заказ не найден.");
        // Начинаем транзакцию до чтения и изменения связанных данных.
        await using var transaction = await db.Database.BeginTransactionAsync();
        // Асинхронно получаем session, не блокируя поток сервера.
        var session = await GetSessionAsync(sessionId);
        // Дожидаемся завершения асинхронной операции.
        await ExpireSessionAsync(sessionId);
        // Получаем booking из базы или подготавливаем запрос к ней.
        var booking = await db.Bookings.Include(x => x.BookingSeats).Include(x => x.Payment).SingleAsync(x => x.Id == id);
        // Выполняем следующую ветку только при истинности условия.
        if (session.StartsAt <= clock.Now)
            // Прерываем выполнение при нарушении обязательного условия.
            throw new BookingException("После начала сеанса отмена и возврат недоступны.");
        // Обрабатываем отсутствие записи или необязательного значения.
        if (refund && booking.Status == BookingStatus.Paid && booking.Payment != null)
        {
            // Записываем вычисленное значение в status.
            booking.Status = BookingStatus.Refunded;
            // Записываем вычисленное значение в is refunded.
            booking.Payment.IsRefunded = true;
        }
        // Обрабатываем альтернативный случай предыдущей проверки.
        else if (!refund && booking.Status == BookingStatus.Pending)
            // Записываем вычисленное значение в status.
            booking.Status = BookingStatus.Cancelled;
        // Обрабатываем альтернативный случай предыдущей проверки.
        else
            // Прерываем выполнение при нарушении обязательного условия.
            throw new BookingException("Операция недоступна для текущего статуса заказа.");
        // Последовательно обрабатываем каждый элемент коллекции.
        foreach (var seat in booking.BookingSeats)
            // Записываем вычисленное значение в active.
            seat.Active = null;
        // Сохраняем накопленные изменения в SQLite.
        await db.SaveChangesAsync();
        // Дожидаемся завершения асинхронной операции.
        await transaction.CommitAsync();
    }
}
