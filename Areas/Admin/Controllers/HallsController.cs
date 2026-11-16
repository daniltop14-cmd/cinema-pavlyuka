// Меняет параметры залов, сохраняя ID существующих кресел.
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
[Area("Admin"), Authorize(Roles = "Admin")]
// Меняет параметры залов, сохраняя ID существующих кресел.
public class HallsController(CinemaDbContext db, BookingService bookings) : Controller
{
    // Готовим основную страницу этого раздела и возвращаем представление.
    public async Task<IActionResult> Index() => View(await db.Halls.AsNoTracking().ToListAsync());
    // Назначаем адрес или тип GET-запроса для этого действия.
    [HttpGet]
    // Показываем или сохраняем форму редактирования записи.
    public async Task<IActionResult> Edit(int id)
    {
        // Получаем hall из базы или подготавливаем запрос к ней.
        var hall = await db.Halls.AsNoTracking().Include(x => x.Seats).SingleOrDefaultAsync(x => x.Id == id);
        // Возвращаем результат вызывающему коду.
        return hall == null ? NotFound() : View(hall);
    }

    // Разрешаем изменение данных только через POST-запрос.
    [HttpPost]
    // Показываем или сохраняем форму редактирования записи.
    public async Task<IActionResult> Edit(int id, [Bind("Name,Description,Rows,SeatsPerRow,Color")] Hall input, int[] disabledSeats)
    {
        // Выполняем следующую ветку только при истинности условия.
        if (!new[]
        {
            // Передаём очередной аргумент или задаём элемент коллекции.
            "blue",
            // Передаём очередной аргумент или задаём элемент коллекции.
            "red",
            // Передаём очередной аргумент или задаём элемент коллекции.
            "green",
            "orange"
        }.Contains(input.Color))
            // Завершаем действие или объявление текущей строки.
            ModelState.AddModelError("", "Выберите допустимый цвет.");
        // Начинаем транзакцию до чтения и изменения связанных данных.
        await using var tx = await db.Database.BeginTransactionAsync();
        // Получаем hall из базы или подготавливаем запрос к ней.
        var hall = await db.Halls.SingleOrDefaultAsync(x => x.Id == id);
        // Обрабатываем отсутствие записи или необязательного значения.
        if (hall == null)
            // Отвечаем 404, когда запрошенная запись не найдена или недоступна.
            return NotFound();
        // Получаем seats из базы или подготавливаем запрос к ней.
        var seats = await db.Seats.Where(x => x.HallId == id).ToListAsync();
        // The SQLite write transaction already serializes seat changes and sales.
        // Получаем sessions из базы или подготавливаем запрос к ней.
        var sessions = await db.Sessions.Where(x => x.HallId == id).OrderBy(x => x.Id).Select(x => x.Id).ToListAsync();
        // Последовательно обрабатываем каждый элемент коллекции.
        foreach (var sessionId in sessions)
            // Дожидаемся завершения асинхронной операции.
            await bookings.GetSessionAsync(sessionId);
        // Вычисляем removed seats для дальнейших проверок или отображения.
        var removedSeats = seats.Where(x => x.Row > input.Rows || x.Number > input.SeatsPerRow).ToList();
        // Вычисляем removed ids для дальнейших проверок или отображения.
        var removedIds = removedSeats.Select(x => x.Id).ToArray();
        // Выполняем следующую ветку только при истинности условия.
        if (removedIds.Length > 0 && await db.BookingSeats.AnyAsync(x => removedIds.Contains(x.SeatId)))
            // Завершаем действие или объявление текущей строки.
            ModelState.AddModelError("", "Нельзя удалить кресла, по которым есть история заказов. Увеличить зал или отключить такие кресла можно.");
        // Выполняем следующую ветку только при истинности условия.
        if (await db.BookingSeats.AnyAsync(x => disabledSeats.Contains(x.SeatId) && x.Active == 1 && x.Seat.HallId == id))
            // Завершаем действие или объявление текущей строки.
            ModelState.AddModelError("", "Нельзя отключить место с действующей бронью или билетом.");
        // Если форма содержит ошибки проверки, не сохраняем данные.
        if (!ModelState.IsValid)
        {
            // Записываем вычисленное значение в id.
            input.Id = id;
            // Записываем вычисленное значение в seats.
            input.Seats = seats;
            // Передаём данные в Razor-представление и возвращаем HTML.
            return View(input);
        }

        // Записываем вычисленное значение в name.
        hall.Name = input.Name;
        // Записываем вычисленное значение в description.
        hall.Description = input.Description;
        // Записываем вычисленное значение в color.
        hall.Color = input.Color;
        // Помечаем сущность на удаление при сохранении контекста.
        db.Seats.RemoveRange(removedSeats);
        // Вычисляем existing для дальнейших проверок или отображения.
        var existing = seats.Except(removedSeats).ToDictionary(x => (x.Row, x.Number));
        // Повторяем следующий блок для заданного диапазона значений.
        for (var row = 1; row <= input.Rows; row++)
            // Повторяем следующий блок для заданного диапазона значений.
            for (var number = 1; number <= input.SeatsPerRow; number++)
                // Выполняем следующую ветку только при истинности условия.
                if (!existing.ContainsKey((row, number)))
                    // Добавляем новую сущность в контекст для последующего сохранения.
                    db.Seats.Add(new Seat { HallId = id, Row = row, Number = number });
        // Последовательно обрабатываем каждый элемент коллекции.
        foreach (var seat in existing.Values)
            // Записываем вычисленное значение в is accessible.
            seat.IsAccessible = !disabledSeats.Contains(seat.Id);
        // Записываем вычисленное значение в rows.
        hall.Rows = input.Rows;
        // Записываем вычисленное значение в seats per row.
        hall.SeatsPerRow = input.SeatsPerRow;
        // Сохраняем накопленные изменения в SQLite.
        await db.SaveChangesAsync();
        // Подтверждаем транзакцию: изменения становятся постоянными.
        await tx.CommitAsync();
        // Записываем вычисленное значение в temp data["success"].
        TempData["Success"] = "Зал сохранён.";
        // После операции перенаправляем браузер на указанную страницу.
        return RedirectToAction(nameof(Index));
    }
}
