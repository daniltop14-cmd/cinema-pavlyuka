// Обрабатывает выбор мест, заказ, оплату, отмену и билет.
// Пояснения ниже относятся к следующей строке или смысловому шагу.
// Подключаем пространство имён System.Security.Claims для типов ниже.
using System.Security.Claims;
// Подключаем пространство имён Cinema.Data для типов ниже.
using Cinema.Data;
// Подключаем пространство имён Cinema.Models для типов ниже.
using Cinema.Models;
// Подключаем пространство имён Cinema.Services для типов ниже.
using Cinema.Services;
// Подключаем пространство имён Cinema.ViewModels для типов ниже.
using Cinema.ViewModels;
// Подключаем пространство имён Microsoft.AspNetCore.Authorization для типов ниже.
using Microsoft.AspNetCore.Authorization;
// Подключаем пространство имён Microsoft.AspNetCore.Mvc для типов ниже.
using Microsoft.AspNetCore.Mvc;
// Подключаем пространство имён Microsoft.EntityFrameworkCore для типов ниже.
using Microsoft.EntityFrameworkCore;

// Помещаем объявленные типы в указанное пространство имён.
namespace Cinema.Controllers;
// Ограничиваем доступ: роль проверяется сервером до вызова действия.
[Authorize]
// Обрабатывает выбор мест, заказ, оплату, отмену и билет.
public class BookingController(CinemaDbContext db, BookingService bookings, IPaymentService payments, CinemaClock clock) : Controller
{
    // Завершаем действие или объявление текущей строки.
    private string UserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [AllowAnonymous]
    // Готовим основную страницу этого раздела и возвращаем представление.
    public IActionResult Index() => RedirectToAction("Index", "Movies");
    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [AllowAnonymous]
    // Объявляем метод Seats: параметры приходят из запроса или вызова сервиса.
    public async Task<IActionResult> Seats(int id)
    {
        // Получаем session из базы или подготавливаем запрос к ней.
        var session = await db.Sessions.AsNoTracking().Include(x => x.Movie).Include(x => x.Hall).SingleOrDefaultAsync(x => x.Id == id && !x.IsCancelled && x.StartsAt > clock.Now);
        // Обрабатываем отсутствие записи или необязательного значения.
        if (session == null)
            // Отвечаем 404, когда запрошенная запись не найдена или недоступна.
            return NotFound();
        // Дожидаемся завершения асинхронной операции.
        await bookings.CleanupSessionAsync(id);
        // Передаём данные в Razor-представление и возвращаем HTML.
        return View(new SeatMapModel { Session = session, Seats = await db.Seats.AsNoTracking().Where(x => x.HallId == session.HallId).OrderBy(x => x.Row).ThenBy(x => x.Number).ToListAsync(), Occupied = (await db.BookingSeats.AsNoTracking().Where(x => x.SessionId == id && x.Active == 1).Select(x => x.SeatId).ToListAsync()).ToHashSet() });
    }

    // Разрешаем изменение данных только через POST-запрос.
    [HttpPost]
    // Создаём запись после проверки входных данных и ограничений.
    public async Task<IActionResult> Create(int sessionId, int[] seatIds)
    {
        // Начинаем блок, ошибки которого будут обработаны ниже.
        try
        {
            // Асинхронно получаем id, не блокируя поток сервера.
            var id = await bookings.CreateAsync(UserId, sessionId, seatIds);
            // После операции перенаправляем браузер на указанную страницу.
            return RedirectToAction(nameof(Checkout), new { id });
        }
        // Перехватываем ошибку, чтобы сохранить управляемое поведение.
        catch (BookingException ex)
        {
            // Записываем вычисленное значение в temp data["error"].
            TempData["Error"] = ex.Message;
        }
        // Перехватываем ошибку, чтобы сохранить управляемое поведение.
        catch (DbUpdateException)
        {
            // Записываем вычисленное значение в temp data["error"].
            TempData["Error"] = "Не удалось закрепить места. Обновите схему и попробуйте снова.";
        }

        // После операции перенаправляем браузер на указанную страницу.
        return RedirectToAction(nameof(Seats), new { id = sessionId });
    }

    // Объявляем метод Checkout: параметры приходят из запроса или вызова сервиса.
    public async Task<IActionResult> Checkout(int id)
    {
        // Получаем session id из базы или подготавливаем запрос к ней.
        var sessionId = await db.Bookings.AsNoTracking().Where(x => x.Id == id && x.UserId == UserId).Select(x => (int? )x.SessionId).SingleOrDefaultAsync();
        // Обрабатываем отсутствие записи или необязательного значения.
        if (sessionId == null)
            // Отвечаем 404, когда запрошенная запись не найдена или недоступна.
            return NotFound();
        // Дожидаемся завершения асинхронной операции.
        await bookings.CleanupSessionAsync(sessionId.Value);
        // Асинхронно получаем booking, не блокируя поток сервера.
        var booking = await OrderQuery().SingleAsync(x => x.Id == id && x.UserId == UserId);
        // Передаём данные в Razor-представление и возвращаем HTML.
        return View(booking);
    }

    // Разрешаем изменение данных только через POST-запрос.
    [HttpPost]
    // Выполняем действие Pay после проверки прав и состояния записи.
    public async Task<IActionResult> Pay(int id)
    {
        // Начинаем блок, ошибки которого будут обработаны ниже.
        try
        {
            // Дожидаемся завершения асинхронной операции.
            await payments.PayAsync(id, UserId);
            // Записываем вычисленное значение в temp data["success"].
            TempData["Success"] = "Оплата выполнена. Ваши билеты готовы!";
            // После операции перенаправляем браузер на указанную страницу.
            return RedirectToAction(nameof(Ticket), new { id });
        }
        // Перехватываем ошибку, чтобы сохранить управляемое поведение.
        catch (BookingException ex)
        {
            // Записываем вычисленное значение в temp data["error"].
            TempData["Error"] = ex.Message;
            // После операции перенаправляем браузер на указанную страницу.
            return RedirectToAction(nameof(Checkout), new { id });
        }
    }

    // Разрешаем изменение данных только через POST-запрос.
    [HttpPost]
    // Выполняем действие Cancel после проверки прав и состояния записи.
    public async Task<IActionResult> Cancel(int id)
    {
        // Начинаем блок, ошибки которого будут обработаны ниже.
        try
        {
            // Дожидаемся завершения асинхронной операции.
            await bookings.CancelAsync(id, UserId);
            // Записываем вычисленное значение в temp data["success"].
            TempData["Success"] = "Бронирование отменено.";
        }
        // Перехватываем ошибку, чтобы сохранить управляемое поведение.
        catch (BookingException ex)
        {
            // Записываем вычисленное значение в temp data["error"].
            TempData["Error"] = ex.Message;
        }

        // После операции перенаправляем браузер на указанную страницу.
        return RedirectToAction("Orders", "Account");
    }

    // Объявляем метод Ticket: параметры приходят из запроса или вызова сервиса.
    public async Task<IActionResult> Ticket(int id)
    {
        // Асинхронно получаем booking, не блокируя поток сервера.
        var booking = await OrderQuery().SingleOrDefaultAsync(x => x.Id == id && x.UserId == UserId && x.Status == BookingStatus.Paid);
        // Возвращаем результат вызывающему коду.
        return booking == null ? NotFound() : View(booking);
    }

    // Объявляем метод OrderQuery: параметры приходят из запроса или вызова сервиса.
    private IQueryable<Booking> OrderQuery() => db.Bookings.AsNoTracking().Include(x => x.Session).ThenInclude(x => x.Movie).Include(x => x.Session).ThenInclude(x => x.Hall).Include(x => x.BookingSeats).ThenInclude(x => x.Seat);
}
