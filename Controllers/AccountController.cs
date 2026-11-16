// Обрабатывает вход, регистрацию, профиль и личные заказы.
// Пояснения ниже относятся к следующей строке или смысловому шагу.
// Подключаем пространство имён Cinema.Data для типов ниже.
using Cinema.Data;
// Подключаем пространство имён Cinema.Models для типов ниже.
using Cinema.Models;
// Подключаем пространство имён Cinema.Services для типов ниже.
using Cinema.Services;
// Подключаем пространство имён Cinema.ViewModels для типов ниже.
using Cinema.ViewModels;
// Подключаем пространство имён Microsoft.AspNetCore.Authorization для типов ниже.
using Microsoft.AspNetCore.Authorization;
// Подключаем пространство имён Microsoft.AspNetCore.Identity для типов ниже.
using Microsoft.AspNetCore.Identity;
// Подключаем пространство имён Microsoft.AspNetCore.Mvc для типов ниже.
using Microsoft.AspNetCore.Mvc;
// Подключаем пространство имён Microsoft.EntityFrameworkCore для типов ниже.
using Microsoft.EntityFrameworkCore;

// Помещаем объявленные типы в указанное пространство имён.
namespace Cinema.Controllers;
// Ограничиваем доступ: роль проверяется сервером до вызова действия.
[Authorize]
// Обрабатывает вход, регистрацию, профиль и личные заказы.
public class AccountController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn, CinemaDbContext db, BookingService bookings) : Controller
{
    // Назначаем адрес или тип GET-запроса для этого действия.
    [AllowAnonymous, HttpGet]
    // Объявляем метод Login: параметры приходят из запроса или вызова сервиса.
    public IActionResult Login(string? returnUrl) => View(new LoginModel { ReturnUrl = returnUrl });
    // Разрешаем изменение данных только через POST-запрос.
    [AllowAnonymous, HttpPost]
    // Объявляем метод Login: параметры приходят из запроса или вызова сервиса.
    public async Task<IActionResult> Login(LoginModel model)
    {
        // Если форма содержит ошибки проверки, не сохраняем данные.
        if (!ModelState.IsValid)
            // Передаём данные в Razor-представление и возвращаем HTML.
            return View(model);
        // Асинхронно получаем user, не блокируя поток сервера.
        var user = await users.FindByEmailAsync(model.Email);
        // Асинхронно получаем result, не блокируя поток сервера.
        var result = user == null ? Microsoft.AspNetCore.Identity.SignInResult.Failed : await signIn.PasswordSignInAsync(user, model.Password, model.RememberMe, true);
        // Выполняем следующую ветку только при истинности условия.
        if (result.Succeeded)
            // Возвращаем результат вызывающему коду.
            return Url.IsLocalUrl(model.ReturnUrl) ? LocalRedirect(model.ReturnUrl!) : RedirectToAction(nameof(Orders));
        // Завершаем действие или объявление текущей строки.
        ModelState.AddModelError("", result.IsLockedOut ? "Слишком много попыток. Попробуйте через 10 минут." : "Неверный email или пароль.");
        // Передаём данные в Razor-представление и возвращаем HTML.
        return View(model);
    }

    // Назначаем адрес или тип GET-запроса для этого действия.
    [AllowAnonymous, HttpGet]
    // Объявляем метод Register: параметры приходят из запроса или вызова сервиса.
    public IActionResult Register() => View(new RegisterModel());
    // Разрешаем изменение данных только через POST-запрос.
    [AllowAnonymous, HttpPost]
    // Объявляем метод Register: параметры приходят из запроса или вызова сервиса.
    public async Task<IActionResult> Register(RegisterModel model)
    {
        // Если форма содержит ошибки проверки, не сохраняем данные.
        if (!ModelState.IsValid)
            // Передаём данные в Razor-представление и возвращаем HTML.
            return View(model);
        // Вычисляем user для дальнейших проверок или отображения.
        var user = new ApplicationUser
        {
            // Записываем вычисленное значение в display name.
            DisplayName = model.DisplayName,
            // Записываем вычисленное значение в user name.
            UserName = model.Email,
            // Записываем вычисленное значение в email.
            Email = model.Email
        };
        // Асинхронно получаем result, не блокируя поток сервера.
        var result = await users.CreateAsync(user, model.Password);
        // Выполняем следующую ветку только при истинности условия.
        if (result.Succeeded)
        {
            // Дожидаемся завершения асинхронной операции.
            await users.AddToRoleAsync(user, "User");
            // Дожидаемся завершения асинхронной операции.
            await signIn.SignInAsync(user, false);
            // После операции перенаправляем браузер на указанную страницу.
            return RedirectToAction(nameof(Orders));
        }

        // Завершаем действие или объявление текущей строки.
        AddErrors(result);
        // Передаём данные в Razor-представление и возвращаем HTML.
        return View(model);
    }

    // Разрешаем изменение данных только через POST-запрос.
    [HttpPost]
    // Объявляем метод Logout: параметры приходят из запроса или вызова сервиса.
    public async Task<IActionResult> Logout()
    {
        // Дожидаемся завершения асинхронной операции.
        await signIn.SignOutAsync();
        // После операции перенаправляем браузер на указанную страницу.
        return RedirectToAction("Index", "Home");
    }

    // Готовим основную страницу этого раздела и возвращаем представление.
    public IActionResult Index() => RedirectToAction(nameof(Orders));
    // Объявляем метод Orders: параметры приходят из запроса или вызова сервиса.
    public async Task<IActionResult> Orders(string? filter, int page = 1)
    {
        // Вычисляем user id для дальнейших проверок или отображения.
        var userId = users.GetUserId(User)!;
        // Получаем expired sessions из базы или подготавливаем запрос к ней.
        var expiredSessions = await db.Bookings.AsNoTracking().Where(x => x.UserId == userId && x.Status == BookingStatus.Pending && x.ExpiresAtUtc <= DateTime.UtcNow).Select(x => x.SessionId).Distinct().ToListAsync();
        // Последовательно обрабатываем каждый элемент коллекции.
        foreach (var sessionId in expiredSessions)
            // Дожидаемся завершения асинхронной операции.
            await bookings.CleanupSessionAsync(sessionId);
        // Получаем query из базы или подготавливаем запрос к ней.
        var query = db.Bookings.AsNoTracking().Where(x => x.UserId == userId);
        // Записываем вычисленное значение в query.
        query = filter switch
        {
            // Оставляем в выборке только записи, подходящие под условие.
            "active" => query.Where(x => x.Status == BookingStatus.Pending),
            // Оставляем в выборке только записи, подходящие под условие.
            "tickets" => query.Where(x => x.Status == BookingStatus.Paid),
            // Оставляем в выборке только записи, подходящие под условие.
            "cancelled" => query.Where(x => x.Status == BookingStatus.Cancelled || x.Status == BookingStatus.Expired || x.Status == BookingStatus.Refunded),
            _ => query
        };
        // Записываем вычисленное значение в page.
        page = Math.Max(1, page);
        // Записываем вычисленное значение в page.
        ViewBag.Page = page;
        // Записываем вычисленное значение в filter.
        ViewBag.Filter = filter;
        // Записываем вычисленное значение в has next.
        ViewBag.HasNext = await query.CountAsync() > page * 12;
        // Передаём данные в Razor-представление и возвращаем HTML.
        return View(await query.Include(x => x.Session).ThenInclude(x => x.Movie).Include(x => x.Session).ThenInclude(x => x.Hall).Include(x => x.BookingSeats).ThenInclude(x => x.Seat).OrderByDescending(x => x.CreatedAtUtc).Skip((page - 1) * 12).Take(12).ToListAsync());
    }

    // Назначаем адрес или тип GET-запроса для этого действия.
    [HttpGet]
    // Объявляем метод Profile: параметры приходят из запроса или вызова сервиса.
    public async Task<IActionResult> Profile()
    {
        // Асинхронно получаем user, не блокируя поток сервера.
        var user = (await users.GetUserAsync(User))!;
        // Передаём данные в Razor-представление и возвращаем HTML.
        return View(new ProfileModel { DisplayName = user.DisplayName, Email = user.Email! });
    }

    // Разрешаем изменение данных только через POST-запрос.
    [HttpPost]
    // Объявляем метод Profile: параметры приходят из запроса или вызова сервиса.
    public async Task<IActionResult> Profile(ProfileModel model)
    {
        // Если форма содержит ошибки проверки, не сохраняем данные.
        if (!ModelState.IsValid)
            // Передаём данные в Razor-представление и возвращаем HTML.
            return View(model);
        // Асинхронно получаем user, не блокируя поток сервера.
        var user = (await users.GetUserAsync(User))!;
        // One Identity update: duplicate emails/user names cannot leave partial changes.
        // Записываем вычисленное значение в display name.
        user.DisplayName = model.DisplayName;
        // Записываем вычисленное значение в email.
        user.Email = model.Email;
        // Записываем вычисленное значение в user name.
        user.UserName = model.Email;
        // Асинхронно получаем result, не блокируя поток сервера.
        var result = await users.UpdateAsync(user);
        // Выполняем следующую ветку только при истинности условия.
        if (!result.Succeeded)
        {
            // Завершаем действие или объявление текущей строки.
            AddErrors(result);
            // Передаём данные в Razor-представление и возвращаем HTML.
            return View(model);
        }

        // Дожидаемся завершения асинхронной операции.
        await signIn.RefreshSignInAsync(user);
        // Записываем вычисленное значение в temp data["success"].
        TempData["Success"] = "Профиль обновлён.";
        // После операции перенаправляем браузер на указанную страницу.
        return RedirectToAction(nameof(Profile));
    }

    // Назначаем адрес или тип GET-запроса для этого действия.
    [HttpGet]
    // Объявляем метод Password: параметры приходят из запроса или вызова сервиса.
    public IActionResult Password() => View(new PasswordModel());
    // Разрешаем изменение данных только через POST-запрос.
    [HttpPost]
    // Объявляем метод Password: параметры приходят из запроса или вызова сервиса.
    public async Task<IActionResult> Password(PasswordModel model)
    {
        // Если форма содержит ошибки проверки, не сохраняем данные.
        if (!ModelState.IsValid)
            // Передаём данные в Razor-представление и возвращаем HTML.
            return View(model);
        // Асинхронно получаем user, не блокируя поток сервера.
        var user = (await users.GetUserAsync(User))!;
        // Асинхронно получаем result, не блокируя поток сервера.
        var result = await users.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        // Выполняем следующую ветку только при истинности условия.
        if (!result.Succeeded)
        {
            // Завершаем действие или объявление текущей строки.
            AddErrors(result);
            // Передаём данные в Razor-представление и возвращаем HTML.
            return View(model);
        }

        // Дожидаемся завершения асинхронной операции.
        await signIn.RefreshSignInAsync(user);
        // Записываем вычисленное значение в temp data["success"].
        TempData["Success"] = "Пароль изменён.";
        // После операции перенаправляем браузер на указанную страницу.
        return RedirectToAction(nameof(Profile));
    }

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [AllowAnonymous]
    // Объявляем метод Denied: параметры приходят из запроса или вызова сервиса.
    public IActionResult Denied()
    {
        // Записываем вычисленное значение в status code.
        Response.StatusCode = 403;
        // Передаём данные в Razor-представление и возвращаем HTML.
        return View();
    }

    // Объявляем метод AddErrors: параметры приходят из запроса или вызова сервиса.
    private void AddErrors(IdentityResult result)
    {
        // Последовательно обрабатываем каждый элемент коллекции.
        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Code switch
            {
                // Передаём очередной аргумент или задаём элемент коллекции.
                "DuplicateEmail" or "DuplicateUserName" => "Этот email уже используется.",
                // Передаём очередной аргумент или задаём элемент коллекции.
                "PasswordTooShort" => "Пароль должен содержать не менее 8 символов.",
                // Передаём очередной аргумент или задаём элемент коллекции.
                "PasswordRequiresDigit" => "Добавьте в пароль цифру.",
                // Передаём очередной аргумент или задаём элемент коллекции.
                "PasswordRequiresUpper" => "Добавьте заглавную латинскую букву.",
                // Передаём очередной аргумент или задаём элемент коллекции.
                "PasswordRequiresLower" => "Добавьте строчную латинскую букву.",
                // Передаём очередной аргумент или задаём элемент коллекции.
                "PasswordMismatch" => "Текущий пароль введён неверно.",
                _ => "Проверьте email и пароль. Пароль: минимум 8 символов, латинские буквы обоих регистров и цифра."
            });
    }
}
