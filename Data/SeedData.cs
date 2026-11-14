using Cinema.Models;
using Cinema.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Data;
public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        var db = services.GetRequiredService<CinemaDbContext>();
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        foreach (var role in new[]
        {
            "Admin",
            "Manager",
            "User"
        }

        )
            if (!await roles.RoleExistsAsync(role))
                await roles.CreateAsync(new IdentityRole(role));
        if (!await db.Halls.AnyAsync())
        {
            var names = new[]
            {
                "Синий зал",
                "Красный зал",
                "Зелёный зал",
                "Оранжевый зал"
            };
            var colors = new[]
            {
                "blue",
                "red",
                "green",
                "orange"
            };
            for (var i = 0; i < 4; i++)
            {
                var hall = new Hall
                {
                    Name = names[i],
                    Color = colors[i],
                    Rows = 6 + i,
                    SeatsPerRow = 10 + i,
                    Description = "Лазерная проекция 4K, объёмный звук 7.1 и удобные кресла с широкими подлокотниками."
                };
                for (var row = 1; row <= hall.Rows; row++)
                    for (var seat = 1; seat <= hall.SeatsPerRow; seat++)
                        hall.Seats.Add(new Seat { Row = row, Number = seat });
                db.Halls.Add(hall);
            }

            await db.SaveChangesAsync();
        }

        if (!await db.Movies.AnyAsync())
        {
            var today = services.GetRequiredService<CinemaClock>().Now.Date;
            db.Movies.AddRange(ReferenceContent.Movies(environment.ContentRootPath, today));
            await db.SaveChangesAsync();
        }

        if (!await db.Sessions.AnyAsync())
        {
            var movies = await db.Movies.OrderBy(x => x.Id).ToListAsync();
            var halls = await db.Halls.OrderBy(x => x.Id).ToListAsync();
            var today = services.GetRequiredService<CinemaClock>().Now.Date;
            for (var day = 0; day < 31; day++)
                for (var h = 0; h < halls.Count; h++)
                    for (var slot = 0; slot < 4; slot++)
                    {
                        var available = movies.Where(x => x.ReleaseDate <= today.AddDays(day)).ToList();
                        var movie = available[(day * 4 + h * 4 + slot) % available.Count];
                        var starts = today.AddDays(day).AddHours(10 + slot * 3.5);
                        db.Sessions.Add(new Session { MovieId = movie.Id, HallId = halls[h].Id, StartsAt = starts, EndsAt = starts.AddMinutes(movie.DurationMinutes + 20), Price = 350 + slot * 70 + h * 30 });
                    }
        }

        if (!await db.Services.AnyAsync())
            db.Services.AddRange(new Service { Name = "Кинобар", Description = "Свежий попкорн, напитки и любимые закуски перед сеансом.", Price = 250, ImagePath = "/images/reference/340_about-1_big.jpg" }, new Service { Name = "Аренда зала", Description = "Ваш фильм на большом экране: день рождения, встреча друзей или презентация.", Price = 5000, ImagePath = "/images/reference/674_about-3_big.jpg" }, new Service { Name = "Подарочный вечер", Description = "Подарочный сертификат на два билета и набор из кинобара.", Price = 1500, ImagePath = "/images/reference/610_11.jpg" });
        if (!await db.FaqItems.AnyAsync())
            db.FaqItems.AddRange(new FaqItem { Question = "Как купить билет?", Answer = "Откройте афишу, выберите сеанс и места, войдите в аккаунт и подтвердите оплату. Билет появится в личном кабинете.", SortOrder = 1 }, new FaqItem { Question = "Сколько действует бронь?", Answer = "Места закрепляются за вами на 10 минут. После истечения времени неоплаченная бронь освобождается.", SortOrder = 2 }, new FaqItem { Question = "Можно ли вернуть билет?", Answer = "До начала сеанса обратитесь к менеджеру через контакты, указав номер заказа. Менеджер может оформить возврат.", SortOrder = 3 }, new FaqItem { Question = "Где найти билет?", Answer = "После оформления заказа электронный билет доступен в личном кабинете в разделе заказов.", SortOrder = 4 });
        if (!await db.SiteSettings.AnyAsync())
            db.SiteSettings.AddRange(new[] { new SiteSetting { Key = "Name", Value = "Кинотеатр им. Павлюка" }, new SiteSetting { Key = "Tagline", Value = "Кинотеатр им. Павлюка" }, new SiteSetting { Key = "About", Value = "Кинотеатр им. Павлюка — место, где большой экран объединяет. Четыре уютных зала, насыщенный звук и истории, которые остаются с вами после финальных титров. Мы создали пространство для встреч, открытий и маленьких традиций." }, new SiteSetting { Key = "Address", Value = "Ижевск, ул. Карла Маркса, 242" }, new SiteSetting { Key = "Phone", Value = "+7 (000) 000-00-00" }, new SiteSetting { Key = "Email", Value = "hello@cinema.example" }, new SiteSetting { Key = "Hours", Value = "Ежедневно, 09:30 — 00:00" }, new SiteSetting { Key = "MapQuery", Value = "Ижевск Карла Маркса 242" }, new SiteSetting { Key = "SocialVk", Value = "https://vk.com" }, new SiteSetting { Key = "SocialTelegram", Value = "https://t.me" }, new SiteSetting { Key = "Promo", Value = "Больше впечатлений на большом экране. Выберите свой следующий фильм." } });
        if (!await db.TeamMembers.AnyAsync())
            db.TeamMembers.AddRange(new TeamMember { Name = "Участник 1", Role = "Серверная часть", Tasks = "База данных, контроллеры, бронирование" }, new TeamMember { Name = "Участник 2", Role = "Интерфейс", Tasks = "Дизайн, адаптивность, схема зала" }, new TeamMember { Name = "Участник 3", Role = "Тестирование", Tasks = "Проверка сценариев и ограничений доступа" }, new TeamMember { Name = "Участник 4", Role = "Документация", Tasks = "ТЗ, инструкция и описание проекта" });
        await db.SaveChangesAsync();
        if (!await db.Reviews.AnyAsync())
        {
            var visitor = new ApplicationUser
            {
                UserName = "visitor@cinema.example",
                Email = "visitor@cinema.example",
                DisplayName = "Посетитель",
                EmailConfirmed = true
            };
            var existing = await users.FindByEmailAsync(visitor.Email);
            if (existing == null)
            {
                // No password: this record credits seed reviews, it is not a demo login.
                var result = await users.CreateAsync(visitor);
                if (!result.Succeeded)
                    throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
                await users.AddToRoleAsync(visitor, "User");
            }
            else
                visitor = existing;
            db.Reviews.AddRange(new[] { "Отличный звук и очень уютные залы. Обязательно вернёмся!", "Удобно выбирать места заранее. Вечер с семьёй удался.", "Красивое пространство и внимательные сотрудники." }.Select(text => new Review { UserId = visitor.Id, Rating = 5, Text = text, Status = ReviewStatus.Approved }));
            await db.SaveChangesAsync();
        }

        await ReferenceContent.SeedBannersAsync(db);
        await PublicCopy.RefreshAsync(db);
        await CoverCatalog.RefreshAsync(db, environment.ContentRootPath, services.GetRequiredService<CinemaClock>().Now.Date);
        var email = configuration["SeedAdmin:Email"];
        var password = configuration["SeedAdmin:Password"];
        // Local first launch: prepare the account used to manage the cinema.
        // Existing administrators and passwords are never overwritten.
        if (environment.IsDevelopment() && string.IsNullOrWhiteSpace(email)
            && !(await users.GetUsersInRoleAsync("Admin")).Any()
            && await users.FindByEmailAsync("admin@cinema.local") == null)
        {
            email = "admin@cinema.local";
            password = "Pavlyuk2026!";
        }

        if (environment.IsDevelopment() && !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
        {
            var admin = await users.FindByEmailAsync(email);
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    DisplayName = "Администратор",
                    EmailConfirmed = true
                };
                var result = await users.CreateAsync(admin, password);
                if (!result.Succeeded)
                    throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
            }

            var roleResult = await users.AddToRoleAsync(admin, "Admin");
            if (!roleResult.Succeeded && !await users.IsInRoleAsync(admin, "Admin"))
                throw new InvalidOperationException("Не удалось назначить администратора.");
        }
    }
}
