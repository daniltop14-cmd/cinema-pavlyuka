using Cinema.Data;
using Cinema.Models;
using Cinema.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Admin.Controllers;
[Area("Admin"), Authorize(Roles = "Admin,Manager")]
public class DashboardController(CinemaDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        // SQLite EF Core 8 does not aggregate decimal; calculate exactly in .NET.
        var amounts = await db.Payments.AsNoTracking().Where(x => !x.IsRefunded)
            .Select(x => x.Amount).ToListAsync();
        return View(new DashboardModel
        {
            Users = await db.Users.CountAsync(), Movies = await db.Movies.CountAsync(),
            Sessions = await db.Sessions.CountAsync(),
            SoldTickets = await db.BookingSeats.CountAsync(x => x.Booking.Status == BookingStatus.Paid),
            Revenue = amounts.Sum(),
            Recent = await db.Bookings.AsNoTracking().Include(x => x.User).Include(x => x.Session)
                .ThenInclude(x => x.Movie).OrderByDescending(x => x.CreatedAtUtc).Take(8).ToListAsync()
        });
    }
}
