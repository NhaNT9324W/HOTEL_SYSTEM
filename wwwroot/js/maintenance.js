/**
 * [V.2.13.JS Maintenance Operations Frontend]
 * Kịch bản điều khiển giao diện (Client-side) phân hệ Quản lý và Điều phối sự cố bảo trì vật chất (UC20.1).
 * Kết nối trực tiếp với API endpoints phân hệ sự cố kỹ thuật của `ITaskService` trên nền tảng .NET Core.
 * Quản lý vòng đời hỏng hóc thiết bị buồng phòng từ khi tiếp nhận đến khi kỹ sư sửa chữa hoàn tất.
 */

// ===== MÁY TRẠNG THÁI / CẤU HÌNH ĐỒNG BỘ CÁC TRẠNG THÁI BẢO TRÌ =====
const MAINTENANCE_STATUSES = {
    'PENDING': '<span class="badge bg-warning text-dark"><i class="bi bi-clock me-1"></i>Chờ xử lý</span>',
    'IN_PROGRESS': '<span class="badge bg-primary"><i class="bi bi-gear-fill spin me-1"></i>Đang sửa chữa</span>',
    'RESOLVED': '<span class="badge bg-success"><i class="bi bi-check-circle me-1"></i>Đã hoàn thành</span>'
};

// ===== LOAD DATA & RENDERING =====
/**
 * Tải danh sách toàn bộ các sự cố kỹ thuật vật chất buồng phòng từ Server.
 * Khớp nối dữ liệu trực tiếp với cấu trúc MaintenanceIssueDto từ Backend (UC20.1).
 */
async function loadMaintenanceIssues() {
    try {
        const response = await fetch('/api/tasks/maintenance');
        if (!response.ok) throw new Error('Failed to fetch maintenance issues database.');

        const issues = await response.json();
        renderTable(issues);
    } catch (error) {
        console.error('[loadMaintenanceIssues] Root cause analysis:', error);
        showAlert('danger', 'Không thể kết nối tới máy chủ để tải danh sách sự cố bảo trì.');
    }
}

/**
 * Kết xuất cấu trúc cây dữ liệu MaintenanceIssueDto lên lưới bảng HTML quản trị.
 * @param {Array} issues - Mảng danh sách các thực thể sự cố hỏng hóc thiết bị.
 */
function renderTable(issues) {
    const tbody = document.getElementById('maintenanceTableBody');
    if (!tbody) return;

    if (issues.length === 0) {
        tbody.innerHTML = `<tr><td colspan="8" class="text-center text-muted py-3">Không có sự cố hỏng hóc vật chất nào cần xử lý</td></tr>`;
        return;
    }

    tbody.innerHTML = issues.map((m, index) => `
        <tr>
            <td>${index + 1}</td>
            <td><span class="fw-bold text-dark">Phòng ${m.roomNumber}</span></td>
            <td>${m.reportedByName}</td>
            <td><span class="badge bg-light text-secondary border">${m.issueType}</span></td>
            <td class="text-wrap" style="max-width: 250px;">${m.description}</td>
            <td>${getStatusBadge(m.status)}</td>
            <td>${new Date(m.createdAt).toLocaleDateString('vi-VN')}</td>
            <td>
                ${m.status !== 'RESOLVED' ? `
                <button class="btn btn-sm btn-outline-warning"
                        onclick="openUpdateStatusModal(${m.id}, '${m.roomNumber}', '${m.status}')" title="Cập nhật tiến độ">
                    <i class="bi bi-arrow-repeat"></i> Cập nhật
                </button>` : `
                <span class="text-success small fw-bold">
                    <i class="bi bi-check-lg"></i> Đã đóng sự cố
                </span>`}
            </td>
        </tr>
    `).join('');
}

// ===== UPDATE STATUS WORKFLOW (STATE MACHINE) =====
/**
 * Bật cấu hình và hiển thị Modal điều phối tiến độ xử lý sự cố.
 * @param {number} id - Mã primary key của bản ghi sự cố cần xử lý.
 * @param {string} roomNumber - Số phòng xảy ra sự cố kỹ thuật.
 * @param {string} currentStatus - Trạng thái xử lý hiện tại (PENDING, IN_PROGRESS).
 */
function openUpdateStatusModal(id, roomNumber, currentStatus) {
    const idInput = document.getElementById('update_id');
    const roomSpan = document.getElementById('update_room');
    const statusSelect = document.getElementById('update_status');

    if (idInput) idInput.value = id;
    if (roomSpan) roomSpan.textContent = roomNumber;
    if (statusSelect) statusSelect.value = currentStatus;

    const modalElement = document.getElementById('updateStatusModal');
    if (modalElement) {
        new bootstrap.Modal(modalElement).show();
    }
}

/** Đẩy trạng thái tiến độ sửa chữa thiết bị buồng phòng mới cập nhật lên Server. */
async function submitUpdateStatus() {
    const id = document.getElementById('update_id').value;
    const status = document.getElementById('update_status').value;

    try {
        const response = await fetch(`/api/tasks/maintenance/${id}/status`, {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ status })
        });

        const result = await response.json();

        if (response.ok) {
            const modalElement = document.getElementById('updateStatusModal');
            const instance = bootstrap.Modal.getInstance(modalElement);
            if (instance) instance.hide();

            showAlert('success', result.message || 'Cập nhật tiến độ xử lý sự cố bảo trì thành công.');
            loadMaintenanceIssues();
        } else {
            showAlert('danger', result.message || 'Thay đổi trạng thái lỗi do vi phạm luồng chuyển dịch.');
        }
    } catch (error) {
        console.error('[submitUpdateStatus] Connection breakdown:', error);
        showAlert('danger', 'Gặp lỗi trong quá trình đồng bộ trạng thái xử lý lên máy chủ.');
    }
}

// ===== PRIVATE HELPERS =====
/** 
 * Chuyển hóa chuỗi trạng thái kỹ thuật sang thẻ giao diện Badge tương ứng. 
 * @param {string} status - Trạng thái hệ thống (PENDING, IN_PROGRESS, RESOLVED).
 */
function getStatusBadge(status) {
    return MAINTENANCE_STATUSES[status] || `<span class="badge bg-dark">${status}</span>`;
}

/**
 * Hiển thị Banner thông báo nhanh tiến trình dạng Alert nổi trên tiêu đề ứng dụng.
 * @param {string} type - Phân loại lớp định dạng định hướng Bootstrap (success, danger, warning, info).
 * @param {string} message - Nội dung chuỗi văn bản thông báo.
 */
function showAlert(type, message) {
    const alertBox = document.getElementById('alertBox');
    if (!alertBox) return;

    alertBox.className = `alert alert-${type} position-fixed top-0 start-50 translate-middle-x mt-3 z-3 shadow`;
    alertBox.textContent = message;
    alertBox.classList.remove('d-none');

    // Tự động giải phóng ẩn hộp thoại sau 3000ms vận hành ổn định
    setTimeout(() => alertBox.classList.add('d-none'), 3000);
}

// Kích hoạt tiến trình tải danh sách sự cố ngay khi cấu trúc cấu trúc cây DOM hoàn tất
document.addEventListener('DOMContentLoaded', () => loadMaintenanceIssues());