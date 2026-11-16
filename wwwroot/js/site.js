// Оживляет интерфейс: меню, баннеры, поиск, места, таймер и печать.
// Выполняем шаг браузерной логики этого блока.
'use strict';
// Подписываемся на событие пользователя и обновляем интерфейс.
document.querySelector('.menu-toggle')?.addEventListener('click', function () {
  // Сохраняем элемент страницы или состояние, нужное обработчику ниже.
  const menu = document.getElementById('main-nav');
  // Сохраняем элемент страницы или состояние, нужное обработчику ниже.
  const open = menu.classList.toggle('open');
  // Выполняем шаг браузерной логики этого блока.
  this.setAttribute('aria-expanded', String(open));
});
// Сохраняем элемент страницы или состояние, нужное обработчику ниже.
const slides = [...document.querySelectorAll('[data-slide]')];
// Сохраняем элемент страницы или состояние, нужное обработчику ниже.
const dots = [...document.querySelectorAll('[data-goto]')];
// Сохраняем элемент страницы или состояние, нужное обработчику ниже.
let slideIndex = 0;
// Определяем функцию, которая обновляет отображаемое состояние.
function showSlide(index) {
  // Выполняем действие только при выполнении условия.
  if (!slides.length) return;
  // Выполняем шаг браузерной логики этого блока.
  slideIndex = (index + slides.length) % slides.length;
  // Меняем CSS-класс элемента, чтобы обновить его внешний вид.
  slides.forEach((el, i) => { el.classList.toggle('is-active', i === slideIndex); el.setAttribute('aria-hidden', String(i !== slideIndex)); });
  // Меняем CSS-класс элемента, чтобы обновить его внешний вид.
  dots.forEach((el, i) => { el.classList.toggle('active', i === slideIndex); el.setAttribute('aria-pressed', String(i === slideIndex)); });
}
// Подписываемся на событие пользователя и обновляем интерфейс.
dots.forEach(el => el.addEventListener('click', () => showSlide(Number(el.dataset.goto))));
// Подписываемся на событие пользователя и обновляем интерфейс.
document.querySelector('[data-next]')?.addEventListener('click', () => showSlide(slideIndex + 1));
// Подписываемся на событие пользователя и обновляем интерфейс.
document.querySelector('[data-prev]')?.addEventListener('click', () => showSlide(slideIndex - 1));
// Manual slider: no unexpected motion or focus loss while reading.
// Сохраняем элемент страницы или состояние, нужное обработчику ниже.
const seatForm = document.getElementById('seat-form');
// Выполняем действие только при выполнении условия.
if (seatForm) {
  // Сохраняем элемент страницы или состояние, нужное обработчику ниже.
  const price = Number(seatForm.dataset.price);
  // Подписываемся на событие пользователя и обновляем интерфейс.
  seatForm.addEventListener('change', event => {
    // Сохраняем элемент страницы или состояние, нужное обработчику ниже.
    let checked = [...seatForm.querySelectorAll('input[name="seatIds"]:checked')];
    // Выполняем действие только при выполнении условия.
    if (checked.length > 12) { event.target.checked = false; checked = checked.filter(el => el !== event.target); }
    // Обновляем видимый текст на странице.
    document.getElementById('selected-seats').textContent = checked.length ? checked.map(el => `Ряд ${el.dataset.row}, место ${el.dataset.number}`).join(' · ') : 'Выберите места на схеме';
    // Обновляем видимый текст на странице.
    document.getElementById('total').textContent = (checked.length * price).toLocaleString('ru-RU') + ' ₽';
    // Сохраняем элемент страницы или состояние, нужное обработчику ниже.
    const button = document.getElementById('reserve-button');
    // Выполняем действие только при выполнении условия.
    if (button) button.disabled = checked.length === 0;
  });
}
// Сохраняем элемент страницы или состояние, нужное обработчику ниже.
const expiry = document.querySelector('[data-countdown]');
// Выполняем действие только при выполнении условия.
if (expiry) {
  // Сохраняем элемент страницы или состояние, нужное обработчику ниже.
  const tick = () => {
    // Сохраняем элемент страницы или состояние, нужное обработчику ниже.
    const seconds = Math.max(0, Math.floor((Date.parse(expiry.dataset.countdown) - Date.now()) / 1000));
    // Обновляем видимый текст на странице.
    document.getElementById('countdown').textContent = seconds ? `${Math.floor(seconds / 60)}:${String(seconds % 60).padStart(2, '0')}` : 'время истекло';
    // Сохраняем элемент страницы или состояние, нужное обработчику ниже.
    const button = document.getElementById('pay-button'); if (button && !seconds) button.disabled = true;
  };
  // Запускаем периодическое обновление интерфейса.
  tick(); setInterval(tick, 1000);
}
// Подписываемся на событие пользователя и обновляем интерфейс.
document.querySelectorAll('[data-print]').forEach(el => el.addEventListener('click', () => window.print()));
// Подписываемся на событие пользователя и обновляем интерфейс.
document.querySelectorAll('form[data-confirm]').forEach(form => form.addEventListener('submit', event => { if (!window.confirm(form.dataset.confirm)) event.preventDefault(); }));

// Carousel follows the reference layout: a centred banner and neighbour previews.
// Сохраняем элемент страницы или состояние, нужное обработчику ниже.
const bannerHero = document.querySelector('.reference-hero');
// Выполняем действие только при выполнении условия.
if (bannerHero) {
  // Сохраняем элемент страницы или состояние, нужное обработчику ниже.
  const track = bannerHero.querySelector('[data-banner-track]');
  // Сохраняем элемент страницы или состояние, нужное обработчику ниже.
  const count = Number(bannerHero.dataset.bannerCount);
  // Сохраняем элемент страницы или состояние, нужное обработчику ниже.
  const indicators = [...bannerHero.querySelectorAll('[data-banner-index]')];
  // Сохраняем элемент страницы или состояние, нужное обработчику ниже.
  let current = 0;
  // Сохраняем элемент страницы или состояние, нужное обработчику ниже.
  let paused = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  // Сохраняем элемент страницы или состояние, нужное обработчику ниже.
  let hovering = false;
  // Сохраняем элемент страницы или состояние, нужное обработчику ниже.
  const showBanner = index => {
    // Выполняем шаг браузерной логики этого блока.
    current = (index + count) % count;
    // Выполняем шаг браузерной логики этого блока.
    track.style.transform = `translateX(calc((100vw - var(--banner-width)) / 2 - (var(--banner-width) + 16px) * ${current + 1}))`;
    // Меняем CSS-класс элемента, чтобы обновить его внешний вид.
    indicators.forEach((el, i) => { el.classList.toggle('active', i === current); el.setAttribute('aria-pressed', String(i === current)); });
    // Ищем нужный HTML-элемент в текущем документе.
    bannerHero.querySelectorAll('[data-banner-slide]').forEach((el, i) => { el.tabIndex = i === current ? 0 : -1; });
  };
  // Подписываемся на событие пользователя и обновляем интерфейс.
  indicators.forEach(el => el.addEventListener('click', () => showBanner(Number(el.dataset.bannerIndex))));
  // Подписываемся на событие пользователя и обновляем интерфейс.
  bannerHero.querySelector('[data-banner-prev]').addEventListener('click', () => showBanner(current - 1));
  // Подписываемся на событие пользователя и обновляем интерфейс.
  bannerHero.querySelector('[data-banner-next]').addEventListener('click', () => showBanner(current + 1));
  // Сохраняем элемент страницы или состояние, нужное обработчику ниже.
  const pause = bannerHero.querySelector('[data-banner-pause]');
  // Подписываемся на событие пользователя и обновляем интерфейс.
  pause.addEventListener('click', () => { paused = !paused; pause.textContent = paused ? '▶' : 'Ⅱ'; pause.setAttribute('aria-label', paused ? 'Запустить слайдер' : 'Остановить слайдер'); });
  // Подписываемся на событие пользователя и обновляем интерфейс.
  bannerHero.addEventListener('mouseenter', () => { hovering = true; });
  // Подписываемся на событие пользователя и обновляем интерфейс.
  bannerHero.addEventListener('mouseleave', () => { hovering = false; });
  // Запускаем периодическое обновление интерфейса.
  setInterval(() => { if (!paused && !hovering && !bannerHero.contains(document.activeElement)) showBanner(current + 1); }, 7000);
}
// Подписываемся на событие пользователя и обновляем интерфейс.
document.querySelector('[data-dates-next]')?.addEventListener('click', () => document.getElementById('home-dates').scrollBy({left:450,behavior:'smooth'}));
// Сохраняем элемент страницы или состояние, нужное обработчику ниже.
const filmSearch = document.getElementById('film-search');
// Подписываемся на событие пользователя и обновляем интерфейс.
document.querySelector('.search-toggle')?.addEventListener('click', function () {
  // Выполняем шаг браузерной логики этого блока.
  filmSearch.hidden = !filmSearch.hidden;
  // Выполняем шаг браузерной логики этого блока.
  this.setAttribute('aria-expanded', String(!filmSearch.hidden));
  // Выполняем действие только при выполнении условия.
  if (!filmSearch.hidden) filmSearch.focus();
});
// Подписываемся на событие пользователя и обновляем интерфейс.
filmSearch?.addEventListener('input', () => {
  // Сохраняем элемент страницы или состояние, нужное обработчику ниже.
  const term = filmSearch.value.trim().toLocaleLowerCase('ru-RU');
  // Сохраняем элемент страницы или состояние, нужное обработчику ниже.
  const cards = [...document.querySelectorAll('[data-film-title]')];
  // Выполняем шаг браузерной логики этого блока.
  cards.forEach(card => { card.hidden = !card.dataset.filmTitle.includes(term); });
  // Сохраняем элемент страницы или состояние, нужное обработчику ниже.
  const empty = document.querySelector('.search-empty');
  // Выполняем действие только при выполнении условия.
  if (empty) empty.hidden = cards.some(card => !card.hidden);
});
