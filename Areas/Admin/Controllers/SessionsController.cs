// Создаёт и меняет сеансы с проверкой пересечений.
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
// Подключаем пространство имён Microsoft.AspNetCore.Mvc.Rendering для типов ниже.
using Microsoft.AspNetCore.Mvc.Rendering;
// Подключаем пространство имён Microsoft.EntityFrameworkCore для типов ниже.
using Microsoft.EntityFrameworkCore;

// Помещаем объявленные типы в указанное пространство имён.
namespace Cinema.Areas.Admin.Controllers;
// Ограничиваем доступ: роль проверяется сервером до вызова действия.
[Area("Admin"), Authorize(Roles = "Admin")]
// Создаёт и меняет сеансы с проверкой пересечений.
public class SessionsController(CinemaDbContext db, CinemaClock clock, BookingService bookings) : Controller
{
    // Готовим основную страницу этого раздела и возвращаем представление.
    public async Task<IActionResult> Index(int page = 1)
    {
        // Записываем вычисленное значение в page.
        page = Math.Max(1, page);
        // Записываем вычисленное значение в page.
        ViewBag.Page = page;
        // Записываем вычисленное значение в has next.
        ViewBag.HasNext = await db.Sessions.CountAsync() > page * 30;
        // Передаём данные в Razor-представление и возвращаем HTML.
        return View(await db.Sessions.AsNoTracking().Include(x => x.Movie).Include(x => x.Hall).OrderBy(x => x.StartsAt).Skip((page - 1) * 30).Take(30).ToListAsync());
    }

    // Объявляем метод Lists: параметры приходят из запроса или вызова сервиса.
    private async Task Lists()
    {
        // Упорядочиваем записи перед выводом.
        ViewBag.Movies = new SelectList(await db.Movies.AsNoTracking().OrderBy(x => x.Title).ToListAsync(), "Id", "Title");
        // Выполняем запрос к SQLite асинхронно.
        ViewBag.Halls = new SelectList(await db.Halls.AsNoTracking().ToListAsync(), "Id", "Name");
    }

    // Назначаем адрес или тип GET-запроса для этого действия.
    [HttpGet]
    // Показываем или сохраняем форму редактирования записи.
    public async Task<IActionResult> Edit(int? id)
    {
        // Дожидаемся завершения асинхронной операции.
        await Lists();
        // Обрабатываем отсутствие записи или необязательного значения.
        if (id == null)
            // Передаём данные в Razor-представление и возвращаем HTML.
            return View(new Session { StartsAt = clock.Now.Date.AddDays(1).AddHours(12), Price = 450 });
        // Получаем session из базы или подготавливаем запрос к ней.
        var session = await db.Sessions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        // Возвращаем результат вызывающему коду.
        return session == null ? NotFound() : View(session);
    }

    // Разрешаем изменение данных только через POST-запрос.
    [HttpPost]
    // Показываем или сохраняем форму редактирования записи.
    public async Task<IActionResult> Edit(int id, [Bind("MovieId,HallId,StartsAt,Price")] Session input)
    {
        // Завершаем действие или объявление текущей строки.
        ModelState.Remove(nameof(Session.Movie));
        // Завершаем действие или объявление текущей строки.
        ModelState.Remove(nameof(Session.Hall));
        // Получаем movie из базы или подготавливаем запрос к ней.
        var movie = await db.Movies.AsNoTracking().SingleOrDefaultAsync(x => x.Id == input.MovieId);
        // Обрабатываем отсутствие записи или необязательного значения.
        if (movie == null)
            // Завершаем действие или объявление текущей строки.
            ModelState.AddModelError("", "Выберите существующий фильм.");
        // Выполняем следующую ветку только при истинности условия.
        if (input.StartsAt <= clock.Now)
            // Завершаем действие или объявление текущей строки.
            ModelState.AddModelError("", "Укажите будущее время сеанса.");
        // Если форма содержит ошибки проверки, не сохраняем данные.
        if (!ModelState.IsValid)
        {
            // Записываем вычисленное значение в id.
            input.Id = id;
            // Дожидаемся завершения асинхронной операции.
            await Lists();
            // Передаём данные в Razor-представление и возвращаем HTML.
            return View(input);
        }

        // Начинаем транзакцию до чтения и изменения связанных данных.
        await using var tx = await db.Database.BeginTransactionAsync();
        // The immediate SQLite transaction prevents concurrent overlapping edits.
        // Получаем hall из базы или подготавливаем запрос к ней.
        var hall = await db.Halls.SingleOrDefaultAsync(x => x.Id == input.HallId);
        // Обрабатываем отсутствие записи или необязательного значения.
        if (hall == null)
            // Отвечаем 404, когда запрошенная запись не найдена или недоступна.
            return NotFound();
        // Асинхронно получаем session, не блокируя поток сервера.
        var session = id == 0 ? new Session() : await bookings.GetSessionAsync(id);
        // Выполняем следующую ветку только при истинности условия.
        if (id != 0 && session.HallId != input.HallId)
            // Завершаем действие или объявление текущей строки.
            ModelState.AddModelError("", "Для смены зала удалите сеанс без заказов и создайте новый.");
        // Выполняем следующую ветку только при истинности условия.
        if (id != 0 && await db.Bookings.AnyAsync(x => x.SessionId == id))
            // Завершаем действие или объявление текущей строки.
            ModelState.AddModelError("", "Сеанс с заказами нельзя изменять.");
        // Вычисляем end для дальнейших проверок или отображения.
        var end = input.StartsAt.AddMinutes(movie!.DurationMinutes + 20);
        // Выполняем следующую ветку только при истинности условия.
        if (await db.Sessions.AnyAsync(x => x.Id != id && x.HallId == input.HallId && !x.IsCancelled && x.StartsAt < end && x.EndsAt > input.StartsAt))
            // Завершаем действие или объявление текущей строки.
            ModelState.AddModelError("", "Сеанс пересекается с другим показом. Учтите 20 минут на уборку.");
        // Если форма содержит ошибки проверки, не сохраняем данные.
        if (!ModelState.IsValid)
        {
            // Записываем вычисленное значение в id.
            input.Id = id;
            // Дожидаемся завершения асинхронной операции.
            await Lists();
            // Передаём данные в Razor-представление и возвращаем HTML.
            return View(input);
        }

        // Записываем вычисленное значение в movie id.
        session.MovieId = input.MovieId;
        // Записываем вычисленное значение в hall id.
        session.HallId = input.HallId;
        // Записываем вычисленное значение в starts at.
        session.StartsAt = input.StartsAt;
        // Записываем вычисленное значение в ends at.
        session.EndsAt = end;
        // Записываем вычисленное значение в price.
        session.Price = input.Price;
        // Выполняем следующую ветку только при истинности условия.
        if (id == 0)
            // Добавляем новую сущность в контекст для последующего сохранения.
            db.Sessions.Add(session);
        // Сохраняем накопленные изменения в SQLite.
        await db.SaveChangesAsync();
        // Подтверждаем транзакцию: изменения становятся постоянными.
        await tx.CommitAsync();
        // Записываем вычисленное значение в temp data["success"].
        TempData["Success"] = "Сеанс сохранён.";
        // После операции перенаправляем браузер на указанную страницу.
        return RedirectToAction(nameof(Index));
    }

    // Разрешаем изменение данных только через POST-запрос.
    [HttpPost]
    // Выполняем действие Delete после проверки прав и состояния записи.
    public async Task<IActionResult> Delete(int id)
    {
        // Начинаем транзакцию до чтения и изменения связанных данных.
        await using var tx = await db.Database.BeginTransactionAsync();
        // Асинхронно получаем session, не блокируя поток сервера.
        var session = await bookings.GetSessionAsync(id);
        // Выполняем следующую ветку только при истинности условия.
        if (await db.Bookings.AnyAsync(x => x.SessionId == id))
            // Записываем вычисленное значение в temp data["error"].
            TempData["Error"] = "Удаление сеанса с заказами запрещено. История билетов должна сохраняться.";
        // Обрабатываем альтернативный случай предыдущей проверки.
        else
        {
            // Помечаем сущность на удаление при сохранении контекста.
            db.Sessions.Remove(session);
            // Сохраняем накопленные изменения в SQLite.
            await db.SaveChangesAsync();
            // Подтверждаем транзакцию: изменения становятся постоянными.
            await tx.CommitAsync();
        }

        // После операции перенаправляем браузер на указанную страницу.
        return RedirectToAction(nameof(Index));
    }
}
