// Frontend Logic for Order Management System

// API Base URLs
const API_ORDERS = '/api/orders';
const API_CUSTOMERS = '/api/customers';

// Application State
let ordersState = [];
let currentFilterMode = 'all'; // 'all', 'search', 'status', 'top'
let activeStatusFilter = '';
let activeSearchKeyword = '';
let isTop5Active = false;

// LINQ Code Templates
const LINQ_TEMPLATES = {
    all: {
        desc: "Lấy danh sách tất cả đơn hàng, liên kết thông tin Khách hàng, sắp xếp giảm dần theo ngày tạo.",
        code: `// GET /api/orders\nvar orders = await _context.Orders\n    .Include(o => o.Customer)\n    .OrderByDescending(o => o.OrderDate)\n    .Select(o => new OrderResponseDto {\n        Id = o.Id,\n        CustomerId = o.CustomerId,\n        CustomerName = o.Customer.Name,\n        OrderDate = o.OrderDate,\n        TotalAmount = o.TotalAmount,\n        Status = o.Status\n    }).ToListAsync();`
    },
    search: {
        desc: "Tìm kiếm các đơn hàng có tên khách hàng chứa từ khóa được nhập vào (không phân biệt chữ hoa thường).",
        code: `// GET /api/orders/search?keyword=...\nvar orders = await _context.Orders\n    .Include(o => o.Customer)\n    .Where(o => o.Customer.Name.Contains(keyword))\n    .OrderByDescending(o => o.OrderDate)\n    .Select(o => new OrderResponseDto { ... })\n    .ToListAsync();`
    },
    status: {
        desc: "Lọc các đơn hàng có trạng thái trùng khớp với lựa chọn (ví dụ: 'Completed', 'Processing').",
        code: `// GET /api/orders/status/{status}\nvar orders = await _context.Orders\n    .Include(o => o.Customer)\n    .Where(o => o.Status == status)\n    .OrderByDescending(o => o.OrderDate)\n    .Select(o => new OrderResponseDto { ... })\n    .ToListAsync();`
    },
    top: {
        desc: "Sắp xếp giảm dần theo tổng tiền và lấy ra 5 đơn hàng có giá trị cao nhất.",
        code: `// GET /api/orders/top\nvar orders = await _context.Orders\n    .Include(o => o.Customer)\n    .OrderByDescending(o => o.TotalAmount)\n    .Take(5)\n    .Select(o => new OrderResponseDto { ... })\n    .ToListAsync();`
    },
    revenue: {
        desc: "Lọc các đơn hàng đã 'Completed' và tính tổng doanh thu bằng phương thức Sum.",
        code: `// GET /api/orders/revenue\nvar totalRevenue = await _context.Orders\n    .Where(o => o.Status == "Completed")\n    .SumAsync(o => o.TotalAmount);`
    },
    statistics: {
        desc: "Gom nhóm theo Trạng thái (Status) và đếm (Count) số lượng đơn hàng tương ứng mỗi nhóm.",
        code: `// GET /api/orders/statistics\nvar stats = await _context.Orders\n    .GroupBy(o => o.Status)\n    .Select(g => new {\n        Status = g.Key,\n        Count = g.Count()\n    }).ToListAsync();`
    },
    create: {
        desc: "Kiểm tra khách hàng có tồn tại bằng Any() trước khi tạo đơn hàng mới.",
        code: `// POST /api/orders\nvar customerExists = await _context.Customers\n    .AnyAsync(c => c.Id == dto.CustomerId);\n\nif (customerExists) {\n    _context.Orders.Add(newOrder);\n    await _context.SaveChangesAsync();\n}`
    },
    update: {
        desc: "Cập nhật trạng thái của đơn hàng có mã trùng khớp bằng FirstOrDefault().",
        code: `// PUT /api/orders/{id}\nvar order = await _context.Orders\n    .FirstOrDefaultAsync(o => o.Id == id);\nif (order != null) {\n    order.Status = dto.Status;\n    await _context.SaveChangesAsync();\n}`
    },
    delete: {
        desc: "Tìm kiếm đơn hàng theo mã bằng FirstOrDefault() và tiến hành xóa.",
        code: `// DELETE /api/orders/{id}\nvar order = await _context.Orders\n    .FirstOrDefaultAsync(o => o.Id == id);\nif (order != null) {\n    _context.Orders.Remove(order);\n    await _context.SaveChangesAsync();\n}`
    }
};

// DOM Elements
const ordersTableBody = document.getElementById('orders-table-body');
const statTotalRevenue = document.getElementById('stat-total-revenue');
const statTotalOrders = document.getElementById('stat-total-orders');
const statsStatusGroup = document.getElementById('stats-status-group');
const searchInput = document.getElementById('search-customer');
const btnClearSearch = document.getElementById('btn-clear-search');
const filterStatusSelect = document.getElementById('filter-status');
const btnToggleTop5 = document.getElementById('btn-toggle-top-5');
const btnResetFilters = document.getElementById('btn-reset-filters');
const tableRecordCount = document.getElementById('table-record-count');
const createOrderModal = document.getElementById('create-order-modal');
const btnOpenCreateModal = document.getElementById('btn-open-create-modal');
const btnCloseCreateModal = document.getElementById('btn-close-create-modal');
const btnCancelCreateModal = document.getElementById('btn-cancel-create-modal');
const createOrderForm = document.getElementById('create-order-form');
const customerSelect = document.getElementById('customer-select');
const linqDesc = document.getElementById('linq-desc');
const linqCode = document.getElementById('linq-code');
const toastContainer = document.getElementById('toast-container');

// Initialize App
document.addEventListener('DOMContentLoaded', () => {
    // Initialize Icons
    lucide.createIcons();
    
    // Set Current Time
    setCurrentDate();
    
    // Set Default LINQ
    updateLinqPanel('all');
    
    // Load Data
    loadDashboardData();
    loadCustomers();
    
    // Setup Event Listeners
    setupEventListeners();
});

// Setup date text
function setCurrentDate() {
    const days = ['Chủ Nhật', 'Thứ Hai', 'Thứ Ba', 'Thứ Tư', 'Thứ Năm', 'Thứ Sáu', 'Thứ Bảy'];
    const now = new Date();
    const dayName = days[now.getDay()];
    const dateStr = `${dayName}, ngày ${now.getDate()} tháng ${now.getMonth() + 1} năm ${now.getFullYear()}`;
    document.getElementById('current-time').innerText = dateStr;
}

// Update LINQ Panel
function updateLinqPanel(key) {
    if (LINQ_TEMPLATES[key]) {
        linqDesc.innerText = LINQ_TEMPLATES[key].desc;
        linqCode.innerText = LINQ_TEMPLATES[key].code;
        
        // Visual cue: Add highlight animation
        const panel = document.querySelector('.linq-panel');
        if (panel) {
            panel.style.boxShadow = '0 0 10px rgba(79, 70, 229, 0.2)';
            setTimeout(() => {
                panel.style.boxShadow = 'none';
            }, 800);
        }
    }
}

// Global click wrapper to highlight LINQ
window.highlightLinq = function(key) {
    updateLinqPanel(key);
};

// Event Listeners
function setupEventListeners() {
    // Search Box Logic
    let searchTimeout;
    searchInput.addEventListener('input', (e) => {
        const value = e.target.value;
        btnClearSearch.style.display = value.length > 0 ? 'block' : 'none';
        
        // Debounce search
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            activeSearchKeyword = value.trim();
            isTop5Active = false; // Disable top 5 when searching
            btnToggleTop5.classList.remove('active');
            filterStatusSelect.value = ''; // Reset status filter
            activeStatusFilter = '';
            
            if (activeSearchKeyword.length > 0) {
                currentFilterMode = 'search';
                loadOrders(`/search?keyword=${encodeURIComponent(activeSearchKeyword)}`, 'search');
            } else {
                currentFilterMode = 'all';
                loadOrders('', 'all');
            }
        }, 400);
    });

    btnClearSearch.addEventListener('click', () => {
        searchInput.value = '';
        btnClearSearch.style.display = 'none';
        activeSearchKeyword = '';
        currentFilterMode = 'all';
        loadOrders('', 'all');
    });

    // Status Filter Logic
    filterStatusSelect.addEventListener('change', (e) => {
        activeStatusFilter = e.target.value;
        searchInput.value = ''; // Reset search
        btnClearSearch.style.display = 'none';
        activeSearchKeyword = '';
        isTop5Active = false; // Disable top 5
        btnToggleTop5.classList.remove('active');

        if (activeStatusFilter) {
            currentFilterMode = 'status';
            loadOrders(`/status/${activeStatusFilter}`, 'status');
        } else {
            currentFilterMode = 'all';
            loadOrders('', 'all');
        }
    });

    // Top 5 Toggle Logic
    btnToggleTop5.addEventListener('click', () => {
        isTop5Active = !isTop5Active;
        btnToggleTop5.classList.toggle('active', isTop5Active);
        
        searchInput.value = ''; // Reset search
        btnClearSearch.style.display = 'none';
        activeSearchKeyword = '';
        filterStatusSelect.value = ''; // Reset status filter
        activeStatusFilter = '';

        if (isTop5Active) {
            currentFilterMode = 'top';
            loadOrders('/top', 'top');
        } else {
            currentFilterMode = 'all';
            loadOrders('', 'all');
        }
    });

    // Reset Filters Logic
    btnResetFilters.addEventListener('click', () => {
        searchInput.value = '';
        btnClearSearch.style.display = 'none';
        activeSearchKeyword = '';
        filterStatusSelect.value = '';
        activeStatusFilter = '';
        isTop5Active = false;
        btnToggleTop5.classList.remove('active');
        
        currentFilterMode = 'all';
        loadDashboardData();
        showToast('Đã làm mới dữ liệu!', 'success');
    });

    // Modal Events
    btnOpenCreateModal.addEventListener('click', () => {
        createOrderModal.classList.add('show');
        updateLinqPanel('create');
    });

    const closeModal = () => {
        createOrderModal.classList.remove('show');
        createOrderForm.reset();
        updateLinqPanel(currentFilterMode);
    };

    btnCloseCreateModal.addEventListener('click', closeModal);
    btnCancelCreateModal.addEventListener('click', closeModal);
    
    // Close modal when clicking outside
    createOrderModal.addEventListener('click', (e) => {
        if (e.target === createOrderModal) {
            closeModal();
        }
    });

    // Create Order Submission
    createOrderForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        
        const customerId = parseInt(customerSelect.value);
        const amount = parseFloat(document.getElementById('order-amount').value);
        const status = document.getElementById('order-status').value;

        if (isNaN(customerId) || isNaN(amount) || amount <= 0) {
            showToast('Dữ liệu nhập vào không hợp lệ.', 'error');
            return;
        }

        try {
            updateLinqPanel('create');
            const response = await fetch(API_ORDERS, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    customerId: customerId,
                    totalAmount: amount,
                    status: status
                })
            });

            if (response.ok) {
                showToast('Tạo đơn hàng mới thành công!', 'success');
                closeModal();
                loadDashboardData(); // Reload all data
            } else {
                const errorData = await response.json();
                showToast(errorData.Message || 'Có lỗi xảy ra khi tạo đơn.', 'error');
            }
        } catch (error) {
            console.error('Error creating order:', error);
            showToast('Lỗi kết nối máy chủ.', 'error');
        }
    });

    // Dynamic hover glow effect on cards
    const cards = document.querySelectorAll('.stat-card');
    cards.forEach(card => {
        card.addEventListener('mousemove', (e) => {
            const rect = card.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            card.style.setProperty('--x', `${x}px`);
            card.style.setProperty('--y', `${y}px`);
        });
    });

    // Close open inline status dropdowns when clicking outside
    document.addEventListener('click', (e) => {
        if (!e.target.closest('.status-change-wrapper')) {
            document.querySelectorAll('.status-dropdown').forEach(dropdown => {
                dropdown.classList.remove('show');
            });
        }
    });
}

// Load both orders and statistics
async function loadDashboardData() {
    await Promise.all([
        loadOrders('', 'all'),
        loadStatistics(),
        loadRevenue()
    ]);
}

// Load Orders from specific endpoint
async function loadOrders(endpointQuery = '', linqKey = 'all') {
    renderTableLoading();
    updateLinqPanel(linqKey);

    try {
        const response = await fetch(`${API_ORDERS}${endpointQuery}`);
        if (!response.ok) {
            throw new Error('Không thể tải danh sách đơn hàng');
        }
        
        ordersState = await response.json();
        renderOrdersTable(ordersState);
        tableRecordCount.innerText = `Hiển thị ${ordersState.length} đơn hàng`;
    } catch (error) {
        console.error('Error loading orders:', error);
        renderTableError(error.message);
    }
}

// Load Customers for dropdown
async function loadCustomers() {
    try {
        const response = await fetch(API_CUSTOMERS);
        if (!response.ok) throw new Error('Không thể tải khách hàng');
        
        const customers = await response.json();
        customerSelect.innerHTML = '<option value="" disabled selected>Chọn khách hàng...</option>';
        customers.forEach(c => {
            const option = document.createElement('option');
            option.value = c.id; // Map camelCase property "id"
            option.innerText = `${c.name} (${c.email})`; // Map camelCase properties
            customerSelect.appendChild(option);
        });
    } catch (error) {
        console.error('Error loading customers:', error);
        customerSelect.innerHTML = '<option value="" disabled>Lỗi tải danh sách khách hàng</option>';
    }
}

// Load Statistics (GroupBy)
async function loadStatistics() {
    try {
        const response = await fetch(`${API_ORDERS}/statistics`);
        if (!response.ok) throw new Error('Lỗi tải thống kê');
        
        const stats = await response.json();
        
        // Process stats. Expected array of { status: "...", count: X }
        const statsMap = {
            Pending: 0,
            Processing: 0,
            Completed: 0,
            Cancelled: 0
        };
        
        let total = 0;
        stats.forEach(item => {
            if (statsMap[item.status] !== undefined) {
                statsMap[item.status] = item.count;
                total += item.count;
            }
        });
        
        statTotalOrders.innerText = total;
        
        // Render progress bars
        statsStatusGroup.innerHTML = '';
        
        const VietnameseLabels = {
            Pending: 'Chờ xử lý',
            Processing: 'Đang xử lý',
            Completed: 'Hoàn thành',
            Cancelled: 'Đã hủy'
        };
        
        const statusClasses = {
            Pending: 'pending',
            Processing: 'processing',
            Completed: 'completed',
            Cancelled: 'cancelled'
        };

        Object.keys(statsMap).forEach(status => {
            const count = statsMap[status];
            const percent = total > 0 ? Math.round((count / total) * 100) : 0;
            
            const progressHtml = `
                <div class="progress-item" title="${VietnameseLabels[status]}: ${count} đơn (${percent}%)">
                    <div class="progress-label-row">
                        <div class="progress-status">
                            <span class="status-dot ${statusClasses[status]}"></span>
                            <span>${VietnameseLabels[status]}</span>
                        </div>
                        <span class="progress-count">${count} (${percent}%)</span>
                    </div>
                    <div class="progress-bar-bg">
                        <div class="progress-bar-fill ${statusClasses[status]}" style="width: ${percent}%"></div>
                    </div>
                </div>
            `;
            statsStatusGroup.insertAdjacentHTML('beforeend', progressHtml);
        });
    } catch (error) {
        console.error('Error loading stats:', error);
        statsStatusGroup.innerHTML = '<p style="color: var(--color-cancelled); font-size: 0.8rem;">Lỗi tải biểu đồ thống kê</p>';
    }
}

// Load Revenue (Sum)
async function loadRevenue() {
    try {
        const response = await fetch(`${API_ORDERS}/revenue`);
        if (!response.ok) throw new Error('Lỗi tải doanh thu');
        
        const data = await response.json();
        statTotalRevenue.innerText = formatCurrency(data.totalRevenue); // Map camelCase property "totalRevenue"
    } catch (error) {
        console.error('Error loading revenue:', error);
        statTotalRevenue.innerText = 'Lỗi tải ₫';
    }
}

// Render Table Functions
function renderTableLoading() {
    ordersTableBody.innerHTML = `
        <tr>
            <td colspan="6" class="text-center py-5">
                <div class="loading-spinner"></div>
                <p style="margin-top: 10px; color: var(--text-muted);">Đang tải dữ liệu từ máy chủ...</p>
            </td>
        </tr>
    `;
}

function renderTableError(msg) {
    ordersTableBody.innerHTML = `
        <tr>
            <td colspan="6" class="text-center py-5" style="color: var(--color-cancelled);">
                <i data-lucide="alert-triangle" style="width: 40px; height: 40px; margin: 0 auto 10px;"></i>
                <p><strong>Lỗi:</strong> ${msg}</p>
                <button class="btn btn-secondary" style="margin-top: 15px;" onclick="loadDashboardData()">Thử lại</button>
            </td>
        </tr>
    `;
    lucide.createIcons();
}

function renderOrdersTable(orders) {
    if (orders.length === 0) {
        ordersTableBody.innerHTML = `
            <tr>
                <td colspan="6" class="text-center py-5">
                    <i data-lucide="inbox" style="width: 40px; height: 40px; margin: 0 auto 10px; color: var(--text-muted);"></i>
                    <p style="color: var(--text-muted);">Không tìm thấy đơn hàng nào.</p>
                </td>
            </tr>
        `;
        lucide.createIcons();
        return;
    }

    ordersTableBody.innerHTML = '';
    orders.forEach(o => {
        const row = document.createElement('tr');
        row.id = `order-row-${o.id}`; // Map camelCase property "id"
        
        const statusBadgeClass = `badge-${o.status.toLowerCase()}`; // Map camelCase property "status"
        
        row.innerHTML = `
            <td><span class="order-id-badge">#${o.id}</span></td>
            <td>
                <div class="customer-name-cell">${escapeHtml(o.customerName)}</div> <!-- Map camelCase "customerName" -->
                <div style="font-size: 0.75rem; color: var(--text-muted);">Mã KH: #${o.customerId}</div> <!-- Map camelCase "customerId" -->
            </td>
            <td>
                <div>${formatDate(o.orderDate)}</div> <!-- Map camelCase "orderDate" -->
                <div style="font-size: 0.75rem; color: var(--text-muted);">${formatTime(o.orderDate)}</div>
            </td>
            <td><span class="order-amount-cell">${formatCurrency(o.totalAmount)}</span></td> <!-- Map camelCase "totalAmount" -->
            <td><span class="badge ${statusBadgeClass}">${o.status}</span></td>
            <td>
                <div class="status-actions-cell">
                    <!-- Inline status dropdown wrapper -->
                    <div class="status-change-wrapper">
                        <button class="btn-status-change" onclick="toggleStatusDropdown(event, ${o.id})">
                            <span>Đổi trạng thái</span>
                            <i data-lucide="chevron-down" style="width: 12px; height: 12px;"></i>
                        </button>
                        <div class="status-dropdown" id="status-dropdown-${o.id}">
                            <button class="status-dropdown-item" onclick="changeStatus(${o.id}, 'Pending')">
                                <span class="status-dot pending"></span>Pending
                            </button>
                            <button class="status-dropdown-item" onclick="changeStatus(${o.id}, 'Processing')">
                                <span class="status-dot processing"></span>Processing
                            </button>
                            <button class="status-dropdown-item" onclick="changeStatus(${o.id}, 'Completed')">
                                <span class="status-dot completed"></span>Completed
                            </button>
                            <button class="status-dropdown-item" onclick="changeStatus(${o.id}, 'Cancelled')">
                                <span class="status-dot cancelled"></span>Cancelled
                            </button>
                        </div>
                    </div>
                    <!-- Delete Button -->
                    <button class="btn-delete" title="Xóa đơn hàng" onclick="deleteOrder(${o.id})">
                        <i data-lucide="trash-2"></i>
                    </button>
                </div>
            </td>
        `;
        ordersTableBody.appendChild(row);
    });

    lucide.createIcons();
}

// Status Toggle Handler
window.toggleStatusDropdown = function(event, orderId) {
    event.stopPropagation();
    
    // Close other dropdowns
    document.querySelectorAll('.status-dropdown').forEach(dropdown => {
        if (dropdown.id !== `status-dropdown-${orderId}`) {
            dropdown.classList.remove('show');
        }
    });

    const dropdown = document.getElementById(`status-dropdown-${orderId}`);
    dropdown.classList.toggle('show');
};

// Update Order Status API call
window.changeStatus = async function(orderId, newStatus) {
    updateLinqPanel('update');
    try {
        const response = await fetch(`${API_ORDERS}/${orderId}`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                status: newStatus
            })
        });

        if (response.ok) {
            showToast(`Đã chuyển đơn hàng #${orderId} sang ${newStatus}!`, 'success');
            
            // Refresh stats & data
            await Promise.all([
                refreshCurrentView(),
                loadStatistics(),
                loadRevenue()
            ]);
        } else {
            const error = await response.json();
            showToast(error.Message || 'Lỗi cập nhật trạng thái.', 'error');
        }
    } catch (error) {
        console.error('Error changing status:', error);
        showToast('Lỗi kết nối máy chủ.', 'error');
    }
};

// Delete Order API call
window.deleteOrder = async function(orderId) {
    if (!confirm(`Bạn có chắc chắn muốn xóa đơn hàng #${orderId}?`)) {
        return;
    }

    updateLinqPanel('delete');
    try {
        const response = await fetch(`${API_ORDERS}/${orderId}`, {
            method: 'DELETE'
        });

        if (response.ok) {
            showToast(`Đã xóa đơn hàng #${orderId} thành công!`, 'success');
            
            // Fade-out effect
            const row = document.getElementById(`order-row-${orderId}`);
            if (row) {
                row.style.transform = 'translateX(-20px)';
                row.style.opacity = '0';
                row.style.transition = 'all 0.3s ease-out';
                setTimeout(async () => {
                    await Promise.all([
                        refreshCurrentView(),
                        loadStatistics(),
                        loadRevenue()
                    ]);
                }, 300);
            } else {
                await Promise.all([
                    refreshCurrentView(),
                    loadStatistics(),
                    loadRevenue()
                ]);
            }
        } else {
            const error = await response.json();
            showToast(error.Message || 'Lỗi khi xóa đơn hàng.', 'error');
        }
    } catch (error) {
        console.error('Error deleting order:', error);
        showToast('Lỗi kết nối máy chủ.', 'error');
    }
};

// Helper to reload orders based on current filters
async function refreshCurrentView() {
    if (currentFilterMode === 'search') {
        await loadOrders(`/search?keyword=${encodeURIComponent(activeSearchKeyword)}`, 'search');
    } else if (currentFilterMode === 'status') {
        await loadOrders(`/status/${activeStatusFilter}`, 'status');
    } else if (currentFilterMode === 'top') {
        await loadOrders('/top', 'top');
    } else {
        await loadOrders('', 'all');
    }
}

// Utility Functions
function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
}

function formatDate(dateString) {
    const d = new Date(dateString);
    return d.toLocaleDateString('vi-VN', { year: 'numeric', month: '2-digit', day: '2-digit' });
}

function formatTime(dateString) {
    const d = new Date(dateString);
    return d.toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
}

function escapeHtml(unsafe) {
    if (!unsafe) return '';
    return unsafe
         .replace(/&/g, "&amp;")
         .replace(/</g, "&lt;")
         .replace(/>/g, "&gt;")
         .replace(/"/g, "&quot;")
         .replace(/'/g, "&#039;");
}

function showToast(message, type = 'success') {
    const toast = document.createElement('div');
    toast.className = `toast ${type}`;
    
    const icon = type === 'success' ? 'check-circle' : 'alert-circle';
    
    toast.innerHTML = `
        <i data-lucide="${icon}"></i>
        <span>${message}</span>
    `;
    
    toastContainer.appendChild(toast);
    lucide.createIcons();

    // Auto-remove toast
    setTimeout(() => {
        toast.classList.add('fade-out');
        toast.addEventListener('animationend', () => {
            toast.remove();
        });
    }, 3500);
}
