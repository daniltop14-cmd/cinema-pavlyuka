// Редактирует новости, услуги, FAQ, настройки и обращения в SQLite.
using Cinema.Data;
using Cinema.Models;
using Cinema.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Admin.Controllers;
[Area("Admin"), Authorize(Roles = "Admin")]
public class ContentController(CinemaDbContext db) : Controller
{

    public async Task<IActionResult> Index(string kind = "services")
    {
        ViewBag.Kind = kind;
        return kind switch
        {
            // Загружаем услуги из SQLite; их описание и цена задаются в админке.
            "services" => View(await db.Services.AsNoTracking().Select(x => new ContentModel { Id = x.Id, Title = x.Name, Text = x.Description, Price = x.Price }).ToListAsync()),
            // Загружаем вопросы и ответы из SQLite в заданном администратором порядке.
            "faq" => View(await db.FaqItems.AsNoTracking().OrderBy(x => x.SortOrder).Select(x => new ContentModel { Id = x.Id, Title = x.Question, Text = x.Answer }).ToListAsync()),
            // Загружаем список сотрудников из SQLite для страницы команды.
            "team" => View(await db.TeamMembers.AsNoTracking().Select(x => new ContentModel { Id = x.Id, Title = x.Name, Text = x.Tasks, Extra = x.Role }).ToListAsync()),
            _ => NotFound()};
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string kind, int id = 0)
    {
        ContentModel? model = null;
        if (!new[]
        {
            "services",
            "faq",
            "team"
        }.Contains(kind))
            return NotFound();
        if (id == 0)

            return View(new ContentModel { Kind = kind });
        if (kind == "services")
        {
            // Загружаем услуги из SQLite; их описание и цена задаются в админке.
            var x = await db.Services.FindAsync(id);
            if (x != null)
                model = new ContentModel
                {
                    Id = id,
                    Kind = kind,
                    Title = x.Name,
                    Text = x.Description,
                    Price = x.Price,
                    Extra = x.ImagePath
                };
        }

        if (kind == "faq")
        {
            // Загружаем вопросы и ответы из SQLite в заданном администратором порядке.
            var x = await db.FaqItems.FindAsync(id);
            if (x != null)
                model = new ContentModel
                {
                    Id = id,
                    Kind = kind,
                    Title = x.Question,
                    Text = x.Answer,
                    Order = x.SortOrder
                };
        }

        if (kind == "team")
        {
            // Загружаем список сотрудников из SQLite для страницы команды.
            var x = await db.TeamMembers.FindAsync(id);
            if (x != null)
                model = new ContentModel
                {
                    Id = id,
                    Kind = kind,
                    Title = x.Name,
                    Text = x.Tasks,
                    Extra = x.Role
                };
        }

        return model == null ? NotFound() : View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(ContentModel model)
    {
        if (!new[]
        {
            "services",
            "faq",
            "team"
        }.Contains(model.Kind))
            return NotFound();
        // Проверяем атрибуты модели и ошибки привязки формы; при ошибках снова показываем форму.
        if (!ModelState.IsValid)

            return View(model);
        if (model.Kind == "services" && model.Title.Length > 120 || model.Kind == "team" && model.Title.Length > 100)
            ModelState.AddModelError("", "Слишком длинное название.");
        if (model.Kind == "services" && model.Text.Length > 2000 || model.Kind == "team" && model.Text.Length > 1000)
            ModelState.AddModelError("", "Сократите описание.");
        if (model.Kind == "team" && (string.IsNullOrWhiteSpace(model.Extra) || model.Extra.Length > 100))
            ModelState.AddModelError("", "Укажите роль до 100 символов.");
        if (model.Kind == "services" && model.Extra != null && !model.Extra.StartsWith("/images/") && !model.Extra.StartsWith("/uploads/"))
            ModelState.AddModelError("", "Изображение должно находиться в /images/ или /uploads/.");
        // Проверяем атрибуты модели и ошибки привязки формы; при ошибках снова показываем форму.
        if (!ModelState.IsValid)

            return View(model);
        if (model.Kind == "services")
        {
            // Загружаем услуги из SQLite; их описание и цена задаются в админке.
            var x = model.Id == 0 ? new Service() : await db.Services.FindAsync(model.Id);
            if (x == null)
                return NotFound();
            x.Name = model.Title;
            x.Description = model.Text;
            x.Price = model.Price;
            x.ImagePath = model.Extra ?? "/images/hall.svg";
            if (model.Id == 0)
                db.Services.Add(x);
        }

        if (model.Kind == "faq")
        {
            // Загружаем вопросы и ответы из SQLite в заданном администратором порядке.
            var x = model.Id == 0 ? new FaqItem() : await db.FaqItems.FindAsync(model.Id);
            if (x == null)
                return NotFound();
            x.Question = model.Title;
            x.Answer = model.Text;
            x.SortOrder = model.Order;
            if (model.Id == 0)
                db.FaqItems.Add(x);
        }

        if (model.Kind == "team")
        {
            // Загружаем список сотрудников из SQLite для страницы команды.
            var x = model.Id == 0 ? new TeamMember() : await db.TeamMembers.FindAsync(model.Id);
            if (x == null)
                return NotFound();
            x.Name = model.Title;
            x.Tasks = model.Text;
            x.Role = model.Extra!;
            if (model.Id == 0)
                db.TeamMembers.Add(x);
        }

        // Записываем накопленные изменения сущностей в SQLite.
        await db.SaveChangesAsync();
        TempData["Success"] = "Изменения сохранены.";
        return RedirectToAction(nameof(Index), new { kind = model.Kind });
    }

    [HttpPost]
    // id записи приходит из POST-формы; после проверки связей удаляет запись из SQLite.
    public async Task<IActionResult> Delete(string kind, int id)
    {
        if (kind == "services")
        {
            // Загружаем услуги из SQLite; их описание и цена задаются в админке.
            var x = await db.Services.FindAsync(id);
            if (x != null)
                db.Remove(x);
        }
        else if (kind == "faq")
        {
            // Загружаем вопросы и ответы из SQLite в заданном администратором порядке.
            var x = await db.FaqItems.FindAsync(id);
            if (x != null)
                db.Remove(x);
        }
        else if (kind == "team")
        {
            // Загружаем список сотрудников из SQLite для страницы команды.
            var x = await db.TeamMembers.FindAsync(id);
            if (x != null)
                db.Remove(x);
        }
        else
            return NotFound();
        // Записываем накопленные изменения сущностей в SQLite.
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index), new { kind });
    }

    [HttpGet]
    // Загружаем тексты и ссылки из SQLite; на странице они используются для баннеров и информационных блоков.
    public async Task<IActionResult> Settings() => View(await db.SiteSettings.AsNoTracking().OrderBy(x => x.Key).ToListAsync());
    [HttpPost]
    public async Task<IActionResult> Settings(Dictionary<string, string> values)
    {
        // Загружаем тексты и ссылки из SQLite; на странице они используются для баннеров и информационных блоков.
        var settings = await db.SiteSettings.ToListAsync();
        foreach (var setting in settings)
        {
            if (!values.TryGetValue(setting.Key, out var value) || string.IsNullOrWhiteSpace(value) || value.Length > 8000)
            {
                TempData["Error"] = "Заполните все настройки (не более 8000 символов).";
                return RedirectToAction(nameof(Settings));
            }

            if (setting.Key.StartsWith("Social") && (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != "https"))
            {
                TempData["Error"] = "Социальные ссылки должны начинаться с https://";
                return RedirectToAction(nameof(Settings));
            }

            if (setting.Key.StartsWith("Banner") && setting.Key.EndsWith("Target") && !Url.IsLocalUrl(value))
            {
                TempData["Error"] = "Ссылка баннера должна вести на страницу этого сайта.";
                return RedirectToAction(nameof(Settings));
            }

            if ((setting.Key.StartsWith("Banner") || setting.Key.StartsWith("News")) && setting.Key.EndsWith("Image") &&
                !value.StartsWith("/images/") && !value.StartsWith("/uploads/"))
            {
                TempData["Error"] = "Изображение должно находиться в /images/ или /uploads/.";
                return RedirectToAction(nameof(Settings));
            }

            setting.Value = value;
        }

        // Записываем накопленные изменения сущностей в SQLite.
        await db.SaveChangesAsync();
        TempData["Success"] = "Настройки сохранены.";
        return RedirectToAction(nameof(Settings));
    }

    public async Task<IActionResult> Messages(int page = 1)
    {
        page = Math.Max(1, page);
        ViewBag.Page = page;
        // Читаем обращения посетителей, сохранённые в SQLite.
        ViewBag.HasNext = await db.ContactMessages.CountAsync() > page * 20;
        // Читаем обращения посетителей, сохранённые в SQLite.
        return View(await db.ContactMessages.AsNoTracking().OrderByDescending(x => x.CreatedAtUtc).Skip((page - 1) * 20).Take(20).ToListAsync());
    }
}
