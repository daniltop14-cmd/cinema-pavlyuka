using System.Text.Json;
using Cinema.Models;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Data;

// Imported visual catalogue is separate from application logic. The DB remains
// the source of truth after seeding; administrators edit films normally.
public static class ReferenceContent
{
    public class FilmData
    {
        public string Title { get; set; } = "";
        public string Genre { get; set; } = "";
        public string Country { get; set; } = "";
        public int DurationMinutes { get; set; }
        public int AgeRating { get; set; }
        public string PosterPath { get; set; } = "";
        public string? TrailerUrl { get; set; }
        public int DayOffset { get; set; }
    }

    public static List<Movie> Movies(string root, DateTime today)
    {
        var json = File.ReadAllText(Path.Combine(root, "Data", "reference-movies.json"));
        var films = JsonSerializer.Deserialize<List<FilmData>>(json) ?? [];
        return films.Select((x, i) => new Movie
        {
            Title = x.Title, Genre = x.Genre, Country = x.Country,
            DurationMinutes = x.DurationMinutes, AgeRating = x.AgeRating,
            PosterPath = x.PosterPath, TrailerUrl = x.TrailerUrl,
            ReleaseDate = today.AddDays(x.DayOffset), IsFeatured = i < 8,
            Description = $"«{x.Title}» на большом экране. {x.Genre}. Выберите удобную дату, сеанс и места в зале. "
        }).ToList();
    }

    public static async Task SeedBannersAsync(CinemaDbContext db)
    {
        var images = new[] { "../pavlyuk-banner.svg", "../covers/banner-03.svg", "../covers/banner-04.svg", "../covers/banner-05.svg", "../covers/banner-06.svg", "../covers/banner-09.svg", "../covers/banner-10.svg" };
        var titles = new[] { "Кинотеатр им. Павлюка", "Отверженные", "Навар", "Живая книга", "Рождение империи", "Чудо-Юдо", "Сказка о потерянном времени" };
        var keys = await db.SiteSettings.Select(x => x.Key).ToListAsync();
        var news = new[]
        {
            new[] { "Кино начинается здесь", "Добро пожаловать в кинотеатр им. Павлюка! Выбирайте фильм и места онлайн.", "05 ноября 2026 г.", "../pavlyuk-news.svg" },
            new[] { "Требуется на работу", "Ищем кассира.", "06 апреля 2026 г.", "image_19_2063.png" },
            new[] { "Требуется на работу", "Ищем уборщицу помещений.", "08 октября 2025 г.", "image_17_8691.jpg" }
        };
        for (var i = 0; i < news.Length; i++)
        {
            var values = new Dictionary<string, string>
            {
                [$"News{i}Title"] = news[i][0], [$"News{i}Text"] = news[i][1],
                [$"News{i}Date"] = news[i][2], [$"News{i}Image"] = news[i][3].StartsWith("../") ? "/images/" + news[i][3][3..] : "/images/reference/" + news[i][3]
            };
            foreach (var item in values)
                if (!keys.Contains(item.Key)) db.SiteSettings.Add(new SiteSetting { Key = item.Key, Value = item.Value });
        }
        for (var i = 0; i < images.Length; i++)
        {
            var title = titles[i];
            var movieId = await db.Movies.Where(x => x.Title == title).Select(x => (int?)x.Id).FirstOrDefaultAsync();
            var target = i == 0 ? "/Movies" : movieId.HasValue ? $"/Movies/Details/{movieId.Value}" : "/Movies";
            var values = new Dictionary<string, string> { [$"Banner{i}Image"] = images[i].StartsWith("../") ? "/images/" + images[i][3..] : "/images/reference/" + images[i], [$"Banner{i}Title"] = title, [$"Banner{i}Target"] = target };
            foreach (var item in values)
                if (!keys.Contains(item.Key)) db.SiteSettings.Add(new SiteSetting { Key = item.Key, Value = item.Value });
        }
        await db.SaveChangesAsync();
    }
}
