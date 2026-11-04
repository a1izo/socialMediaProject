// Small helpers for things C# cannot do from the server: the remembered site mode, the sparkle
// cursor trail, measuring the profile header, and dragging stickers / module edges in the editor.
// No libraries.
window.cybernet = {
    getShell() {
        try { return localStorage.getItem('cn-shell') || 'void'; } catch (e) { return 'void'; }
    },

    setShell(mode) {
        document.documentElement.dataset.shell = mode;
        try { localStorage.setItem('cn-shell', mode); } catch (e) { }
    },

    scrollToBottom(id) {
        const el = document.getElementById(id);
        if (el) el.scrollTop = el.scrollHeight;
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
    },

    // The avatar sits on the banner at a % position chosen in the editor. This measures how far it
    // hangs below the banner and on which side, and hands that to CSS as variables.
    header: {
        observe(header) {
            if (!header || header._cnObserved) return;
            header._cnObserved = true;
            const observer = new ResizeObserver(() => this.measure(header));
            observer.observe(header.querySelector('.banner') || header);
            header.addEventListener('load', () => this.measure(header), true);
            this.measure(header);
        },

        measure(header) {
            if (!header) return;
            const banner = header.querySelector('.banner');
            const avatar = header.querySelector('.avatar-anchor');
            if (!banner || !avatar) return;
            const b = banner.getBoundingClientRect();
            const box = avatar.getBoundingClientRect();
            // grow the avatar's box by how far its decoration reaches (set per decoration in CSS)
            const pad = (parseFloat(getComputedStyle(avatar).getPropertyValue('--deco-pad')) || 0) * box.width;
            const a = { left: box.left - pad, right: box.right + pad, top: box.top - pad, bottom: box.bottom + pad, width: box.width + 2 * pad };
            const overflow = Math.max(0, a.bottom - b.bottom);
            const center = (a.left + a.width / 2 - b.left) / b.width;
            let left = 0, right = 0, top = 0;
            if (overflow > 0) {
                if (center < 0.35) left = a.right - b.left + 20;
                else if (center > 0.65) right = b.right - a.left + 20;
                else top = overflow;
            } else {
                // layouts that put the name over the banner: keep the text beside the avatar
                const id = header.querySelector('.profile-id');
                const r = id ? id.getBoundingClientRect() : null;
                if (r && a.bottom > r.top + 8 && a.top < r.bottom) {
                    if (center < 0.5) left = a.right - b.left + 20;
                    else right = b.right - a.left + 20;
                }
            }
            header.style.setProperty('--avatar-overflow', overflow + 'px');
            header.style.setProperty('--avatar-indent-left', left + 'px');
            header.style.setProperty('--avatar-indent-right', right + 'px');
            header.style.setProperty('--avatar-pad-top', top + 'px');
        }
    },

    // Edit mode on a profile: drag stickers around, and drag a module's right edge to resize it.
    // When a drag ends, the new position is sent to the Blazor page, which owns the data.
    editor: {
        root: null,
        page: null,
        down: null,

        enable(root, page) {
            this.disable();
            this.root = root;
            this.page = page;
            this.down = (e) => this.start(e);
            root.addEventListener('pointerdown', this.down);
            root.addEventListener('dragstart', this.blockDrag, true);
        },

        disable() {
            if (this.root && this.down) {
                this.root.removeEventListener('pointerdown', this.down);
                this.root.removeEventListener('dragstart', this.blockDrag, true);
            }
            this.root = null;
            this.page = null;
        },

        blockDrag(e) {
            if (window.cybernet.editor.busy) e.preventDefault();
        },

        busy: false,

        start(e) {
            const sticker = e.target.closest('.sticker');
            const handle = e.target.closest('.module-resize');
            if (!sticker && !handle) return;
            e.preventDefault();
            e.stopPropagation();
            this.busy = true;
            if (sticker) this.dragSticker(e, sticker);
            else this.resizeModule(e, handle.closest('.module'));
        },

        dragSticker(e, el) {
            const layer = el.parentElement.getBoundingClientRect();
            const startX = e.clientX, startY = e.clientY;
            let dx = 0, dy = 0;
            const move = (ev) => {
                dx = ev.clientX - startX;
                dy = ev.clientY - startY;
                el.style.translate = dx + 'px ' + dy + 'px';
            };
            const up = () => {
                window.removeEventListener('pointermove', move);
                window.removeEventListener('pointerup', up);
                this.busy = false;
                const r = el.getBoundingClientRect();
                el.style.translate = '';
                const id = el.dataset.stickerId;
                if (Math.abs(dx) < 4 && Math.abs(dy) < 4) {
                    this.page.invokeMethodAsync('StickerClicked', id);
                    return;
                }
                const x = ((r.left + r.width / 2) - layer.left) / layer.width * 100;
                const y = (r.top + r.height / 2) - layer.top;
                this.page.invokeMethodAsync('StickerMoved', id, x, Math.round(y));
            };
            window.addEventListener('pointermove', move);
            window.addEventListener('pointerup', up);
        },

        resizeModule(e, module) {
            const grid = module.parentElement.getBoundingClientRect();
            const column = grid.width / 12;
            const startX = e.clientX;
            const startWidth = module.getBoundingClientRect().width;
            module.classList.add('is-resizing');
            let dx = 0;
            const move = (ev) => {
                dx = ev.clientX - startX;
                module.style.width = Math.max(column, startWidth + dx) + 'px';
            };
            const up = () => {
                window.removeEventListener('pointermove', move);
                window.removeEventListener('pointerup', up);
                module.classList.remove('is-resizing');
                module.style.width = '';
                setTimeout(() => this.busy = false, 0);
                const columns = Math.round((startWidth + dx) / column);
                this.page.invokeMethodAsync('ModuleResized', parseInt(module.dataset.index), columns);
            };
            window.addEventListener('pointermove', move);
            window.addEventListener('pointerup', up);
        }
    }
};
