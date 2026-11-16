// Точка входа: конфигурация, сервисы, HTTP-конвейер, миграции и запуск сервера.
// Пояснения ниже относятся к следующей строке или смысловому шагу.
// Подключаем пространство имён Cinema.Data для типов ниже.
using Cinema.Data;
// Подключаем пространство имён Cinema.Models для типов ниже.
using Cinema.Models;
// Подключаем пространство имён Cinema.Services для типов ниже.
using Cinema.Services;
// Подключаем пространство имён Microsoft.AspNetCore.Identity для типов ниже.
using Microsoft.AspNetCore.Identity;
// Подключаем пространство имён Microsoft.AspNetCore.Localization для типов ниже.
using Microsoft.AspNetCore.Localization;
// Подключаем пространство имён Microsoft.AspNetCore.DataProtection для типов ниже.
using Microsoft.AspNetCore.DataProtection;
// Подключаем пространство имён Microsoft.AspNetCore.HttpOverrides для типов ниже.
using Microsoft.AspNetCore.HttpOverrides;
// Подключаем пространство имён Microsoft.Data.Sqlite для типов ниже.
using Microsoft.Data.Sqlite;
// Подключаем пространство имён Microsoft.EntityFrameworkCore для типов ниже.
using Microsoft.EntityFrameworkCore;
// Подключаем пространство имён System.Globalization для типов ниже.
using System.Globalization;

// Вычисляем builder для дальнейших проверок или отображения.
var builder = WebApplication.CreateBuilder(args);
// Optional writable location for test/sandbox hosts; default uses ASP.NET's user key store.
// Вычисляем key path для дальнейших проверок или отображения.
var keyPath = builder.Configuration["DataProtection:KeysPath"];
// Выполняем следующую ветку только при истинности условия.
if (!string.IsNullOrWhiteSpace(keyPath))
{
    // Вычисляем protection для дальнейших проверок или отображения.
    var protection = builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(keyPath));
    // Выполняем следующую ветку только при истинности условия.
    if (OperatingSystem.IsWindows())
        // Завершаем действие или объявление текущей строки.
        protection.ProtectKeysWithDpapi();
}

// Вычисляем sqlite для дальнейших проверок или отображения.
var sqlite = new SqliteConnectionStringBuilder(builder.Configuration.GetConnectionString("DefaultConnection")
    // Продолжаем выражение, начатое на предыдущей строке.
    ?? "Data Source=App_Data/cinema.db;Default Timeout=30;Foreign Keys=True");
// Выполняем следующую ветку только при истинности условия.
if (sqlite.DataSource != ":memory:")
{
    // Записываем вычисленное значение в data source.
    sqlite.DataSource = Path.GetFullPath(sqlite.DataSource, builder.Environment.ContentRootPath);
    // Завершаем действие или объявление текущей строки.
    Directory.CreateDirectory(Path.GetDirectoryName(sqlite.DataSource)!);
}
// Регистрируем зависимость или настройку в контейнере ASP.NET Core.
builder.Services.AddDbContext<CinemaDbContext>(options => options.UseSqlite(sqlite.ToString()));
// Регистрируем зависимость или настройку в контейнере ASP.NET Core.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Записываем вычисленное значение в require unique email.
    options.User.RequireUniqueEmail = true;
    // Записываем вычисленное значение в required length.
    options.Password.RequiredLength = 8;
    // Записываем вычисленное значение в require non alphanumeric.
    options.Password.RequireNonAlphanumeric = false;
    // Записываем вычисленное значение в max failed access attempts.
    options.Lockout.MaxFailedAccessAttempts = 5;
    // Записываем вычисленное значение в default lockout time span.
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
// Завершаем действие или объявление текущей строки.
}).AddEntityFrameworkStores<CinemaDbContext>().AddDefaultTokenProviders();
// Регистрируем зависимость или настройку в контейнере ASP.NET Core.
builder.Services.ConfigureApplicationCookie(options =>
{
    // Записываем вычисленное значение в login path.
    options.LoginPath = "/Account/Login";
    // Записываем вычисленное значение в access denied path.
    options.AccessDeniedPath = "/Account/Denied";
    // Записываем вычисленное значение в http only.
    options.Cookie.HttpOnly = true;
    // Записываем вычисленное значение в same site.
    options.Cookie.SameSite = SameSiteMode.Lax;
    // Записываем вычисленное значение в secure policy.
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});
// Регистрируем зависимость или настройку в контейнере ASP.NET Core.
builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);
// Регистрируем зависимость или настройку в контейнере ASP.NET Core.
builder.Services.AddControllersWithViews(options => options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()));
// Регистрируем зависимость или настройку в контейнере ASP.NET Core.
builder.Services.AddScoped<BookingService>();
// Регистрируем зависимость или настройку в контейнере ASP.NET Core.
builder.Services.AddScoped<IPaymentService, TrainingPaymentService>();
// Регистрируем зависимость или настройку в контейнере ASP.NET Core.
builder.Services.AddSingleton<CinemaClock>();
// Регистрируем зависимость или настройку в контейнере ASP.NET Core.
builder.Services.AddHostedService<BookingCleanupService>();
// Вычисляем app для дальнейших проверок или отображения.
var app = builder.Build();
// Trust forwarding headers only from the local HTTPS reverse proxy (default loopback proxies).
// Добавляем middleware в конвейер обработки HTTP-запросов.
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    // Записываем вычисленное значение в forwarded headers.
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
// Вычисляем culture для дальнейших проверок или отображения.
var culture = new CultureInfo("ru-RU");
// Добавляем middleware в конвейер обработки HTTP-запросов.
app.UseRequestLocalization(new RequestLocalizationOptions { DefaultRequestCulture = new RequestCulture(culture), SupportedCultures = [culture], SupportedUICultures = [culture] });
// Выполняем следующую ветку только при истинности условия.
if (!app.Environment.IsDevelopment())
{
    // Добавляем middleware в конвейер обработки HTTP-запросов.
    app.UseExceptionHandler("/Home/Error");
    // Добавляем middleware в конвейер обработки HTTP-запросов.
    app.UseHsts();
    // Добавляем middleware в конвейер обработки HTTP-запросов.
    app.UseHttpsRedirection();
}

// Добавляем middleware в конвейер обработки HTTP-запросов.
app.UseStaticFiles(new StaticFileOptions { OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "public,max-age=86400" });
// Добавляем middleware в конвейер обработки HTTP-запросов.
app.UseRouting();
// Добавляем middleware в конвейер обработки HTTP-запросов.
app.Use(async (context, next) =>
{
    // Записываем вычисленное значение в xcontent type options.
    context.Response.Headers.XContentTypeOptions = "nosniff";
    // Записываем вычисленное значение в headers["referrer-policy"].
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    // Дожидаемся завершения асинхронной операции.
    await next();
});
// Добавляем middleware в конвейер обработки HTTP-запросов.
app.UseAuthentication();
// Добавляем middleware в конвейер обработки HTTP-запросов.
app.UseAuthorization();
// Регистрируем шаблон URL для MVC-контроллеров.
app.MapControllerRoute("areas", "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
// Регистрируем шаблон URL для MVC-контроллеров.
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
// Local SQLite is ready on first launch: no database server or manual setup.
{
    // Создаём область зависимостей, чтобы применить миграции и начальные данные до запуска сервера.
    using var scope = app.Services.CreateScope();
    // Дожидаемся завершения асинхронной операции.
    await scope.ServiceProvider.GetRequiredService<CinemaDbContext>().Database.MigrateAsync();
    // Дожидаемся завершения асинхронной операции.
    await SeedData.InitializeAsync(scope.ServiceProvider, app.Configuration, app.Environment);
}
// Выполняем следующую ветку только при истинности условия.
if (args.Contains("--seed")) return;

// Дожидаемся завершения асинхронной операции.
await app.RunAsync();
