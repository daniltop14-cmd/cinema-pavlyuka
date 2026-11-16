// Выводит афишу и подробности фильма.
// Пояснения ниже относятся к следующей строке или смысловому шагу.
// Подключаем пространство имён Cinema.Data для типов ниже.
using Cinema.Data;
// Подключаем пространство имён Cinema.Services для типов ниже.
using Cinema.Services;
// Подключаем пространство имён Cinema.ViewModels для типов ниже.
using Cinema.ViewModels;
// Подключаем пространство имён Microsoft.AspNetCore.Mvc для типов ниже.
using Microsoft.AspNetCore.Mvc;
// Подключаем пространство имён Microsoft.EntityFrameworkCore для типов ниже.
using Microsoft.EntityFrameworkCore;

// Помещаем объявленные типы в указанное пространство имён.
namespace Cinema.Controllers;
// Выводит афишу и подробности фильма.
public class MoviesController(CinemaDbContext db, CinemaClock clock) : Controller
{
    // Готовим основную страницу этого раздела и возвращаем представление.
    public async Task<IActionResult> Index(DateTime? date, string? genre)
    {
        // Вычисляем today для дальнейших проверок или отображения.
        var today = clock.Now.Date;
        // Вычисляем selected для дальнейших проверок или отображения.
        var selected = (date ?? today).Date;
        // Выполняем следующую ветку только при истинности условия.
        if (selected < today || selected > today.AddDays(30))
            // Записываем вычисленное значение в selected.
            selected = today;
        // Получаем query из базы или подготавливаем запрос к ней.
        var query = db.Sessions.AsNoTracking().Include(x => x.Movie).Include(x => x.Hall).Where(x => !x.IsCancelled && x.StartsAt >= selected && x.StartsAt < selected.AddDays(1) && x.StartsAt > clock.Now);
        // Выполняем следующую ветку только при истинности условия.
        if (!string.IsNullOrWhiteSpace(genre))
            // Оставляем в выборке только записи, подходящие под условие.
            query = query.Where(x => x.Movie.Genre == genre);
        // Асинхронно получаем sessions, не блокируя поток сервера.
        var sessions = await query.OrderBy(x => x.StartsAt).ToListAsync();
        // Передаём данные в Razor-представление и возвращаем HTML.
        return View(new ScheduleModel { Date = selected, Genre = genre, Sessions = sessions, Movies = sessions.Select(x => x.Movie).DistinctBy(x => x.Id).ToList(), Genres = await db.Movies.AsNoTracking().Select(x => x.Genre).Distinct().OrderBy(x => x).ToListAsync() });
    }

    // Объявляем метод Details: параметры приходят из запроса или вызова сервиса.
    public async Task<IActionResult> Details(int id)
    {
        // Получаем movie из базы или подготавливаем запрос к ней.
        var movie = await db.Movies.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        // Обрабатываем отсутствие записи или необязательного значения.
        if (movie == null)
            // Отвечаем 404, когда запрошенная запись не найдена или недоступна.
            return NotFound();
        // Оставляем в выборке только записи, подходящие под условие.
        ViewBag.Sessions = await db.Sessions.AsNoTracking().Include(x => x.Hall).Where(x => x.MovieId == id && !x.IsCancelled && x.StartsAt > clock.Now && x.StartsAt < clock.Now.Date.AddDays(31)).OrderBy(x => x.StartsAt).ToListAsync();
        // Передаём данные в Razor-представление и возвращаем HTML.
        return View(movie);
    }
}
