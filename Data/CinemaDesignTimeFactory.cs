// Создаёт DbContext для команд миграции без запуска сайта.
// Пояснения ниже относятся к следующей строке или смысловому шагу.
// Подключаем пространство имён Microsoft.EntityFrameworkCore для типов ниже.
using Microsoft.EntityFrameworkCore;
// Подключаем пространство имён Microsoft.EntityFrameworkCore.Design для типов ниже.
using Microsoft.EntityFrameworkCore.Design;

// Помещаем объявленные типы в указанное пространство имён.
namespace Cinema.Data;

// EF CLI creates migrations without starting the web app or filling its database.
// Создаёт DbContext для команд миграции без запуска сайта.
public class CinemaDesignTimeFactory : IDesignTimeDbContextFactory<CinemaDbContext>
{
    // Объявляем метод CreateDbContext: параметры приходят из запроса или вызова сервиса.
    public CinemaDbContext CreateDbContext(string[] args)
    {
        // Завершаем действие или объявление текущей строки.
        Directory.CreateDirectory("App_Data");
        // Вычисляем connection для дальнейших проверок или отображения.
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            // Продолжаем выражение, начатое на предыдущей строке.
            ?? "Data Source=App_Data/cinema.db;Default Timeout=30;Foreign Keys=True";
        // Возвращаем результат вызывающему коду.
        return new CinemaDbContext(new DbContextOptionsBuilder<CinemaDbContext>().UseSqlite(connection).Options);
    }
}
