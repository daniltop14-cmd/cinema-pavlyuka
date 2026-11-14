using Cinema.Data;
using Cinema.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Cinema.Controllers;
public class ReviewsController(CinemaDbContext db) : Controller
{
    public async Task<IActionResult> Index(int page = 1)
    {
        page = Math.Max(1, page);
        var query = db.Reviews.AsNoTracking().Where(x => x.Status == ReviewStatus.Approved);
        ViewBag.Page = page;
        ViewBag.HasNext = await query.CountAsync() > page * 12;
        return View(await query.Include(x => x.User).OrderByDescending(x => x.CreatedAtUtc).Skip((page - 1) * 12).Take(12).ToListAsync());
    }

    [Authorize, HttpPost]
    public async Task<IActionResult> Create([Bind("Text,Rating")] Review review)
    {
        // Navigation and owner are server values, never submitted by the browser.
        ModelState.Remove(nameof(Review.User));
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Укажите оценку от 1 до 5 и текст от 5 до 2000 символов.";
            return RedirectToAction(nameof(Index));
        }

        review.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        review.Status = ReviewStatus.Pending;
        db.Reviews.Add(review);
        await db.SaveChangesAsync();
        TempData["Success"] = "Спасибо! Отзыв отправлен на модерацию.";
        return RedirectToAction(nameof(Index));
    }
}
