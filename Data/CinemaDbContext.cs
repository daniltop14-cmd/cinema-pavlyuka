using Cinema.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Data;
public class CinemaDbContext(DbContextOptions<CinemaDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Movie> Movies => Set<Movie>();
    public DbSet<Hall> Halls => Set<Hall>();
    public DbSet<Seat> Seats => Set<Seat>();
    public DbSet<Session> Sessions => Set<Session>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingSeat> BookingSeats => Set<BookingSeat>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<FaqItem> FaqItems => Set<FaqItem>();
    public DbSet<ContactMessage> ContactMessages => Set<ContactMessage>();
    public DbSet<SiteSetting> SiteSettings => Set<SiteSetting>();
    public DbSet<TeamMember> TeamMembers => Set<TeamMember>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);
        b.Entity<Seat>().HasIndex(x => new { x.HallId, x.Row, x.Number }).IsUnique();
        b.Entity<Session>().HasIndex(x => new { x.HallId, x.StartsAt });
        b.Entity<Session>().HasIndex(x => new { x.MovieId, x.StartsAt });
        b.Entity<Booking>().HasIndex(x => new { x.Status, x.ExpiresAtUtc });
        b.Entity<Booking>().HasIndex(x => new { x.UserId, x.CreatedAtUtc });
        b.Entity<Review>().HasIndex(x => new { x.Status, x.CreatedAtUtc });
        b.Entity<BookingSeat>().HasIndex(x => new { x.SessionId, x.SeatId, x.Active }).IsUnique();
        b.Entity<BookingSeat>().ToTable(t => t.HasCheckConstraint("CK_BookingSeat_Active", "`Active` IS NULL OR `Active` = 1"));
        b.Entity<BookingSeat>().HasIndex(x => new { x.BookingId, x.SeatId }).IsUnique();
        b.Entity<Payment>().HasIndex(x => x.BookingId).IsUnique();
        b.Entity<Booking>().HasOne(x => x.Payment).WithOne(x => x.Booking).HasForeignKey<Payment>(x => x.BookingId);
        // Keep sold tickets and the related film/session/hall as permanent history.
        foreach (var fk in b.Model.GetEntityTypes().SelectMany(x => x.GetForeignKeys()))
            fk.DeleteBehavior = DeleteBehavior.Restrict;
        foreach (var property in b.Model.GetEntityTypes().SelectMany(x => x.GetProperties()).Where(x => x.ClrType == typeof(decimal) || x.ClrType == typeof(decimal? )))
        {
            property.SetPrecision(10);
            property.SetScale(2);
        }
    }
}
