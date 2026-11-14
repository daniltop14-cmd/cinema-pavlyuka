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
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [AllowAnonymous]
    public IActionResult Index() => RedirectToAction("Index", "Movies");
    [AllowAnonymous]
    public async Task<IActionResult> Seats(int id)
    {
        var session = await db.Sessions.AsNoTracking().Include(x => x.Movie).Include(x => x.Hall).SingleOrDefaultAsync(x => x.Id == id && !x.IsCancelled && x.StartsAt > clock.Now);
        if (session == null)
            return NotFound();
        await bookings.CleanupSessionAsync(id);
        return View(new SeatMapModel { Session = session, Seats = await db.Seats.AsNoTracking().Where(x => x.HallId == session.HallId).OrderBy(x => x.Row).ThenBy(x => x.Number).ToListAsync(), Occupied = (await db.BookingSeats.AsNoTracking().Where(x => x.SessionId == id && x.Active == 1).Select(x => x.SeatId).ToListAsync()).ToHashSet() });
    }

    [HttpPost]
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

    public async Task<IActionResult> Checkout(int id)
    {
        var sessionId = await db.Bookings.AsNoTracking().Where(x => x.Id == id && x.UserId == UserId).Select(x => (int? )x.SessionId).SingleOrDefaultAsync();
        if (sessionId == null)
            return NotFound();
        await bookings.CleanupSessionAsync(sessionId.Value);
        var booking = await OrderQuery().SingleAsync(x => x.Id == id && x.UserId == UserId);
        return View(booking);
    }

    [HttpPost]
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

    public async Task<IActionResult> Ticket(int id)
    {
        var booking = await OrderQuery().SingleOrDefaultAsync(x => x.Id == id && x.UserId == UserId && x.Status == BookingStatus.Paid);
        return booking == null ? NotFound() : View(booking);
    }

    private IQueryable<Booking> OrderQuery() => db.Bookings.AsNoTracking().Include(x => x.Session).ThenInclude(x => x.Movie).Include(x => x.Session).ThenInclude(x => x.Hall).Include(x => x.BookingSeats).ThenInclude(x => x.Seat);
}
