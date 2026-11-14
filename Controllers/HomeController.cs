using Cinema.Data;
using Cinema.Models;
using Cinema.Services;
using Cinema.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Controllers;
public class HomeController(CinemaDbContext db, CinemaClock clock) : Controller
{
    public async Task<IActionResult> Index(DateTime? date)
    {
        var today = clock.Now.Date;
        var selected = (date ?? today).Date;
        if (selected < today || selected > today.AddDays(30)) selected = today;
        if (date == null)
        {
            var next = await db.Sessions.AsNoTracking().Where(x => !x.IsCancelled && x.StartsAt > clock.Now)
                .OrderBy(x => x.StartsAt).Select(x => (DateTime?)x.StartsAt).FirstOrDefaultAsync();
            if (next.HasValue) selected = next.Value.Date;
        }
        var settings = await db.SiteSettings.AsNoTracking().ToDictionaryAsync(x => x.Key, x => x.Value);
        var model = new HomeModel
        {
            ScheduleDate = selected,
            Sessions = await db.Sessions.AsNoTracking().Include(x => x.Movie).Include(x => x.Hall)
                .Where(x => !x.IsCancelled && x.StartsAt > clock.Now && x.StartsAt >= selected && x.StartsAt < selected.AddDays(1))
                .OrderBy(x => x.StartsAt).ToListAsync(),
            UpcomingMovies = await db.Movies.AsNoTracking().Where(x => x.ReleaseDate > today && x.ReleaseDate <= today.AddDays(31))
                .OrderBy(x => x.ReleaseDate).ThenBy(x => x.Id).Take(14).ToListAsync(),
            Halls = await db.Halls.AsNoTracking().ToListAsync(),
            Reviews = await db.Reviews.AsNoTracking().Include(x => x.User).Where(x => x.Status == ReviewStatus.Approved)
                .OrderByDescending(x => x.CreatedAtUtc).Take(3).ToListAsync(),
            Faq = await db.FaqItems.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(),
            Settings = settings
        };
        model.Movies = model.Sessions.Select(x => x.Movie).DistinctBy(x => x.Id).ToList();
        for (var i = 0; i < 7; i++)
            if (settings.TryGetValue($"Banner{i}Image", out var image))
                model.Banners.Add(new BannerModel { Image = image, Title = settings.GetValueOrDefault($"Banner{i}Title", "Премьера"), Target = settings.GetValueOrDefault($"Banner{i}Target", "/Movies") });
        return View(model);
    }
    public async Task<IActionResult> About()
    {
        ViewBag.About = await db.SiteSettings.Where(x => x.Key == "About").Select(x => x.Value).FirstOrDefaultAsync();
        return View(await db.Halls.AsNoTracking().ToListAsync());
    }

    public async Task<IActionResult> Services() => View(await db.Services.AsNoTracking().ToListAsync());
    public async Task<IActionResult> News() => View(await db.SiteSettings.AsNoTracking().ToDictionaryAsync(x => x.Key, x => x.Value));
    public IActionResult Returns() => View();
    [HttpGet]
    public async Task<IActionResult> Contacts()
    {
        ViewBag.Settings = await db.SiteSettings.AsNoTracking().ToDictionaryAsync(x => x.Key, x => x.Value);
        return View(new ContactMessage());
    }

    [HttpPost]
    public async Task<IActionResult> Contacts([Bind("Name,Email,Message")] ContactMessage input)
    {
        if (ModelState.IsValid)
        {
            db.ContactMessages.Add(input);
            await db.SaveChangesAsync();
            TempData["Success"] = "Сообщение отправлено. Спасибо за обращение!";
            return RedirectToAction(nameof(Contacts));
        }

        ViewBag.Settings = await db.SiteSettings.AsNoTracking().ToDictionaryAsync(x => x.Key, x => x.Value);
        return View(input);
    }

    [HttpGet("/Team")]
    public async Task<IActionResult> Team() => View(await db.TeamMembers.AsNoTracking().ToListAsync());
    public IActionResult Legal() => View();
    public IActionResult Error()
    {
        Response.StatusCode = 500;
        return View();
    }
}
