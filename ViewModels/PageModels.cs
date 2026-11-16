// Типы данных, которые контроллеры передают в Razor-представления.
using Cinema.Models;

namespace Cinema.ViewModels;
public class HomeModel
{
    public DateTime ScheduleDate { get; set; }
    public List<Movie> UpcomingMovies { get; set; } = [];
    public List<BannerModel> Banners { get; set; } = [];
    public List<Movie> Movies { get; set; } = [];
    public List<Session> Sessions { get; set; } = [];
    public List<Hall> Halls { get; set; } = [];
    public List<Review> Reviews { get; set; } = [];
    public List<FaqItem> Faq { get; set; } = [];
    public Dictionary<string, string> Settings { get; set; } = [];
}

public class BannerModel
{
    public string Image { get; set; } = "";
    public string Title { get; set; } = "";
    public string Target { get; set; } = "/Movies";
}

public class ScheduleCardModel
{
    public Movie Movie { get; set; } = null!;
    public List<Session> Sessions { get; set; } = [];
}

public class ScheduleModel
{
    public List<Movie> Movies { get; set; } = [];
    public List<Session> Sessions { get; set; } = [];
    public List<string> Genres { get; set; } = [];
    public DateTime Date { get; set; }
    public string? Genre { get; set; }
}

public class SeatMapModel
{
    public Session Session { get; set; } = null !;
    public List<Seat> Seats { get; set; } = [];
    public HashSet<int> Occupied { get; set; } = [];
}

public class DashboardModel
{
    public int Users { get; set; }
    public int Movies { get; set; }
    public int Sessions { get; set; }
    public int SoldTickets { get; set; }
    public decimal Revenue { get; set; }
    public List<Booking> Recent { get; set; } = [];
}

public static class Labels
{
    public static string Status(BookingStatus status) => status switch
    {
        BookingStatus.Pending => "Ожидает оплаты",
        BookingStatus.Paid => "Оплачен",
        BookingStatus.Cancelled => "Отменён",
        BookingStatus.Expired => "Срок истёк",
        BookingStatus.Refunded => "Возвращён",
        _ => "Неизвестно"
    };
    public static string Role(string role) => role switch
    {
        "Admin" => "Администратор",
        "Manager" => "Менеджер",
        _ => "Пользователь"
    };
}
