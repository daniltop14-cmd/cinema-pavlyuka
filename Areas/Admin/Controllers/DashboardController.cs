// Собирает сводку панели управления.
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
[Area("Admin"), Authorize(Roles = "Admin,Manager")]
// Собирает сводку панели управления.
public class DashboardController(CinemaDbContext db) : Controller
{
    // Готовим основную страницу этого раздела и возвращаем представление.
    public async Task<IActionResult> Index()
    {
        // SQLite EF Core 8 does not aggregate decimal; calculate exactly in .NET.
        // Получаем amounts из базы или подготавливаем запрос к ней.
        var amounts = await db.Payments.AsNoTracking().Where(x => !x.IsRefunded)
            // Выполняем запрос к SQLite асинхронно.
            .Select(x => x.Amount).ToListAsync();
        // Передаём данные в Razor-представление и возвращаем HTML.
        return View(new DashboardModel
        {
            // Записываем вычисленное значение в users.
            Users = await db.Users.CountAsync(), Movies = await db.Movies.CountAsync(),
            // Записываем вычисленное значение в sessions.
            Sessions = await db.Sessions.CountAsync(),
            // Записываем вычисленное значение в sold tickets.
            SoldTickets = await db.BookingSeats.CountAsync(x => x.Booking.Status == BookingStatus.Paid),
            // Записываем вычисленное значение в revenue.
            Revenue = amounts.Sum(),
            // Загружаем связанную сущность вместе с основной записью.
            Recent = await db.Bookings.AsNoTracking().Include(x => x.User).Include(x => x.Session)
                // Упорядочиваем записи перед выводом.
                .ThenInclude(x => x.Movie).OrderByDescending(x => x.CreatedAtUtc).Take(8).ToListAsync()
        });
    }
}
