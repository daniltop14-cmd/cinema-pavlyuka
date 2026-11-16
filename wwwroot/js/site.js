// Этот скрипт работает в браузере после загрузки Razor-страницы; он меняет отображение, а итоговые проверки заказа выполняет сервер.
'use strict';
// Находим кнопку меню из _Layout.cshtml; по нажатию меняем видимость навигации и aria-expanded.
document.querySelector('.menu-toggle')?.addEventListener('click', function () {
  const menu = document.getElementById('main-nav');
  const open = menu.classList.toggle('open');
  this.setAttribute('aria-expanded', String(open));
});
// Берём слайды из HTML по data-slide; их количество определяет допустимые индексы.
const slides = [...document.querySelectorAll('[data-slide]')];
// Кнопки с data-goto содержат номер слайда в data-атрибуте Razor-разметки.
const dots = [...document.querySelectorAll('[data-goto]')];
let slideIndex = 0;
// Нормализуем индекс по числу слайдов и отмечаем активные слайд и индикатор.
function showSlide(index) {
  if (!slides.length) return;
  slideIndex = (index + slides.length) % slides.length;
  slides.forEach((el, i) => { el.classList.toggle('is-active', i === slideIndex); el.setAttribute('aria-hidden', String(i !== slideIndex)); });
  dots.forEach((el, i) => { el.classList.toggle('active', i === slideIndex); el.setAttribute('aria-pressed', String(i === slideIndex)); });
}
dots.forEach(el => el.addEventListener('click', () => showSlide(Number(el.dataset.goto))));
// Стрелки передают следующий индекс той же функции; остаток от деления зацикливает переход.
document.querySelector('[data-next]')?.addEventListener('click', () => showSlide(slideIndex + 1));
document.querySelector('[data-prev]')?.addEventListener('click', () => showSlide(slideIndex - 1));
// Manual slider: no unexpected motion or focus loss while reading.
// Берём форму выбора мест из Seats.cshtml; цена в data-price нужна только для предварительного показа.
const seatForm = document.getElementById('seat-form');
if (seatForm) {
  const price = Number(seatForm.dataset.price);
  // Читаем отмеченные seatIds, ограничиваем выбор и обновляем сумму; BookingService проверит места и цену заново.
  seatForm.addEventListener('change', event => {
    let checked = [...seatForm.querySelectorAll('input[name="seatIds"]:checked')];
    if (checked.length > 12) { event.target.checked = false; checked = checked.filter(el => el !== event.target); }
    document.getElementById('selected-seats').textContent = checked.length ? checked.map(el => `Ряд ${el.dataset.row}, место ${el.dataset.number}`).join(' · ') : 'Выберите места на схеме';
    // Цена берётся из data-price страницы Seats.cshtml, но при POST сервер снова читает цену сеанса из SQLite.
    document.getElementById('total').textContent = (checked.length * price).toLocaleString('ru-RU') + ' ₽';
    const button = document.getElementById('reserve-button');
    if (button) button.disabled = checked.length === 0;
  });
}
// Таймер читает время окончания брони из HTML; действительность заказа всегда проверяется на сервере.
const expiry = document.querySelector('[data-countdown]');
if (expiry) {
  const tick = () => {
    const seconds = Math.max(0, Math.floor((Date.parse(expiry.dataset.countdown) - Date.now()) / 1000));
    document.getElementById('countdown').textContent = seconds ? `${Math.floor(seconds / 60)}:${String(seconds % 60).padStart(2, '0')}` : 'время истекло';
    const button = document.getElementById('pay-button'); if (button && !seconds) button.disabled = true;
  };
  tick(); setInterval(tick, 1000);
}
document.querySelectorAll('[data-print]').forEach(el => el.addEventListener('click', () => window.print()));
// Подтверждение удаления или возврата работает в браузере; разрешение действия контроллер проверяет отдельно.
document.querySelectorAll('form[data-confirm]').forEach(form => form.addEventListener('submit', event => { if (!window.confirm(form.dataset.confirm)) event.preventDefault(); }));

// Carousel follows the reference layout: a centred banner and neighbour previews.
// HomeController собрал баннеры из SiteSettings SQLite, а Index.cshtml выдал их как ссылки и кнопки с data-атрибутами.
const bannerHero = document.querySelector('.reference-hero');
if (bannerHero) {
  const track = bannerHero.querySelector('[data-banner-track]');
  const count = Number(bannerHero.dataset.bannerCount);
  const indicators = [...bannerHero.querySelectorAll('[data-banner-index]')];
  let current = 0;
  let paused = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  let hovering = false;
  // Двигаем всю ленту; два крайних клона в HTML позволяют показывать соседние превью без пустого края.
  const showBanner = index => {
    current = (index + count) % count;
    track.style.transform = `translateX(calc((100vw - var(--banner-width)) / 2 - (var(--banner-width) + 16px) * ${current + 1}))`;
    indicators.forEach((el, i) => { el.classList.toggle('active', i === current); el.setAttribute('aria-pressed', String(i === current)); });
    bannerHero.querySelectorAll('[data-banner-slide]').forEach((el, i) => { el.tabIndex = i === current ? 0 : -1; });
  };
  indicators.forEach(el => el.addEventListener('click', () => showBanner(Number(el.dataset.bannerIndex))));
  bannerHero.querySelector('[data-banner-prev]').addEventListener('click', () => showBanner(current - 1));
  bannerHero.querySelector('[data-banner-next]').addEventListener('click', () => showBanner(current + 1));
  const pause = bannerHero.querySelector('[data-banner-pause]');
  pause.addEventListener('click', () => { paused = !paused; pause.textContent = paused ? '▶' : 'Ⅱ'; pause.setAttribute('aria-label', paused ? 'Запустить слайдер' : 'Остановить слайдер'); });
  bannerHero.addEventListener('mouseenter', () => { hovering = true; });
  bannerHero.addEventListener('mouseleave', () => { hovering = false; });
  // Автопереход каждые семь секунд не срабатывает при паузе, наведении и фокусе внутри слайдера.
  setInterval(() => { if (!paused && !hovering && !bannerHero.contains(document.activeElement)) showBanner(current + 1); }, 7000);
}
// Лента дат построена в Index.cshtml от CinemaClock.Now; кнопка прокручивает её по горизонтали.
document.querySelector('[data-dates-next]')?.addEventListener('click', () => document.getElementById('home-dates').scrollBy({left:450,behavior:'smooth'}));
const filmSearch = document.getElementById('film-search');
document.querySelector('.search-toggle')?.addEventListener('click', function () {
  filmSearch.hidden = !filmSearch.hidden;
  this.setAttribute('aria-expanded', String(!filmSearch.hidden));
  if (!filmSearch.hidden) filmSearch.focus();
});
filmSearch?.addEventListener('input', () => {
  // Имена карточек приходят из Model.Movies и записаны Razor в data-film-title; запрос к серверу здесь не нужен.
  const term = filmSearch.value.trim().toLocaleLowerCase('ru-RU');
  const cards = [...document.querySelectorAll('[data-film-title]')];
  cards.forEach(card => { card.hidden = !card.dataset.filmTitle.includes(term); });
  const empty = document.querySelector('.search-empty');
  if (empty) empty.hidden = cards.some(card => !card.hidden);
});
