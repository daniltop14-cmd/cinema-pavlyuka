// Показывает и модерирует отзывы.
// Пояснения ниже относятся к следующей строке или смысловому шагу.
// Подключаем пространство имён Cinema.Data для типов ниже.
using Cinema.Data;
// Подключаем пространство имён Cinema.Models для типов ниже.
using Cinema.Models;
// Подключаем пространство имён Microsoft.AspNetCore.Authorization для типов ниже.
using Microsoft.AspNetCore.Authorization;
// Подключаем пространство имён Microsoft.AspNetCore.Mvc для типов ниже.
using Microsoft.AspNetCore.Mvc;
// Подключаем пространство имён Microsoft.EntityFrameworkCore для типов ниже.
using Microsoft.EntityFrameworkCore;

// Помещаем объявленные типы в указанное пространство имён.
namespace Cinema.Areas.Admin.Controllers;
// Ограничиваем доступ: роль проверяется сервером до вызова действия.
[Area("Admin"), Authorize(Roles = "Admin,Manager")]
// Показывает и модерирует отзывы.
public class ReviewsController(CinemaDbContext db) : Controller
{
    // Готовим основную страницу этого раздела и возвращаем представление.
    public async Task<IActionResult> Index(int page = 1)
    {
        // Записываем вычисленное значение в page.
        page = Math.Max(1, page);
        // Записываем вычисленное значение в page.
        ViewBag.Page = page;
        // Записываем вычисленное значение в has next.
        ViewBag.HasNext = await db.Reviews.CountAsync() > page * 20;
        // Передаём данные в Razor-представление и возвращаем HTML.
        return View(await db.Reviews.AsNoTracking().Include(x => x.User).OrderBy(x => x.Status).ThenByDescending(x => x.CreatedAtUtc).Skip((page - 1) * 20).Take(20).ToListAsync());
    }

    // Ограничиваем доступ: роль проверяется сервером до вызова действия.
    [HttpPost, Authorize(Roles = "Admin")]
    // Выполняем действие Moderate после проверки прав и состояния записи.
    public async Task<IActionResult> Moderate(int id, bool approved)
    {
        // Получаем review из базы или подготавливаем запрос к ней.
        var review = await db.Reviews.FindAsync(id);
        // Обрабатываем отсутствие записи или необязательного значения.
        if (review == null)
            // Отвечаем 404, когда запрошенная запись не найдена или недоступна.
            return NotFound();
        // Записываем вычисленное значение в status.
        review.Status = approved ? ReviewStatus.Approved : ReviewStatus.Rejected;
        // Сохраняем накопленные изменения в SQLite.
        await db.SaveChangesAsync();
        // После операции перенаправляем браузер на указанную страницу.
        return RedirectToAction(nameof(Index));
    }
}
