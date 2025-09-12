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


// ----- Demo data helpers
const months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
const weeks = Array.from({ length: 12 }, (_, i) => `W${i + 1}`);
const hours = Array.from({ length: 24 }, (_, i) => String(i).padStart(2, "0") + ":00");

function rnd(n, min = 10, max = 45) {
    const out = []; for (let i = 0; i < n; i++) out.push(Math.round(min + Math.random() * (max - min)));
    return out;
}
const datasetByRange = {
    monthly: { labels: months, bars: rnd(12, 10, 44), line: rnd(12, 18, 38) },
    weekly: { labels: weeks, bars: rnd(12, 6, 28), line: rnd(12, 10, 24) },
    today: { labels: hours, bars: rnd(24, 1, 10), line: rnd(24, 2, 12) }
};

// ----- Revenue main chart (bar + line)
const ctx = document.getElementById('revenueChart');
const start = datasetByRange.monthly;

const revenueChart = new Chart(ctx, {
    data: {
        labels: start.labels,
        datasets: [
            {
                type: 'bar',
                label: 'Sales',
                data: start.bars,
                backgroundColor: 'rgba(99, 102, 241, .8)',  // indigo-500
                borderRadius: 8,
                maxBarThickness: 26
            },
            {
                type: 'line',
                label: 'Trend',
                data: start.line,
                borderColor: '#10b981', // emerald-500
                borderWidth: 3,
                pointRadius: 0,
                tension: .35,
                fill: false
            }
        ]
    },
    options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false }, tooltip: { mode: 'index', intersect: false } },
        scales: {
            x: { grid: { display: false } },
            y: { beginAtZero: true, grid: { drawBorder: false } }
        }
    }
});

// ----- Timeframe filter buttons
document.querySelectorAll('.btn-filter .btn').forEach(btn => {
    btn.addEventListener('click', () => {
        document.querySelectorAll('.btn-filter .btn').forEach(b => b.classList.remove('active'));
        btn.classList.add('active');

        const key = btn.getAttribute('data-range');
        const ds = datasetByRange[key];
        revenueChart.data.labels = ds.labels;
        revenueChart.data.datasets[0].data = ds.bars;
        revenueChart.data.datasets[1].data = ds.line;
        revenueChart.update();

        // Fake stats refresh (replace with real numbers)
        if (key === 'monthly') {
            document.getElementById('statThisMonth').textContent = '$ 12,253';
        } else if (key === 'weekly') {
            document.getElementById('statThisMonth').textContent = '$ 2,874';
        } else {
            document.getElementById('statThisMonth').textContent = '$ 643';
        }
    });
});

// ----- Sparklines
function spark(el, data) {
    new Chart(el, {
        type: 'line',
        data: { labels: data.map((_, i) => i + 1), datasets: [{ data, borderWidth: 2, pointRadius: 0, fill: false }] },
        options: {
            responsive: true, maintainAspectRatio: false,
            plugins: { legend: { display: false }, tooltip: { enabled: false } },
            scales: { x: { display: false }, y: { display: false } }
        }
    });
}
spark(document.getElementById('spark1'), rnd(18, 20, 40));
spark(document.getElementById('spark2'), rnd(18, 10, 35));