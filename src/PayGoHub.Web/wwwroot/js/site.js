// PayGoHub Dashboard JavaScript

// Theme Management
const ThemeManager = {
    STORAGE_KEY: 'paygohub-theme',

    init() {
        this.applyStoredTheme();
        this.setupThemeToggle();
        this.listenForSystemChanges();
    },

    getStoredTheme() {
        return localStorage.getItem(this.STORAGE_KEY) || 'system';
    },

    setTheme(theme) {
        localStorage.setItem(this.STORAGE_KEY, theme);
        this.applyTheme(theme);
        this.updateToggleIcon(theme);
    },

    applyTheme(theme) {
        const root = document.documentElement;

        if (theme === 'system') {
            const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
            root.setAttribute('data-theme', prefersDark ? 'dark' : 'light');
        } else {
            root.setAttribute('data-theme', theme);
        }
    },

    applyStoredTheme() {
        const theme = this.getStoredTheme();
        this.applyTheme(theme);
    },

    setupThemeToggle() {
        const toggle = document.getElementById('themeToggle');
        if (toggle) {
            toggle.addEventListener('click', () => {
                const current = this.getStoredTheme();
                const themes = ['light', 'dark', 'system'];
                const nextIndex = (themes.indexOf(current) + 1) % themes.length;
                this.setTheme(themes[nextIndex]);
            });
            this.updateToggleIcon(this.getStoredTheme());
        }
    },

    updateToggleIcon(theme) {
        const toggle = document.getElementById('themeToggle');
        if (!toggle) return;

        const icons = {
            light: 'bi-sun-fill',
            dark: 'bi-moon-fill',
            system: 'bi-circle-half'
        };

        const icon = toggle.querySelector('.theme-icon');
        if (icon) {
            icon.className = `bi ${icons[theme]} theme-icon`;
        }
    },

    listenForSystemChanges() {
        window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', () => {
            if (this.getStoredTheme() === 'system') {
                this.applyTheme('system');
            }
        });
    }
};

// Notification Manager
const NotificationManager = {
    notifications: [],
    unreadCount: 0,

    init() {
        this.loadFromStorage();
        this.setupEventListeners();
        this.updateBadge();
    },

    loadFromStorage() {
        const stored = localStorage.getItem('paygohub-notifications');
        if (stored) {
            this.notifications = JSON.parse(stored);
            this.unreadCount = this.notifications.filter(n => !n.read).length;
        }
    },

    saveToStorage() {
        localStorage.setItem('paygohub-notifications', JSON.stringify(this.notifications));
    },

    add(notification) {
        const newNotification = {
            id: Date.now(),
            ...notification,
            timestamp: new Date().toISOString(),
            read: false
        };

        this.notifications.unshift(newNotification);
        if (this.notifications.length > 50) {
            this.notifications = this.notifications.slice(0, 50);
        }

        this.unreadCount++;
        this.updateBadge();
        this.saveToStorage();
        this.renderNotifications();
        this.showToast(newNotification);
    },

    markAsRead(id) {
        const notification = this.notifications.find(n => n.id === id);
        if (notification && !notification.read) {
            notification.read = true;
            this.unreadCount = Math.max(0, this.unreadCount - 1);
            this.updateBadge();
            this.saveToStorage();
        }
    },

    markAllAsRead() {
        this.notifications.forEach(n => n.read = true);
        this.unreadCount = 0;
        this.updateBadge();
        this.saveToStorage();
        this.renderNotifications();
    },

    clear() {
        this.notifications = [];
        this.unreadCount = 0;
        this.updateBadge();
        this.saveToStorage();
        this.renderNotifications();
    },

    updateBadge() {
        const badge = document.getElementById('notificationBadge');
        if (badge) {
            if (this.unreadCount > 0) {
                badge.textContent = this.unreadCount > 99 ? '99+' : this.unreadCount;
                badge.style.display = 'inline-flex';
            } else {
                badge.style.display = 'none';
            }
        }
    },

    setupEventListeners() {
        const bellBtn = document.getElementById('notificationBell');
        const panel = document.getElementById('notificationPanel');
        const markAllBtn = document.getElementById('markAllRead');
        const clearAllBtn = document.getElementById('clearAllNotifications');

        if (bellBtn && panel) {
            bellBtn.addEventListener('click', (e) => {
                e.preventDefault();
                e.stopPropagation();
                panel.classList.toggle('show');
                if (panel.classList.contains('show')) {
                    this.renderNotifications();
                }
            });

            document.addEventListener('click', (e) => {
                if (!panel.contains(e.target) && !bellBtn.contains(e.target)) {
                    panel.classList.remove('show');
                }
            });
        }

        if (markAllBtn) {
            markAllBtn.addEventListener('click', () => this.markAllAsRead());
        }

        if (clearAllBtn) {
            clearAllBtn.addEventListener('click', () => this.clear());
        }
    },

    renderNotifications() {
        const list = document.getElementById('notificationList');
        if (!list) return;

        if (this.notifications.length === 0) {
            list.innerHTML = `
                <div class="text-center py-4 text-muted">
                    <i class="bi bi-bell-slash fs-1 mb-2"></i>
                    <p class="mb-0">No notifications</p>
                </div>
            `;
            return;
        }

        list.innerHTML = this.notifications.map(n => `
            <div class="notification-item ${n.read ? '' : 'unread'}" data-id="${n.id}">
                <div class="notification-icon ${n.type || 'info'}">
                    <i class="bi ${this.getIcon(n.type)}"></i>
                </div>
                <div class="notification-content">
                    <div class="notification-title">${n.title}</div>
                    <div class="notification-message">${n.message}</div>
                    <div class="notification-time">${this.formatTime(n.timestamp)}</div>
                </div>
            </div>
        `).join('');

        list.querySelectorAll('.notification-item').forEach(item => {
            item.addEventListener('click', () => {
                this.markAsRead(parseInt(item.dataset.id));
                item.classList.remove('unread');
            });
        });
    },

    getIcon(type) {
        const icons = {
            success: 'bi-check-circle-fill',
            error: 'bi-x-circle-fill',
            warning: 'bi-exclamation-triangle-fill',
            info: 'bi-info-circle-fill'
        };
        return icons[type] || icons.info;
    },

    formatTime(timestamp) {
        const date = new Date(timestamp);
        const now = new Date();
        const diff = (now - date) / 1000;

        if (diff < 60) return 'Just now';
        if (diff < 3600) return `${Math.floor(diff / 60)}m ago`;
        if (diff < 86400) return `${Math.floor(diff / 3600)}h ago`;
        return date.toLocaleDateString();
    },

    showToast(notification) {
        const toast = document.createElement('div');
        toast.className = `toast align-items-center text-white bg-${this.getBootstrapColor(notification.type)} border-0`;
        toast.setAttribute('role', 'alert');
        toast.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">
                    <strong>${notification.title}</strong>: ${notification.message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        `;

        let container = document.getElementById('toastContainer');
        if (!container) {
            container = document.createElement('div');
            container.id = 'toastContainer';
            container.className = 'toast-container position-fixed top-0 end-0 p-3';
            container.style.zIndex = '1100';
            document.body.appendChild(container);
        }

        container.appendChild(toast);
        const bsToast = new bootstrap.Toast(toast, { delay: 5000 });
        bsToast.show();

        toast.addEventListener('hidden.bs.toast', () => toast.remove());
    },

    getBootstrapColor(type) {
        const colors = {
            success: 'success',
            error: 'danger',
            warning: 'warning',
            info: 'primary'
        };
        return colors[type] || 'primary';
    }
};

// API Notification Helper - Call this after API operations
window.PayGoHub = {
    notify: function(title, message, type = 'info') {
        NotificationManager.add({ title, message, type });
    },

    notifySuccess: function(title, message) {
        this.notify(title, message, 'success');
    },

    notifyError: function(title, message) {
        this.notify(title, message, 'error');
    },

    notifyWarning: function(title, message) {
        this.notify(title, message, 'warning');
    }
};

document.addEventListener('DOMContentLoaded', function () {
    // Initialize Theme, Notifications, and Search
    ThemeManager.init();
    NotificationManager.init();
    SearchManager.init();

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

    // Fetch user authentication info
    fetchUserInfo();

    console.log('PayGoHub Dashboard initialized with theme and notifications support');
});

// Search Manager
const SearchManager = {
    searchInput: null,
    searchDropdown: null,
    debounceTimer: null,
    currentQuery: '',

    init() {
        this.searchInput = document.getElementById('globalSearch');
        this.searchDropdown = document.getElementById('searchResults');

        if (this.searchInput && this.searchDropdown) {
            this.setupEventListeners();
        }
    },

    setupEventListeners() {
        // Handle input with debounce
        this.searchInput.addEventListener('input', (e) => {
            const query = e.target.value.trim();
            this.currentQuery = query;

            if (this.debounceTimer) {
                clearTimeout(this.debounceTimer);
            }

            if (query.length < 2) {
                this.hideDropdown();
                return;
            }

            this.debounceTimer = setTimeout(() => {
                this.performSearch(query);
            }, 300);
        });

        // Handle focus
        this.searchInput.addEventListener('focus', () => {
            if (this.currentQuery.length >= 2) {
                this.showDropdown();
            }
        });

        // Hide dropdown when clicking outside
        document.addEventListener('click', (e) => {
            if (!this.searchInput.contains(e.target) && !this.searchDropdown.contains(e.target)) {
                this.hideDropdown();
            }
        });

        // Handle keyboard navigation
        this.searchInput.addEventListener('keydown', (e) => {
            if (e.key === 'Escape') {
                this.hideDropdown();
                this.searchInput.blur();
            }
        });
    },

    async performSearch(query) {
        this.showLoading();

        try {
            const response = await fetch(`/api/search?q=${encodeURIComponent(query)}`);
            const data = await response.json();

            if (data.query !== this.currentQuery) {
                return; // Stale response
            }

            this.renderResults(data.results, query);
        } catch (error) {
            console.error('Search error:', error);
            this.showError();
        }
    },

    showLoading() {
        this.searchDropdown.innerHTML = `
            <div class="search-loading">
                <i class="bi bi-arrow-repeat spin"></i> Searching...
            </div>
        `;
        this.showDropdown();
    },

    showError() {
        this.searchDropdown.innerHTML = `
            <div class="search-no-results">
                <i class="bi bi-exclamation-circle"></i> Error searching. Please try again.
            </div>
        `;
    },

    renderResults(results, query) {
        if (results.length === 0) {
            this.searchDropdown.innerHTML = `
                <div class="search-no-results">
                    <i class="bi bi-search mb-2" style="font-size: 1.5rem; display: block;"></i>
                    No results found for "${query}"
                </div>
            `;
            this.showDropdown();
            return;
        }

        const html = results.map(result => `
            <a href="${result.url}" class="search-result-item">
                <div class="search-result-icon ${result.type}">
                    <i class="bi ${result.icon}"></i>
                </div>
                <div class="search-result-content">
                    <div class="search-result-title">${this.highlightMatch(result.title, query)}</div>
                    <div class="search-result-subtitle">${result.subtitle || ''}</div>
                </div>
                <span class="search-result-type">${result.type}</span>
            </a>
        `).join('');

        const viewAllLink = `
            <a href="/Search?q=${encodeURIComponent(query)}" class="search-view-all">
                View all results <i class="bi bi-arrow-right"></i>
            </a>
        `;

        this.searchDropdown.innerHTML = html + viewAllLink;
        this.showDropdown();
    },

    highlightMatch(text, query) {
        if (!text) return '';
        const regex = new RegExp(`(${query})`, 'gi');
        return text.replace(regex, '<mark>$1</mark>');
    },

    showDropdown() {
        this.searchDropdown.classList.add('show');
    },

    hideDropdown() {
        this.searchDropdown.classList.remove('show');
    }
};

// User authentication management
async function fetchUserInfo() {
    try {
        const response = await fetch('/Account/GetUserInfo');
        const data = await response.json();

        const userAvatar = document.getElementById('userAvatar');
        const avatarFallback = document.getElementById('avatarFallback');
        const googleSignInItem = document.getElementById('googleSignInItem');
        const logoutItem = document.getElementById('logoutItem');
        const userName = document.querySelector('#userDropdown span');

        if (data.authenticated) {
            // Update avatar with Google profile picture
            if (data.picture && userAvatar) {
                userAvatar.src = data.picture;
                userAvatar.style.display = 'block';
                if (avatarFallback) avatarFallback.style.display = 'none';
            }

            // Update user name
            if (data.name && userName) {
                userName.textContent = data.name;
            }

            // Show logout, hide sign in
            if (googleSignInItem) googleSignInItem.style.display = 'none';
            if (logoutItem) logoutItem.style.display = 'block';

            // Store user info
            localStorage.setItem('paygohub-user', JSON.stringify(data));
        } else {
            // Show sign in, hide logout
            if (googleSignInItem) googleSignInItem.style.display = 'block';
            if (logoutItem) logoutItem.style.display = 'none';

            // Clear stored user info
            localStorage.removeItem('paygohub-user');
        }
    } catch (error) {
        console.log('User not authenticated');
    }
}
