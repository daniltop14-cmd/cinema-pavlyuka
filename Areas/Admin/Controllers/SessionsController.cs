using Cinema.Data;
using Cinema.Models;
using Cinema.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Admin.Controllers;
[Area("Admin"), Authorize(Roles = "Admin")]
public class SessionsController(CinemaDbContext db, CinemaClock clock, BookingService bookings) : Controller
{
    public async Task<IActionResult> Index(int page = 1)
    {
        page = Math.Max(1, page);
        ViewBag.Page = page;
        ViewBag.HasNext = await db.Sessions.CountAsync() > page * 30;
        return View(await db.Sessions.AsNoTracking().Include(x => x.Movie).Include(x => x.Hall).OrderBy(x => x.StartsAt).Skip((page - 1) * 30).Take(30).ToListAsync());
    }

    private async Task Lists()
    {
        ViewBag.Movies = new SelectList(await db.Movies.AsNoTracking().OrderBy(x => x.Title).ToListAsync(), "Id", "Title");
        ViewBag.Halls = new SelectList(await db.Halls.AsNoTracking().ToListAsync(), "Id", "Name");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        await Lists();
        if (id == null)
            return View(new Session { StartsAt = clock.Now.Date.AddDays(1).AddHours(12), Price = 450 });
        var session = await db.Sessions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        return session == null ? NotFound() : View(session);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, [Bind("MovieId,HallId,StartsAt,Price")] Session input)
    {
        ModelState.Remove(nameof(Session.Movie));
        ModelState.Remove(nameof(Session.Hall));
        var movie = await db.Movies.AsNoTracking().SingleOrDefaultAsync(x => x.Id == input.MovieId);
        if (movie == null)
            ModelState.AddModelError("", "Выберите существующий фильм.");
        if (input.StartsAt <= clock.Now)
            ModelState.AddModelError("", "Укажите будущее время сеанса.");
        if (!ModelState.IsValid)
        {
            input.Id = id;
            await Lists();
            return View(input);
        }

        await using var tx = await db.Database.BeginTransactionAsync();
        // The immediate SQLite transaction prevents concurrent overlapping edits.
        var hall = await db.Halls.SingleOrDefaultAsync(x => x.Id == input.HallId);
        if (hall == null)
            return NotFound();
        var session = id == 0 ? new Session() : await bookings.GetSessionAsync(id);
        if (id != 0 && session.HallId != input.HallId)
            ModelState.AddModelError("", "Для смены зала удалите сеанс без заказов и создайте новый.");
        if (id != 0 && await db.Bookings.AnyAsync(x => x.SessionId == id))
            ModelState.AddModelError("", "Сеанс с заказами нельзя изменять.");
        var end = input.StartsAt.AddMinutes(movie!.DurationMinutes + 20);
        if (await db.Sessions.AnyAsync(x => x.Id != id && x.HallId == input.HallId && !x.IsCancelled && x.StartsAt < end && x.EndsAt > input.StartsAt))
            ModelState.AddModelError("", "Сеанс пересекается с другим показом. Учтите 20 минут на уборку.");
        if (!ModelState.IsValid)
        {
            input.Id = id;
            await Lists();
            return View(input);
        }

        session.MovieId = input.MovieId;
        session.HallId = input.HallId;
        session.StartsAt = input.StartsAt;
        session.EndsAt = end;
        session.Price = input.Price;
        if (id == 0)
            db.Sessions.Add(session);
        await db.SaveChangesAsync();
        await tx.CommitAsync();
        TempData["Success"] = "Сеанс сохранён.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        var session = await bookings.GetSessionAsync(id);
        if (await db.Bookings.AnyAsync(x => x.SessionId == id))
            TempData["Error"] = "Удаление сеанса с заказами запрещено. История билетов должна сохраняться.";
        else
        {
            db.Sessions.Remove(session);
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
