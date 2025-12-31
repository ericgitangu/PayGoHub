// PayGoHub Dashboard JavaScript

document.addEventListener('DOMContentLoaded', function () {
    // Sidebar Toggle
    const sidebarToggle = document.getElementById('sidebarToggle');
    const sidebar = document.getElementById('sidebar');
    const pageContent = document.getElementById('page-content');

    if (sidebarToggle) {
        sidebarToggle.addEventListener('click', function () {
            sidebar.classList.toggle('collapsed');
            pageContent.classList.toggle('expanded');

            // Save state to localStorage
            const isCollapsed = sidebar.classList.contains('collapsed');
            localStorage.setItem('sidebarCollapsed', isCollapsed);
        });

        // Restore sidebar state from localStorage
        const savedState = localStorage.getItem('sidebarCollapsed');
        if (savedState === 'true') {
            sidebar.classList.add('collapsed');
            pageContent.classList.add('expanded');
        }
    }

    // Mobile sidebar handling
    function handleMobileMenu() {
        if (window.innerWidth < 992) {
            sidebar.classList.remove('collapsed');
            sidebar.classList.remove('show');
            pageContent.classList.remove('expanded');
        }
    }

    // Mobile toggle
    if (sidebarToggle && window.innerWidth < 992) {
        sidebarToggle.addEventListener('click', function () {
            sidebar.classList.toggle('show');
        });
    }

    // Close sidebar when clicking outside on mobile
    document.addEventListener('click', function (e) {
        if (window.innerWidth < 992) {
            if (!sidebar.contains(e.target) && !sidebarToggle.contains(e.target)) {
                sidebar.classList.remove('show');
            }
        }
    });

    window.addEventListener('resize', handleMobileMenu);

    // Initialize tooltips
    const tooltipTriggerList = document.querySelectorAll('[data-bs-toggle="tooltip"]');
    tooltipTriggerList.forEach(function (tooltipTriggerEl) {
        new bootstrap.Tooltip(tooltipTriggerEl);
    });

    // Animate numbers on load
    function animateValue(element, start, end, duration) {
        const range = end - start;
        const startTime = performance.now();

        function update(currentTime) {
            const elapsed = currentTime - startTime;
            const progress = Math.min(elapsed / duration, 1);
            const easeProgress = 1 - Math.pow(1 - progress, 3);
            const current = Math.floor(start + range * easeProgress);

            if (element.dataset.prefix) {
                element.textContent = element.dataset.prefix + current.toLocaleString();
            } else {
                element.textContent = current.toLocaleString();
            }

            if (progress < 1) {
                requestAnimationFrame(update);
            }
        }

        requestAnimationFrame(update);
    }

    // Animate metric numbers
    document.querySelectorAll('[data-animate-number]').forEach(function (el) {
        const target = parseInt(el.dataset.animateNumber, 10);
        animateValue(el, 0, target, 1500);
    });

    // Add hover effect to table rows
    document.querySelectorAll('.table tbody tr').forEach(function (row) {
        row.style.cursor = 'pointer';
    });

    console.log('PayGoHub Dashboard initialized');
});
