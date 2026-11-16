// Создаёт роли и начальный каталог, не стирая существующие данные.
// Пояснения ниже относятся к следующей строке или смысловому шагу.
// Подключаем пространство имён Cinema.Models для типов ниже.
using Cinema.Models;
// Подключаем пространство имён Cinema.Services для типов ниже.
using Cinema.Services;
// Подключаем пространство имён Microsoft.AspNetCore.Identity для типов ниже.
using Microsoft.AspNetCore.Identity;
// Подключаем пространство имён Microsoft.EntityFrameworkCore для типов ниже.
using Microsoft.EntityFrameworkCore;

// Помещаем объявленные типы в указанное пространство имён.
namespace Cinema.Data;
// Создаёт роли и начальный каталог, не стирая существующие данные.
public static class SeedData
{
    // Объявляем метод InitializeAsync: параметры приходят из запроса или вызова сервиса.
    public static async Task InitializeAsync(IServiceProvider services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Вычисляем db для дальнейших проверок или отображения.
        var db = services.GetRequiredService<CinemaDbContext>();
        // Вычисляем roles для дальнейших проверок или отображения.
        var roles = services.GetRequiredService<RoleManager<IdentityRole>>();
        // Вычисляем users для дальнейших проверок или отображения.
        var users = services.GetRequiredService<UserManager<ApplicationUser>>();
        // Последовательно обрабатываем каждый элемент коллекции.
        foreach (var role in new[]
        {
            // Передаём очередной аргумент или задаём элемент коллекции.
            "Admin",
            // Передаём очередной аргумент или задаём элемент коллекции.
            "Manager",
            "User"
        }

        )
            // Выполняем следующую ветку только при истинности условия.
            if (!await roles.RoleExistsAsync(role))
                // Дожидаемся завершения асинхронной операции.
                await roles.CreateAsync(new IdentityRole(role));
        // Выполняем следующую ветку только при истинности условия.
        if (!await db.Halls.AnyAsync())
        {
            // Вычисляем names для дальнейших проверок или отображения.
            var names = new[]
            {
                // Передаём очередной аргумент или задаём элемент коллекции.
                "Синий зал",
                // Передаём очередной аргумент или задаём элемент коллекции.
                "Красный зал",
                // Передаём очередной аргумент или задаём элемент коллекции.
                "Зелёный зал",
                "Оранжевый зал"
            };
            // Вычисляем colors для дальнейших проверок или отображения.
            var colors = new[]
            {
                // Передаём очередной аргумент или задаём элемент коллекции.
                "blue",
                // Передаём очередной аргумент или задаём элемент коллекции.
                "red",
                // Передаём очередной аргумент или задаём элемент коллекции.
                "green",
                "orange"
            };
            // Повторяем следующий блок для заданного диапазона значений.
            for (var i = 0; i < 4; i++)
            {
                // Вычисляем hall для дальнейших проверок или отображения.
                var hall = new Hall
                {
                    // Записываем вычисленное значение в name.
                    Name = names[i],
                    // Записываем вычисленное значение в color.
                    Color = colors[i],
                    // Записываем вычисленное значение в rows.
                    Rows = 6 + i,
                    // Записываем вычисленное значение в seats per row.
                    SeatsPerRow = 10 + i,
                    // Записываем вычисленное значение в description.
                    Description = "Лазерная проекция 4K, объёмный звук 7.1 и удобные кресла с широкими подлокотниками."
                };
                // Повторяем следующий блок для заданного диапазона значений.
                for (var row = 1; row <= hall.Rows; row++)
                    // Повторяем следующий блок для заданного диапазона значений.
                    for (var seat = 1; seat <= hall.SeatsPerRow; seat++)
                        // Записываем вычисленное значение в add(new seat { row.
                        hall.Seats.Add(new Seat { Row = row, Number = seat });
                // Добавляем новую сущность в контекст для последующего сохранения.
                db.Halls.Add(hall);
            }

            // Сохраняем накопленные изменения в SQLite.
            await db.SaveChangesAsync();
        }

        // Выполняем следующую ветку только при истинности условия.
        if (!await db.Movies.AnyAsync())
        {
            // Вычисляем today для дальнейших проверок или отображения.
            var today = services.GetRequiredService<CinemaClock>().Now.Date;
            // Добавляем новую сущность в контекст для последующего сохранения.
            db.Movies.AddRange(ReferenceContent.Movies(environment.ContentRootPath, today));
            // Сохраняем накопленные изменения в SQLite.
            await db.SaveChangesAsync();
        }

        // Выполняем следующую ветку только при истинности условия.
        if (!await db.Sessions.AnyAsync())
        {
            // Получаем movies из базы или подготавливаем запрос к ней.
            var movies = await db.Movies.OrderBy(x => x.Id).ToListAsync();
            // Получаем halls из базы или подготавливаем запрос к ней.
            var halls = await db.Halls.OrderBy(x => x.Id).ToListAsync();
            // Вычисляем today для дальнейших проверок или отображения.
            var today = services.GetRequiredService<CinemaClock>().Now.Date;
            // Повторяем следующий блок для заданного диапазона значений.
            for (var day = 0; day < 31; day++)
                // Повторяем следующий блок для заданного диапазона значений.
                for (var h = 0; h < halls.Count; h++)
                    // Повторяем следующий блок для заданного диапазона значений.
                    for (var slot = 0; slot < 4; slot++)
                    {
                        // Вычисляем available для дальнейших проверок или отображения.
                        var available = movies.Where(x => x.ReleaseDate <= today.AddDays(day)).ToList();
                        // Вычисляем movie для дальнейших проверок или отображения.
                        var movie = available[(day * 4 + h * 4 + slot) % available.Count];
                        // Вычисляем starts для дальнейших проверок или отображения.
                        var starts = today.AddDays(day).AddHours(10 + slot * 3.5);
                        // Добавляем новую сущность в контекст для последующего сохранения.
                        db.Sessions.Add(new Session { MovieId = movie.Id, HallId = halls[h].Id, StartsAt = starts, EndsAt = starts.AddMinutes(movie.DurationMinutes + 20), Price = 350 + slot * 70 + h * 30 });
                    }
        }

        // Выполняем следующую ветку только при истинности условия.
        if (!await db.Services.AnyAsync())
            // Добавляем новую сущность в контекст для последующего сохранения.
            db.Services.AddRange(new Service { Name = "Кинобар", Description = "Свежий попкорн, напитки и любимые закуски перед сеансом.", Price = 250, ImagePath = "/images/reference/340_about-1_big.jpg" }, new Service { Name = "Аренда зала", Description = "Ваш фильм на большом экране: день рождения, встреча друзей или презентация.", Price = 5000, ImagePath = "/images/reference/674_about-3_big.jpg" }, new Service { Name = "Подарочный вечер", Description = "Подарочный сертификат на два билета и набор из кинобара.", Price = 1500, ImagePath = "/images/reference/610_11.jpg" });
        // Выполняем следующую ветку только при истинности условия.
        if (!await db.FaqItems.AnyAsync())
            // Добавляем новую сущность в контекст для последующего сохранения.
            db.FaqItems.AddRange(new FaqItem { Question = "Как купить билет?", Answer = "Откройте афишу, выберите сеанс и места, войдите в аккаунт и подтвердите оплату. Билет появится в личном кабинете.", SortOrder = 1 }, new FaqItem { Question = "Сколько действует бронь?", Answer = "Места закрепляются за вами на 10 минут. После истечения времени неоплаченная бронь освобождается.", SortOrder = 2 }, new FaqItem { Question = "Можно ли вернуть билет?", Answer = "До начала сеанса обратитесь к менеджеру через контакты, указав номер заказа. Менеджер может оформить возврат.", SortOrder = 3 }, new FaqItem { Question = "Где найти билет?", Answer = "После оформления заказа электронный билет доступен в личном кабинете в разделе заказов.", SortOrder = 4 });
        // Выполняем следующую ветку только при истинности условия.
        if (!await db.SiteSettings.AnyAsync())
            // Добавляем новую сущность в контекст для последующего сохранения.
            db.SiteSettings.AddRange(new[] { new SiteSetting { Key = "Name", Value = "Кинотеатр им. Павлюка" }, new SiteSetting { Key = "Tagline", Value = "Кинотеатр им. Павлюка" }, new SiteSetting { Key = "About", Value = "Кинотеатр им. Павлюка — место, где большой экран объединяет. Четыре уютных зала, насыщенный звук и истории, которые остаются с вами после финальных титров. Мы создали пространство для встреч, открытий и маленьких традиций." }, new SiteSetting { Key = "Address", Value = "Ижевск, ул. Карла Маркса, 242" }, new SiteSetting { Key = "Phone", Value = "+7 (000) 000-00-00" }, new SiteSetting { Key = "Email", Value = "hello@cinema.example" }, new SiteSetting { Key = "Hours", Value = "Ежедневно, 09:30 — 00:00" }, new SiteSetting { Key = "MapQuery", Value = "Ижевск Карла Маркса 242" }, new SiteSetting { Key = "SocialVk", Value = "https://vk.com" }, new SiteSetting { Key = "SocialTelegram", Value = "https://t.me" }, new SiteSetting { Key = "Promo", Value = "Больше впечатлений на большом экране. Выберите свой следующий фильм." } });
        // Выполняем следующую ветку только при истинности условия.
        if (!await db.TeamMembers.AnyAsync())
            // Добавляем новую сущность в контекст для последующего сохранения.
            db.TeamMembers.AddRange(new TeamMember { Name = "Участник 1", Role = "Серверная часть", Tasks = "База данных, контроллеры, бронирование" }, new TeamMember { Name = "Участник 2", Role = "Интерфейс", Tasks = "Дизайн, адаптивность, схема зала" }, new TeamMember { Name = "Участник 3", Role = "Тестирование", Tasks = "Проверка сценариев и ограничений доступа" }, new TeamMember { Name = "Участник 4", Role = "Документация", Tasks = "ТЗ, инструкция и описание проекта" });
        // Сохраняем накопленные изменения в SQLite.
        await db.SaveChangesAsync();
        // Выполняем следующую ветку только при истинности условия.
        if (!await db.Reviews.AnyAsync())
        {
            // Вычисляем visitor для дальнейших проверок или отображения.
            var visitor = new ApplicationUser
            {
                // Записываем вычисленное значение в user name.
                UserName = "visitor@cinema.example",
                // Записываем вычисленное значение в email.
                Email = "visitor@cinema.example",
                // Записываем вычисленное значение в display name.
                DisplayName = "Посетитель",
                // Записываем вычисленное значение в email confirmed.
                EmailConfirmed = true
            };
            // Асинхронно получаем existing, не блокируя поток сервера.
            var existing = await users.FindByEmailAsync(visitor.Email);
            // Обрабатываем отсутствие записи или необязательного значения.
            if (existing == null)
            {
                // No password: this record credits seed reviews, it is not a demo login.
                // Асинхронно получаем result, не блокируя поток сервера.
                var result = await users.CreateAsync(visitor);
                // Выполняем следующую ветку только при истинности условия.
                if (!result.Succeeded)
                    // Прерываем выполнение при нарушении обязательного условия.
                    throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
                // Дожидаемся завершения асинхронной операции.
                await users.AddToRoleAsync(visitor, "User");
            }
            // Обрабатываем альтернативный случай предыдущей проверки.
            else
                // Записываем вычисленное значение в visitor.
                visitor = existing;
            // Добавляем новую сущность в контекст для последующего сохранения.
            db.Reviews.AddRange(new[] { "Отличный звук и очень уютные залы. Обязательно вернёмся!", "Удобно выбирать места заранее. Вечер с семьёй удался.", "Красивое пространство и внимательные сотрудники." }.Select(text => new Review { UserId = visitor.Id, Rating = 5, Text = text, Status = ReviewStatus.Approved }));
            // Сохраняем накопленные изменения в SQLite.
            await db.SaveChangesAsync();
        }

        // Дожидаемся завершения асинхронной операции.
        await ReferenceContent.SeedBannersAsync(db);
        // Дожидаемся завершения асинхронной операции.
        await PublicCopy.RefreshAsync(db);
        // Дожидаемся завершения асинхронной операции.
        await CoverCatalog.RefreshAsync(db, environment.ContentRootPath, services.GetRequiredService<CinemaClock>().Now.Date);
        // Вычисляем email для дальнейших проверок или отображения.
        var email = configuration["SeedAdmin:Email"];
        // Вычисляем password для дальнейших проверок или отображения.
        var password = configuration["SeedAdmin:Password"];
        // Local first launch: prepare the account used to manage the cinema.
        // Existing administrators and passwords are never overwritten.
        // Выполняем следующую ветку только при истинности условия.
        if (environment.IsDevelopment() && string.IsNullOrWhiteSpace(email)
            // Продолжаем выражение, начатое на предыдущей строке.
            && !(await users.GetUsersInRoleAsync("Admin")).Any()
            // Продолжаем выражение, начатое на предыдущей строке.
            && await users.FindByEmailAsync("admin@cinema.local") == null)
        {
            // Записываем вычисленное значение в email.
            email = "admin@cinema.local";
            // Записываем вычисленное значение в password.
            password = "Pavlyuk2026!";
        }

        // Выполняем следующую ветку только при истинности условия.
        if (environment.IsDevelopment() && !string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(password))
        {
            // Асинхронно получаем admin, не блокируя поток сервера.
            var admin = await users.FindByEmailAsync(email);
            // Обрабатываем отсутствие записи или необязательного значения.
            if (admin == null)
            {
                // Записываем вычисленное значение в admin.
                admin = new ApplicationUser
                {
                    // Записываем вычисленное значение в user name.
                    UserName = email,
                    // Записываем вычисленное значение в email.
                    Email = email,
                    // Записываем вычисленное значение в display name.
                    DisplayName = "Администратор",
                    // Записываем вычисленное значение в email confirmed.
                    EmailConfirmed = true
                };
                // Асинхронно получаем result, не блокируя поток сервера.
                var result = await users.CreateAsync(admin, password);
                // Выполняем следующую ветку только при истинности условия.
                if (!result.Succeeded)
                    // Прерываем выполнение при нарушении обязательного условия.
                    throw new InvalidOperationException(string.Join("; ", result.Errors.Select(x => x.Description)));
            }

            // Асинхронно получаем role result, не блокируя поток сервера.
            var roleResult = await users.AddToRoleAsync(admin, "Admin");
            // Проверяем роль пользователя перед выполнением защищённого действия.
            if (!roleResult.Succeeded && !await users.IsInRoleAsync(admin, "Admin"))
                // Прерываем выполнение при нарушении обязательного условия.
                throw new InvalidOperationException("Не удалось назначить администратора.");
        }
    }
}
