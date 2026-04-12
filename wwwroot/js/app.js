/**
 * app.js — Client-side logic for the protected App shell (_AppLayout)
 * Handles: sidebar toggle, dark mode, user profile population, admin visibility, logout
 */
(function () {
    // ── 1. Populate user data from localStorage ──────────────────────────────
    const userJson = localStorage.getItem('user');
    if (userJson) {
        try {
            const user = JSON.parse(userJson);
            const nameEl = document.getElementById('sidebarUserName');
            const emailEl = document.getElementById('sidebarUserEmail');
            if (nameEl) nameEl.textContent = `${user.firstName || user.first_name || 'Traveler'} ${user.lastName || user.last_name || ''}`.trim();
            if (emailEl) emailEl.textContent = user.email || '';

            // Show admin switch button if user is admin
            const adminBtn = document.getElementById('adminSwitchBtn');
            if (adminBtn && (user.role === 'admin')) {
                adminBtn.classList.remove('hidden');
                adminBtn.classList.add('flex');
            }
        } catch (e) {
            console.error('[App] Failed to parse user data from localStorage:', e);
        }
    }

    // ── 2. Dark mode toggle in sidebar ───────────────────────────────────────
    const themeToggle = document.getElementById('appThemeToggle');
    const moonIcon = document.getElementById('appMoonIcon');
    const sunIcon = document.getElementById('appSunIcon');
    const themeLabel = document.getElementById('appThemeLabel');

    function updateThemeIcons() {
        const isDark = document.documentElement.classList.contains('dark');
        if (isDark) {
            moonIcon?.classList.add('hidden');
            sunIcon?.classList.remove('hidden');
            if (themeLabel) themeLabel.textContent = 'Light Mode';
        } else {
            moonIcon?.classList.remove('hidden');
            sunIcon?.classList.add('hidden');
            if (themeLabel) themeLabel.textContent = 'Dark Mode';
        }
    }

    updateThemeIcons(); // Set on load

    themeToggle?.addEventListener('click', () => {
        const isDark = document.documentElement.classList.contains('dark');
        if (isDark) {
            document.documentElement.classList.remove('dark');
            localStorage.setItem('theme', 'light');
        } else {
            document.documentElement.classList.add('dark');
            localStorage.setItem('theme', 'dark');
        }
        updateThemeIcons();
    });

    // ── 3. Logout ─────────────────────────────────────────────────────────────
    document.getElementById('logoutBtn')?.addEventListener('click', () => {
        localStorage.removeItem('token');
        localStorage.removeItem('user');
        localStorage.removeItem('nativeLanguage');
        window.location.href = '/Auth/Login';
    });

    // ── 4. Mobile sidebar toggle ─────────────────────────────────────────────
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebarOverlay');
    const menuIcon = document.getElementById('sidebarMenuIcon');
    const closeIcon = document.getElementById('sidebarCloseIcon');

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

    document.getElementById('mobileSidebarToggle')?.addEventListener('click', () => {
        const isOpen = !sidebar?.classList.contains('-translate-x-full');
        isOpen ? closeSidebar() : openSidebar();
    });

    overlay?.addEventListener('click', closeSidebar);
})();

// Landing page responsiveness and micro-animations polished
