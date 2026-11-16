// Редактирует фильмы и проверяет загружаемые постеры.
// Пояснения ниже относятся к следующей строке или смысловому шагу.
// Подключаем пространство имён Cinema.Data для типов ниже.
using Cinema.Data;
// Подключаем пространство имён Cinema.Models для типов ниже.
using Cinema.Models;
// Подключаем пространство имён Microsoft.AspNetCore.Authorization для типов ниже.
using Microsoft.AspNetCore.Authorization;
// Подключаем пространство имён Microsoft.AspNetCore.Mvc для типов ниже.
using Microsoft.AspNetCore.Mvc;
// Подключаем пространство имён Microsoft.EntityFrameworkCore для типов ниже.
using Microsoft.EntityFrameworkCore;

// Помещаем объявленные типы в указанное пространство имён.
namespace Cinema.Areas.Admin.Controllers;
// Ограничиваем доступ: роль проверяется сервером до вызова действия.
[Area("Admin"), Authorize(Roles = "Admin")]
// Редактирует фильмы и проверяет загружаемые постеры.
public class MoviesController(CinemaDbContext db, IWebHostEnvironment env) : Controller
{
    // Готовим основную страницу этого раздела и возвращаем представление.
    public async Task<IActionResult> Index(int page = 1)
    {
        // Записываем вычисленное значение в page.
        page = Math.Max(1, page);
        // Записываем вычисленное значение в page.
        ViewBag.Page = page;
        // Записываем вычисленное значение в has next.
        ViewBag.HasNext = await db.Movies.CountAsync() > page * 20;
        // Передаём данные в Razor-представление и возвращаем HTML.
        return View(await db.Movies.AsNoTracking().OrderByDescending(x => x.Id).Skip((page - 1) * 20).Take(20).ToListAsync());
    }

    // Назначаем адрес или тип GET-запроса для этого действия.
    [HttpGet]
    // Показываем или сохраняем форму редактирования записи.
    public async Task<IActionResult> Edit(int? id)
    {
        // Обрабатываем отсутствие записи или необязательного значения.
        if (id == null)
            // Передаём данные в Razor-представление и возвращаем HTML.
            return View(new Movie());
        // Получаем movie из базы или подготавливаем запрос к ней.
        var movie = await db.Movies.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        // Возвращаем результат вызывающему коду.
        return movie == null ? NotFound() : View(movie);
    }

    // Разрешаем изменение данных только через POST-запрос.
    [HttpPost, RequestSizeLimit(4 * 1024 * 1024)]
    // Показываем или сохраняем форму редактирования записи.
    public async Task<IActionResult> Edit(int id, [Bind("Title,Genre,Country,DurationMinutes,AgeRating,Description,TrailerUrl,ReleaseDate,IsFeatured")] Movie input, IFormFile? poster)
    {
        // Обрабатываем отсутствие записи или необязательного значения.
        if (input.TrailerUrl != null && (!Uri.TryCreate(input.TrailerUrl, UriKind.Absolute, out var uri) || uri.Scheme != "https"))
            // Завершаем действие или объявление текущей строки.
            ModelState.AddModelError(nameof(Movie.TrailerUrl), "Используйте HTTPS-ссылку на трейлер.");
        // Записываем вычисленное значение в string? path.
        string? path = null;
        // Обрабатываем отсутствие записи или необязательного значения.
        if (poster != null)
        {
            // Выполняем следующую ветку только при истинности условия.
            if (poster.Length is < 12 or > 3 * 1024 * 1024)
                // Завершаем действие или объявление текущей строки.
                ModelState.AddModelError("", "Постер должен быть не больше 3 МБ.");
            // Обрабатываем альтернативный случай предыдущей проверки.
            else
            {
                // Вычисляем header для дальнейших проверок или отображения.
                var header = new byte[12];
                // Асинхронно получаем stream, не блокируя поток сервера.
                await using var stream = poster.OpenReadStream();
                // Дожидаемся завершения асинхронной операции.
                await stream.ReadExactlyAsync(header);
                // Вычисляем ext для дальнейших проверок или отображения.
                var ext = header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4e && header[3] == 0x47 && header[4] == 13 && header[5] == 10 && header[6] == 26 && header[7] == 10 ? ".png" : header[0] == 0xff && header[1] == 0xd8 && header[2] == 0xff ? ".jpg" : null;
                // Обрабатываем отсутствие записи или необязательного значения.
                if (ext == null)
                    // Завершаем действие или объявление текущей строки.
                    ModelState.AddModelError("", "Поддерживаются только PNG и JPEG с корректной сигнатурой.");
                // Обрабатываем альтернативный случай предыдущей проверки.
                else if (ModelState.IsValid)
                {
                    // Вычисляем directory для дальнейших проверок или отображения.
                    var directory = Path.Combine(env.WebRootPath, "uploads");
                    // Завершаем действие или объявление текущей строки.
                    Directory.CreateDirectory(directory);
                    // Записываем вычисленное значение в path.
                    path = "/uploads/" + Guid.NewGuid().ToString("N") + ext;
                    // Асинхронно получаем target, не блокируя поток сервера.
                    await using var target = System.IO.File.Create(Path.Combine(env.WebRootPath, path.TrimStart('/')));
                    // Записываем вычисленное значение в position.
                    stream.Position = 0;
                    // Дожидаемся завершения асинхронной операции.
                    await stream.CopyToAsync(target);
                }
            }
        }

        // Если форма содержит ошибки проверки, не сохраняем данные.
        if (!ModelState.IsValid)
        {
            // Записываем вычисленное значение в id.
            input.Id = id;
            // Передаём данные в Razor-представление и возвращаем HTML.
            return View(input);
        }

        // Получаем movie из базы или подготавливаем запрос к ней.
        var movie = id == 0 ? new Movie() : await db.Movies.SingleOrDefaultAsync(x => x.Id == id);
        // Обрабатываем отсутствие записи или необязательного значения.
        if (movie == null)
            // Отвечаем 404, когда запрошенная запись не найдена или недоступна.
            return NotFound();
        // Existing schedules depend on duration; use a new film record to change it.
        // Выполняем следующую ветку только при истинности условия.
        if (id != 0 && movie.DurationMinutes != input.DurationMinutes && await db.Sessions.AnyAsync(x => x.MovieId == id))
        {
            // Завершаем действие или объявление текущей строки.
            ModelState.AddModelError("", "Нельзя менять длительность фильма с созданными сеансами. Сначала удалите сеансы без заказов.");
            // Записываем вычисленное значение в id.
            input.Id = id;
            // Передаём данные в Razor-представление и возвращаем HTML.
            return View(input);
        }

        // Записываем вычисленное значение в title.
        movie.Title = input.Title;
        // Записываем вычисленное значение в genre.
        movie.Genre = input.Genre;
        // Записываем вычисленное значение в country.
        movie.Country = input.Country;
        // Записываем вычисленное значение в duration minutes.
        movie.DurationMinutes = input.DurationMinutes;
        // Записываем вычисленное значение в age rating.
        movie.AgeRating = input.AgeRating;
        // Записываем вычисленное значение в description.
        movie.Description = input.Description;
        // Записываем вычисленное значение в trailer url.
        movie.TrailerUrl = input.TrailerUrl;
        // Записываем вычисленное значение в release date.
        movie.ReleaseDate = input.ReleaseDate;
        // Записываем вычисленное значение в is featured.
        movie.IsFeatured = input.IsFeatured;
        // Обрабатываем отсутствие записи или необязательного значения.
        if (path != null)
            // Записываем вычисленное значение в poster path.
            movie.PosterPath = path;
        // Выполняем следующую ветку только при истинности условия.
        if (id == 0)
            // Добавляем новую сущность в контекст для последующего сохранения.
            db.Movies.Add(movie);
        // Сохраняем накопленные изменения в SQLite.
        await db.SaveChangesAsync();
        // Записываем вычисленное значение в temp data["success"].
        TempData["Success"] = "Фильм сохранён.";
        // После операции перенаправляем браузер на указанную страницу.
        return RedirectToAction(nameof(Index));
    }

    // Разрешаем изменение данных только через POST-запрос.
    [HttpPost]
    // Выполняем действие Delete после проверки прав и состояния записи.
    public async Task<IActionResult> Delete(int id)
    {
        // Получаем movie из базы или подготавливаем запрос к ней.
        var movie = await db.Movies.FindAsync(id);
        // Обрабатываем отсутствие записи или необязательного значения.
        if (movie == null)
            // Отвечаем 404, когда запрошенная запись не найдена или недоступна.
            return NotFound();
        // Выполняем следующую ветку только при истинности условия.
        if (await db.Sessions.AnyAsync(x => x.MovieId == id))
            // Записываем вычисленное значение в temp data["error"].
            TempData["Error"] = "Сначала удалите сеансы фильма. Сеансы с заказами удалить нельзя.";
        // Обрабатываем альтернативный случай предыдущей проверки.
        else
        {
            // Помечаем сущность на удаление при сохранении контекста.
            db.Movies.Remove(movie);
            // Начинаем блок, ошибки которого будут обработаны ниже.
            try
            {
                // Сохраняем накопленные изменения в SQLite.
                await db.SaveChangesAsync();
            }
            // Перехватываем ошибку, чтобы сохранить управляемое поведение.
            catch (DbUpdateException)
            {
                // Записываем вычисленное значение в temp data["error"].
                TempData["Error"] = "Фильм уже используется в расписании.";
            }
        }

        // После операции перенаправляем браузер на указанную страницу.
        return RedirectToAction(nameof(Index));
    }
}
