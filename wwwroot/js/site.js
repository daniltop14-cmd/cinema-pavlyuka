'use strict';
document.querySelector('.menu-toggle')?.addEventListener('click', function () {
  const menu = document.getElementById('main-nav');
  const open = menu.classList.toggle('open');
  this.setAttribute('aria-expanded', String(open));
});
const slides = [...document.querySelectorAll('[data-slide]')];
const dots = [...document.querySelectorAll('[data-goto]')];
let slideIndex = 0;
function showSlide(index) {
  if (!slides.length) return;
  slideIndex = (index + slides.length) % slides.length;
  slides.forEach((el, i) => { el.classList.toggle('is-active', i === slideIndex); el.setAttribute('aria-hidden', String(i !== slideIndex)); });
  dots.forEach((el, i) => { el.classList.toggle('active', i === slideIndex); el.setAttribute('aria-pressed', String(i === slideIndex)); });
}
dots.forEach(el => el.addEventListener('click', () => showSlide(Number(el.dataset.goto))));
document.querySelector('[data-next]')?.addEventListener('click', () => showSlide(slideIndex + 1));
document.querySelector('[data-prev]')?.addEventListener('click', () => showSlide(slideIndex - 1));
// Manual slider: no unexpected motion or focus loss while reading.
const seatForm = document.getElementById('seat-form');
if (seatForm) {
  const price = Number(seatForm.dataset.price);
  seatForm.addEventListener('change', event => {
    let checked = [...seatForm.querySelectorAll('input[name="seatIds"]:checked')];
    if (checked.length > 12) { event.target.checked = false; checked = checked.filter(el => el !== event.target); }
    document.getElementById('selected-seats').textContent = checked.length ? checked.map(el => `Ряд ${el.dataset.row}, место ${el.dataset.number}`).join(' · ') : 'Выберите места на схеме';
    document.getElementById('total').textContent = (checked.length * price).toLocaleString('ru-RU') + ' ₽';
    const button = document.getElementById('reserve-button');
    if (button) button.disabled = checked.length === 0;
  });
}
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
document.querySelectorAll('form[data-confirm]').forEach(form => form.addEventListener('submit', event => { if (!window.confirm(form.dataset.confirm)) event.preventDefault(); }));

// Carousel follows the reference layout: a centred banner and neighbour previews.
const bannerHero = document.querySelector('.reference-hero');
if (bannerHero) {
  const track = bannerHero.querySelector('[data-banner-track]');
  const count = Number(bannerHero.dataset.bannerCount);
  const indicators = [...bannerHero.querySelectorAll('[data-banner-index]')];
  let current = 0;
  let paused = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  let hovering = false;
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
  setInterval(() => { if (!paused && !hovering && !bannerHero.contains(document.activeElement)) showBanner(current + 1); }, 7000);
}
document.querySelector('[data-dates-next]')?.addEventListener('click', () => document.getElementById('home-dates').scrollBy({left:450,behavior:'smooth'}));
const filmSearch = document.getElementById('film-search');
document.querySelector('.search-toggle')?.addEventListener('click', function () {
  filmSearch.hidden = !filmSearch.hidden;
  this.setAttribute('aria-expanded', String(!filmSearch.hidden));
  if (!filmSearch.hidden) filmSearch.focus();
});
filmSearch?.addEventListener('input', () => {
  const term = filmSearch.value.trim().toLocaleLowerCase('ru-RU');
  const cards = [...document.querySelectorAll('[data-film-title]')];
  cards.forEach(card => { card.hidden = !card.dataset.filmTitle.includes(term); });
  const empty = document.querySelector('.search-empty');
  if (empty) empty.hidden = cards.some(card => !card.hidden);
});
