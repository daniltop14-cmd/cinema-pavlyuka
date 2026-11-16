// Сущности базы: фильмы, сеансы, места, заказы, пользователи и содержимое сайта.
// Пояснения ниже относятся к следующей строке или смысловому шагу.
// Подключаем пространство имён System.ComponentModel.DataAnnotations для типов ниже.
using System.ComponentModel.DataAnnotations;
// Подключаем пространство имён Microsoft.AspNetCore.Identity для типов ниже.
using Microsoft.AspNetCore.Identity;

// Помещаем объявленные типы в указанное пространство имён.
namespace Cinema.Models;
// Сущности базы: фильмы, сеансы, места, заказы, пользователи и содержимое сайта.
public class ApplicationUser : IdentityUser
{
    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, StringLength(80)]
    // Записываем вычисленное значение в public string display name { get; set; }.
    public string DisplayName { get; set; } = "";
    // Записываем вычисленное значение в public icollection<booking> bookings { get; set; }.
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

// Сущности базы: фильмы, сеансы, места, заказы, пользователи и содержимое сайта.
public class Movie
{
    // Свойство хранит значение модели и доступно другим компонентам.
    public int Id { get; set; }

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, StringLength(150)]
    // Записываем вычисленное значение в public string title { get; set; }.
    public string Title { get; set; } = "";

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, StringLength(60)]
    // Записываем вычисленное значение в public string genre { get; set; }.
    public string Genre { get; set; } = "";
    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [StringLength(100)]
    // Записываем вычисленное значение в public string country { get; set; }.
    public string Country { get; set; } = "Россия";

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Range(1, 600)]
    // Свойство хранит значение модели и доступно другим компонентам.
    public int DurationMinutes { get; set; }

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Range(0, 18)]
    // Свойство хранит значение модели и доступно другим компонентам.
    public int AgeRating { get; set; }

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, StringLength(8000)]
    // Записываем вычисленное значение в public string description { get; set; }.
    public string Description { get; set; } = "";

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [StringLength(300)]
    // Записываем вычисленное значение в public string poster path { get; set; }.
    public string PosterPath { get; set; } = "/images/poster-1.svg";

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [StringLength(500)]
    // Свойство хранит значение модели и доступно другим компонентам.
    public string? TrailerUrl { get; set; }

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [DataType(DataType.Date)]
    // Записываем вычисленное значение в public date time release date { get; set; }.
    public DateTime ReleaseDate { get; set; } = DateTime.Today;
    // Свойство хранит значение модели и доступно другим компонентам.
    public bool IsFeatured { get; set; }
    // Записываем вычисленное значение в public icollection<session> sessions { get; set; }.
    public ICollection<Session> Sessions { get; set; } = new List<Session>();
}

// Сущности базы: фильмы, сеансы, места, заказы, пользователи и содержимое сайта.
public class Hall
{
    // Свойство хранит значение модели и доступно другим компонентам.
    public int Id { get; set; }

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, StringLength(80)]
    // Записываем вычисленное значение в public string name { get; set; }.
    public string Name { get; set; } = "";

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [StringLength(30)]
    // Записываем вычисленное значение в public string color { get; set; }.
    public string Color { get; set; } = "blue";

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Range(1, 20)]
    // Записываем вычисленное значение в public int rows { get; set; }.
    public int Rows { get; set; } = 6;

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Range(1, 20)]
    // Записываем вычисленное значение в public int seats per row { get; set; }.
    public int SeatsPerRow { get; set; } = 10;

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, StringLength(2000)]
    // Записываем вычисленное значение в public string description { get; set; }.
    public string Description { get; set; } = "";
    // Записываем вычисленное значение в public icollection<seat> seats { get; set; }.
    public ICollection<Seat> Seats { get; set; } = new List<Seat>();
}

// Сущности базы: фильмы, сеансы, места, заказы, пользователи и содержимое сайта.
public class Seat
{
    // Свойство хранит значение модели и доступно другим компонентам.
    public int Id { get; set; }
    // Свойство хранит значение модели и доступно другим компонентам.
    public int HallId { get; set; }
    // Записываем вычисленное значение в public hall hall { get; set; }.
    public Hall Hall { get; set; } = null !;
    // Свойство хранит значение модели и доступно другим компонентам.
    public int Row { get; set; }
    // Свойство хранит значение модели и доступно другим компонентам.
    public int Number { get; set; }
    // Записываем вычисленное значение в public bool is accessible { get; set; }.
    public bool IsAccessible { get; set; } = true;
}

// Сущности базы: фильмы, сеансы, места, заказы, пользователи и содержимое сайта.
public class Session
{
    // Свойство хранит значение модели и доступно другим компонентам.
    public int Id { get; set; }
    // Свойство хранит значение модели и доступно другим компонентам.
    public int MovieId { get; set; }
    // Записываем вычисленное значение в public movie movie { get; set; }.
    public Movie Movie { get; set; } = null !;
    // Свойство хранит значение модели и доступно другим компонентам.
    public int HallId { get; set; }
    // Записываем вычисленное значение в public hall hall { get; set; }.
    public Hall Hall { get; set; } = null !;
    // Session timestamps use the cinema's local clock. Booking expiry uses UTC.
    // Свойство хранит значение модели и доступно другим компонентам.
    public DateTime StartsAt { get; set; }
    // Свойство хранит значение модели и доступно другим компонентам.
    public DateTime EndsAt { get; set; }

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Range(1, 100000)]
    // Свойство хранит значение модели и доступно другим компонентам.
    public decimal Price { get; set; }
    // Свойство хранит значение модели и доступно другим компонентам.
    public bool IsCancelled { get; set; }
}

// Сущности базы: фильмы, сеансы, места, заказы, пользователи и содержимое сайта.
public enum BookingStatus
{
    // Передаём очередной аргумент или задаём элемент коллекции.
    Pending,
    // Передаём очередной аргумент или задаём элемент коллекции.
    Paid,
    // Передаём очередной аргумент или задаём элемент коллекции.
    Cancelled,
    // Передаём очередной аргумент или задаём элемент коллекции.
    Expired,
    Refunded
}

// Сущности базы: фильмы, сеансы, места, заказы, пользователи и содержимое сайта.
public class Booking
{
    // Свойство хранит значение модели и доступно другим компонентам.
    public int Id { get; set; }
    // Записываем вычисленное значение в public string user id { get; set; }.
    public string UserId { get; set; } = "";
    // Записываем вычисленное значение в public application user user { get; set; }.
    public ApplicationUser User { get; set; } = null !;
    // Свойство хранит значение модели и доступно другим компонентам.
    public int SessionId { get; set; }
    // Записываем вычисленное значение в public session session { get; set; }.
    public Session Session { get; set; } = null !;
    // Свойство хранит значение модели и доступно другим компонентам.
    public DateTime CreatedAtUtc { get; set; }
    // Свойство хранит значение модели и доступно другим компонентам.
    public DateTime ExpiresAtUtc { get; set; }
    // Свойство хранит значение модели и доступно другим компонентам.
    public decimal Total { get; set; }
    // Свойство хранит значение модели и доступно другим компонентам.
    public BookingStatus Status { get; set; }
    // Записываем вычисленное значение в public icollection<booking seat> booking seats { get; set; }.
    public ICollection<BookingSeat> BookingSeats { get; set; } = new List<BookingSeat>();
    // Свойство хранит значение модели и доступно другим компонентам.
    public Payment? Payment { get; set; }
}

// Сущности базы: фильмы, сеансы, места, заказы, пользователи и содержимое сайта.
public class BookingSeat
{
    // Свойство хранит значение модели и доступно другим компонентам.
    public int Id { get; set; }
    // Свойство хранит значение модели и доступно другим компонентам.
    public int BookingId { get; set; }
    // Записываем вычисленное значение в public booking booking { get; set; }.
    public Booking Booking { get; set; } = null !;
    // Свойство хранит значение модели и доступно другим компонентам.
    public int SeatId { get; set; }
    // Записываем вычисленное значение в public seat seat { get; set; }.
    public Seat Seat { get; set; } = null !;
    // Свойство хранит значение модели и доступно другим компонентам.
    public int SessionId { get; set; }
    // Записываем вычисленное значение в public session session { get; set; }.
    public Session Session { get; set; } = null !;
    // 1 = holds the seat; null = historical record. SQLite permits multiple NULLs
    // in a unique index, but only one (SessionId, SeatId, 1).
    // Записываем вычисленное значение в public int? active { get; set; }.
    public int? Active { get; set; } = 1;
    // Свойство хранит значение модели и доступно другим компонентам.
    public decimal Price { get; set; }
}

// Сущности базы: фильмы, сеансы, места, заказы, пользователи и содержимое сайта.
public class Payment
{
    // Свойство хранит значение модели и доступно другим компонентам.
    public int Id { get; set; }
    // Свойство хранит значение модели и доступно другим компонентам.
    public int BookingId { get; set; }
    // Записываем вычисленное значение в public booking booking { get; set; }.
    public Booking Booking { get; set; } = null !;
    // Свойство хранит значение модели и доступно другим компонентам.
    public decimal Amount { get; set; }
    // Свойство хранит значение модели и доступно другим компонентам.
    public DateTime PaidAtUtc { get; set; }

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [MaxLength(50)]
    // Записываем вычисленное значение в public string provider { get; set; }.
    public string Provider { get; set; } = "Оплата";
    // Свойство хранит значение модели и доступно другим компонентам.
    public bool IsRefunded { get; set; }
}

// Сущности базы: фильмы, сеансы, места, заказы, пользователи и содержимое сайта.
public class Review
{
    // Свойство хранит значение модели и доступно другим компонентам.
    public int Id { get; set; }
    // Записываем вычисленное значение в public string user id { get; set; }.
    public string UserId { get; set; } = "";
    // Записываем вычисленное значение в public application user user { get; set; }.
    public ApplicationUser User { get; set; } = null !;

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Range(1, 5)]
    // Записываем вычисленное значение в public int rating { get; set; }.
    public int Rating { get; set; } = 5;

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, StringLength(2000, MinimumLength = 5)]
    // Записываем вычисленное значение в public string text { get; set; }.
    public string Text { get; set; } = "";
    // Записываем вычисленное значение в public date time created at utc { get; set; }.
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    // Свойство хранит значение модели и доступно другим компонентам.
    public ReviewStatus Status { get; set; }
}

// Сущности базы: фильмы, сеансы, места, заказы, пользователи и содержимое сайта.
public enum ReviewStatus
{
    // Передаём очередной аргумент или задаём элемент коллекции.
    Pending,
    // Передаём очередной аргумент или задаём элемент коллекции.
    Approved,
    Rejected
}

// Сущности базы: фильмы, сеансы, места, заказы, пользователи и содержимое сайта.
public class Service
{
    // Свойство хранит значение модели и доступно другим компонентам.
    public int Id { get; set; }

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, StringLength(120)]
    // Записываем вычисленное значение в public string name { get; set; }.
    public string Name { get; set; } = "";

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, StringLength(2000)]
    // Записываем вычисленное значение в public string description { get; set; }.
    public string Description { get; set; } = "";

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Range(0, 1000000)]
    // Свойство хранит значение модели и доступно другим компонентам.
    public decimal? Price { get; set; }

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [StringLength(300)]
    // Записываем вычисленное значение в public string image path { get; set; }.
    public string ImagePath { get; set; } = "/images/hall.svg";
}

// Сущности базы: фильмы, сеансы, места, заказы, пользователи и содержимое сайта.
public class FaqItem
{
    // Свойство хранит значение модели и доступно другим компонентам.
    public int Id { get; set; }

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, StringLength(300)]
    // Записываем вычисленное значение в public string question { get; set; }.
    public string Question { get; set; } = "";

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, StringLength(3000)]
    // Записываем вычисленное значение в public string answer { get; set; }.
    public string Answer { get; set; } = "";
    // Свойство хранит значение модели и доступно другим компонентам.
    public int SortOrder { get; set; }
}

// Сущности базы: фильмы, сеансы, места, заказы, пользователи и содержимое сайта.
public class ContactMessage
{
    // Свойство хранит значение модели и доступно другим компонентам.
    public int Id { get; set; }

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, StringLength(80)]
    // Записываем вычисленное значение в public string name { get; set; }.
    public string Name { get; set; } = "";

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, EmailAddress, StringLength(150)]
    // Записываем вычисленное значение в public string email { get; set; }.
    public string Email { get; set; } = "";

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, StringLength(4000, MinimumLength = 5)]
    // Записываем вычисленное значение в public string message { get; set; }.
    public string Message { get; set; } = "";
    // Записываем вычисленное значение в public date time created at utc { get; set; }.
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

// Сущности базы: фильмы, сеансы, места, заказы, пользователи и содержимое сайта.
public class SiteSetting
{
    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Key, StringLength(100)]
    // Записываем вычисленное значение в public string key { get; set; }.
    public string Key { get; set; } = "";

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, StringLength(8000)]
    // Записываем вычисленное значение в public string value { get; set; }.
    public string Value { get; set; } = "";
}

// Сущности базы: фильмы, сеансы, места, заказы, пользователи и содержимое сайта.
public class TeamMember
{
    // Свойство хранит значение модели и доступно другим компонентам.
    public int Id { get; set; }

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, StringLength(100)]
    // Записываем вычисленное значение в public string name { get; set; }.
    public string Name { get; set; } = "";

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, StringLength(100)]
    // Записываем вычисленное значение в public string role { get; set; }.
    public string Role { get; set; } = "";

    // Атрибут задаёт правило проверки или отображения следующего элемента.
    [Required, StringLength(1000)]
    // Записываем вычисленное значение в public string tasks { get; set; }.
    public string Tasks { get; set; } = "";
}
