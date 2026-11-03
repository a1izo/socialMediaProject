// Small helpers for things C# cannot do from the server: the remembered site mode and the
// sparkle cursor trail on profiles. No libraries.
window.cybernet = {
    getShell() {
        try { return localStorage.getItem('cn-shell') || 'void'; } catch (e) { return 'void'; }
    },

    setShell(mode) {
        document.documentElement.dataset.shell = mode;
        try { localStorage.setItem('cn-shell', mode); } catch (e) { }
    },

    trail: {
        handler: null,
        last: 0,

        enable(color) {
            this.disable();
            if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return;
            this.handler = (e) => {
                const now = performance.now();
                if (now - this.last < 35) return;
                this.last = now;
                const star = document.createElement('span');
                star.className = 'trail-star';
                star.style.left = e.clientX + 'px';
                star.style.top = e.clientY + 'px';
                star.style.setProperty('--trail-color', color);
                star.style.setProperty('--trail-size', (6 + Math.random() * 10) + 'px');
                star.style.setProperty('--trail-dx', (Math.random() * 30 - 15) + 'px');
                document.body.appendChild(star);
                setTimeout(() => star.remove(), 800);
            };
            window.addEventListener('mousemove', this.handler);
        },

        disable() {
            if (this.handler) window.removeEventListener('mousemove', this.handler);
            this.handler = null;
        }
    }
};
