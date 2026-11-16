// Меняет статус отзывов после проверки прав администратора.
using Cinema.Data;
using Cinema.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Admin.Controllers;
[Area("Admin"), Authorize(Roles = "Admin,Manager")]
public class ReviewsController(CinemaDbContext db) : Controller
{

    public async Task<IActionResult> Index(int page = 1)
    {
        page = Math.Max(1, page);
        ViewBag.Page = page;
        // Читаем отзывы из SQLite вместе с данными автора и статусом модерации.
        ViewBag.HasNext = await db.Reviews.CountAsync() > page * 20;
        // Читаем отзывы из SQLite вместе с данными автора и статусом модерации.
        return View(await db.Reviews.AsNoTracking().Include(x => x.User).OrderBy(x => x.Status).ThenByDescending(x => x.CreatedAtUtc).Skip((page - 1) * 20).Take(20).ToListAsync());
    }

    [HttpPost, Authorize(Roles = "Admin")]
    // Получает id отзыва и решение из формы, затем меняет его статус в SQLite.
    public async Task<IActionResult> Moderate(int id, bool approved)
    {
        // Берём из SQLite только отзывы, прошедшие модерацию; черновики посетители не видят.
        var review = await db.Reviews.FindAsync(id);
        if (review == null)
            return NotFound();
        review.Status = approved ? ReviewStatus.Approved : ReviewStatus.Rejected;
        // Записываем накопленные изменения сущностей в SQLite.
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }
}
