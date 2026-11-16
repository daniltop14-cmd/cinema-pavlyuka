// Отделяет вызов оплаты от её конкретной реализации.
// Пояснения ниже относятся к следующей строке или смысловому шагу.
// Помещаем объявленные типы в указанное пространство имён.
namespace Cinema.Services;
// Training mode deliberately has no card fields and no client-side payment status.
// A real adapter must verify the provider's signed callback / server API first.
// Отделяет вызов оплаты от её конкретной реализации.
public interface IPaymentService
{
    // Завершаем действие или объявление текущей строки.
    Task PayAsync(int bookingId, string userId);
}

// Отделяет вызов оплаты от её конкретной реализации.
public class TrainingPaymentService(BookingService bookings) : IPaymentService
{
    // Объявляем метод PayAsync: параметры приходят из запроса или вызова сервиса.
    public Task PayAsync(int bookingId, string userId) => bookings.PayAsync(bookingId, userId);
}
