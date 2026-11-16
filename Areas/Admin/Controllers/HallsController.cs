// Меняет зал и добавляет кресла, сохраняя связи уже оформленных заказов.
using Cinema.Data;
using Cinema.Models;
using Cinema.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Areas.Admin.Controllers;
[Area("Admin"), Authorize(Roles = "Admin")]
public class HallsController(CinemaDbContext db, BookingService bookings) : Controller
{

    public async Task<IActionResult> Index() => View(await db.Halls.AsNoTracking().ToListAsync());
    [HttpGet]
    // GET берёт зал и кресла из SQLite; POST получает поля формы, проверяет изменение схемы и сохраняет новые места.
    public async Task<IActionResult> Edit(int id)
    {
        // Загружаем описание залов из SQLite для вывода страницы или списка выбора.
        var hall = await db.Halls.AsNoTracking().Include(x => x.Seats).SingleOrDefaultAsync(x => x.Id == id);
        return hall == null ? NotFound() : View(hall);
    }

    [HttpPost]
    // GET берёт зал и кресла из SQLite; POST получает поля формы, проверяет изменение схемы и сохраняет новые места.
    public async Task<IActionResult> Edit(int id, [Bind("Name,Description,Rows,SeatsPerRow,Color")] Hall input, int[] disabledSeats)
    {
        if (!new[]
        {
            "blue",
            "red",
            "green",
            "orange"
        }.Contains(input.Color))
            ModelState.AddModelError("", "Выберите допустимый цвет.");
        // Транзакция не позволяет одновременно изменить схему зала и оформить билет на её кресло.
        await using var tx = await db.Database.BeginTransactionAsync();
        // id зала берётся из URL формы; загружаем текущую запись, которую будем обновлять.
        var hall = await db.Halls.SingleOrDefaultAsync(x => x.Id == id);
        if (hall == null)
            return NotFound();
        // Запоминаем существующие кресла и их ID: уже выданные билеты должны ссылаться на те же места.
        var seats = await db.Seats.Where(x => x.HallId == id).ToListAsync();
        // The SQLite write transaction already serializes seat changes and sales.
        var sessions = await db.Sessions.Where(x => x.HallId == id).OrderBy(x => x.Id).Select(x => x.Id).ToListAsync();
        foreach (var sessionId in sessions)
            await bookings.GetSessionAsync(sessionId);
        var removedSeats = seats.Where(x => x.Row > input.Rows || x.Number > input.SeatsPerRow).ToList();
        var removedIds = removedSeats.Select(x => x.Id).ToArray();
        // Уменьшение зала запрещено, если удаляемое кресло встречается хотя бы в одном старом заказе.
        if (removedIds.Length > 0 && await db.BookingSeats.AnyAsync(x => removedIds.Contains(x.SeatId)))
            ModelState.AddModelError("", "Нельзя удалить кресла, по которым есть история заказов. Увеличить зал или отключить такие кресла можно.");
        // Отключить кресло с действующей бронью или билетом нельзя: Active=1 означает занятую связь.
        if (await db.BookingSeats.AnyAsync(x => disabledSeats.Contains(x.SeatId) && x.Active == 1 && x.Seat.HallId == id))
            ModelState.AddModelError("", "Нельзя отключить место с действующей бронью или билетом.");
        // Проверяем атрибуты модели и ошибки привязки формы; при ошибках снова показываем форму.
        if (!ModelState.IsValid)
        {
            input.Id = id;
            input.Seats = seats;

            return View(input);
        }

        hall.Name = input.Name;
        hall.Description = input.Description;
        hall.Color = input.Color;
        // Удаляем только свободные кресла за пределами новой схемы; для расширения создаём недостающие координаты.
        db.Seats.RemoveRange(removedSeats);
        var existing = seats.Except(removedSeats).ToDictionary(x => (x.Row, x.Number));
        for (var row = 1; row <= input.Rows; row++)
            for (var number = 1; number <= input.SeatsPerRow; number++)
                if (!existing.ContainsKey((row, number)))
                    db.Seats.Add(new Seat { HallId = id, Row = row, Number = number });
        foreach (var seat in existing.Values)
            seat.IsAccessible = !disabledSeats.Contains(seat.Id);
        hall.Rows = input.Rows;
        hall.SeatsPerRow = input.SeatsPerRow;
        // Записываем накопленные изменения сущностей в SQLite.
        await db.SaveChangesAsync();
        // Подтверждаем транзакцию: связанные изменения заказа и мест сохраняются вместе.
        await tx.CommitAsync();
        TempData["Success"] = "Зал сохранён.";
        return RedirectToAction(nameof(Index));
    }
}
