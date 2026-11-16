// Готовит исходные тексты, настройки и баннеры сайта.
// Пояснения ниже относятся к следующей строке или смысловому шагу.
// Подключаем пространство имён System.Text.Json для типов ниже.
using System.Text.Json;
// Подключаем пространство имён Cinema.Models для типов ниже.
using Cinema.Models;
// Подключаем пространство имён Microsoft.EntityFrameworkCore для типов ниже.
using Microsoft.EntityFrameworkCore;

// Помещаем объявленные типы в указанное пространство имён.
namespace Cinema.Data;

// Imported visual catalogue is separate from application logic. The DB remains
// the source of truth after seeding; administrators edit films normally.
// Готовит исходные тексты, настройки и баннеры сайта.
public static class ReferenceContent
{
    // Готовит исходные тексты, настройки и баннеры сайта.
    public class FilmData
    {
        // Записываем вычисленное значение в public string title { get; set; }.
        public string Title { get; set; } = "";
        // Записываем вычисленное значение в public string genre { get; set; }.
        public string Genre { get; set; } = "";
        // Записываем вычисленное значение в public string country { get; set; }.
        public string Country { get; set; } = "";
        // Свойство хранит значение модели и доступно другим компонентам.
        public int DurationMinutes { get; set; }
        // Свойство хранит значение модели и доступно другим компонентам.
        public int AgeRating { get; set; }
        // Записываем вычисленное значение в public string poster path { get; set; }.
        public string PosterPath { get; set; } = "";
        // Свойство хранит значение модели и доступно другим компонентам.
        public string? TrailerUrl { get; set; }
        // Свойство хранит значение модели и доступно другим компонентам.
        public int DayOffset { get; set; }
    }

    // Объявляем метод Movies: параметры приходят из запроса или вызова сервиса.
    public static List<Movie> Movies(string root, DateTime today)
    {
        // Вычисляем json для дальнейших проверок или отображения.
        var json = File.ReadAllText(Path.Combine(root, "Data", "reference-movies.json"));
        // Вычисляем films для дальнейших проверок или отображения.
        var films = JsonSerializer.Deserialize<List<FilmData>>(json) ?? [];
        // Возвращаем результат вызывающему коду.
        return films.Select((x, i) => new Movie
        {
            // Записываем вычисленное значение в title.
            Title = x.Title, Genre = x.Genre, Country = x.Country,
            // Записываем вычисленное значение в duration minutes.
            DurationMinutes = x.DurationMinutes, AgeRating = x.AgeRating,
            // Записываем вычисленное значение в poster path.
            PosterPath = x.PosterPath, TrailerUrl = x.TrailerUrl,
            // Записываем вычисленное значение в release date.
            ReleaseDate = today.AddDays(x.DayOffset), IsFeatured = i < 8,
            // Записываем вычисленное значение в description.
            Description = $"«{x.Title}» на большом экране. {x.Genre}. Выберите удобную дату, сеанс и места в зале. "
        // Завершаем действие или объявление текущей строки.
        }).ToList();
    }

    // Объявляем метод SeedBannersAsync: параметры приходят из запроса или вызова сервиса.
    public static async Task SeedBannersAsync(CinemaDbContext db)
    {
        // Вычисляем images для дальнейших проверок или отображения.
        var images = new[] { "../pavlyuk-banner.svg", "../covers/banner-03.svg", "../covers/banner-04.svg", "../covers/banner-05.svg", "../covers/banner-06.svg", "../covers/banner-09.svg", "../covers/banner-10.svg" };
        // Вычисляем titles для дальнейших проверок или отображения.
        var titles = new[] { "Кинотеатр им. Павлюка", "Отверженные", "Навар", "Живая книга", "Рождение империи", "Чудо-Юдо", "Сказка о потерянном времени" };
        // Получаем keys из базы или подготавливаем запрос к ней.
        var keys = await db.SiteSettings.Select(x => x.Key).ToListAsync();
        // Вычисляем news для дальнейших проверок или отображения.
        var news = new[]
        {
            // Передаём очередной аргумент или задаём элемент коллекции.
            new[] { "Кино начинается здесь", "Добро пожаловать в кинотеатр им. Павлюка! Выбирайте фильм и места онлайн.", "05 ноября 2026 г.", "../pavlyuk-news.svg" },
            // Передаём очередной аргумент или задаём элемент коллекции.
            new[] { "Требуется на работу", "Ищем кассира.", "06 апреля 2026 г.", "image_19_2063.png" },
            new[] { "Требуется на работу", "Ищем уборщицу помещений.", "08 октября 2025 г.", "image_17_8691.jpg" }
        };
        // Повторяем следующий блок для заданного диапазона значений.
        for (var i = 0; i < news.Length; i++)
        {
            // Вычисляем values для дальнейших проверок или отображения.
            var values = new Dictionary<string, string>
            {
                // Атрибут задаёт правило проверки или отображения следующего элемента.
                [$"News{i}Title"] = news[i][0], [$"News{i}Text"] = news[i][1],
                // Атрибут задаёт правило проверки или отображения следующего элемента.
                [$"News{i}Date"] = news[i][2], [$"News{i}Image"] = news[i][3].StartsWith("../") ? "/images/" + news[i][3][3..] : "/images/reference/" + news[i][3]
            };
            // Последовательно обрабатываем каждый элемент коллекции.
            foreach (var item in values)
                // Выполняем следующую ветку только при истинности условия.
                if (!keys.Contains(item.Key)) db.SiteSettings.Add(new SiteSetting { Key = item.Key, Value = item.Value });
        }
        // Повторяем следующий блок для заданного диапазона значений.
        for (var i = 0; i < images.Length; i++)
        {
            // Вычисляем title для дальнейших проверок или отображения.
            var title = titles[i];
            // Получаем movie id из базы или подготавливаем запрос к ней.
            var movieId = await db.Movies.Where(x => x.Title == title).Select(x => (int?)x.Id).FirstOrDefaultAsync();
            // Вычисляем target для дальнейших проверок или отображения.
            var target = i == 0 ? "/Movies" : movieId.HasValue ? $"/Movies/Details/{movieId.Value}" : "/Movies";
            // Вычисляем values для дальнейших проверок или отображения.
            var values = new Dictionary<string, string> { [$"Banner{i}Image"] = images[i].StartsWith("../") ? "/images/" + images[i][3..] : "/images/reference/" + images[i], [$"Banner{i}Title"] = title, [$"Banner{i}Target"] = target };
            // Последовательно обрабатываем каждый элемент коллекции.
            foreach (var item in values)
                // Выполняем следующую ветку только при истинности условия.
                if (!keys.Contains(item.Key)) db.SiteSettings.Add(new SiteSetting { Key = item.Key, Value = item.Value });
        }
        // Сохраняем накопленные изменения в SQLite.
        await db.SaveChangesAsync();
    }
}
