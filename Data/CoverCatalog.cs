using Cinema.Models;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Data;

public static class CoverCatalog
{
    // Replace catalogue posters once when an existing local database is opened.
    // Orders, users, prices and later administrator uploads remain untouched.
    public static async Task RefreshAsync(CinemaDbContext db, string root, DateTime today)
    {
        const string version = "pavlyuk-original-2026-11-10";
        var setting = await db.SiteSettings.FindAsync("CoverCatalogVersion");
        if (setting?.Value == version) return;
        var catalogue = ReferenceContent.Movies(root, today).ToDictionary(x => x.Title, x => x.PosterPath);
        var movies = await db.Movies.ToListAsync();
        foreach (var movie in movies)
            if (catalogue.TryGetValue(movie.Title, out var poster)) movie.PosterPath = poster;
        var banners = await db.SiteSettings.Where(x => x.Key.StartsWith("Banner")).ToListAsync();
        for (var i = 1; i < 7; i++)
        {
            var title = banners.FirstOrDefault(x => x.Key == $"Banner{i}Title")?.Value;
            var image = banners.FirstOrDefault(x => x.Key == $"Banner{i}Image");
            if (title != null && image != null && catalogue.TryGetValue(title, out var cover))
                image.Value = cover.Replace("/movie-", "/banner-");
        }
        if (setting == null) db.SiteSettings.Add(new SiteSetting { Key = "CoverCatalogVersion", Value = version });
        else setting.Value = version;
        await db.SaveChangesAsync();
    }
}
