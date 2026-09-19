(() => {
    'use strict';
    document.addEventListener('DOMContentLoaded', () => {
        const category = document.getElementById('headerCategoryNavigation');
        category?.addEventListener('change', () => { if (category.value) window.location.assign(category.value); });
        const header = document.querySelector('.storefront-header');
        const updateOffset = () => {
            const height = header && getComputedStyle(header).position === 'sticky' ? header.getBoundingClientRect().height + 16 : 16;
            document.documentElement.style.setProperty('--storefront-header-offset', `${height}px`);
        };
        updateOffset();
        if (header && window.ResizeObserver) new ResizeObserver(updateOffset).observe(header);
        window.addEventListener('resize', updateOffset);
        const navigateToSection = link => {
            const destination = new URL(link.href, window.location.href);
            const target = document.getElementById(decodeURIComponent(destination.hash.slice(1)));
            if (!target) return;
            target.scrollIntoView({ behavior: window.matchMedia('(prefers-reduced-motion: reduce)').matches ? 'auto' : 'smooth' });
            if (!target.hasAttribute('tabindex')) target.setAttribute('tabindex', '-1');
            target.focus({ preventScroll: true });
            history.pushState(null, '', destination.hash);
        };
        document.querySelectorAll('a[href*="#"]').forEach(link => {
            link.addEventListener('click', event => {
                const destination = new URL(link.href, window.location.href);
                if (event.ctrlKey || event.metaKey || event.shiftKey || event.altKey ||
                    destination.origin !== window.location.origin || destination.pathname !== window.location.pathname ||
                    destination.search !== window.location.search || !destination.hash) return;
                if (!document.getElementById(decodeURIComponent(destination.hash.slice(1)))) return;
                event.preventDefault();
                const panel = link.closest('.offcanvas.show');
                if (panel && window.bootstrap?.Offcanvas) {
                    panel.addEventListener('hidden.bs.offcanvas', () => navigateToSection(link), { once: true });
                    bootstrap.Offcanvas.getOrCreateInstance(panel).hide();
                } else navigateToSection(link);
            });
        });
        document.querySelectorAll('img[data-fallback]').forEach(img => {
            const fallback = () => {
                if (img.dataset.fallbackApplied) return;
                img.dataset.fallbackApplied = 'true';
                img.src = img.dataset.fallback;
            };
            img.addEventListener('error', fallback);
            if (img.complete && img.naturalWidth === 0) fallback();
        });
        if (window.Swiper) {
            const a11y = {
                prevSlideMessage: 'Önceki slayt', nextSlideMessage: 'Sonraki slayt',
                firstSlideMessage: 'İlk slayttasınız', lastSlideMessage: 'Son slayttasınız',
                paginationBulletMessage: '{{index}}. slayta git', slideLabelMessage: '{{slidesLength}} slayttan {{index}}.'
            };
            document.querySelectorAll('.main-swiper, .category-carousel, .products-carousel').forEach(element => {
                const main = element.classList.contains('main-swiper');
                const category = element.classList.contains('category-carousel');
                const section = element.closest('section');
                const prefix = category ? '.category-carousel' : '.products-carousel';
                new Swiper(element, {
                    speed: window.matchMedia('(prefers-reduced-motion: reduce)').matches ? 0 : 500,
                    slidesPerView: 1, spaceBetween: 24, watchOverflow: true, a11y,
                    pagination: main ? { el: element.querySelector('.swiper-pagination'), clickable: true } : undefined,
                    navigation: main ? undefined : { nextEl: section.querySelector(prefix + '-next'), prevEl: section.querySelector(prefix + '-prev') },
                    breakpoints: main ? undefined : { 0: { slidesPerView: category ? 2 : 1 }, 576: { slidesPerView: 2 }, 768: { slidesPerView: 3 }, 1200: { slidesPerView: category ? 6 : 4 } }
                });
            });
        }
        // Reapply the anchor after images/fonts have settled on a cross-page navigation.
        if (window.location.hash) {
            window.addEventListener('load', () => {
                updateOffset();
                document.getElementById(decodeURIComponent(window.location.hash.slice(1)))?.scrollIntoView({ behavior: 'auto' });
            }, { once: true });
        }
    });
})();
