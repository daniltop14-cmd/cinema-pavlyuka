using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Cinema.Models;
public class ApplicationUser : IdentityUser
{
    [Required, StringLength(80)]
    public string DisplayName { get; set; } = "";
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

public class Movie
{
    public int Id { get; set; }

    [Required, StringLength(150)]
    public string Title { get; set; } = "";

    [Required, StringLength(60)]
    public string Genre { get; set; } = "";
    [StringLength(100)]
    public string Country { get; set; } = "Россия";

    [Range(1, 600)]
    public int DurationMinutes { get; set; }

    [Range(0, 18)]
    public int AgeRating { get; set; }

    [Required, StringLength(8000)]
    public string Description { get; set; } = "";

    [StringLength(300)]
    public string PosterPath { get; set; } = "/images/poster-1.svg";

    [StringLength(500)]
    public string? TrailerUrl { get; set; }

    [DataType(DataType.Date)]
    public DateTime ReleaseDate { get; set; } = DateTime.Today;
    public bool IsFeatured { get; set; }
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}

public class Hall
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = "";

    [StringLength(30)]
    public string Color { get; set; } = "blue";

    [Range(1, 20)]
    public int Rows { get; set; } = 6;

    [Range(1, 20)]
    public int SeatsPerRow { get; set; } = 10;

    [Required, StringLength(2000)]
    public string Description { get; set; } = "";
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
}

public class Seat
{
    public int Id { get; set; }
    public int HallId { get; set; }
    public Hall Hall { get; set; } = null !;
    public int Row { get; set; }
    public int Number { get; set; }
    public bool IsAccessible { get; set; } = true;
}

public class Session
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public Movie Movie { get; set; } = null !;
    public int HallId { get; set; }
    public Hall Hall { get; set; } = null !;
    // Session timestamps use the cinema's local clock. Booking expiry uses UTC.
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }

    [Range(1, 100000)]
    public decimal Price { get; set; }
    public bool IsCancelled { get; set; }
}

public enum BookingStatus
{
    Pending,
    Paid,
    Cancelled,
    Expired,
    Refunded
}

public class Booking
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null !;
    public int SessionId { get; set; }
    public Session Session { get; set; } = null !;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime ExpiresAtUtc { get; set; }
    public decimal Total { get; set; }
    public BookingStatus Status { get; set; }
    public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
    public Payment? Payment { get; set; }
}

public class BookingSeat
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null !;
    public int SeatId { get; set; }
    public Seat Seat { get; set; } = null !;
    public int SessionId { get; set; }
    public Session Session { get; set; } = null !;
    // 1 = holds the seat; null = historical record. SQLite permits multiple NULLs
    // in a unique index, but only one (SessionId, SeatId, 1).
    public int? Active { get; set; } = 1;
    public decimal Price { get; set; }
}

public class Payment
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null !;
    public decimal Amount { get; set; }
    public DateTime PaidAtUtc { get; set; }

    [MaxLength(50)]
    public string Provider { get; set; } = "Оплата";
    public bool IsRefunded { get; set; }
}

public class Review
{
    public int Id { get; set; }
    public string UserId { get; set; } = "";
    public ApplicationUser User { get; set; } = null !;

    [Range(1, 5)]
    public int Rating { get; set; } = 5;

    [Required, StringLength(2000, MinimumLength = 5)]
    public string Text { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public ReviewStatus Status { get; set; }
}

public enum ReviewStatus
{
    Pending,
    Approved,
    Rejected
}

public class Service
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = "";

    [Required, StringLength(2000)]
    public string Description { get; set; } = "";

    [Range(0, 1000000)]
    public decimal? Price { get; set; }

    [StringLength(300)]
    public string ImagePath { get; set; } = "/images/hall.svg";
}

public class FaqItem
{
    public int Id { get; set; }

    [Required, StringLength(300)]
    public string Question { get; set; } = "";

    [Required, StringLength(3000)]
    public string Answer { get; set; } = "";
    public int SortOrder { get; set; }
}

public class ContactMessage
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = "";

    [Required, EmailAddress, StringLength(150)]
    public string Email { get; set; } = "";

    [Required, StringLength(4000, MinimumLength = 5)]
    public string Message { get; set; } = "";
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

public class SiteSetting
{
    [Key, StringLength(100)]
    public string Key { get; set; } = "";

    [Required, StringLength(8000)]
    public string Value { get; set; } = "";
}

public class TeamMember
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = "";

    [Required, StringLength(100)]
    public string Role { get; set; } = "";

    [Required, StringLength(1000)]
    public string Tasks { get; set; } = "";
}
