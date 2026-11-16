// Интерфейс оплаты и текущая реализация, которая передаёт заказ в BookingService.
namespace Cinema.Services;
// Training mode deliberately has no card fields and no client-side payment status.
// A real adapter must verify the provider's signed callback / server API first.
public interface IPaymentService
{
    Task PayAsync(int bookingId, string userId);
}

public class TrainingPaymentService(BookingService bookings) : IPaymentService
{
    public Task PayAsync(int bookingId, string userId) => bookings.PayAsync(bookingId, userId);
}
