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
        foreach (var setting in await db.SiteSettings.ToListAsync()) setting.Value = Clean(setting.Value);
        foreach (var faq in await db.FaqItems.ToListAsync())
        {
            if (faq.Question == "Списываются ли реальные деньги?")
            {
                faq.Question = "Где найти билет?";
                faq.Answer = "После оформления заказа электронный билет доступен в личном кабинете в разделе заказов.";
            }
            else faq.Answer = Clean(faq.Answer);
        }
        foreach (var payment in await db.Payments.Where(x => x.Provider == "Учебная оплата").ToListAsync())
            payment.Provider = "Оплата";
        await db.SaveChangesAsync();
    }
}
