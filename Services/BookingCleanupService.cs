// Фоновая задача периодически освобождает места с истёкшей неоплаченной бронью.
using Cinema.Data;
using Cinema.Models;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Services;
public class BookingCleanupService(IServiceScopeFactory scopes, ILogger<BookingCleanupService> logger) : BackgroundService
{
    // Каждые 30 секунд создаёт область зависимостей и через BookingService проверяет истёкшие брони.
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                using var scope = scopes.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<CinemaDbContext>();
                var service = scope.ServiceProvider.GetRequiredService<BookingService>();
                // Читаем заказы из SQLite; статус и дата используются для отбора и вывода.
                var ids = await db.Bookings.AsNoTracking().Where(x => x.Status == BookingStatus.Pending && x.ExpiresAtUtc <= DateTime.UtcNow).Select(x => x.SessionId).Distinct().ToListAsync(stoppingToken);
                foreach (var id in ids)
                    await service.CleanupSessionAsync(id);
            }
            catch (Exception ex)when (!stoppingToken.IsCancellationRequested)
            {
                logger.LogError(ex, "Не удалось освободить истёкшие брони");
            }
        }
    }
}
