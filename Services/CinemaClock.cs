// Преобразует UTC в местное время кинотеатра из конфигурации.
namespace Cinema.Services;
public class CinemaClock(IConfiguration configuration)
{
    public DateTime Now => DateTime.UtcNow.AddHours(configuration.GetValue<int>("Cinema:UtcOffsetHours", 4));

    public DateTime Local(DateTime utc) => utc.AddHours(configuration.GetValue<int>("Cinema:UtcOffsetHours", 4));
}
