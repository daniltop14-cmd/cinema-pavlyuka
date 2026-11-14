using Cinema.Data;
using Cinema.Models;
using Cinema.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Admin.Controllers;
[Area("Admin"), Authorize(Roles = "Admin,Manager")]
public class BookingsController(CinemaDbContext db, BookingService bookings, IPaymentService payments) : Controller
{
    public async Task<IActionResult> Index(string? search, BookingStatus? status, int page = 1)
    {
        var query = db.Bookings.AsNoTracking().Include(x => x.User).Include(x => x.Session).ThenInclude(x => x.Movie).Include(x => x.Session).ThenInclude(x => x.Hall).Include(x => x.BookingSeats).ThenInclude(x => x.Seat).Include(x => x.Payment).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.User.Email!.Contains(search) || x.Session.Movie.Title.Contains(search) || x.Id.ToString() == search);
        if (status != null)
            query = query.Where(x => x.Status == status);
        page = Math.Max(1, page);
        ViewBag.Page = page;
        ViewBag.HasNext = await query.CountAsync() > page * 20;
        ViewBag.Search = search;
        ViewBag.Status = status;
        return View(await query.OrderByDescending(x => x.CreatedAtUtc).Skip((page - 1) * 20).Take(20).ToListAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Confirm(int id)
    {
        var owner = await db.Bookings.AsNoTracking().Where(x => x.Id == id).Select(x => x.UserId).SingleOrDefaultAsync();
        if (owner == null)
            return NotFound();
        try
        {
            await payments.PayAsync(id, owner);
            TempData["Success"] = "Заказ подтверждён и оплачен.";
        }
        catch (BookingException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Cancel(int id, bool refund = false)
    {
        try
        {
            await bookings.CancelAsync(id, null, refund);
            TempData["Success"] = refund ? "Возврат выполнен." : "Бронь отменена.";
        }
        catch (BookingException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
