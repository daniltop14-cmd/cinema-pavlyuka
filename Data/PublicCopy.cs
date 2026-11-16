// Заменяет устаревшие публичные формулировки в базе.
// Пояснения ниже относятся к следующей строке или смысловому шагу.
// Подключаем пространство имён Microsoft.EntityFrameworkCore для типов ниже.
using Microsoft.EntityFrameworkCore;

// Помещаем объявленные типы в указанное пространство имён.
namespace Cinema.Data;

// Заменяет устаревшие публичные формулировки в базе.
public static class PublicCopy
{
    // Объявляем метод RefreshAsync: параметры приходят из запроса или вызова сервиса.
    public static async Task RefreshAsync(CinemaDbContext db)
    {
        static string Clean(string text) => text
            // Продолжаем выражение, начатое на предыдущей строке.
            .Replace(" (учебный проект)", "")
            // Продолжаем выражение, начатое на предыдущей строке.
            .Replace(" Расписание и цены в этом проекте учебные.", "")
            // Продолжаем выражение, начатое на предыдущей строке.
            .Replace("учебную оплату", "оплату")
            // Продолжаем выражение, начатое на предыдущей строке.
            .Replace("В учебном режиме менеджер", "Менеджер")
            // Продолжаем выражение, начатое на предыдущей строке.
            .Replace("Учебная оплата", "Оплата");
        // Последовательно обрабатываем каждый элемент коллекции.
        foreach (var movie in await db.Movies.ToListAsync()) movie.Description = Clean(movie.Description);
        // Последовательно обрабатываем каждый элемент коллекции.
        foreach (var setting in await db.SiteSettings.ToListAsync()) setting.Value = Clean(setting.Value);
        // Последовательно обрабатываем каждый элемент коллекции.
        foreach (var faq in await db.FaqItems.ToListAsync())
        {
            // Выполняем следующую ветку только при истинности условия.
            if (faq.Question == "Списываются ли реальные деньги?")
            {
                // Записываем вычисленное значение в question.
                faq.Question = "Где найти билет?";
                // Записываем вычисленное значение в answer.
                faq.Answer = "После оформления заказа электронный билет доступен в личном кабинете в разделе заказов.";
            }
            // Обрабатываем альтернативный случай предыдущей проверки.
            else faq.Answer = Clean(faq.Answer);
        }
        // Последовательно обрабатываем каждый элемент коллекции.
        foreach (var payment in await db.Payments.Where(x => x.Provider == "Учебная оплата").ToListAsync())
            // Записываем вычисленное значение в provider.
            payment.Provider = "Оплата";
        // Сохраняем накопленные изменения в SQLite.
        await db.SaveChangesAsync();
    }
}
