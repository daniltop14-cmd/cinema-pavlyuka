// Обновляет сохранённые публичные тексты сайта.
using Microsoft.EntityFrameworkCore;

namespace Cinema.Data;

public static class PublicCopy
{
    public static async Task RefreshAsync(CinemaDbContext db)
    {
        static string Clean(string text) => text
            .Replace(" (учебный проект)", "")
            .Replace(" Расписание и цены в этом проекте учебные.", "")
            .Replace("учебную оплату", "оплату")
            .Replace("В учебном режиме менеджер", "Менеджер")
            .Replace("Учебная оплата", "Оплата");
        foreach (var movie in await db.Movies.ToListAsync()) movie.Description = Clean(movie.Description);
        // Загружаем тексты и ссылки из SQLite; на странице они используются для баннеров и информационных блоков.
        foreach (var setting in await db.SiteSettings.ToListAsync()) setting.Value = Clean(setting.Value);
        // Загружаем вопросы и ответы из SQLite в заданном администратором порядке.
        foreach (var faq in await db.FaqItems.ToListAsync())
        {
            if (faq.Question == "Списываются ли реальные деньги?")
            {
                faq.Question = "Где найти билет?";
                faq.Answer = "После оформления заказа электронный билет доступен в личном кабинете в разделе заказов.";
            }
            else faq.Answer = Clean(faq.Answer);
        }
        // Читаем сведения о подтверждениях и возвратах платежей из SQLite.
        foreach (var payment in await db.Payments.Where(x => x.Provider == "Учебная оплата").ToListAsync())
            payment.Provider = "Оплата";
        // Записываем накопленные изменения сущностей в SQLite.
        await db.SaveChangesAsync();
    }
}
