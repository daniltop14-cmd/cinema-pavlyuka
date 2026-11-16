// Обновляет пути обложек существующих фильмов.
// Пояснения ниже относятся к следующей строке или смысловому шагу.
// Подключаем пространство имён Cinema.Models для типов ниже.
using Cinema.Models;
// Подключаем пространство имён Microsoft.EntityFrameworkCore для типов ниже.
using Microsoft.EntityFrameworkCore;

// Помещаем объявленные типы в указанное пространство имён.
namespace Cinema.Data;

// Обновляет пути обложек существующих фильмов.
public static class CoverCatalog
{
    // Replace catalogue posters once when an existing local database is opened.
    // Orders, users, prices and later administrator uploads remain untouched.
    // Объявляем метод RefreshAsync: параметры приходят из запроса или вызова сервиса.
    public static async Task RefreshAsync(CinemaDbContext db, string root, DateTime today)
    {
        // Записываем вычисленное значение в const string version.
        const string version = "pavlyuk-original-2026-11-10";
        // Получаем setting из базы или подготавливаем запрос к ней.
        var setting = await db.SiteSettings.FindAsync("CoverCatalogVersion");
        // Выполняем следующую ветку только при истинности условия.
        if (setting?.Value == version) return;
        // Вычисляем catalogue для дальнейших проверок или отображения.
        var catalogue = ReferenceContent.Movies(root, today).ToDictionary(x => x.Title, x => x.PosterPath);
        // Получаем movies из базы или подготавливаем запрос к ней.
        var movies = await db.Movies.ToListAsync();
        // Последовательно обрабатываем каждый элемент коллекции.
        foreach (var movie in movies)
            // Выполняем следующую ветку только при истинности условия.
            if (catalogue.TryGetValue(movie.Title, out var poster)) movie.PosterPath = poster;
        // Получаем banners из базы или подготавливаем запрос к ней.
        var banners = await db.SiteSettings.Where(x => x.Key.StartsWith("Banner")).ToListAsync();
        // Повторяем следующий блок для заданного диапазона значений.
        for (var i = 1; i < 7; i++)
        {
            // Вычисляем title для дальнейших проверок или отображения.
            var title = banners.FirstOrDefault(x => x.Key == $"Banner{i}Title")?.Value;
            // Вычисляем image для дальнейших проверок или отображения.
            var image = banners.FirstOrDefault(x => x.Key == $"Banner{i}Image");
            // Обрабатываем отсутствие записи или необязательного значения.
            if (title != null && image != null && catalogue.TryGetValue(title, out var cover))
                // Записываем вычисленное значение в value.
                image.Value = cover.Replace("/movie-", "/banner-");
        }
        // Обрабатываем отсутствие записи или необязательного значения.
        if (setting == null) db.SiteSettings.Add(new SiteSetting { Key = "CoverCatalogVersion", Value = version });
        // Обрабатываем альтернативный случай предыдущей проверки.
        else setting.Value = version;
        // Сохраняем накопленные изменения в SQLite.
        await db.SaveChangesAsync();
    }
}
