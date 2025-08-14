function tick() {
    const now = new Date();
    const h = String(now.getHours()).padStart(2, '0');
    const m = String(now.getMinutes()).padStart(2, '0');
    const s = String(now.getSeconds()).padStart(2, '0');
    const el = document.getElementById('clock');
    if (el) el.textContent = `${h}.${m}.${s}`;
}
setInterval(tick, 1000); tick();

// Toggle submenu on click (no href)
function initSubmenus(root) {
    root.querySelectorAll('.has-submenu').forEach(container => {
        const btn = container.querySelector('.submenu-toggle');
        if (!btn) return;
        btn.addEventListener('click', () => {
            const open = container.classList.toggle('is-open');
            btn.setAttribute('aria-expanded', open ? 'true' : 'false');
        });
    });
}
initSubmenus(document);