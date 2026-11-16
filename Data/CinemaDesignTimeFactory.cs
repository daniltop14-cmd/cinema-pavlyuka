// Фабрика создаёт контекст для команд dotnet ef без запуска веб-сервера.
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Cinema.Data;

// EF CLI creates migrations without starting the web app or filling its database.
public class CinemaDesignTimeFactory : IDesignTimeDbContextFactory<CinemaDbContext>
{
    public CinemaDbContext CreateDbContext(string[] args)
    {
        Directory.CreateDirectory("App_Data");
        var connection = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Data Source=App_Data/cinema.db;Default Timeout=30;Foreign Keys=True";
        return new CinemaDbContext(new DbContextOptionsBuilder<CinemaDbContext>().UseSqlite(connection).Options);
    }
}
