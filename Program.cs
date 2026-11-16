// Запускает сайт: подключает SQLite и сервисы, задаёт маршруты MVC и принимает HTTP-запросы.
using Cinema.Data;
using Cinema.Models;
using Cinema.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);
// DataProtection:KeysPath можно задать в конфигурации: cookie входа шифруются ключами из этой папки.
// Optional writable location for test/sandbox hosts; default uses ASP.NET's user key store.
var keyPath = builder.Configuration["DataProtection:KeysPath"];
if (!string.IsNullOrWhiteSpace(keyPath))
{
    var protection = builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(keyPath));
    if (OperatingSystem.IsWindows())
        protection.ProtectKeysWithDpapi();
}

var sqlite = new SqliteConnectionStringBuilder(builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=App_Data/cinema.db;Default Timeout=30;Foreign Keys=True");
// Относительный путь App_Data/cinema.db считаем от корня приложения, а не от текущей папки команды.
if (sqlite.DataSource != ":memory:")
{
    sqlite.DataSource = Path.GetFullPath(sqlite.DataSource, builder.Environment.ContentRootPath);
    Directory.CreateDirectory(Path.GetDirectoryName(sqlite.DataSource)!);
}
builder.Services.AddDbContext<CinemaDbContext>(options => options.UseSqlite(sqlite.ToString()));
// Identity хранит пользователей и роли в том же CinemaDbContext, то есть в SQLite вместе с заказами.
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 8;
    options.Password.RequireNonAlphanumeric = false;
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
}).AddEntityFrameworkStores<CinemaDbContext>().AddDefaultTokenProviders();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/Denied";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});
builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);
builder.Services.AddControllersWithViews(options => options.Filters.Add(new Microsoft.AspNetCore.Mvc.AutoValidateAntiforgeryTokenAttribute()));
// Контроллеры получают эти сервисы через конструктор; фоновая очистка работает независимо от HTTP-запросов.
builder.Services.AddScoped<BookingService>();
builder.Services.AddScoped<IPaymentService, TrainingPaymentService>();
builder.Services.AddSingleton<CinemaClock>();
builder.Services.AddHostedService<BookingCleanupService>();
var app = builder.Build();
// Trust forwarding headers only from the local HTTPS reverse proxy (default loopback proxies).
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});
var culture = new CultureInfo("ru-RU");
app.UseRequestLocalization(new RequestLocalizationOptions { DefaultRequestCulture = new RequestCulture(culture), SupportedCultures = [culture], SupportedUICultures = [culture] });
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseStaticFiles(new StaticFileOptions { OnPrepareResponse = ctx => ctx.Context.Response.Headers.CacheControl = "public,max-age=86400" });
// После статических файлов MVC сопоставляет URL с контроллером, затем проверяет cookie и роли.
app.UseRouting();
app.Use(async (context, next) =>
{
    context.Response.Headers.XContentTypeOptions = "nosniff";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    await next();
});
app.UseAuthentication();
app.UseAuthorization();
// /Admin/... направляется в Areas/Admin/Controllers; [Area("Admin")] помечает соответствующие контроллеры.
app.MapControllerRoute("areas", "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
// / ведёт в HomeController.Index, /Movies/Details/5 — в MoviesController.Details(5).
// [HttpGet("/Team")] и другие атрибуты в контроллерах могут задать отдельный маршрут или HTTP-метод.
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
// Local SQLite is ready on first launch: no database server or manual setup.
{
    // Scope даёт отдельный DbContext: миграции и начальные данные применяются до приёма запросов.
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<CinemaDbContext>().Database.MigrateAsync();
    await SeedData.InitializeAsync(scope.ServiceProvider, app.Configuration, app.Environment);
}
if (args.Contains("--seed")) return;

await app.RunAsync();
