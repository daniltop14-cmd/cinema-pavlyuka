// Выбирает фильмы и сеансы из SQLite для афиши и страницы фильма.
using Cinema.Data;
using Cinema.Services;
using Cinema.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Controllers;
public class MoviesController(CinemaDbContext db, CinemaClock clock) : Controller
{
    // Фильтры date и genre приходят из URL; запрос к SQLite оставляет подходящие фильмы и передаёт их в афишу.
    public async Task<IActionResult> Index(DateTime? date, string? genre)
    {
        var today = clock.Now.Date;
        var selected = (date ?? today).Date;
        if (selected < today || selected > today.AddDays(30))
            selected = today;
        // Читаем из SQLite только неотменённые сеансы; дата и время ограничивают показ расписания.
        var query = db.Sessions.AsNoTracking().Include(x => x.Movie).Include(x => x.Hall).Where(x => !x.IsCancelled && x.StartsAt >= selected && x.StartsAt < selected.AddDays(1) && x.StartsAt > clock.Now);
        if (!string.IsNullOrWhiteSpace(genre))
            query = query.Where(x => x.Movie.Genre == genre);
        var sessions = await query.OrderBy(x => x.StartsAt).ToListAsync();
        return View(new ScheduleModel { Date = selected, Genre = genre, Sessions = sessions, Movies = sessions.Select(x => x.Movie).DistinctBy(x => x.Id).ToList(), Genres = await db.Movies.AsNoTracking().Select(x => x.Genre).Distinct().OrderBy(x => x).ToListAsync() });
    }

    // id фильма берётся из URL; загружаются карточка фильма и доступные сеансы для Views/Movies/Details.cshtml.
    public async Task<IActionResult> Details(int id)
    {
        var movie = await db.Movies.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (movie == null)
            return NotFound();
        // Читаем из SQLite только неотменённые сеансы; дата и время ограничивают показ расписания.
        ViewBag.Sessions = await db.Sessions.AsNoTracking().Include(x => x.Hall).Where(x => x.MovieId == id && !x.IsCancelled && x.StartsAt > clock.Now && x.StartsAt < clock.Now.Date.AddDays(31)).OrderBy(x => x.StartsAt).ToListAsync();

        return View(movie);
    }
}
