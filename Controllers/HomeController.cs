// Формирует главную и информационные страницы.
// Пояснения ниже относятся к следующей строке или смысловому шагу.
// Подключаем пространство имён Cinema.Data для типов ниже.
using Cinema.Data;
// Подключаем пространство имён Cinema.Models для типов ниже.
using Cinema.Models;
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
// Формирует главную и информационные страницы.
public class HomeController(CinemaDbContext db, CinemaClock clock) : Controller
{
    // Готовим основную страницу этого раздела и возвращаем представление.
    public async Task<IActionResult> Index(DateTime? date)
    {
        // Вычисляем today для дальнейших проверок или отображения.
        var today = clock.Now.Date;
        // Вычисляем selected для дальнейших проверок или отображения.
        var selected = (date ?? today).Date;
        // Выполняем следующую ветку только при истинности условия.
        if (selected < today || selected > today.AddDays(30)) selected = today;
        // Обрабатываем отсутствие записи или необязательного значения.
        if (date == null)
        {
            // Получаем next из базы или подготавливаем запрос к ней.
            var next = await db.Sessions.AsNoTracking().Where(x => !x.IsCancelled && x.StartsAt > clock.Now)
                // Упорядочиваем записи перед выводом.
                .OrderBy(x => x.StartsAt).Select(x => (DateTime?)x.StartsAt).FirstOrDefaultAsync();
            // Выполняем следующую ветку только при истинности условия.
            if (next.HasValue) selected = next.Value.Date;
        }
        // Получаем settings из базы или подготавливаем запрос к ней.
        var settings = await db.SiteSettings.AsNoTracking().ToDictionaryAsync(x => x.Key, x => x.Value);
        // Вычисляем model для дальнейших проверок или отображения.
        var model = new HomeModel
        {
            // Записываем вычисленное значение в schedule date.
            ScheduleDate = selected,
            // Загружаем связанную сущность вместе с основной записью.
            Sessions = await db.Sessions.AsNoTracking().Include(x => x.Movie).Include(x => x.Hall)
                // Оставляем в выборке только записи, подходящие под условие.
                .Where(x => !x.IsCancelled && x.StartsAt > clock.Now && x.StartsAt >= selected && x.StartsAt < selected.AddDays(1))
                // Упорядочиваем записи перед выводом.
                .OrderBy(x => x.StartsAt).ToListAsync(),
            // Оставляем в выборке только записи, подходящие под условие.
            UpcomingMovies = await db.Movies.AsNoTracking().Where(x => x.ReleaseDate > today && x.ReleaseDate <= today.AddDays(31))
                // Упорядочиваем записи перед выводом.
                .OrderBy(x => x.ReleaseDate).ThenBy(x => x.Id).Take(14).ToListAsync(),
            // Выполняем запрос к SQLite асинхронно.
            Halls = await db.Halls.AsNoTracking().ToListAsync(),
            // Оставляем в выборке только записи, подходящие под условие.
            Reviews = await db.Reviews.AsNoTracking().Include(x => x.User).Where(x => x.Status == ReviewStatus.Approved)
                // Упорядочиваем записи перед выводом.
                .OrderByDescending(x => x.CreatedAtUtc).Take(3).ToListAsync(),
            // Упорядочиваем записи перед выводом.
            Faq = await db.FaqItems.AsNoTracking().OrderBy(x => x.SortOrder).ToListAsync(),
            // Записываем вычисленное значение в settings.
            Settings = settings
        };
        // Записываем вычисленное значение в movies.
        model.Movies = model.Sessions.Select(x => x.Movie).DistinctBy(x => x.Id).ToList();
        // Повторяем следующий блок для заданного диапазона значений.
        for (var i = 0; i < 7; i++)
            // Выполняем следующую ветку только при истинности условия.
            if (settings.TryGetValue($"Banner{i}Image", out var image))
                // Записываем вычисленное значение в add(new banner model { image.
                model.Banners.Add(new BannerModel { Image = image, Title = settings.GetValueOrDefault($"Banner{i}Title", "Премьера"), Target = settings.GetValueOrDefault($"Banner{i}Target", "/Movies") });
        // Передаём данные в Razor-представление и возвращаем HTML.
        return View(model);
    }
    // Объявляем метод About: параметры приходят из запроса или вызова сервиса.
    public async Task<IActionResult> About()
    {
        // Оставляем в выборке только записи, подходящие под условие.
        ViewBag.About = await db.SiteSettings.Where(x => x.Key == "About").Select(x => x.Value).FirstOrDefaultAsync();
        // Передаём данные в Razor-представление и возвращаем HTML.
        return View(await db.Halls.AsNoTracking().ToListAsync());
    }

    // Объявляем метод Services: параметры приходят из запроса или вызова сервиса.
    public async Task<IActionResult> Services() => View(await db.Services.AsNoTracking().ToListAsync());
    // Объявляем метод News: параметры приходят из запроса или вызова сервиса.
    public async Task<IActionResult> News() => View(await db.SiteSettings.AsNoTracking().ToDictionaryAsync(x => x.Key, x => x.Value));
    // Объявляем метод Returns: параметры приходят из запроса или вызова сервиса.
    public IActionResult Returns() => View();
    // Назначаем адрес или тип GET-запроса для этого действия.
    [HttpGet]
    // Объявляем метод Contacts: параметры приходят из запроса или вызова сервиса.
    public async Task<IActionResult> Contacts()
    {
        // Записываем вычисленное значение в settings.
        ViewBag.Settings = await db.SiteSettings.AsNoTracking().ToDictionaryAsync(x => x.Key, x => x.Value);
        // Передаём данные в Razor-представление и возвращаем HTML.
        return View(new ContactMessage());
    }

    // Разрешаем изменение данных только через POST-запрос.
    [HttpPost]
    // Объявляем метод Contacts: параметры приходят из запроса или вызова сервиса.
    public async Task<IActionResult> Contacts([Bind("Name,Email,Message")] ContactMessage input)
    {
        // Если форма содержит ошибки проверки, не сохраняем данные.
        if (ModelState.IsValid)
        {
            // Добавляем новую сущность в контекст для последующего сохранения.
            db.ContactMessages.Add(input);
            // Сохраняем накопленные изменения в SQLite.
            await db.SaveChangesAsync();
            // Записываем вычисленное значение в temp data["success"].
            TempData["Success"] = "Сообщение отправлено. Спасибо за обращение!";
            // После операции перенаправляем браузер на указанную страницу.
            return RedirectToAction(nameof(Contacts));
        }

        // Записываем вычисленное значение в settings.
        ViewBag.Settings = await db.SiteSettings.AsNoTracking().ToDictionaryAsync(x => x.Key, x => x.Value);
        // Передаём данные в Razor-представление и возвращаем HTML.
        return View(input);
    }

    // Назначаем адрес или тип GET-запроса для этого действия.
    [HttpGet("/Team")]
    // Объявляем метод Team: параметры приходят из запроса или вызова сервиса.
    public async Task<IActionResult> Team() => View(await db.TeamMembers.AsNoTracking().ToListAsync());
    // Объявляем метод Legal: параметры приходят из запроса или вызова сервиса.
    public IActionResult Legal() => View();
    // Объявляем метод Error: параметры приходят из запроса или вызова сервиса.
    public IActionResult Error()
    {
        // Записываем вычисленное значение в status code.
        Response.StatusCode = 500;
        // Передаём данные в Razor-представление и возвращаем HTML.
        return View();
    }
}
