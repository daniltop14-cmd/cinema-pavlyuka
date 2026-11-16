// Позволяет управлять заказами и возвратами.
// Пояснения ниже относятся к следующей строке или смысловому шагу.
// Подключаем пространство имён Cinema.Data для типов ниже.
using Cinema.Data;
// Подключаем пространство имён Cinema.Models для типов ниже.
using Cinema.Models;
// Подключаем пространство имён Cinema.Services для типов ниже.
using Cinema.Services;
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
// Позволяет управлять заказами и возвратами.
public class BookingsController(CinemaDbContext db, BookingService bookings, IPaymentService payments) : Controller
{
    // Готовим основную страницу этого раздела и возвращаем представление.
    public async Task<IActionResult> Index(string? search, BookingStatus? status, int page = 1)
    {
        // Получаем query из базы или подготавливаем запрос к ней.
        var query = db.Bookings.AsNoTracking().Include(x => x.User).Include(x => x.Session).ThenInclude(x => x.Movie).Include(x => x.Session).ThenInclude(x => x.Hall).Include(x => x.BookingSeats).ThenInclude(x => x.Seat).Include(x => x.Payment).AsQueryable();
        // Выполняем следующую ветку только при истинности условия.
        if (!string.IsNullOrWhiteSpace(search))
            // Оставляем в выборке только записи, подходящие под условие.
            query = query.Where(x => x.User.Email!.Contains(search) || x.Session.Movie.Title.Contains(search) || x.Id.ToString() == search);
        // Обрабатываем отсутствие записи или необязательного значения.
        if (status != null)
            // Оставляем в выборке только записи, подходящие под условие.
            query = query.Where(x => x.Status == status);
        // Записываем вычисленное значение в page.
        page = Math.Max(1, page);
        // Записываем вычисленное значение в page.
        ViewBag.Page = page;
        // Записываем вычисленное значение в has next.
        ViewBag.HasNext = await query.CountAsync() > page * 20;
        // Записываем вычисленное значение в search.
        ViewBag.Search = search;
        // Записываем вычисленное значение в status.
        ViewBag.Status = status;
        // Передаём данные в Razor-представление и возвращаем HTML.
        return View(await query.OrderByDescending(x => x.CreatedAtUtc).Skip((page - 1) * 20).Take(20).ToListAsync());
    }

    // Разрешаем изменение данных только через POST-запрос.
    [HttpPost]
    // Выполняем действие Confirm после проверки прав и состояния записи.
    public async Task<IActionResult> Confirm(int id)
    {
        // Получаем owner из базы или подготавливаем запрос к ней.
        var owner = await db.Bookings.AsNoTracking().Where(x => x.Id == id).Select(x => x.UserId).SingleOrDefaultAsync();
        // Обрабатываем отсутствие записи или необязательного значения.
        if (owner == null)
            // Отвечаем 404, когда запрошенная запись не найдена или недоступна.
            return NotFound();
        // Начинаем блок, ошибки которого будут обработаны ниже.
        try
        {
            // Дожидаемся завершения асинхронной операции.
            await payments.PayAsync(id, owner);
            // Записываем вычисленное значение в temp data["success"].
            TempData["Success"] = "Заказ подтверждён и оплачен.";
        }
        // Перехватываем ошибку, чтобы сохранить управляемое поведение.
        catch (BookingException ex)
        {
            // Записываем вычисленное значение в temp data["error"].
            TempData["Error"] = ex.Message;
        }

        // После операции перенаправляем браузер на указанную страницу.
        return RedirectToAction(nameof(Index));
    }

    // Разрешаем изменение данных только через POST-запрос.
    [HttpPost]
    // Выполняем действие Cancel после проверки прав и состояния записи.
    public async Task<IActionResult> Cancel(int id, bool refund = false)
    {
        // Начинаем блок, ошибки которого будут обработаны ниже.
        try
        {
            // Дожидаемся завершения асинхронной операции.
            await bookings.CancelAsync(id, null, refund);
            // Записываем вычисленное значение в temp data["success"].
            TempData["Success"] = refund ? "Возврат выполнен." : "Бронь отменена.";
        }
        // Перехватываем ошибку, чтобы сохранить управляемое поведение.
        catch (BookingException ex)
        {
            // Записываем вычисленное значение в temp data["error"].
            TempData["Error"] = ex.Message;
        }

        // После операции перенаправляем браузер на указанную страницу.
        return RedirectToAction(nameof(Index));
    }
}
