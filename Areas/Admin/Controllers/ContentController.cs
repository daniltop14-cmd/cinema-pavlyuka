// Управляет текстами, услугами, FAQ, командой и обращениями.
// Пояснения ниже относятся к следующей строке или смысловому шагу.
// Подключаем пространство имён Cinema.Data для типов ниже.
using Cinema.Data;
// Подключаем пространство имён Cinema.Models для типов ниже.
using Cinema.Models;
// Подключаем пространство имён Cinema.ViewModels для типов ниже.
using Cinema.ViewModels;
// Подключаем пространство имён Microsoft.AspNetCore.Authorization для типов ниже.
using Microsoft.AspNetCore.Authorization;
// Подключаем пространство имён Microsoft.AspNetCore.Mvc для типов ниже.
using Microsoft.AspNetCore.Mvc;
// Подключаем пространство имён Microsoft.EntityFrameworkCore для типов ниже.
using Microsoft.EntityFrameworkCore;

// Помещаем объявленные типы в указанное пространство имён.
namespace Cinema.Areas.Admin.Controllers;
// Ограничиваем доступ: роль проверяется сервером до вызова действия.
[Area("Admin"), Authorize(Roles = "Admin")]
// Управляет текстами, услугами, FAQ, командой и обращениями.
public class ContentController(CinemaDbContext db) : Controller
{
    // Готовим основную страницу этого раздела и возвращаем представление.
    public async Task<IActionResult> Index(string kind = "services")
    {
        // Записываем вычисленное значение в kind.
        ViewBag.Kind = kind;
        // Возвращаем результат вызывающему коду.
        return kind switch
        {
            // Выполняем запрос к SQLite асинхронно.
            "services" => View(await db.Services.AsNoTracking().Select(x => new ContentModel { Id = x.Id, Title = x.Name, Text = x.Description, Price = x.Price }).ToListAsync()),
            // Упорядочиваем записи перед выводом.
            "faq" => View(await db.FaqItems.AsNoTracking().OrderBy(x => x.SortOrder).Select(x => new ContentModel { Id = x.Id, Title = x.Question, Text = x.Answer }).ToListAsync()),
            // Выполняем запрос к SQLite асинхронно.
            "team" => View(await db.TeamMembers.AsNoTracking().Select(x => new ContentModel { Id = x.Id, Title = x.Name, Text = x.Tasks, Extra = x.Role }).ToListAsync()),
            // Завершаем действие или объявление текущей строки.
            _ => NotFound()};
    }

    // Назначаем адрес или тип GET-запроса для этого действия.
    [HttpGet]
    // Показываем или сохраняем форму редактирования записи.
    public async Task<IActionResult> Edit(string kind, int id = 0)
    {
        // Записываем вычисленное значение в content model? model.
        ContentModel? model = null;
        // Выполняем следующую ветку только при истинности условия.
        if (!new[]
        {
            // Передаём очередной аргумент или задаём элемент коллекции.
            "services",
            // Передаём очередной аргумент или задаём элемент коллекции.
            "faq",
            "team"
        }.Contains(kind))
            // Отвечаем 404, когда запрошенная запись не найдена или недоступна.
            return NotFound();
        // Выполняем следующую ветку только при истинности условия.
        if (id == 0)
            // Передаём данные в Razor-представление и возвращаем HTML.
            return View(new ContentModel { Kind = kind });
        // Выполняем следующую ветку только при истинности условия.
        if (kind == "services")
        {
            // Получаем x из базы или подготавливаем запрос к ней.
            var x = await db.Services.FindAsync(id);
            // Обрабатываем отсутствие записи или необязательного значения.
            if (x != null)
                // Записываем вычисленное значение в model.
                model = new ContentModel
                {
                    // Записываем вычисленное значение в id.
                    Id = id,
                    // Записываем вычисленное значение в kind.
                    Kind = kind,
                    // Записываем вычисленное значение в title.
                    Title = x.Name,
                    // Записываем вычисленное значение в text.
                    Text = x.Description,
                    // Записываем вычисленное значение в price.
                    Price = x.Price,
                    // Записываем вычисленное значение в extra.
                    Extra = x.ImagePath
                };
        }

        // Выполняем следующую ветку только при истинности условия.
        if (kind == "faq")
        {
            // Получаем x из базы или подготавливаем запрос к ней.
            var x = await db.FaqItems.FindAsync(id);
            // Обрабатываем отсутствие записи или необязательного значения.
            if (x != null)
                // Записываем вычисленное значение в model.
                model = new ContentModel
                {
                    // Записываем вычисленное значение в id.
                    Id = id,
                    // Записываем вычисленное значение в kind.
                    Kind = kind,
                    // Записываем вычисленное значение в title.
                    Title = x.Question,
                    // Записываем вычисленное значение в text.
                    Text = x.Answer,
                    // Записываем вычисленное значение в order.
                    Order = x.SortOrder
                };
        }

        // Выполняем следующую ветку только при истинности условия.
        if (kind == "team")
        {
            // Получаем x из базы или подготавливаем запрос к ней.
            var x = await db.TeamMembers.FindAsync(id);
            // Обрабатываем отсутствие записи или необязательного значения.
            if (x != null)
                // Записываем вычисленное значение в model.
                model = new ContentModel
                {
                    // Записываем вычисленное значение в id.
                    Id = id,
                    // Записываем вычисленное значение в kind.
                    Kind = kind,
                    // Записываем вычисленное значение в title.
                    Title = x.Name,
                    // Записываем вычисленное значение в text.
                    Text = x.Tasks,
                    // Записываем вычисленное значение в extra.
                    Extra = x.Role
                };
        }

        // Возвращаем результат вызывающему коду.
        return model == null ? NotFound() : View(model);
    }

    // Разрешаем изменение данных только через POST-запрос.
    [HttpPost]
    // Показываем или сохраняем форму редактирования записи.
    public async Task<IActionResult> Edit(ContentModel model)
    {
        // Выполняем следующую ветку только при истинности условия.
        if (!new[]
        {
            // Передаём очередной аргумент или задаём элемент коллекции.
            "services",
            // Передаём очередной аргумент или задаём элемент коллекции.
            "faq",
            "team"
        }.Contains(model.Kind))
            // Отвечаем 404, когда запрошенная запись не найдена или недоступна.
            return NotFound();
        // Если форма содержит ошибки проверки, не сохраняем данные.
        if (!ModelState.IsValid)
            // Передаём данные в Razor-представление и возвращаем HTML.
            return View(model);
        // Выполняем следующую ветку только при истинности условия.
        if (model.Kind == "services" && model.Title.Length > 120 || model.Kind == "team" && model.Title.Length > 100)
            // Завершаем действие или объявление текущей строки.
            ModelState.AddModelError("", "Слишком длинное название.");
        // Выполняем следующую ветку только при истинности условия.
        if (model.Kind == "services" && model.Text.Length > 2000 || model.Kind == "team" && model.Text.Length > 1000)
            // Завершаем действие или объявление текущей строки.
            ModelState.AddModelError("", "Сократите описание.");
        // Выполняем следующую ветку только при истинности условия.
        if (model.Kind == "team" && (string.IsNullOrWhiteSpace(model.Extra) || model.Extra.Length > 100))
            // Завершаем действие или объявление текущей строки.
            ModelState.AddModelError("", "Укажите роль до 100 символов.");
        // Обрабатываем отсутствие записи или необязательного значения.
        if (model.Kind == "services" && model.Extra != null && !model.Extra.StartsWith("/images/") && !model.Extra.StartsWith("/uploads/"))
            // Завершаем действие или объявление текущей строки.
            ModelState.AddModelError("", "Изображение должно находиться в /images/ или /uploads/.");
        // Если форма содержит ошибки проверки, не сохраняем данные.
        if (!ModelState.IsValid)
            // Передаём данные в Razor-представление и возвращаем HTML.
            return View(model);
        // Выполняем следующую ветку только при истинности условия.
        if (model.Kind == "services")
        {
            // Получаем x из базы или подготавливаем запрос к ней.
            var x = model.Id == 0 ? new Service() : await db.Services.FindAsync(model.Id);
            // Обрабатываем отсутствие записи или необязательного значения.
            if (x == null)
                // Отвечаем 404, когда запрошенная запись не найдена или недоступна.
                return NotFound();
            // Записываем вычисленное значение в name.
            x.Name = model.Title;
            // Записываем вычисленное значение в description.
            x.Description = model.Text;
            // Записываем вычисленное значение в price.
            x.Price = model.Price;
            // Записываем вычисленное значение в image path.
            x.ImagePath = model.Extra ?? "/images/hall.svg";
            // Выполняем следующую ветку только при истинности условия.
            if (model.Id == 0)
                // Добавляем новую сущность в контекст для последующего сохранения.
                db.Services.Add(x);
        }

        // Выполняем следующую ветку только при истинности условия.
        if (model.Kind == "faq")
        {
            // Получаем x из базы или подготавливаем запрос к ней.
            var x = model.Id == 0 ? new FaqItem() : await db.FaqItems.FindAsync(model.Id);
            // Обрабатываем отсутствие записи или необязательного значения.
            if (x == null)
                // Отвечаем 404, когда запрошенная запись не найдена или недоступна.
                return NotFound();
            // Записываем вычисленное значение в question.
            x.Question = model.Title;
            // Записываем вычисленное значение в answer.
            x.Answer = model.Text;
            // Записываем вычисленное значение в sort order.
            x.SortOrder = model.Order;
            // Выполняем следующую ветку только при истинности условия.
            if (model.Id == 0)
                // Добавляем новую сущность в контекст для последующего сохранения.
                db.FaqItems.Add(x);
        }

        // Выполняем следующую ветку только при истинности условия.
        if (model.Kind == "team")
        {
            // Получаем x из базы или подготавливаем запрос к ней.
            var x = model.Id == 0 ? new TeamMember() : await db.TeamMembers.FindAsync(model.Id);
            // Обрабатываем отсутствие записи или необязательного значения.
            if (x == null)
                // Отвечаем 404, когда запрошенная запись не найдена или недоступна.
                return NotFound();
            // Записываем вычисленное значение в name.
            x.Name = model.Title;
            // Записываем вычисленное значение в tasks.
            x.Tasks = model.Text;
            // Записываем вычисленное значение в role.
            x.Role = model.Extra!;
            // Выполняем следующую ветку только при истинности условия.
            if (model.Id == 0)
                // Добавляем новую сущность в контекст для последующего сохранения.
                db.TeamMembers.Add(x);
        }

        // Сохраняем накопленные изменения в SQLite.
        await db.SaveChangesAsync();
        // Записываем вычисленное значение в temp data["success"].
        TempData["Success"] = "Изменения сохранены.";
        // После операции перенаправляем браузер на указанную страницу.
        return RedirectToAction(nameof(Index), new { kind = model.Kind });
    }

    // Разрешаем изменение данных только через POST-запрос.
    [HttpPost]
    // Выполняем действие Delete после проверки прав и состояния записи.
    public async Task<IActionResult> Delete(string kind, int id)
    {
        // Выполняем следующую ветку только при истинности условия.
        if (kind == "services")
        {
            // Получаем x из базы или подготавливаем запрос к ней.
            var x = await db.Services.FindAsync(id);
            // Обрабатываем отсутствие записи или необязательного значения.
            if (x != null)
                // Помечаем сущность на удаление при сохранении контекста.
                db.Remove(x);
        }
        // Обрабатываем альтернативный случай предыдущей проверки.
        else if (kind == "faq")
        {
            // Получаем x из базы или подготавливаем запрос к ней.
            var x = await db.FaqItems.FindAsync(id);
            // Обрабатываем отсутствие записи или необязательного значения.
            if (x != null)
                // Помечаем сущность на удаление при сохранении контекста.
                db.Remove(x);
        }
        // Обрабатываем альтернативный случай предыдущей проверки.
        else if (kind == "team")
        {
            // Получаем x из базы или подготавливаем запрос к ней.
            var x = await db.TeamMembers.FindAsync(id);
            // Обрабатываем отсутствие записи или необязательного значения.
            if (x != null)
                // Помечаем сущность на удаление при сохранении контекста.
                db.Remove(x);
        }
        // Обрабатываем альтернативный случай предыдущей проверки.
        else
            // Отвечаем 404, когда запрошенная запись не найдена или недоступна.
            return NotFound();
        // Сохраняем накопленные изменения в SQLite.
        await db.SaveChangesAsync();
        // После операции перенаправляем браузер на указанную страницу.
        return RedirectToAction(nameof(Index), new { kind });
    }

    // Назначаем адрес или тип GET-запроса для этого действия.
    [HttpGet]
    // Объявляем метод Settings: параметры приходят из запроса или вызова сервиса.
    public async Task<IActionResult> Settings() => View(await db.SiteSettings.AsNoTracking().OrderBy(x => x.Key).ToListAsync());
    // Разрешаем изменение данных только через POST-запрос.
    [HttpPost]
    // Объявляем метод Settings: параметры приходят из запроса или вызова сервиса.
    public async Task<IActionResult> Settings(Dictionary<string, string> values)
    {
        // Получаем settings из базы или подготавливаем запрос к ней.
        var settings = await db.SiteSettings.ToListAsync();
        // Последовательно обрабатываем каждый элемент коллекции.
        foreach (var setting in settings)
        {
            // Выполняем следующую ветку только при истинности условия.
            if (!values.TryGetValue(setting.Key, out var value) || string.IsNullOrWhiteSpace(value) || value.Length > 8000)
            {
                // Записываем вычисленное значение в temp data["error"].
                TempData["Error"] = "Заполните все настройки (не более 8000 символов).";
                // После операции перенаправляем браузер на указанную страницу.
                return RedirectToAction(nameof(Settings));
            }

            // Выполняем следующую ветку только при истинности условия.
            if (setting.Key.StartsWith("Social") && (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != "https"))
            {
                // Записываем вычисленное значение в temp data["error"].
                TempData["Error"] = "Социальные ссылки должны начинаться с https://";
                // После операции перенаправляем браузер на указанную страницу.
                return RedirectToAction(nameof(Settings));
            }

            // Выполняем следующую ветку только при истинности условия.
            if (setting.Key.StartsWith("Banner") && setting.Key.EndsWith("Target") && !Url.IsLocalUrl(value))
            {
                // Записываем вычисленное значение в temp data["error"].
                TempData["Error"] = "Ссылка баннера должна вести на страницу этого сайта.";
                // После операции перенаправляем браузер на указанную страницу.
                return RedirectToAction(nameof(Settings));
            }

            // Выполняем следующую ветку только при истинности условия.
            if ((setting.Key.StartsWith("Banner") || setting.Key.StartsWith("News")) && setting.Key.EndsWith("Image") &&
                !value.StartsWith("/images/") && !value.StartsWith("/uploads/"))
            {
                // Записываем вычисленное значение в temp data["error"].
                TempData["Error"] = "Изображение должно находиться в /images/ или /uploads/.";
                // После операции перенаправляем браузер на указанную страницу.
                return RedirectToAction(nameof(Settings));
            }

            // Записываем вычисленное значение в value.
            setting.Value = value;
        }

        // Сохраняем накопленные изменения в SQLite.
        await db.SaveChangesAsync();
        // Записываем вычисленное значение в temp data["success"].
        TempData["Success"] = "Настройки сохранены.";
        // После операции перенаправляем браузер на указанную страницу.
        return RedirectToAction(nameof(Settings));
    }

    // Объявляем метод Messages: параметры приходят из запроса или вызова сервиса.
    public async Task<IActionResult> Messages(int page = 1)
    {
        // Записываем вычисленное значение в page.
        page = Math.Max(1, page);
        // Записываем вычисленное значение в page.
        ViewBag.Page = page;
        // Записываем вычисленное значение в has next.
        ViewBag.HasNext = await db.ContactMessages.CountAsync() > page * 20;
        // Передаём данные в Razor-представление и возвращаем HTML.
        return View(await db.ContactMessages.AsNoTracking().OrderByDescending(x => x.CreatedAtUtc).Skip((page - 1) * 20).Take(20).ToListAsync());
    }
}
