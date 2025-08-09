let chart;

// Sử dụng data từ backend được truyền xuống từ view
const backendData = window.statisticsData || {};
const baseUrl = backendData.baseUrl || '/Admin/Statistics';

// Hiển thị tổng doanh thu từ backend
if (backendData.totalRevenue) {
    document.getElementById('totalRevenue').textContent =
        backendData.totalRevenue.toLocaleString('vi-VN') + ' đ';
}

// Tải dữ liệu mặc định (tháng hiện tại)
async function loadChartData(branchId = null) {
    try {
        const url = branchId
            ? `${baseUrl}/GetCurrentMonthRevenueData?branchId=${encodeURIComponent(branchId)}`
            : `${baseUrl}/GetCurrentMonthRevenueData`;

        const response = await fetch(url);
        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();

        if (data && data.length > 0) {
            renderChart(data, 'rgba(76, 175, 80, 0.8)', '#4CAF50');
            showResultMessage('Dữ liệu đã được tải thành công!');
        } else {
            renderChart([], 'rgba(76, 175, 80, 0.8)', '#4CAF50');
            showResultMessage('Không có dữ liệu cho tháng hiện tại', 'warning');
        }
    } catch (error) {
        console.error('Lỗi khi tải dữ liệu biểu đồ:', error);
        handleError('Không thể tải dữ liệu. Vui lòng thử lại sau.');
    }
}

// Vẽ biểu đồ với data validation
function renderChart(data, bgColor = '#4CAF50', borderColor = '#388E3C') {
    // Validate và xử lý data
    const validData = Array.isArray(data) ? data : [];
    const labels = validData.map(d => d.dateLabel || 'N/A');
    const values = validData.map(d => parseFloat(d.revenue) || 0);

    // Destroy existing chart
    if (chart) {
        chart.destroy();
    }
    const canvas = document.getElementById('revenueChart');
    if (!canvas) {
        console.error('Canvas element not found');
        return;
    }
    const ctx = canvas.getContext('2d');
    if (!ctx) {
        console.error('Canvas element not found');
        return;
    }
    // Tạo gradient hiện đại
    const gradient = ctx.createLinearGradient(0, 0, 0, 400);
    gradient.addColorStop(0, 'rgba(76, 175, 80, 0.9)');
    gradient.addColorStop(1, 'rgba(76, 175, 80, 0.2)');
    // Khởi tạo biểu đồ
    chart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [{
                label: 'Doanh thu (VNĐ)',
                data: values,
                backgroundColor: gradient,
                borderColor: borderColor,
                borderWidth: 2,
                borderRadius: 10,
                borderSkipped: false,
                hoverBackgroundColor: '#66bb6a',
                hoverBorderColor: '#2e7d32',
                tension: 0.4,
                fill: true,
                pointRadius: 5,
                pointBackgroundColor: '#4CAF50',
                pointBorderColor: '#fff',
                pointBorderWidth: 2,
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: {
                    display: false
                },
                tooltip: {
                    backgroundColor: 'rgba(0,0,0,0.8)',
                    titleColor: '#fff',
                    bodyColor: '#fff',
                    borderColor: borderColor,
                    borderWidth: 1,
                    cornerRadius: 8,
                    displayColors: false,
                    callbacks: {
                        label: function (context) {
                            return 'Doanh thu: ' + context.parsed.y.toLocaleString('vi-VN') + ' đ';
                        }
                    }
                },
                title: {
                    display: false
                }
            },
            scales: {
                y: {
                    beginAtZero: true,
                    grid: {
                        color: 'rgba(0,0,0,0.1)',
                        borderDash: [5, 5]
                    },
                    ticks: {
                        maxRotation: 0,
                        minRotation: 0,
                        callback: function (value) {
                            return value >= 1_000_000
                                ? (value / 1_000_000).toLocaleString('vi-VN') + 'M'
                                : value.toLocaleString('vi-VN');
                        },
                        color: '#444',
                        font: {
                            size: 12,
                            weight: 500
                        }
                    }
                },
                x: {
                    grid: {
                        display: false
                    },
                    ticks: {
                        color: '#444',
                        font: {
                            size: 12,
                            weight: 500
                        }
                    }
                }
            },
            interaction: {
                intersect: false,
                mode: 'index'
            },
            animation: {
                duration: 1200,
                easing: 'easeOutQuart',
                delay: (context) => context.dataIndex * 100
            }
        }
    });
    // Cập nhật tổng doanh thu nếu cần
    updateTotalRevenue(validData);
}

// Cập nhật tổng doanh thu
function updateTotalRevenue(data) {
    if (!data || data.length === 0) return;

    const total = data.reduce((sum, item) => sum + (parseFloat(item.revenue) || 0), 0);
    document.getElementById('totalRevenue').textContent =
        total.toLocaleString('vi-VN') + ' đ';
}

// Hiển thị thông báo kết quả
function showResultMessage(message, type = 'success') {
    const resultSection = document.getElementById('resultSection');
    const resultText = document.querySelector('.result-text');
    const resultIcon = document.querySelector('.result-icon i');

    if (resultSection && resultText) {
        resultText.textContent = message;

        // Thay đổi icon theo loại thông báo
        if (resultIcon) {
            resultIcon.className = type === 'warning' ? 'fas fa-exclamation-triangle' : 'fas fa-check-circle';
        }

        resultSection.style.display = 'block';

        // Tự động ẩn sau 3 giây
        setTimeout(() => {
            resultSection.style.display = 'none';
        }, 3000);
    }
}

// Xử lý lỗi
function handleError(message) {
    showResultMessage(message, 'error');
    renderChart([], 'rgba(255, 0, 0, 0.3)', '#f44336');
}

// Hiển thị/ẩn loading
function toggleLoading(show) {
    const loading = document.getElementById('loading');
    if (loading) {
        loading.style.display = show ? 'flex' : 'none';
    }
}

// Xử lý lọc theo khoảng ngày
document.getElementById('filterForm').addEventListener('submit', async function (e) {
    e.preventDefault();

    const fromDate = document.getElementById('fromDate').value;
    const toDate = document.getElementById('toDate').value;

    if (!fromDate || !toDate) {
        showResultMessage('Vui lòng chọn đầy đủ ngày bắt đầu và kết thúc', 'warning');
        return;
    }

    if (new Date(fromDate) > new Date(toDate)) {
        showResultMessage('Ngày bắt đầu không được lớn hơn ngày kết thúc', 'warning');
        return;
    }

    toggleLoading(true);

    try {
        const branchId = document.getElementById('BranchId')?.value;
        const type = document.getElementById("orderType").value;

        const response = await fetch(`${baseUrl}/GetRevenueByDateRange?branchId=${branchId}&type=${type}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: JSON.stringify({ fromDate, toDate })
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();

        renderChart(data, 'rgba(255, 159, 64, 0.8)', '#FF9800');
        updateChartTitle(`Biểu đồ doanh thu từ ${formatDate(fromDate)} đến ${formatDate(toDate)}`);
        showResultMessage(`Đã tải dữ liệu từ ${formatDate(fromDate)} đến ${formatDate(toDate)}`);

    } catch (error) {
        console.error('Lỗi khi lấy dữ liệu theo khoảng ngày:', error);
        handleError('Lỗi khi lấy dữ liệu theo khoảng ngày. Vui lòng thử lại.');
    }

    toggleLoading(false);
});

// Xử lý lọc theo tháng/năm
document.getElementById('monthYearForm').addEventListener('submit', async function (e) {
    e.preventDefault();

    const month = parseInt(document.getElementById('monthSelect').value);
    const year = parseInt(document.getElementById('yearSelect').value);

    if (!month || !year) {
        showResultMessage('Vui lòng chọn tháng và năm', 'warning');
        return;
    }

    toggleLoading(true);

    try {
        const branchId = document.getElementById('BranchId')?.value;
        const type = document.getElementById("orderType").value;

        const response = await fetch(`${baseUrl}/GetRevenueByMonth?branchId=${branchId}&type=${type}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: JSON.stringify({ month, year })
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();

        renderChart(data, 'rgba(33, 150, 243, 0.8)', '#2196F3');
        updateChartTitle(`Biểu đồ doanh thu tháng ${month}/${year}`);
        showResultMessage(`Đã tải dữ liệu tháng ${month}/${year}`);

    } catch (error) {
        console.error('Lỗi khi lọc theo tháng/năm:', error);
        handleError('Lỗi khi lấy dữ liệu theo tháng/năm. Vui lòng thử lại.');
    }

    toggleLoading(false);
});

// Xử lý lọc theo năm
document.getElementById('yearOnlyForm').addEventListener('submit', async function (e) {
    e.preventDefault();

    const year = parseInt(document.getElementById('yearOnlySelect').value);

    if (!year) {
        showResultMessage('Vui lòng chọn năm', 'warning');
        return;
    }

    toggleLoading(true);

    try {
        const branchId = document.getElementById('BranchId')?.value;
        const type = document.getElementById("orderType").value;

        const response = await fetch(`${baseUrl}/GetRevenueByYear?year=${year}&branchId=${branchId}&type=${type}`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
                'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]')?.value || ''
            },
            body: JSON.stringify({ year })
        });

        if (!response.ok) {
            throw new Error(`HTTP error! status: ${response.status}`);
        }

        const data = await response.json();

        renderChart(data, 'rgba(156, 39, 176, 0.8)', '#9C27B0');
        updateChartTitle(`Biểu đồ doanh thu năm ${year}`);
        showResultMessage(`Đã tải dữ liệu năm ${year}`);

    } catch (error) {
        console.error('Lỗi khi lọc theo năm:', error);
        handleError('Lỗi khi lấy dữ liệu theo năm. Vui lòng thử lại.');
    }

    toggleLoading(false);
});

// Cập nhật tiêu đề biểu đồ
function updateChartTitle(title) {
    const chartTitle = document.getElementById('chartTitle');
    if (chartTitle) {
        chartTitle.innerHTML = '<i class="fas fa-chart-bar"></i> ' + title;
    }
}

// Format date cho hiển thị
function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('vi-VN');
}

// Xử lý chuyển tab
document.querySelectorAll('.tab-btn').forEach(btn => {
    btn.addEventListener('click', () => {
        const tab = btn.getAttribute('data-tab');

        // Đổi tab active
        document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
        btn.classList.add('active');

        // Ẩn hiện nội dung form
        document.querySelectorAll('.tab-content').forEach(form => {
            if (form.getAttribute('data-tab') === tab) {
                form.classList.remove('hidden');
            } else {
                form.classList.add('hidden');
            }
        });
    });
});

// Thiết lập ngày mặc định
function setDefaultDates() {
    const today = new Date();
    const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);

    const fromDateInput = document.getElementById('fromDate');
    const toDateInput = document.getElementById('toDate');

    if (fromDateInput) {
        fromDateInput.value = firstDay.toISOString().split('T')[0];
    }
    if (toDateInput) {
        toDateInput.value = today.toISOString().split('T')[0];
    }

    // Set tháng và năm hiện tại cho select boxes
    const monthSelect = document.getElementById('monthSelect');
    const yearSelects = [document.getElementById('yearSelect'), document.getElementById('yearOnlySelect')];

    if (monthSelect && window.currentMonth) {
        monthSelect.value = window.currentMonth;
    }

    yearSelects.forEach(yearSelect => {
        if (yearSelect && window.currentYear) {
            yearSelect.value = window.currentYear;
        }
    });
}

// Khởi tạo khi trang đã load
document.addEventListener('DOMContentLoaded', function () {
    setDefaultDates();
    loadChartData();
});

// Xử lý resize window
window.addEventListener('resize', function () {
    if (chart) {
        chart.resize();
    }
});