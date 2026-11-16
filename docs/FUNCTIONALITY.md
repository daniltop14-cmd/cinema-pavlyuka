# Что делает каждый файл проекта

Этот путеводитель связывает файлы с функциями сайта. Путь в первом столбце открывает файл прямо на GitHub. Комментарии внутри исходников подробно объясняют отдельные действия.

В каждой основной папке есть короткий README с назначением раздела.

## Запуск, база и модели

| Файл | Какую функцию обеспечивает |
|---|---|
| [`Program.cs`](../Program.cs) | Запускает сервер, подключает SQLite, Identity, сервисы, маршруты, защиту запросов и начальные данные. |
| [`Cinema.csproj`](../Cinema.csproj) | Определяет .NET 8 и зависимости, необходимые для сборки приложения. |
| [`Cinema.sln`](../Cinema.sln) | Открывает проект как решение в Visual Studio. |
| [`appsettings.json`](../appsettings.json) | Содержит общие настройки приложения, включая строку подключения к SQLite. |
| [`Properties/launchSettings.json`](../Properties/launchSettings.json) | Задаёт локальные профили запуска для Visual Studio и `dotnet run`. |
| [`Models/Entities.cs`](../Models/Entities.cs) | Описывает фильмы, залы, места, сеансы, заказы, отзывы, пользователей и другие сущности. |
| [`Data/CinemaDbContext.cs`](../Data/CinemaDbContext.cs) | Связывает сущности с таблицами SQLite и задаёт отношения, индексы и ограничения. |
| [`Data/CinemaDesignTimeFactory.cs`](../Data/CinemaDesignTimeFactory.cs) | Создаёт контекст базы для команд миграций EF Core без запуска сайта. |
| [`Data/SeedData.cs`](../Data/SeedData.cs) | При первом запуске создаёт роли администратора и менеджера, фильмы, залы и сеансы. |
| [`Data/ReferenceContent.cs`](../Data/ReferenceContent.cs) | Подготавливает начальные тексты, баннеры и содержимое информационных разделов. |
| [`Data/CoverCatalog.cs`](../Data/CoverCatalog.cs) | Назначает фильмам актуальные обложки, в том числе в уже созданной базе. |
| [`Data/PublicCopy.cs`](../Data/PublicCopy.cs) | Обновляет сохранённые публичные тексты при изменении оформления сайта. |
| [`Data/reference-movies.json`](../Data/reference-movies.json) | Хранит исходные сведения о фильмах для заполнения каталога. |
| [`Migrations/`](../Migrations/) | Содержит схему базы, которую EF Core применяет при запуске. Эти файлы создаются инструментом миграций. |
| [`ViewModels/AccountModels.cs`](../ViewModels/AccountModels.cs) | Описывает поля и проверки форм регистрации, входа и профиля. |
| [`ViewModels/ContentModel.cs`](../ViewModels/ContentModel.cs) | Передаёт поля редактирования содержимого в админке. |
| [`ViewModels/PageModels.cs`](../ViewModels/PageModels.cs) | Передаёт контроллерами подготовленные данные на публичные и административные страницы. |

## Публичный сервер и личный кабинет

| Файл | Какую функцию обеспечивает |
|---|---|
| [`Controllers/HomeController.cs`](../Controllers/HomeController.cs) | Загружает главную страницу, расписание, баннеры и информационные страницы. |
| [`Controllers/MoviesController.cs`](../Controllers/MoviesController.cs) | Показывает афишу, фильтры и сведения о выбранном фильме. |
| [`Controllers/BookingController.cs`](../Controllers/BookingController.cs) | Принимает выбор мест, оформляет и отменяет заказ, показывает оплату и билет. |
| [`Controllers/AccountController.cs`](../Controllers/AccountController.cs) | Регистрирует пользователей, выполняет вход и выход, показывает профиль и заказы. |
| [`Controllers/ReviewsController.cs`](../Controllers/ReviewsController.cs) | Выводит опубликованные отзывы и принимает новые на модерацию. |
| [`Services/BookingService.cs`](../Services/BookingService.cs) | Проверяет доступность кресел и цену в базе, создаёт заказ в транзакции и меняет его статус. |
| [`Services/BookingCleanupService.cs`](../Services/BookingCleanupService.cs) | Освобождает кресла после истечения времени неоплаченной брони. |
| [`Services/PaymentService.cs`](../Services/PaymentService.cs) | Задаёт интерфейс оплаты и текущую демонстрационную реализацию без платёжного провайдера. |
| [`Services/CinemaClock.cs`](../Services/CinemaClock.cs) | Приводит время сеансов к местному времени кинотеатра. |

## Публичные страницы

| Файл | Что видит посетитель |
|---|---|
| [`Views/_ViewStart.cshtml`](../Views/_ViewStart.cshtml) | Назначает общий шаблон публичным страницам. |
| [`Views/_ViewImports.cshtml`](../Views/_ViewImports.cshtml) | Подключает типы и Tag Helpers, которые используются в Razor-разметке. |
| [`Views/Shared/_Layout.cshtml`](../Views/Shared/_Layout.cshtml) | Общая шапка, меню, подвал, подключение CSS и JavaScript. |
| [`Views/Shared/_MovieCard.cshtml`](../Views/Shared/_MovieCard.cshtml) | Повторно используемая карточка фильма. |
| [`Views/Shared/_ScheduleMovie.cshtml`](../Views/Shared/_ScheduleMovie.cshtml) | Карточка фильма с доступными сеансами в расписании. |
| [`Views/Shared/_Pager.cshtml`](../Views/Shared/_Pager.cshtml) | Переход между страницами списков. |
| [`Views/Home/Index.cshtml`](../Views/Home/Index.cshtml) | Главная: слайдер, даты, расписание, новости и другие блоки. |
| [`Views/Home/About.cshtml`](../Views/Home/About.cshtml) | Страница «О кинотеатре». |
| [`Views/Home/Services.cshtml`](../Views/Home/Services.cshtml) | Описание услуг кинотеатра. |
| [`Views/Home/News.cshtml`](../Views/Home/News.cshtml) | Новости и объявления. |
| [`Views/Home/Contacts.cshtml`](../Views/Home/Contacts.cshtml) | Контакты, адрес и форма обращения. |
| [`Views/Home/Returns.cshtml`](../Views/Home/Returns.cshtml) | Информация о возвратах. |
| [`Views/Home/Legal.cshtml`](../Views/Home/Legal.cshtml) | Правовая информация для посетителей. |
| [`Views/Home/Team.cshtml`](../Views/Home/Team.cshtml) | Страница команды кинотеатра. |
| [`Views/Home/Error.cshtml`](../Views/Home/Error.cshtml) | Страница ошибки приложения. |
| [`Views/Movies/Index.cshtml`](../Views/Movies/Index.cshtml) | Афиша с фильтрами. |
| [`Views/Movies/Details.cshtml`](../Views/Movies/Details.cshtml) | Описание фильма и доступные сеансы. |
| [`Views/Booking/Seats.cshtml`](../Views/Booking/Seats.cshtml) | Схема зала и выбор кресел. |
| [`Views/Booking/Checkout.cshtml`](../Views/Booking/Checkout.cshtml) | Детали заказа и подтверждение оплаты. |
| [`Views/Booking/Ticket.cshtml`](../Views/Booking/Ticket.cshtml) | Электронный билет с данными сеанса и места. |
| [`Views/Account/Login.cshtml`](../Views/Account/Login.cshtml) | Форма входа. |
| [`Views/Account/Register.cshtml`](../Views/Account/Register.cshtml) | Форма регистрации. |
| [`Views/Account/Profile.cshtml`](../Views/Account/Profile.cshtml) | Профиль пользователя. |
| [`Views/Account/Password.cshtml`](../Views/Account/Password.cshtml) | Изменение пароля. |
| [`Views/Account/Orders.cshtml`](../Views/Account/Orders.cshtml) | История и состояния заказов. |
| [`Views/Account/Denied.cshtml`](../Views/Account/Denied.cshtml) | Сообщение об отсутствии прав доступа. |
| [`Views/Account/_AccountNav.cshtml`](../Views/Account/_AccountNav.cshtml) | Навигация по личному кабинету. |
| [`Views/Account/_Management.cshtml`](../Views/Account/_Management.cshtml) | Ссылка на управление кинотеатром для уполномоченного пользователя. |
| [`Views/Reviews/Index.cshtml`](../Views/Reviews/Index.cshtml) | Опубликованные отзывы и форма нового отзыва. |

## Панель администратора

| Файл | Какую функцию обеспечивает |
|---|---|
| [`Areas/Admin/Views/_ViewStart.cshtml`](../Areas/Admin/Views/_ViewStart.cshtml) | Назначает общий шаблон страницам админки. |
| [`Areas/Admin/Views/_ViewImports.cshtml`](../Areas/Admin/Views/_ViewImports.cshtml) | Подключает типы и Tag Helpers для форм админки. |
| [`Areas/Admin/Views/Shared/_AdminLayout.cshtml`](../Areas/Admin/Views/Shared/_AdminLayout.cshtml) | Боковое меню и общий каркас панели управления. |
| [`Areas/Admin/Views/Shared/_Pager.cshtml`](../Areas/Admin/Views/Shared/_Pager.cshtml) | Постраничная навигация административных списков. |
| [`Areas/Admin/Controllers/DashboardController.cs`](../Areas/Admin/Controllers/DashboardController.cs) | Собирает показатели для главной страницы админки. |
| [`Areas/Admin/Views/Dashboard/Index.cshtml`](../Areas/Admin/Views/Dashboard/Index.cshtml) | Показывает сводку заказов, сеансов и других данных. |
| [`Areas/Admin/Controllers/MoviesController.cs`](../Areas/Admin/Controllers/MoviesController.cs) | Создаёт и редактирует фильмы, проверяет загружаемые обложки. |
| [`Areas/Admin/Views/Movies/Index.cshtml`](../Areas/Admin/Views/Movies/Index.cshtml) | Список фильмов с действиями администратора. |
| [`Areas/Admin/Views/Movies/Edit.cshtml`](../Areas/Admin/Views/Movies/Edit.cshtml) | Форма фильма и обложки. |
| [`Areas/Admin/Controllers/SessionsController.cs`](../Areas/Admin/Controllers/SessionsController.cs) | Создаёт и редактирует сеансы, проверяет пересечение времени в зале. |
| [`Areas/Admin/Views/Sessions/Index.cshtml`](../Areas/Admin/Views/Sessions/Index.cshtml) | Список сеансов. |
| [`Areas/Admin/Views/Sessions/Edit.cshtml`](../Areas/Admin/Views/Sessions/Edit.cshtml) | Форма времени, фильма, зала и цены сеанса. |
| [`Areas/Admin/Controllers/HallsController.cs`](../Areas/Admin/Controllers/HallsController.cs) | Меняет зал, добавляет ряды и места, сохраняет уже выданные билеты. |
| [`Areas/Admin/Views/Halls/Index.cshtml`](../Areas/Admin/Views/Halls/Index.cshtml) | Список залов. |
| [`Areas/Admin/Views/Halls/Edit.cshtml`](../Areas/Admin/Views/Halls/Edit.cshtml) | Форма зала и управления недоступными креслами. |
| [`Areas/Admin/Controllers/BookingsController.cs`](../Areas/Admin/Controllers/BookingsController.cs) | Даёт администратору и менеджеру просмотр заказов и действия с ними. |
| [`Areas/Admin/Views/Bookings/Index.cshtml`](../Areas/Admin/Views/Bookings/Index.cshtml) | Таблица заказов и доступные действия. |
| [`Areas/Admin/Controllers/ReviewsController.cs`](../Areas/Admin/Controllers/ReviewsController.cs) | Модерирует отзывы. |
| [`Areas/Admin/Views/Reviews/Index.cshtml`](../Areas/Admin/Views/Reviews/Index.cshtml) | Список отзывов с кнопками модерации. |
| [`Areas/Admin/Controllers/UsersController.cs`](../Areas/Admin/Controllers/UsersController.cs) | Меняет роли и блокирует учётные записи. |
| [`Areas/Admin/Views/Users/Index.cshtml`](../Areas/Admin/Views/Users/Index.cshtml) | Список пользователей и управление доступом. |
| [`Areas/Admin/Controllers/ContentController.cs`](../Areas/Admin/Controllers/ContentController.cs) | Сохраняет баннеры, тексты, новости, услуги, FAQ и обращения. |
| [`Areas/Admin/Views/Content/Index.cshtml`](../Areas/Admin/Views/Content/Index.cshtml) | Список редактируемых разделов сайта. |
| [`Areas/Admin/Views/Content/Edit.cshtml`](../Areas/Admin/Views/Content/Edit.cshtml) | Форма редактирования выбранного содержимого. |
| [`Areas/Admin/Views/Content/Settings.cshtml`](../Areas/Admin/Views/Content/Settings.cshtml) | Настройки главной страницы и контактов. |
| [`Areas/Admin/Views/Content/Messages.cshtml`](../Areas/Admin/Views/Content/Messages.cshtml) | Обращения, отправленные посетителями. |

## Оформление и проверки

| Файл | Какую функцию обеспечивает |
|---|---|
| [`wwwroot/js/site.js`](../wwwroot/js/site.js) | Управляет слайдером, мобильным меню, поиском, выбором мест, таймером и печатью билета в браузере. |
| [`wwwroot/css/site.css`](../wwwroot/css/site.css) | Базовые стили страниц, форм, билетов, залов и адаптивная сетка. |
| [`wwwroot/css/reference.css`](../wwwroot/css/reference.css) | Светлое оформление шапки, баннеров, расписания и админки. |
| [`wwwroot/css/fonts.css`](../wwwroot/css/fonts.css) | Подключает локальные файлы шрифта Montserrat. |
| [`wwwroot/images/covers/`](../wwwroot/images/covers/) | Хранит обложки фильмов и широкие изображения баннеров. |
| [`tests/integration.cjs`](../tests/integration.cjs) | Проверяет через HTTP страницы, вход, бронирование, роли и административные действия на временной SQLite. |

Цепочка работы: **браузер → маршрут в `Program.cs` → контроллер → сервис и `CinemaDbContext` → Razor-страница → CSS и JavaScript в браузере**.
