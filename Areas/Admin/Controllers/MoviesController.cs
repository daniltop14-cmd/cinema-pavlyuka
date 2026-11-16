// Редактирует фильмы в SQLite и проверяет файлы загружаемых обложек.
using Cinema.Data;
using Cinema.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Admin.Controllers;
[Area("Admin"), Authorize(Roles = "Admin")]
public class MoviesController(CinemaDbContext db, IWebHostEnvironment env) : Controller
{

    public async Task<IActionResult> Index(int page = 1)
    {
        page = Math.Max(1, page);
        ViewBag.Page = page;
        ViewBag.HasNext = await db.Movies.CountAsync() > page * 20;
        return View(await db.Movies.AsNoTracking().OrderByDescending(x => x.Id).Skip((page - 1) * 20).Take(20).ToListAsync());
    }

    [HttpGet]
    // GET загружает фильм для формы; POST проверяет поля и изображение, сохраняет фильм и путь к обложке.
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)

            return View(new Movie());
        var movie = await db.Movies.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        return movie == null ? NotFound() : View(movie);
    }

    [HttpPost, RequestSizeLimit(4 * 1024 * 1024)]
    // GET загружает фильм для формы; POST проверяет поля и изображение, сохраняет фильм и путь к обложке.
    public async Task<IActionResult> Edit(int id, [Bind("Title,Genre,Country,DurationMinutes,AgeRating,Description,TrailerUrl,ReleaseDate,IsFeatured")] Movie input, IFormFile? poster)
    {
        if (input.TrailerUrl != null && (!Uri.TryCreate(input.TrailerUrl, UriKind.Absolute, out var uri) || uri.Scheme != "https"))
            ModelState.AddModelError(nameof(Movie.TrailerUrl), "Используйте HTTPS-ссылку на трейлер.");
        string? path = null;
        if (poster != null)
        {
            if (poster.Length is < 12 or > 3 * 1024 * 1024)
                ModelState.AddModelError("", "Постер должен быть не больше 3 МБ.");
            else
            {
                var header = new byte[12];
                await using var stream = poster.OpenReadStream();
                await stream.ReadExactlyAsync(header);
                var ext = header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4e && header[3] == 0x47 && header[4] == 13 && header[5] == 10 && header[6] == 26 && header[7] == 10 ? ".png" : header[0] == 0xff && header[1] == 0xd8 && header[2] == 0xff ? ".jpg" : null;
                if (ext == null)
                    ModelState.AddModelError("", "Поддерживаются только PNG и JPEG с корректной сигнатурой.");
                // Проверяем атрибуты модели и ошибки привязки формы; при ошибках снова показываем форму.
                else if (ModelState.IsValid)
                {
                    var directory = Path.Combine(env.WebRootPath, "uploads");
                    Directory.CreateDirectory(directory);
                    path = "/uploads/" + Guid.NewGuid().ToString("N") + ext;
                    await using var target = System.IO.File.Create(Path.Combine(env.WebRootPath, path.TrimStart('/')));
                    stream.Position = 0;
                    await stream.CopyToAsync(target);
                }
            }
        }

        // Проверяем атрибуты модели и ошибки привязки формы; при ошибках снова показываем форму.
        if (!ModelState.IsValid)
        {
            input.Id = id;

            return View(input);
        }

        var movie = id == 0 ? new Movie() : await db.Movies.SingleOrDefaultAsync(x => x.Id == id);
        if (movie == null)
            return NotFound();
        // Existing schedules depend on duration; use a new film record to change it.
        if (id != 0 && movie.DurationMinutes != input.DurationMinutes && await db.Sessions.AnyAsync(x => x.MovieId == id))
        {
            ModelState.AddModelError("", "Нельзя менять длительность фильма с созданными сеансами. Сначала удалите сеансы без заказов.");
            input.Id = id;

            return View(input);
        }

        movie.Title = input.Title;
        movie.Genre = input.Genre;
        movie.Country = input.Country;
        movie.DurationMinutes = input.DurationMinutes;
        movie.AgeRating = input.AgeRating;
        movie.Description = input.Description;
        movie.TrailerUrl = input.TrailerUrl;
        movie.ReleaseDate = input.ReleaseDate;
        movie.IsFeatured = input.IsFeatured;
        if (path != null)
            movie.PosterPath = path;
        if (id == 0)
            db.Movies.Add(movie);
        // Записываем накопленные изменения сущностей в SQLite.
        await db.SaveChangesAsync();
        TempData["Success"] = "Фильм сохранён.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    // id записи приходит из POST-формы; после проверки связей удаляет запись из SQLite.
    public async Task<IActionResult> Delete(int id)
    {
        var movie = await db.Movies.FindAsync(id);
        if (movie == null)
            return NotFound();
        // Получаем сеансы из SQLite; связанные фильм и зал загружаются через Include, если нужны странице.
        if (await db.Sessions.AnyAsync(x => x.MovieId == id))
            TempData["Error"] = "Сначала удалите сеансы фильма. Сеансы с заказами удалить нельзя.";
        else
        {
            db.Movies.Remove(movie);
            try
            {
                // Записываем накопленные изменения сущностей в SQLite.
                await db.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                TempData["Error"] = "Фильм уже используется в расписании.";
            }
        }

        return RedirectToAction(nameof(Index));
    }
}
