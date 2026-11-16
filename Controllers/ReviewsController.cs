// Читает опубликованные отзывы и сохраняет новые на модерацию.
using Cinema.Data;
using Cinema.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Cinema.Controllers;
public class ReviewsController(CinemaDbContext db) : Controller
{
    // Из SQLite выбираются только опубликованные отзывы, затем передаются в страницу отзывов.
    public async Task<IActionResult> Index(int page = 1)
    {
        page = Math.Max(1, page);
        // Берём из SQLite только отзывы, прошедшие модерацию; черновики посетители не видят.
        var query = db.Reviews.AsNoTracking().Where(x => x.Status == ReviewStatus.Approved);
        ViewBag.Page = page;
        ViewBag.HasNext = await query.CountAsync() > page * 12;

        return View(await query.Include(x => x.User).OrderByDescending(x => x.CreatedAtUtc).Skip((page - 1) * 12).Take(12).ToListAsync());
    }

    [Authorize, HttpPost]
    // Текст и оценка приходят из формы; отзыв связывается с текущим пользователем и сохраняется на модерацию.
    public async Task<IActionResult> Create([Bind("Text,Rating")] Review review)
    {
        // Navigation and owner are server values, never submitted by the browser.
        ModelState.Remove(nameof(Review.User));
        // Проверяем атрибуты модели и ошибки привязки формы; при ошибках снова показываем форму.
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Укажите оценку от 1 до 5 и текст от 5 до 2000 символов.";
            return RedirectToAction(nameof(Index));
        }

        // Берём идентификатор текущего пользователя из подтверждённой cookie входа, а не из формы браузера.
        review.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        review.Status = ReviewStatus.Pending;
        db.Reviews.Add(review);
        // Записываем накопленные изменения сущностей в SQLite.
        await db.SaveChangesAsync();
        TempData["Success"] = "Спасибо! Отзыв отправлен на модерацию.";
        return RedirectToAction(nameof(Index));
    }
}
