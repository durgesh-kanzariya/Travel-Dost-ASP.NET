// Document ready
document.addEventListener('DOMContentLoaded', () => {
    
    // --- Dark Mode Logic ---
    const themeToggleBtn = document.getElementById('themeToggleBtn');
    const darkIcon = document.getElementById('themeToggleDarkIcon');
    const lightIcon = document.getElementById('themeToggleLightIcon');

    // Check system preference or localStorage
    const savedTheme = localStorage.getItem('theme');
    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;

    if (savedTheme === 'dark' || (!savedTheme && prefersDark)) {
        document.documentElement.classList.add('dark');
        lightIcon.classList.remove('hidden');
    } else {
        document.documentElement.classList.remove('dark');
        darkIcon.classList.remove('hidden');
    }

    themeToggleBtn?.addEventListener('click', () => {
        darkIcon.classList.toggle('hidden');
        lightIcon.classList.toggle('hidden');

        if (document.documentElement.classList.contains('dark')) {
            document.documentElement.classList.remove('dark');
            localStorage.setItem('theme', 'light');
        } else {
            document.documentElement.classList.add('dark');
            localStorage.setItem('theme', 'dark');
        }
    });

    // --- Mobile Menu Logic ---
    const mobileMenuBtn = document.getElementById('mobileMenuBtn');
    const mobileMenu = document.getElementById('mobileMenu');

    mobileMenuBtn?.addEventListener('click', () => {
        mobileMenu.classList.toggle('hidden');
    });

    // --- Navbar Scroll Effect ---
    const header = document.querySelector('header');
    const scrollClasses = ['border-b', 'border-slate-200', 'dark:border-slate-800', 'bg-white/95', 'dark:bg-slate-900/95', 'backdrop-blur', 'shadow-sm'];
    const transparentClasses = ['border-transparent', 'bg-transparent'];

    function handleScroll() {
        if (window.scrollY > 10) {
            header?.classList.add(...scrollClasses);
            header?.classList.remove(...transparentClasses);
        } else {
            header?.classList.remove(...scrollClasses);
            header?.classList.add(...transparentClasses);
        }
    }

    window.addEventListener('scroll', handleScroll);
    handleScroll(); // Initial check
});
