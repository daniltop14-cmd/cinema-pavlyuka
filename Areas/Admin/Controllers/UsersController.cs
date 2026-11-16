// Назначает роли и блокирует пользователей.
// Пояснения ниже относятся к следующей строке или смысловому шагу.
// Подключаем пространство имён Cinema.Data для типов ниже.
using Cinema.Data;
// Подключаем пространство имён Cinema.Models для типов ниже.
using Cinema.Models;
// Подключаем пространство имён Microsoft.AspNetCore.Authorization для типов ниже.
using Microsoft.AspNetCore.Authorization;
// Подключаем пространство имён Microsoft.AspNetCore.Identity для типов ниже.
using Microsoft.AspNetCore.Identity;
// Подключаем пространство имён Microsoft.AspNetCore.Mvc для типов ниже.
using Microsoft.AspNetCore.Mvc;
// Подключаем пространство имён Microsoft.EntityFrameworkCore для типов ниже.
using Microsoft.EntityFrameworkCore;

// Помещаем объявленные типы в указанное пространство имён.
namespace Cinema.Areas.Admin.Controllers;
// Ограничиваем доступ: роль проверяется сервером до вызова действия.
[Area("Admin"), Authorize(Roles = "Admin")]
// Назначает роли и блокирует пользователей.
public class UsersController(CinemaDbContext db, UserManager<ApplicationUser> users) : Controller
{
    // Готовим основную страницу этого раздела и возвращаем представление.
    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        // Получаем query из базы или подготавливаем запрос к ней.
        var query = db.Users.AsNoTracking().AsQueryable();
        // Выполняем следующую ветку только при истинности условия.
        if (!string.IsNullOrWhiteSpace(search))
            // Оставляем в выборке только записи, подходящие под условие.
            query = query.Where(x => x.Email!.Contains(search) || x.DisplayName.Contains(search));
        // Записываем вычисленное значение в page.
        page = Math.Max(1, page);
        // Записываем вычисленное значение в page.
        ViewBag.Page = page;
        // Записываем вычисленное значение в search.
        ViewBag.Search = search;
        // Записываем вычисленное значение в has next.
        ViewBag.HasNext = await query.CountAsync() > page * 20;
        // Асинхронно получаем list, не блокируя поток сервера.
        var list = await query.OrderBy(x => x.Email).Skip((page - 1) * 20).Take(20).ToListAsync();
        // Вычисляем role map для дальнейших проверок или отображения.
        var roleMap = new Dictionary<string, IList<string>>();
        // Последовательно обрабатываем каждый элемент коллекции.
        foreach (var user in list)
            // Записываем вычисленное значение в id].
            roleMap[user.Id] = await users.GetRolesAsync(user);
        // Записываем вычисленное значение в roles.
        ViewBag.Roles = roleMap;
        // Передаём данные в Razor-представление и возвращаем HTML.
        return View(list);
    }

    // Разрешаем изменение данных только через POST-запрос.
    [HttpPost]
    // Выполняем действие Role после проверки прав и состояния записи.
    public async Task<IActionResult> Role(string id, string role)
    {
        // Выполняем следующую ветку только при истинности условия.
        if (!new[]
        {
            // Передаём очередной аргумент или задаём элемент коллекции.
            "Admin",
            // Передаём очередной аргумент или задаём элемент коллекции.
            "Manager",
            "User"
        }.Contains(role))
            // Возвращаем результат вызывающему коду.
            return BadRequest();
        // Асинхронно получаем user, не блокируя поток сервера.
        var user = await users.FindByIdAsync(id);
        // Обрабатываем отсутствие записи или необязательного значения.
        if (user == null)
            // Отвечаем 404, когда запрошенная запись не найдена или недоступна.
            return NotFound();
        // Выполняем следующую ветку только при истинности условия.
        if (id == users.GetUserId(User))
        {
            // Записываем вычисленное значение в temp data["error"].
            TempData["Error"] = "Нельзя изменить собственную роль. Используйте другую учётную запись администратора.";
            // После операции перенаправляем браузер на указанную страницу.
            return RedirectToAction(nameof(Index));
        }

        // Начинаем транзакцию до чтения и изменения связанных данных.
        await using var tx = await db.Database.BeginTransactionAsync();
        // Асинхронно получаем current roles, не блокируя поток сервера.
        var currentRoles = await users.GetRolesAsync(user);
        // Асинхронно получаем removed, не блокируя поток сервера.
        var removed = await users.RemoveFromRolesAsync(user, currentRoles);
        // Асинхронно получаем added, не блокируя поток сервера.
        var added = removed.Succeeded ? await users.AddToRoleAsync(user, role) : IdentityResult.Failed();
        // Выполняем следующую ветку только при истинности условия.
        if (added.Succeeded)
        {
            // Дожидаемся завершения асинхронной операции.
            await users.UpdateSecurityStampAsync(user);
            // Подтверждаем транзакцию: изменения становятся постоянными.
            await tx.CommitAsync();
            // Записываем вычисленное значение в temp data["success"].
            TempData["Success"] = "Роль сохранена. Изменения применятся после обновления сеанса пользователя.";
        }
        // Обрабатываем альтернативный случай предыдущей проверки.
        else
            // Записываем вычисленное значение в temp data["error"].
            TempData["Error"] = "Не удалось назначить роль.";
        // После операции перенаправляем браузер на указанную страницу.
        return RedirectToAction(nameof(Index));
    }

    // Разрешаем изменение данных только через POST-запрос.
    [HttpPost]
    // Выполняем действие Block после проверки прав и состояния записи.
    public async Task<IActionResult> Block(string id, bool blocked)
    {
        // Асинхронно получаем user, не блокируя поток сервера.
        var user = await users.FindByIdAsync(id);
        // Обрабатываем отсутствие записи или необязательного значения.
        if (user == null)
            // Отвечаем 404, когда запрошенная запись не найдена или недоступна.
            return NotFound();
        // Выполняем следующую ветку только при истинности условия.
        if (id == users.GetUserId(User))
            // Возвращаем результат вызывающему коду.
            return BadRequest("Нельзя блокировать себя.");
        // Дожидаемся завершения асинхронной операции.
        await users.SetLockoutEnabledAsync(user, true);
        // Дожидаемся завершения асинхронной операции.
        await users.SetLockoutEndDateAsync(user, blocked ? DateTimeOffset.UtcNow.AddYears(100) : null);
        // Дожидаемся завершения асинхронной операции.
        await users.UpdateSecurityStampAsync(user);
        // После операции перенаправляем браузер на указанную страницу.
        return RedirectToAction(nameof(Index));
    }
}
