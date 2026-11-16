// Редактирует сеансы и проверяет, что один зал не занят двумя показами одновременно.
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
        // Получаем сеансы из SQLite; связанные фильм и зал загружаются через Include, если нужны странице.
        ViewBag.HasNext = await db.Sessions.CountAsync() > page * 30;
        // Получаем сеансы из SQLite; связанные фильм и зал загружаются через Include, если нужны странице.
        return View(await db.Sessions.AsNoTracking().Include(x => x.Movie).Include(x => x.Hall).OrderBy(x => x.StartsAt).Skip((page - 1) * 30).Take(30).ToListAsync());
    }

    private async Task Lists()
    {
        ViewBag.Movies = new SelectList(await db.Movies.AsNoTracking().OrderBy(x => x.Title).ToListAsync(), "Id", "Title");
        // Загружаем описание залов из SQLite для вывода страницы или списка выбора.
        ViewBag.Halls = new SelectList(await db.Halls.AsNoTracking().ToListAsync(), "Id", "Name");
    }

    [HttpGet]
    // GET загружает сеанс и списки фильмов/залов; POST проверяет время и цену, затем сохраняет сеанс.
    public async Task<IActionResult> Edit(int? id)
    {
        await Lists();
        if (id == null)

            return View(new Session { StartsAt = clock.Now.Date.AddDays(1).AddHours(12), Price = 450 });
        // Получаем сеансы из SQLite; связанные фильм и зал загружаются через Include, если нужны странице.
        var session = await db.Sessions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        return session == null ? NotFound() : View(session);
    }

    [HttpPost]
    // GET загружает сеанс и списки фильмов/залов; POST проверяет время и цену, затем сохраняет сеанс.
    public async Task<IActionResult> Edit(int id, [Bind("MovieId,HallId,StartsAt,Price")] Session input)
    {
        ModelState.Remove(nameof(Session.Movie));
        ModelState.Remove(nameof(Session.Hall));
        var movie = await db.Movies.AsNoTracking().SingleOrDefaultAsync(x => x.Id == input.MovieId);
        if (movie == null)
            ModelState.AddModelError("", "Выберите существующий фильм.");
        if (input.StartsAt <= clock.Now)
            ModelState.AddModelError("", "Укажите будущее время сеанса.");
        // Проверяем атрибуты модели и ошибки привязки формы; при ошибках снова показываем форму.
        if (!ModelState.IsValid)
        {
            input.Id = id;
            await Lists();

            return View(input);
        }

        // Начинаем транзакцию SQLite до проверки и изменения мест, чтобы параллельные запросы не продали одно кресло дважды.
        await using var tx = await db.Database.BeginTransactionAsync();
        // The immediate SQLite transaction prevents concurrent overlapping edits.
        var hall = await db.Halls.SingleOrDefaultAsync(x => x.Id == input.HallId);
        if (hall == null)
            return NotFound();
        var session = id == 0 ? new Session() : await bookings.GetSessionAsync(id);
        if (id != 0 && session.HallId != input.HallId)
            ModelState.AddModelError("", "Для смены зала удалите сеанс без заказов и создайте новый.");
        // Читаем заказы из SQLite; статус и дата используются для отбора и вывода.
        if (id != 0 && await db.Bookings.AnyAsync(x => x.SessionId == id))
            ModelState.AddModelError("", "Сеанс с заказами нельзя изменять.");
        var end = input.StartsAt.AddMinutes(movie!.DurationMinutes + 20);
        // Читаем из SQLite только неотменённые сеансы; дата и время ограничивают показ расписания.
        if (await db.Sessions.AnyAsync(x => x.Id != id && x.HallId == input.HallId && !x.IsCancelled && x.StartsAt < end && x.EndsAt > input.StartsAt))
            ModelState.AddModelError("", "Сеанс пересекается с другим показом. Учтите 20 минут на уборку.");
        // Проверяем атрибуты модели и ошибки привязки формы; при ошибках снова показываем форму.
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
        // Записываем накопленные изменения сущностей в SQLite.
        await db.SaveChangesAsync();
        // Подтверждаем транзакцию: связанные изменения заказа и мест сохраняются вместе.
        await tx.CommitAsync();
        TempData["Success"] = "Сеанс сохранён.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    // id записи приходит из POST-формы; после проверки связей удаляет запись из SQLite.
    public async Task<IActionResult> Delete(int id)
    {
        // Начинаем транзакцию SQLite до проверки и изменения мест, чтобы параллельные запросы не продали одно кресло дважды.
        await using var tx = await db.Database.BeginTransactionAsync();
        var session = await bookings.GetSessionAsync(id);
        // Читаем заказы из SQLite; статус и дата используются для отбора и вывода.
        if (await db.Bookings.AnyAsync(x => x.SessionId == id))
            TempData["Error"] = "Удаление сеанса с заказами запрещено. История билетов должна сохраняться.";
        else
        {
            db.Sessions.Remove(session);
            // Записываем накопленные изменения сущностей в SQLite.
            await db.SaveChangesAsync();
            // Подтверждаем транзакцию: связанные изменения заказа и мест сохраняются вместе.
            await tx.CommitAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}
