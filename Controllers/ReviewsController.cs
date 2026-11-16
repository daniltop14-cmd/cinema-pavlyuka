// Показывает отзывы и принимает новые на модерацию.
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
// Подключаем пространство имён System.Security.Claims для типов ниже.
using System.Security.Claims;

// Помещаем объявленные типы в указанное пространство имён.
namespace Cinema.Controllers;
// Показывает отзывы и принимает новые на модерацию.
public class ReviewsController(CinemaDbContext db) : Controller
{
    // Готовим основную страницу этого раздела и возвращаем представление.
    public async Task<IActionResult> Index(int page = 1)
    {
        // Записываем вычисленное значение в page.
        page = Math.Max(1, page);
        // Получаем query из базы или подготавливаем запрос к ней.
        var query = db.Reviews.AsNoTracking().Where(x => x.Status == ReviewStatus.Approved);
        // Записываем вычисленное значение в page.
        ViewBag.Page = page;
        // Записываем вычисленное значение в has next.
        ViewBag.HasNext = await query.CountAsync() > page * 12;
        // Передаём данные в Razor-представление и возвращаем HTML.
        return View(await query.Include(x => x.User).OrderByDescending(x => x.CreatedAtUtc).Skip((page - 1) * 12).Take(12).ToListAsync());
    }

    // Ограничиваем доступ: роль проверяется сервером до вызова действия.
    [Authorize, HttpPost]
    // Создаём запись после проверки входных данных и ограничений.
    public async Task<IActionResult> Create([Bind("Text,Rating")] Review review)
    {
        // Navigation and owner are server values, never submitted by the browser.
        // Завершаем действие или объявление текущей строки.
        ModelState.Remove(nameof(Review.User));
        // Если форма содержит ошибки проверки, не сохраняем данные.
        if (!ModelState.IsValid)
        {
            // Записываем вычисленное значение в temp data["error"].
            TempData["Error"] = "Укажите оценку от 1 до 5 и текст от 5 до 2000 символов.";
            // После операции перенаправляем браузер на указанную страницу.
            return RedirectToAction(nameof(Index));
        }

        // Записываем вычисленное значение в user id.
        review.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        // Записываем вычисленное значение в status.
        review.Status = ReviewStatus.Pending;
        // Добавляем новую сущность в контекст для последующего сохранения.
        db.Reviews.Add(review);
        // Сохраняем накопленные изменения в SQLite.
        await db.SaveChangesAsync();
        // Записываем вычисленное значение в temp data["success"].
        TempData["Success"] = "Спасибо! Отзыв отправлен на модерацию.";
        // После операции перенаправляем браузер на указанную страницу.
        return RedirectToAction(nameof(Index));
    }
}
