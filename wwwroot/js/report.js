/**
 * [V.2.8.JS Report Management Frontend]
 * Kịch bản điều khiển giao diện (Client-side) phân hệ Báo cáo và Thống kê quản trị (UC21).
 * Kết nối trực tiếp với API endpoints phân hệ `IReportService` trên nền tảng .NET Core.
 * Hỗ trợ tổng hợp số liệu động, kết xuất báo cáo trực quan đa chiều và xuất dữ liệu báo cáo ra tệp Excel.
 */

// ===== KHỞI TẠO MẶC ĐỊNH THỜI GIAN =====
document.addEventListener('DOMContentLoaded', () => {
    const today = new Date();
    // Thiết lập mặc định ngày bắt đầu là ngày đầu tiên của tháng hiện tại
    const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);

    const fromDateInput = document.getElementById('fromDate');
    const toDateInput = document.getElementById('toDate');

    if (fromDateInput) fromDateInput.value = firstDay.toISOString().split('T')[0];
    if (toDateInput) toDateInput.value = today.toISOString().split('T')[0];

    // Tự động tải dữ liệu mặc định ban đầu
    generateReport();
});

// ===== TỔNG HỢP VÀ KẾT XUẤT BÁO CÁO DỮ LIỆU ĐỘNG (UC21) =====
/**
 * Gọi API tổng hợp dữ liệu báo cáo dựa trên khoảng thời gian và phân loại yêu cầu.
 * Khớp nối trực tiếp cấu trúc với OccupancyReportDto, RevenueReportDto, FinancialReportDto, StaffPerformanceReportDto.
 */
async function generateReport() {
    const fromDate = document.getElementById('fromDate').value;
    const toDate = document.getElementById('toDate').value;
    const reportType = document.getElementById('reportType').value;

    // 1. Kiểm tra điều kiện ràng buộc dữ liệu đầu vào (Validation nghiệp vụ)
    if (!fromDate || !toDate) {
        showAlert('warning', 'Vui lòng chọn đầy đủ khoảng thời gian cần kết xuất báo cáo.');
        return;
    }

    if (fromDate > toDate) {
        showAlert('warning', 'Thời gian bắt đầu (From date) phải trước hoặc trùng với thời gian kết thúc (To date).');
        return;
    }

    //Ẩn toàn bộ các khu vực hiển thị báo cáo cũ để chuẩn bị render dữ liệu mới
    hideAllReports();

    try {
        const response = await fetch(`/api/reports/${reportType}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ fromDate, toDate })
        });

        if (!response.ok) throw new Error('Failed to compile report metrics.');

        const data = await response.json();

        const reportContentContainer = document.getElementById('reportContent');
        if (reportContentContainer) reportContentContainer.classList.remove('d-none');

        // 2. Định tuyến xử lý Renderer tương ứng với phân loại biểu mẫu báo cáo tuyển chọn
        switch (reportType) {
            case 'occupancy':
                renderOccupancy(data);
                break;
            case 'revenue':
                renderRevenue(data);
                break;
            case 'financial':
                renderFinancial(data);
                break;
            case 'staffperformance':
                renderStaffPerformance(data);
                break;
            default:
                console.warn('Unknown report type specification:', reportType);
        }
    } catch (error) {
        console.error('[generateReport] Root cause analysis:', error);
        showAlert('danger', 'Hệ thống gặp lỗi trong quá trình tổng hợp số liệu từ máy chủ.');
    }
}

/** Giải phóng vùng hiển thị, đưa các sub-container báo cáo về trạng thái ẩn. */
function hideAllReports() {
    ['occupancyReport', 'revenueReport', 'financialReport', 'staffReport']
        .forEach(id => {
            const el = document.getElementById(id);
            if (el) el.classList.add('d-none');
        });
}

// ===== RENDER CHI TIẾT CÁC PHÂN LOẠI BÁO CÁO =====

/**
 * Kết xuất dữ liệu Báo cáo công suất phòng (Occupancy Report).
 * @param {Object} data - Đối tượng ánh xạ từ OccupancyReportDto.
 */
function renderOccupancy(data) {
    const container = document.getElementById('occupancyReport');
    if (!container) return;

    container.classList.remove('d-none');
    document.getElementById('occ_totalRooms').textContent = data.totalRooms;
    document.getElementById('occ_occupiedRooms').textContent = data.occupiedRooms;
    document.getElementById('occ_rate').textContent = `${data.occupancyRate}%`;

    const tbody = document.getElementById('occ_tableBody');
    tbody.innerHTML = data.details.length === 0
        ? `<tr><td colspan="5" class="text-center text-muted py-3">Không có dữ liệu lưu trú ghi nhận trong kỳ phát sinh</td></tr>`
        : data.details.map((d, i) => `
            <tr>
                <td>${i + 1}</td>
                <td class="fw-bold">Phòng ${d.roomNumber}</td>
                <td>${d.roomTypeName}</td>
                <td><i class="bi bi-moon-stars me-1 text-secondary"></i>${d.totalNights} đêm</td>
                <td>${getStatusBadge(d.status)}</td>
            </tr>
        `).join('');
}

/**
 * Kết xuất dữ liệu Báo cáo chi tiết doanh thu (Revenue Report).
 * @param {Object} data - Đối tượng ánh xạ từ RevenueReportDto.
 */
function renderRevenue(data) {
    const container = document.getElementById('revenueReport');
    if (!container) return;

    container.classList.remove('d-none');
    document.getElementById('rev_totalRevenue').textContent = formatPrice(data.totalRevenue);
    document.getElementById('rev_totalReservations').textContent = data.totalReservations;
    document.getElementById('rev_avgRevenue').textContent = formatPrice(data.averageRevenuePerReservation);

    const tbody = document.getElementById('rev_tableBody');
    tbody.innerHTML = data.details.length === 0
        ? `<tr><td colspan="7" class="text-center text-muted py-3">Không phát sinh giao dịch thanh toán trong khoảng thời gian này</td></tr>`
        : data.details.map((d, i) => `
            <tr>
                <td>${i + 1}</td>
                <td class="fw-bold">${d.guestName}</td>
                <td>Phòng ${d.roomNumber}</td>
                <td>${formatDate(d.checkInDate)}</td>
                <td>${formatDate(d.checkOutDate)}</td>
                <td>${d.nights} đêm</td>
                <td class="text-end text-success fw-bold">${formatPrice(d.revenue)}</td>
            </tr>
        `).join('');
}

/**
 * Kết xuất dữ liệu Báo cáo phân tích tài chính theo hạng phòng (Financial Report).
 * @param {Object} data - Đối tượng ánh xạ từ FinancialReportDto.
 */
function renderFinancial(data) {
    const container = document.getElementById('financialReport');
    if (!container) return;

    container.classList.remove('d-none');
    document.getElementById('fin_totalRevenue').textContent = formatPrice(data.totalRevenue);
    document.getElementById('fin_totalReservations').textContent = data.totalReservations;
    document.getElementById('fin_cancelledReservations').textContent = data.cancelledReservations;
    document.getElementById('fin_roomRevenue').textContent = formatPrice(data.roomRevenue);

    const tbody = document.getElementById('fin_tableBody');
    tbody.innerHTML = data.details.length === 0
        ? `<tr><td colspan="4" class="text-center text-muted py-3">Không có dữ liệu kế toán phân tích hạng phòng</td></tr>`
        : data.details.map((d, i) => `
            <tr>
                <td>${i + 1}</td>
                <td class="fw-bold">${d.roomTypeName}</td>
                <td>${d.totalReservations} lượt đặt</td>
                <td class="text-end fw-bold text-dark">${formatPrice(d.revenue)}</td>
            </tr>
        `).join('');
}

/**
 * Kết xuất dữ liệu Báo cáo năng suất nhân sự buồng phòng (Staff Performance Report).
 * @param {Object} data - Đối tượng ánh xạ từ StaffPerformanceReportDto.
 */
function renderStaffPerformance(data) {
    const container = document.getElementById('staffReport');
    if (!container) return;

    container.classList.remove('d-none');
    document.getElementById('staff_totalStaff').textContent = data.totalStaff;
    document.getElementById('staff_totalCompleted').textContent = data.totalTasksCompleted;

    const tbody = document.getElementById('staff_tableBody');
    tbody.innerHTML = data.details.length === 0
        ? `<tr><td colspan="8" class="text-center text-muted py-3">Không tìm thấy dữ liệu vận hành phân công công việc</td></tr>`
        : data.details.map((d, i) => `
            <tr>
                <td>${i + 1}</td>
                <td class="fw-bold text-dark">${d.staffName}</td>
                <td><span class="badge bg-light text-secondary border">${d.role}</span></td>
                <td>${d.totalTasks}</td>
                <td><span class="badge bg-success-subtle text-success border border-success-subtle">${d.completedTasks}</span></td>
                <td><span class="badge bg-warning-subtle text-warning-emphasis border border-warning-subtle">${d.pendingTasks}</span></td>
                <td><span class="badge bg-primary-subtle text-primary border border-primary-subtle">${d.inProgressTasks}</span></td>
                <td>
                    <div class="d-flex align-items-center gap-2">
                        <div class="progress flex-grow-1" style="height: 16px; border-radius: 8px;">
                            <div class="progress-bar bg-success progress-bar-striped" role="progressbar" 
                                 style="width: ${d.completionRate}%" 
                                 aria-valuenow="${d.completionRate}" aria-valuemin="0" aria-valuemax="100">
                            </div>
                        </div>
                        <span class="small fw-bold text-success">${d.completionRate}%</span>
                    </div>
                </td>
            </tr>
        `).join('');
}

// ===== EXPORT EXCEL DATA STREAM (UC21) =====
/** Kích hoạt luồng tải tệp tin báo cáo Excel từ API endpoint ExportToExcelAsync. */
async function exportReport() {
    const fromDate = document.getElementById('fromDate').value;
    const toDate = document.getElementById('toDate').value;
    const reportType = document.getElementById('reportType').value;

    if (!fromDate || !toDate) {
        showAlert('warning', 'Vui lòng xác định rõ khoảng thời gian trước khi xuất dữ liệu tệp.');
        return;
    }

    // Thiết lập đường dẫn API stream file kèm mã hóa URL an toàn
    const url = `/api/reports/export?type=${encodeURIComponent(reportType)}&fromDate=${encodeURIComponent(fromDate)}&toDate=${encodeURIComponent(toDate)}`;

    // Chuyển hướng trình duyệt kích hoạt tiến trình tải xuống file nhị phân trực tiếp
    window.location.href = url;
}

// ===== PRIVATE HELPERS =====

/**
 * Ánh xạ trạng thái đơn đặt phòng sang thẻ Badge màu giao diện đồng bộ.
 * @param {string} status - Trạng thái chuỗi (PENDING, CONFIRMED, CHECKED_IN, CHECKED_OUT, CANCELLED).
 */
function getStatusBadge(status) {
    const badges = {
        'PENDING': '<span class="badge bg-warning text-dark">Chờ xác nhận</span>',
        'CONFIRMED': '<span class="badge bg-primary">Đã xác nhận</span>',
        'CHECKED_IN': '<span class="badge bg-success">Checked In</span>',
        'CHECKED_OUT': '<span class="badge bg-secondary">Checked Out</span>',
        'CANCELLED': '<span class="badge bg-danger">Đã hủy bỏ</span>'
    };
    return badges[status] || `<span class="badge bg-dark">${status}</span>`;
}

/** Chuyển định dạng ISO chuỗi thời gian sang định dạng hiển thị ngày quốc gia Việt Nam (DD/MM/YYYY). */
function formatDate(dateStr) {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleDateString('vi-VN');
}

/** Định dạng số tiền tệ theo quy chuẩn quốc tế hiển thị tiền Việt Nam Đồng (VND). */
function formatPrice(price) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(price);
}

/**
 * Kích hoạt Banner cảnh báo/thông báo nhanh trạng thái nghiệp vụ.
 * @param {string} type - Định dạng lớp alert Bootstrap (success, danger, warning, info).
 * @param {string} message - Nội dung chuỗi văn bản thông báo.
 */
function showAlert(type, message) {
    const alertBox = document.getElementById('alertBox');
    if (!alertBox) return;

    alertBox.className = `alert alert-${type} position-fixed top-0 start-50 translate-middle-x mt-3 z-3 shadow`;
    alertBox.textContent = message;
    alertBox.classList.remove('d-none');

    // Tự động giải phóng biến mất sau 3000ms vận hành định sẵn
    setTimeout(() => alertBox.classList.add('d-none'), 3000);
}