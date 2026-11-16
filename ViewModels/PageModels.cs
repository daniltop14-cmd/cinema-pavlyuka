// Передаёт подготовленные данные из контроллеров на страницы.
// Пояснения ниже относятся к следующей строке или смысловому шагу.
// Подключаем пространство имён Cinema.Models для типов ниже.
using Cinema.Models;

// Помещаем объявленные типы в указанное пространство имён.
namespace Cinema.ViewModels;
// Передаёт подготовленные данные из контроллеров на страницы.
public class HomeModel
{
    // Свойство хранит значение модели и доступно другим компонентам.
    public DateTime ScheduleDate { get; set; }
    // Записываем вычисленное значение в public list<movie> upcoming movies { get; set; }.
    public List<Movie> UpcomingMovies { get; set; } = [];
    // Записываем вычисленное значение в public list<banner model> banners { get; set; }.
    public List<BannerModel> Banners { get; set; } = [];
    // Записываем вычисленное значение в public list<movie> movies { get; set; }.
    public List<Movie> Movies { get; set; } = [];
    // Записываем вычисленное значение в public list<session> sessions { get; set; }.
    public List<Session> Sessions { get; set; } = [];
    // Записываем вычисленное значение в public list<hall> halls { get; set; }.
    public List<Hall> Halls { get; set; } = [];
    // Записываем вычисленное значение в public list<review> reviews { get; set; }.
    public List<Review> Reviews { get; set; } = [];
    // Записываем вычисленное значение в public list<faq item> faq { get; set; }.
    public List<FaqItem> Faq { get; set; } = [];
    // Записываем вычисленное значение в public dictionary<string, string> settings { get; set; }.
    public Dictionary<string, string> Settings { get; set; } = [];
}

// Передаёт подготовленные данные из контроллеров на страницы.
public class BannerModel
{
    // Записываем вычисленное значение в public string image { get; set; }.
    public string Image { get; set; } = "";
    // Записываем вычисленное значение в public string title { get; set; }.
    public string Title { get; set; } = "";
    // Записываем вычисленное значение в public string target { get; set; }.
    public string Target { get; set; } = "/Movies";
}

// Передаёт подготовленные данные из контроллеров на страницы.
public class ScheduleCardModel
{
    // Записываем вычисленное значение в public movie movie { get; set; }.
    public Movie Movie { get; set; } = null!;
    // Записываем вычисленное значение в public list<session> sessions { get; set; }.
    public List<Session> Sessions { get; set; } = [];
}

// Передаёт подготовленные данные из контроллеров на страницы.
public class ScheduleModel
{
    // Записываем вычисленное значение в public list<movie> movies { get; set; }.
    public List<Movie> Movies { get; set; } = [];
    // Записываем вычисленное значение в public list<session> sessions { get; set; }.
    public List<Session> Sessions { get; set; } = [];
    // Записываем вычисленное значение в public list<string> genres { get; set; }.
    public List<string> Genres { get; set; } = [];
    // Свойство хранит значение модели и доступно другим компонентам.
    public DateTime Date { get; set; }
    // Свойство хранит значение модели и доступно другим компонентам.
    public string? Genre { get; set; }
}

// Передаёт подготовленные данные из контроллеров на страницы.
public class SeatMapModel
{
    // Записываем вычисленное значение в public session session { get; set; }.
    public Session Session { get; set; } = null !;
    // Записываем вычисленное значение в public list<seat> seats { get; set; }.
    public List<Seat> Seats { get; set; } = [];
    // Записываем вычисленное значение в public hash set<int> occupied { get; set; }.
    public HashSet<int> Occupied { get; set; } = [];
}

// Передаёт подготовленные данные из контроллеров на страницы.
public class DashboardModel
{
    // Свойство хранит значение модели и доступно другим компонентам.
    public int Users { get; set; }
    // Свойство хранит значение модели и доступно другим компонентам.
    public int Movies { get; set; }
    // Свойство хранит значение модели и доступно другим компонентам.
    public int Sessions { get; set; }
    // Свойство хранит значение модели и доступно другим компонентам.
    public int SoldTickets { get; set; }
    // Свойство хранит значение модели и доступно другим компонентам.
    public decimal Revenue { get; set; }
    // Записываем вычисленное значение в public list<booking> recent { get; set; }.
    public List<Booking> Recent { get; set; } = [];
}

// Передаёт подготовленные данные из контроллеров на страницы.
public static class Labels
{
    // Объявляем метод Status: параметры приходят из запроса или вызова сервиса.
    public static string Status(BookingStatus status) => status switch
    {
        // Передаём очередной аргумент или задаём элемент коллекции.
        BookingStatus.Pending => "Ожидает оплаты",
        // Передаём очередной аргумент или задаём элемент коллекции.
        BookingStatus.Paid => "Оплачен",
        // Передаём очередной аргумент или задаём элемент коллекции.
        BookingStatus.Cancelled => "Отменён",
        // Передаём очередной аргумент или задаём элемент коллекции.
        BookingStatus.Expired => "Срок истёк",
        // Передаём очередной аргумент или задаём элемент коллекции.
        BookingStatus.Refunded => "Возвращён",
        _ => "Неизвестно"
    };
    // Выполняем действие Role после проверки прав и состояния записи.
    public static string Role(string role) => role switch
    {
        // Передаём очередной аргумент или задаём элемент коллекции.
        "Admin" => "Администратор",
        // Передаём очередной аргумент или задаём элемент коллекции.
        "Manager" => "Менеджер",
        _ => "Пользователь"
    };
}
