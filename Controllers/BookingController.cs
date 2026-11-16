// Принимает запросы выбора мест и заказа; правила бронирования выполняет BookingService.
using System.Security.Claims;
using Cinema.Data;
using Cinema.Models;
using Cinema.Services;
using Cinema.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Controllers;
[Authorize]
public class BookingController(CinemaDbContext db, BookingService bookings, IPaymentService payments, CinemaClock clock) : Controller
{
    // Берём идентификатор текущего пользователя из подтверждённой cookie входа, а не из формы браузера.
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [AllowAnonymous]
    public IActionResult Index() => RedirectToAction("Index", "Movies");
    [AllowAnonymous]
    // id сеанса берётся из URL; загружаются зал, кресла и занятость для схемы выбора мест.
    public async Task<IActionResult> Seats(int id)
    {
        // Читаем из SQLite только неотменённые сеансы; дата и время ограничивают показ расписания.
        var session = await db.Sessions.AsNoTracking().Include(x => x.Movie).Include(x => x.Hall).SingleOrDefaultAsync(x => x.Id == id && !x.IsCancelled && x.StartsAt > clock.Now);
        if (session == null)
            return NotFound();
        await bookings.CleanupSessionAsync(id);
        // Читаем кресла выбранного зала из SQLite; номер ряда и места определяет схему.
        return View(new SeatMapModel { Session = session, Seats = await db.Seats.AsNoTracking().Where(x => x.HallId == session.HallId).OrderBy(x => x.Row).ThenBy(x => x.Number).ToListAsync(), Occupied = (await db.BookingSeats.AsNoTracking().Where(x => x.SessionId == id && x.Active == 1).Select(x => x.SeatId).ToListAsync()).ToHashSet() });
    }

    [HttpPost]
    // sessionId и seatIds приходят из POST-формы; BookingService повторно проверяет места и создаёт заказ.
    public async Task<IActionResult> Create(int sessionId, int[] seatIds)
    {
        try
        {
            var id = await bookings.CreateAsync(UserId, sessionId, seatIds);
            return RedirectToAction(nameof(Checkout), new { id });
        }
        catch (BookingException ex)
        {
            TempData["Error"] = ex.Message;
        }
        catch (DbUpdateException)
        {
            TempData["Error"] = "Не удалось закрепить места. Обновите схему и попробуйте снова.";
        }

        return RedirectToAction(nameof(Seats), new { id = sessionId });
    }

    // По id заказа загружаются фильм, сеанс и места; чужой заказ не показывается.
    public async Task<IActionResult> Checkout(int id)
    {
        // Выбираем из SQLite заказы текущего пользователя, чей id получен из cookie входа.
        var sessionId = await db.Bookings.AsNoTracking().Where(x => x.Id == id && x.UserId == UserId).Select(x => (int? )x.SessionId).SingleOrDefaultAsync();
        if (sessionId == null)
            return NotFound();
        await bookings.CleanupSessionAsync(sessionId.Value);
        var booking = await OrderQuery().SingleAsync(x => x.Id == id && x.UserId == UserId);

        return View(booking);
    }

    [HttpPost]
    // По id заказа и текущему пользователю вызывает сервис оплаты, затем переводит браузер к билету.
    public async Task<IActionResult> Pay(int id)
    {
        try
        {
            await payments.PayAsync(id, UserId);
            TempData["Success"] = "Оплата выполнена. Ваши билеты готовы!";
            return RedirectToAction(nameof(Ticket), new { id });
        }
        catch (BookingException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Checkout), new { id });
        }
    }

    [HttpPost]
    // По id заказа и текущему пользователю отменяет бронь и освобождает места.
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            await bookings.CancelAsync(id, UserId);
            TempData["Success"] = "Бронирование отменено.";
        }
        catch (BookingException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction("Orders", "Account");
    }

    // Читает оплаченный заказ по id и передаёт данные в страницу электронного билета.
    public async Task<IActionResult> Ticket(int id)
    {
        var booking = await OrderQuery().SingleOrDefaultAsync(x => x.Id == id && x.UserId == UserId && x.Status == BookingStatus.Paid);
        return booking == null ? NotFound() : View(booking);
    }

    private IQueryable<Booking> OrderQuery() => db.Bookings.AsNoTracking().Include(x => x.Session).ThenInclude(x => x.Movie).Include(x => x.Session).ThenInclude(x => x.Hall).Include(x => x.BookingSeats).ThenInclude(x => x.Seat);
}
