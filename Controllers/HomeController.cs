// Читает содержимое из SQLite и передаёт его на главную и информационные страницы.
using Cinema.Data;
using Cinema.Models;
using Cinema.Services;
using Cinema.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Controllers;
public class HomeController(CinemaDbContext db, CinemaClock clock) : Controller
{
    // Параметр date приходит из URL; из SQLite выбираются фильмы, сеансы, баннеры и блоки главной страницы, затем передаются в Views/Home/Index.cshtml.
    public async Task<IActionResult> Index(DateTime? date)
    {
        var today = clock.Now.Date;
        var selected = (date ?? today).Date;
        if (selected < today || selected > today.AddDays(30)) selected = today;
        if (date == null)
        {
            // Читаем из SQLite только неотменённые сеансы; дата и время ограничивают показ расписания.
            var next = await db.Sessions.AsNoTracking().Where(x => !x.IsCancelled && x.StartsAt > clock.Now)
                .OrderBy(x => x.StartsAt).Select(x => (DateTime?)x.StartsAt).FirstOrDefaultAsync();
            if (next.HasValue) selected = next.Value.Date;
        }
        // Загружаем тексты и ссылки из SQLite; на странице они используются для баннеров и информационных блоков.
        var settings = await db.SiteSettings.AsNoTracking().ToDictionaryAsync(x => x.Key, x => x.Value);
        var model = new HomeModel
        {
            ScheduleDate = selected,
            // Читаем из SQLite только неотменённые сеансы; дата и время ограничивают показ расписания.
            Sessions = await db.Sessions.AsNoTracking().Include(x => x.Movie).Include(x => x.Hall)
                .Where(x => !x.IsCancelled && x.StartsAt > clock.Now && x.StartsAt >= selected && x.StartsAt < selected.AddDays(1))
                .OrderBy(x => x.StartsAt).ToListAsync(),
            // Выбираем из SQLite фильмы по дате премьеры и сортируем их для афиши.
            UpcomingMovies = await db.Movies.AsNoTracking().Where(x => x.ReleaseDate > today && x.ReleaseDate <= today.AddDays(31))
                .OrderBy(x => x.ReleaseDate).ThenBy(x => x.Id).Take(14).ToListAsync(),
            // Загружаем описание залов из SQLite для вывода страницы или списка выбора.
            Halls = await db.Halls.AsNoTracking().ToListAsync(),
            // Берём из SQLite только отзывы, прошедшие модерацию; черновики посетители не видят.
            Reviews = await db.Reviews.AsNoTracking().Include(x => x.User).Where(x => x.Status == ReviewStatus.Approved)
                .OrderByDescending(x => x.CreatedAtUtc).Take(3).ToListAsync(),
            // Загружаем вопросы и ответы из SQLite в заданном администратором порядке.
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
        // Читаем значение нужного ключа настроек из SQLite; администратор меняет его через раздел содержимого.
        ViewBag.About = await db.SiteSettings.Where(x => x.Key == "About").Select(x => x.Value).FirstOrDefaultAsync();
        // Загружаем описание залов из SQLite для вывода страницы или списка выбора.
        return View(await db.Halls.AsNoTracking().ToListAsync());
    }

    // Загружаем услуги из SQLite; их описание и цена задаются в админке.
    public async Task<IActionResult> Services() => View(await db.Services.AsNoTracking().ToListAsync());
    // Загружаем тексты и ссылки из SQLite; на странице они используются для баннеров и информационных блоков.
    public async Task<IActionResult> News() => View(await db.SiteSettings.AsNoTracking().ToDictionaryAsync(x => x.Key, x => x.Value));
    public IActionResult Returns() => View();
    [HttpGet]
    // GET читает адрес и контакты из SQLite; POST получает поля формы, проверяет их и сохраняет обращение.
    public async Task<IActionResult> Contacts()
    {
        // Загружаем тексты и ссылки из SQLite; на странице они используются для баннеров и информационных блоков.
        ViewBag.Settings = await db.SiteSettings.AsNoTracking().ToDictionaryAsync(x => x.Key, x => x.Value);

        return View(new ContactMessage());
    }

    [HttpPost]
    // GET читает адрес и контакты из SQLite; POST получает поля формы, проверяет их и сохраняет обращение.
    public async Task<IActionResult> Contacts([Bind("Name,Email,Message")] ContactMessage input)
    {
        // Проверяем атрибуты модели и ошибки привязки формы; при ошибках снова показываем форму.
        if (ModelState.IsValid)
        {
            db.ContactMessages.Add(input);
            // Записываем накопленные изменения сущностей в SQLite.
            await db.SaveChangesAsync();
            TempData["Success"] = "Сообщение отправлено. Спасибо за обращение!";
            return RedirectToAction(nameof(Contacts));
        }

        // Загружаем тексты и ссылки из SQLite; на странице они используются для баннеров и информационных блоков.
        ViewBag.Settings = await db.SiteSettings.AsNoTracking().ToDictionaryAsync(x => x.Key, x => x.Value);

        return View(input);
    }

    [HttpGet("/Team")]
    // Загружаем список сотрудников из SQLite для страницы команды.
    public async Task<IActionResult> Team() => View(await db.TeamMembers.AsNoTracking().ToListAsync());
    public IActionResult Legal() => View();
    public IActionResult Error()
    {
        Response.StatusCode = 500;

        return View();
    }
}
