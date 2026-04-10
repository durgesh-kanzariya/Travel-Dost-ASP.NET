/**
 * admin.js — Client-side logic for the Admin shell (_AdminLayout)
 */
(function () {
    // ── 1. Populate user data ────────────────────────────────────────────────
    try {
        const user = JSON.parse(localStorage.getItem('user') || '{}');
        const nameEl = document.getElementById('adminUserName');
        const emailEl = document.getElementById('adminUserEmail');
        if (nameEl) nameEl.textContent = `${user.firstName || user.first_name || 'Admin'} ${user.lastName || user.last_name || ''}`.trim();
        if (emailEl) emailEl.textContent = user.email || '';
    } catch (e) { console.error('[Admin] Failed to parse user:', e); }

    // ── 2. Dark mode toggle ──────────────────────────────────────────────────
    const themeToggle = document.getElementById('adminThemeToggle');
    const moonIcon    = document.getElementById('adminMoonIcon');
    const sunIcon     = document.getElementById('adminSunIcon');
    const themeLabel  = document.getElementById('adminThemeLabel');

    function updateThemeIcons() {
        const isDark = document.documentElement.classList.contains('dark');
        moonIcon?.classList.toggle('hidden', !isDark);
        sunIcon?.classList.toggle('hidden', isDark);
        if (themeLabel) themeLabel.textContent = isDark ? 'Light Mode' : 'Dark Mode';
    }
    updateThemeIcons();

    themeToggle?.addEventListener('click', () => {
        const isDark = document.documentElement.classList.contains('dark');
        document.documentElement.classList.toggle('dark', !isDark);
        localStorage.setItem('theme', isDark ? 'light' : 'dark');
        updateThemeIcons();
    });

    // ── 3. Logout ────────────────────────────────────────────────────────────
    document.getElementById('adminLogoutBtn')?.addEventListener('click', () => {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        localStorage.removeItem('nativeLanguage');
        window.location.href = '/Auth/Login';
    });

    // ── 4. Mobile sidebar toggle ─────────────────────────────────────────────
    const sidebar   = document.getElementById('adminSidebar');
    const overlay   = document.getElementById('adminOverlay');
    const menuIcon  = document.getElementById('adminMenuIcon');
    const closeIcon = document.getElementById('adminCloseIcon');

    function openSidebar() {
        sidebar?.classList.remove('-translate-x-full');
        overlay?.classList.remove('hidden');
        menuIcon?.classList.add('hidden');
        closeIcon?.classList.remove('hidden');
    }
    function closeSidebar() {
        sidebar?.classList.add('-translate-x-full');
        overlay?.classList.add('hidden');
        menuIcon?.classList.remove('hidden');
        closeIcon?.classList.add('hidden');
    }

    document.getElementById('adminMobileToggle')?.addEventListener('click', () => {
        sidebar?.classList.contains('-translate-x-full') ? openSidebar() : closeSidebar();
    });
    overlay?.addEventListener('click', closeSidebar);
})();
