// Освобождает места после истечения срока неоплаченной брони.
// Пояснения ниже относятся к следующей строке или смысловому шагу.
// Подключаем пространство имён Cinema.Data для типов ниже.
using Cinema.Data;
// Подключаем пространство имён Cinema.Models для типов ниже.
using Cinema.Models;
// Подключаем пространство имён Microsoft.EntityFrameworkCore для типов ниже.
using Microsoft.EntityFrameworkCore;

// Помещаем объявленные типы в указанное пространство имён.
namespace Cinema.Services;
// Освобождает места после истечения срока неоплаченной брони.
public class BookingCleanupService(IServiceScopeFactory scopes, ILogger<BookingCleanupService> logger) : BackgroundService
{
    // Объявляем метод ExecuteAsync: параметры приходят из запроса или вызова сервиса.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Создаём таймер, который запускает очистку просроченных броней каждые 30 секунд.
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        // Повторяем действие, пока условие остаётся истинным.
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            // Начинаем блок, ошибки которого будут обработаны ниже.
            try
            {
                // Создаём отдельную область зависимостей для очередного цикла очистки.
                using var scope = scopes.CreateScope();
                // Вычисляем db для дальнейших проверок или отображения.
                var db = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();
                // Вычисляем service для дальнейших проверок или отображения.
                var service = scope.ServiceProvider.GetRequiredService<BookingService>();
                // Получаем ids из базы или подготавливаем запрос к ней.
                var ids = await db.Bookings.AsNoTracking().Where(x => x.Status == BookingStatus.Pending && x.ExpiresAtUtc <= DateTime.UtcNow).Select(x => x.SessionId).Distinct().ToListAsync(stoppingToken);
                // Последовательно обрабатываем каждый элемент коллекции.
                foreach (var id in ids)
                    // Дожидаемся завершения асинхронной операции.
                    await service.CleanupSessionAsync(id);
            }
            // Перехватываем ошибку, чтобы сохранить управляемое поведение.
            catch (Exception ex)when (!stoppingToken.IsCancellationRequested)
            {
                // Завершаем действие или объявление текущей строки.
                logger.LogError(ex, "Не удалось освободить истёкшие брони");
            }
        }
    }
}
