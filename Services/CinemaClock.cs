// Переводит UTC в местное время кинотеатра.
// Пояснения ниже относятся к следующей строке или смысловому шагу.
// Помещаем объявленные типы в указанное пространство имён.
namespace Cinema.Services;
// Переводит UTC в местное время кинотеатра.
public class CinemaClock(IConfiguration configuration)
{
    // Завершаем действие или объявление текущей строки.
    public DateTime Now => DateTime.UtcNow.AddHours(configuration.GetValue<int>("Cinema:UtcOffsetHours", 4));

    // Объявляем метод Local: параметры приходят из запроса или вызова сервиса.
    public DateTime Local(DateTime utc) => utc.AddHours(configuration.GetValue<int>("Cinema:UtcOffsetHours", 4));
}
