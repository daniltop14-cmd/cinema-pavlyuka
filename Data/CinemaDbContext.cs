// Описывает таблицы, индексы и связи Entity Framework Core.
// Пояснения ниже относятся к следующей строке или смысловому шагу.
// Подключаем пространство имён Cinema.Models для типов ниже.
using Cinema.Models;
// Подключаем пространство имён Microsoft.AspNetCore.Identity.EntityFrameworkCore для типов ниже.
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
// Подключаем пространство имён Microsoft.EntityFrameworkCore для типов ниже.
using Microsoft.EntityFrameworkCore;

// Помещаем объявленные типы в указанное пространство имён.
namespace Cinema.Data;
// Описывает таблицы, индексы и связи Entity Framework Core.
public class CinemaDbContext(DbContextOptions<CinemaDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    // Завершаем действие или объявление текущей строки.
    public DbSet<Movie> Movies => Set<Movie>();
    // Завершаем действие или объявление текущей строки.
    public DbSet<Hall> Halls => Set<Hall>();
    // Завершаем действие или объявление текущей строки.
    public DbSet<Seat> Seats => Set<Seat>();
    // Завершаем действие или объявление текущей строки.
    public DbSet<Session> Sessions => Set<Session>();
    // Завершаем действие или объявление текущей строки.
    public DbSet<Booking> Bookings => Set<Booking>();
    // Завершаем действие или объявление текущей строки.
    public DbSet<BookingSeat> BookingSeats => Set<BookingSeat>();
    // Завершаем действие или объявление текущей строки.
    public DbSet<Payment> Payments => Set<Payment>();
    // Завершаем действие или объявление текущей строки.
    public DbSet<Review> Reviews => Set<Review>();
    // Завершаем действие или объявление текущей строки.
    public DbSet<Service> Services => Set<Service>();
    // Завершаем действие или объявление текущей строки.
    public DbSet<FaqItem> FaqItems => Set<FaqItem>();
    // Завершаем действие или объявление текущей строки.
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    // Завершаем действие или объявление текущей строки.
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    // Завершаем действие или объявление текущей строки.
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();

    // Объявляем метод OnModelCreating: параметры приходят из запроса или вызова сервиса.
    protected override void OnModelCreating(ModelBuilder b)
    {
        // Завершаем действие или объявление текущей строки.
        base.OnModelCreating(b);
        // Завершаем действие или объявление текущей строки.
        b.Entity<Seat>().HasIndex(x => new { x.HallId, x.Row, x.Number }).IsUnique();
        // Завершаем действие или объявление текущей строки.
        b.Entity<Session>().HasIndex(x => new { x.HallId, x.StartsAt });
        // Завершаем действие или объявление текущей строки.
        b.Entity<Session>().HasIndex(x => new { x.MovieId, x.StartsAt });
        // Завершаем действие или объявление текущей строки.
        b.Entity<Booking>().HasIndex(x => new { x.Status, x.ExpiresAtUtc });
        // Завершаем действие или объявление текущей строки.
        b.Entity<Booking>().HasIndex(x => new { x.UserId, x.CreatedAtUtc });
        // Завершаем действие или объявление текущей строки.
        b.Entity<Review>().HasIndex(x => new { x.Status, x.CreatedAtUtc });
        // Завершаем действие или объявление текущей строки.
        b.Entity<BookingSeat>().HasIndex(x => new { x.SessionId, x.SeatId, x.Active }).IsUnique();
        // Записываем вычисленное значение в to table(t.
        b.Entity<BookingSeat>().ToTable(t => t.HasCheckConstraint("CK_BookingSeat_Active", "`Active` IS NULL OR `Active` = 1"));
        // Завершаем действие или объявление текущей строки.
        b.Entity<BookingSeat>().HasIndex(x => new { x.BookingId, x.SeatId }).IsUnique();
        // Завершаем действие или объявление текущей строки.
        b.Entity<Payment>().HasIndex(x => x.BookingId).IsUnique();
        // Завершаем действие или объявление текущей строки.
        b.Entity<Booking>().HasOne(x => x.Payment).WithOne(x => x.Booking).HasForeignKey<Payment>(x => x.BookingId);
        // Keep sold tickets and the related film/session/hall as permanent history.
        // Последовательно обрабатываем каждый элемент коллекции.
        foreach (var fk in b.Model.GetEntityTypes().SelectMany(x => x.GetForeignKeys()))
            // Записываем вычисленное значение в delete behavior.
            fk.DeleteBehavior = DeleteBehavior.Restrict;
        // Последовательно обрабатываем каждый элемент коллекции.
        foreach (var property in b.Model.GetEntityTypes().SelectMany(x => x.GetProperties()).Where(x => x.ClrType == typeof(decimal) || x.ClrType == typeof(decimal? )))
        {
            // Завершаем действие или объявление текущей строки.
            property.SetPrecision(10);
            // Завершаем действие или объявление текущей строки.
            property.SetScale(2);
        }
    }
}
