// Использует ASP.NET Identity для входа и профиля, а SQLite — для истории заказов.
using Cinema.Data;
using Cinema.Models;
using Cinema.Services;
using Cinema.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Controllers;
[Authorize]
public class AccountController(UserManager<ApplicationUser> users, SignInManager<ApplicationUser> signIn, CinemaDbContext db, BookingService bookings) : Controller
{
    [AllowAnonymous, HttpGet]
    // GET показывает форму; POST проверяет введённые учётные данные через ASP.NET Identity и выдаёт cookie входа.
    public IActionResult Login(string? returnUrl) => View(new LoginModel { ReturnUrl = returnUrl });
    [AllowAnonymous, HttpPost]
    // GET показывает форму; POST проверяет введённые учётные данные через ASP.NET Identity и выдаёт cookie входа.
    public async Task<IActionResult> Login(LoginModel model)
    {
        // Проверяем атрибуты модели и ошибки привязки формы; при ошибках снова показываем форму.
        if (!ModelState.IsValid)

            return View(model);
        var user = await users.FindByEmailAsync(model.Email);
        var result = user == null ? Microsoft.AspNetCore.Identity.SignInResult.Failed : await signIn.PasswordSignInAsync(user, model.Password, model.RememberMe, true);
        if (result.Succeeded)
            return Url.IsLocalUrl(model.ReturnUrl) ? LocalRedirect(model.ReturnUrl!) : RedirectToAction(nameof(Orders));
        ModelState.AddModelError("", result.IsLockedOut ? "Слишком много попыток. Попробуйте через 10 минут." : "Неверный email или пароль.");

        return View(model);
    }

    [AllowAnonymous, HttpGet]
    // GET показывает форму; POST проверяет поля и создаёт пользователя через ASP.NET Identity.
    public IActionResult Register() => View(new RegisterModel());
    [AllowAnonymous, HttpPost]
    // GET показывает форму; POST проверяет поля и создаёт пользователя через ASP.NET Identity.
    public async Task<IActionResult> Register(RegisterModel model)
    {
        // Проверяем атрибуты модели и ошибки привязки формы; при ошибках снова показываем форму.
        if (!ModelState.IsValid)

            return View(model);
        var user = new ApplicationUser
        {
            DisplayName = model.DisplayName,
            UserName = model.Email,
            Email = model.Email
        };
        var result = await users.CreateAsync(user, model.Password);
        if (result.Succeeded)
        {
            await users.AddToRoleAsync(user, "User");
            await signIn.SignInAsync(user, false);
            return RedirectToAction(nameof(Orders));
        }

        AddErrors(result);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await signIn.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Index() => RedirectToAction(nameof(Orders));
    // id пользователя берётся из cookie входа; из SQLite выбираются только его заказы для личного кабинета.
    public async Task<IActionResult> Orders(string? filter, int page = 1)
    {
        var userId = users.GetUserId(User)!;
        // Выбираем из SQLite заказы текущего пользователя, чей id получен из cookie входа.
        var expiredSessions = await db.Bookings.AsNoTracking().Where(x => x.UserId == userId && x.Status == BookingStatus.Pending && x.ExpiresAtUtc <= DateTime.UtcNow).Select(x => x.SessionId).Distinct().ToListAsync();
        foreach (var sessionId in expiredSessions)
            await bookings.CleanupSessionAsync(sessionId);
        // Выбираем из SQLite заказы текущего пользователя, чей id получен из cookie входа.
        var query = db.Bookings.AsNoTracking().Where(x => x.UserId == userId);
        query = filter switch
        {
            "active" => query.Where(x => x.Status == BookingStatus.Pending),
            "tickets" => query.Where(x => x.Status == BookingStatus.Paid),
            "cancelled" => query.Where(x => x.Status == BookingStatus.Cancelled || x.Status == BookingStatus.Expired || x.Status == BookingStatus.Refunded),
            _ => query
        };
        page = Math.Max(1, page);
        ViewBag.Page = page;
        ViewBag.Filter = filter;
        ViewBag.HasNext = await query.CountAsync() > page * 12;

        return View(await query.Include(x => x.Session).ThenInclude(x => x.Movie).Include(x => x.Session).ThenInclude(x => x.Hall).Include(x => x.BookingSeats).ThenInclude(x => x.Seat).OrderByDescending(x => x.CreatedAtUtc).Skip((page - 1) * 12).Take(12).ToListAsync());
    }

    [HttpGet]
    // GET читает профиль текущего пользователя; POST проверяет и сохраняет изменённые поля.
    public async Task<IActionResult> Profile()
    {
        var user = (await users.GetUserAsync(User))!;

        return View(new ProfileModel { DisplayName = user.DisplayName, Email = user.Email! });
    }

    [HttpPost]
    // GET читает профиль текущего пользователя; POST проверяет и сохраняет изменённые поля.
    public async Task<IActionResult> Profile(ProfileModel model)
    {
        // Проверяем атрибуты модели и ошибки привязки формы; при ошибках снова показываем форму.
        if (!ModelState.IsValid)

            return View(model);
        var user = (await users.GetUserAsync(User))!;
        // One Identity update: duplicate emails/user names cannot leave partial changes.
        user.DisplayName = model.DisplayName;
        user.Email = model.Email;
        user.UserName = model.Email;
        var result = await users.UpdateAsync(user);
        if (!result.Succeeded)
        {
            AddErrors(result);

            return View(model);
        }

        await signIn.RefreshSignInAsync(user);
        TempData["Success"] = "Профиль обновлён.";
        return RedirectToAction(nameof(Profile));
    }

    [HttpGet]
    // GET показывает форму; POST сверяет старый пароль и сохраняет новый через Identity.
    public IActionResult Password() => View(new PasswordModel());
    [HttpPost]
    // GET показывает форму; POST сверяет старый пароль и сохраняет новый через Identity.
    public async Task<IActionResult> Password(PasswordModel model)
    {
        // Проверяем атрибуты модели и ошибки привязки формы; при ошибках снова показываем форму.
        if (!ModelState.IsValid)

            return View(model);
        var user = (await users.GetUserAsync(User))!;
        var result = await users.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
        if (!result.Succeeded)
        {
            AddErrors(result);

            return View(model);
        }

        await signIn.RefreshSignInAsync(user);
        TempData["Success"] = "Пароль изменён.";
        return RedirectToAction(nameof(Profile));
    }

    [AllowAnonymous]
    public IActionResult Denied()
    {
        Response.StatusCode = 403;

        return View();
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
            ModelState.AddModelError("", error.Code switch
            {
                "DuplicateEmail" or "DuplicateUserName" => "Этот email уже используется.",
                "PasswordTooShort" => "Пароль должен содержать не менее 8 символов.",
                "PasswordRequiresDigit" => "Добавьте в пароль цифру.",
                "PasswordRequiresUpper" => "Добавьте заглавную латинскую букву.",
                "PasswordRequiresLower" => "Добавьте строчную латинскую букву.",
                "PasswordMismatch" => "Текущий пароль введён неверно.",
                _ => "Проверьте email и пароль. Пароль: минимум 8 символов, латинские буквы обоих регистров и цифра."
            });
    }
}
