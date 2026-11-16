// Меняет роли и блокировку учётных записей через ASP.NET Identity.
using Cinema.Data;
using Cinema.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Admin.Controllers;
[Area("Admin"), Authorize(Roles = "Admin")]
public class UsersController(CinemaDbContext db, UserManager<ApplicationUser> users) : Controller
{

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        // Читаем учётные записи Identity из SQLite для управления доступом.
        var query = db.Users.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(x => x.Email!.Contains(search) || x.DisplayName.Contains(search));
        page = Math.Max(1, page);
        ViewBag.Page = page;
        ViewBag.Search = search;
        ViewBag.HasNext = await query.CountAsync() > page * 20;
        var list = await query.OrderBy(x => x.Email).Skip((page - 1) * 20).Take(20).ToListAsync();
        var roleMap = new Dictionary<string, IList<string>>();
        foreach (var user in list)
            roleMap[user.Id] = await users.GetRolesAsync(user);
        ViewBag.Roles = roleMap;

        return View(list);
    }

    [HttpPost]
    // Получает id пользователя и роль из формы, проверяет допустимость роли и меняет её через Identity.
    public async Task<IActionResult> Role(string id, string role)
    {
        if (!new[]
        {
            "Admin",
            "Manager",
            "User"
        }.Contains(role))
            return BadRequest();
        var user = await users.FindByIdAsync(id);
        if (user == null)
            return NotFound();
        if (id == users.GetUserId(User))
        {
            TempData["Error"] = "Нельзя изменить собственную роль. Используйте другую учётную запись администратора.";
            return RedirectToAction(nameof(Index));
        }

        // Начинаем транзакцию SQLite до проверки и изменения мест, чтобы параллельные запросы не продали одно кресло дважды.
        await using var tx = await db.Database.BeginTransactionAsync();
        var currentRoles = await users.GetRolesAsync(user);
        var removed = await users.RemoveFromRolesAsync(user, currentRoles);
        var added = removed.Succeeded ? await users.AddToRoleAsync(user, role) : IdentityResult.Failed();
        if (added.Succeeded)
        {
            await users.UpdateSecurityStampAsync(user);
            // Подтверждаем транзакцию: связанные изменения заказа и мест сохраняются вместе.
            await tx.CommitAsync();
            TempData["Success"] = "Роль сохранена. Изменения применятся после обновления сеанса пользователя.";
        }
        else
            TempData["Error"] = "Не удалось назначить роль.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    // Получает id пользователя и признак блокировки из формы, затем меняет доступ через Identity.
    public async Task<IActionResult> Block(string id, bool blocked)
    {
        var user = await users.FindByIdAsync(id);
        if (user == null)
            return NotFound();
        if (id == users.GetUserId(User))
            return BadRequest("Нельзя блокировать себя.");
        await users.SetLockoutEnabledAsync(user, true);
        await users.SetLockoutEndDateAsync(user, blocked ? DateTimeOffset.UtcNow.AddYears(100) : null);
        await users.UpdateSecurityStampAsync(user);
        return RedirectToAction(nameof(Index));
    }
}
