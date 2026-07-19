/**
 * [V.2.13.JS Room Staff Workspace]
 * Kịch bản điều khiển giao diện (Client-side) dành riêng cho phân hệ Nhân viên buồng phòng (Real-time Staff Workstation).
 * Kết nối trực tiếp với API endpoints của `ITaskService` trên nền tảng .NET Core.
 * Quản lý luồng xử lý tác vụ dọn dẹp (UC19.1), đồng bộ trạng thái buồng phòng vật lý, và khai báo sự cố bảo trì thực địa (UC20).
 */

// Đọc định danh nhân sự từ phiên làm việc tập trung hệ thống
const staffId = parseInt(sessionStorage.getItem('userId') || '0');

// ===== MÁY TRẠNG THÁI & CẤU HÌNH ĐỒNG BỘ BẢNG MÀU BADGES DTO =====
const TASK_TYPE_CONFIG = [
    '<span class="badge bg-primary-subtle text-primary border border-primary-subtle"><i class="bi bi-water me-1"></i>Cleaning</span>',
    '<span class="badge bg-warning-subtle text-warning-emphasis border border-warning-subtle"><i class="bi bi-tools me-1"></i>Maintenance</span>',
    '<span class="badge bg-info-subtle text-info border border-info-subtle"><i class="bi bi-shield-check me-1"></i>Inspection</span>'
];

const PRIORITY_CONFIG = [
    '<span class="badge bg-secondary-subtle text-secondary border border-secondary-subtle">Low</span>',
    '<span class="badge bg-primary-subtle text-primary border border-primary-subtle">Medium</span>',
    '<span class="badge bg-danger-subtle text-danger border border-danger-subtle"><i class="bi bi-exclamation-triangle me-1"></i>High</span>'
];

const TASK_STATUS_CONFIG = [
    '<span class="badge bg-warning text-dark"><i class="bi bi-hourglass-split me-1"></i>Pending</span>',
    '<span class="badge bg-primary"><i class="bi bi-gear-wide-connected spin me-1"></i>In Progress</span>',
    '<span class="badge bg-success"><i class="bi bi-check-circle-fill me-1"></i>Completed</span>'
];

const MAINTENANCE_STATUS_CONFIG = {
    'PENDING': '<span class="badge bg-warning text-dark"><i class="bi bi-envelope me-1"></i>Pending</span>',
    'IN_PROGRESS': '<span class="badge bg-primary"><i class="bi bi-wrench me-1"></i>In Progress</span>',
    'RESOLVED': '<span class="badge bg-success"><i class="bi bi-check2-all me-1"></i>Resolved</span>'
};

// Vùng nhớ đệm toàn cục tránh lỗi cú pháp chuyển đổi JSON trực tiếp trên chuỗi HTML trần
let activeTasksCache = [];

// ===== LOAD DATA & DATA SYNCHRONIZATION =====
/** 
 * Tải toàn bộ danh sách tác vụ dọn dẹp/kiểm tra được phân gán cho nhân sự hiện tại.
 * Kết nối API endpoint: `GetByStaffIdAsync` (UC19.1).
 */
async function loadTasks() {
    try {
        const response = await fetch(`/api/tasks/staff/${staffId}`);
        if (!response.ok) throw new Error('Không thể trích xuất dữ liệu tác vụ buồng phòng.');

        const tasks = await response.json();
        activeTasksCache = tasks; // Đồng bộ vùng nhớ đệm
        renderTaskTable(tasks);
    } catch (error) {
        console.error('[loadTasks] Error:', error);
        showAlert('danger', 'Gặp lỗi trong tiến trình cập nhật danh sách tác vụ.');
    }
}

/** 
 * Tải toàn bộ lịch sử danh sách sự cố hỏng hóc thiết bị do nhân viên này khai báo báo cáo.
 * Kết nối API endpoint: `GetMaintenanceIssuesByStaffAsync` (UC20).
 */
async function loadMaintenance() {
    try {
        const response = await fetch(`/api/tasks/maintenance/staff/${staffId}`);
        if (!response.ok) throw new Error('Không thể tải danh sách khai báo bảo trì.');

        const issues = await response.json();
        renderMaintenanceTable(issues);
    } catch (error) {
        console.error('[loadMaintenance] Error:', error);
        showAlert('danger', 'Lỗi hệ thống khi làm mới lịch sử báo cáo sự cố kỹ thuật.');
    }
}

// ===== RENDER COMPONENT TABLES =====
/**
 * Kết xuất danh sách tác vụ dọn dẹp nghiệp vụ lên bảng HTML lưới công việc.
 * @param {Array} tasks - Mảng danh sách các thực thể TaskDto.
 */
function renderTaskTable(tasks) {
    const tbody = document.getElementById('taskTableBody');
    if (!tbody) return;

    if (tasks.length === 0) {
        tbody.innerHTML = `<tr><td colspan="7" class="text-center text-muted py-4">Hôm nay bạn không có tác vụ nào được phân công gán</td></tr>`;
        return;
    }

    tbody.innerHTML = tasks.map((t, index) => `
        <tr class="align-middle">
            <td>${index + 1}</td>
            <td><span class="fw-bold text-dark">Phòng ${t.roomNumber}</span></td>
            <td>${getTaskTypeBadge(t.taskType)}</td>
            <td>${getPriorityBadge(t.priority)}</td>
            <td>${getStatusBadge(t.status)}</td>
            <td>${t.dueDate ? formatDate(t.dueDate) : '<span class="text-muted">-</span>'}</td>
            <td>
                <div class="btn-group" role="group">
                    <button class="btn btn-sm btn-outline-info me-1" onclick="openDetailModal(${t.id})" title="Xem chi tiết">
                        <i class="bi bi-eye"></i> Chi tiết
                    </button>
                    ${t.status !== 2 ? `
                    <button class="btn btn-sm btn-outline-primary me-1" onclick="openUpdateTaskStatusModal(${t.id}, '${t.roomNumber}', ${t.status})" title="Chuyển tiến độ việc">
                        <i class="bi bi-arrow-repeat"></i> Tiến độ
                    </button>
                    <button class="btn btn-sm btn-outline-success" onclick="openUpdateRoomStatusModal(${t.roomId}, '${t.roomNumber}')" title="Nghiệm thu chất lượng phòng">
                        <i class="bi bi-door-open"></i> Trạng thái phòng
                    </button>` : ''}
                </div>
            </td>
        </tr>
    `).join('');
}

/**
 * Kết xuất lịch sử các báo cáo sự cố kỹ thuật do nhân viên thực địa tự khai báo.
 * @param {Array} issues - Mảng danh sách các thực thể MaintenanceIssueDto.
 */
function renderMaintenanceTable(issues) {
    const tbody = document.getElementById('maintenanceTableBody');
    if (!tbody) return;

    if (issues.length === 0) {
        tbody.innerHTML = `<tr><td colspan="6" class="text-center text-muted py-3">Chưa có sự cố vật chất nào do bạn khai báo ghi nhận</td></tr>`;
        return;
    }

    tbody.innerHTML = issues.map((m, index) => `
        <tr class="align-middle">
            <td>${index + 1}</td>
            <td class="fw-bold text-dark">Phòng ${m.roomNumber}</td>
            <td><span class="badge bg-light text-warning-emphasis border border-warning-subtle">${m.issueType}</span></td>
            <td class="text-wrap" style="max-width: 200px;">${m.description}</td>
            <td>${getMaintenanceStatusBadge(m.status)}</td>
            <td>${formatDate(m.createdAt)}</td>
        </tr>
    `).join('');
}

// ===== VIEW WORK TASK DETAIL =====
/** Tra cứu chi tiết cấu trúc tác vụ theo ID và hiển thị lên giao diện View Modal. */
async function openDetailModal(id) {
    try {
        const response = await fetch(`/api/tasks/${id}`);
        if (!response.ok) throw new Error('Không thể tải hồ sơ chi tiết tác vụ.');

        const task = await response.json();

        document.getElementById('detail_room').textContent = `Phòng ${task.roomNumber}`;
        document.getElementById('detail_taskType').innerHTML = getTaskTypeBadge(task.taskType);
        document.getElementById('detail_priority').innerHTML = getPriorityBadge(task.priority);
        document.getElementById('detail_status').innerHTML = getStatusBadge(task.status);
        document.getElementById('detail_description').textContent = task.description || 'Không có ghi chú mô tả thêm.';
        document.getElementById('detail_createdBy').textContent = task.createdByName || 'Hệ thống tự động';
        document.getElementById('detail_dueDate').textContent = task.dueDate ? new Date(task.dueDate).toLocaleString('vi-VN') : '-';
        document.getElementById('detail_completedAt').textContent = task.completedAt ? new Date(task.completedAt).toLocaleString('vi-VN') : '-';

        new bootstrap.Modal(document.getElementById('detailModal')).show();
    } catch (error) {
        console.error('[openDetailModal] Error:', error);
        showAlert('danger', 'Không thể hiển thị cấu trúc chi tiết tác vụ.');
    }
}

// ===== WORKFLOW: UPDATE TASK PROGRESS STATE MACHINE =====
/**
 * Cấu hình hiển thị Modal điều khiển tiến độ công việc dựa trên máy trạng thái logic.
 * Ràng buộc nghiệp vụ nghiêm ngặt: Pending (0) -> InProgress (1) -> Completed (2).
 */
function openUpdateTaskStatusModal(taskId, roomNumber, currentStatus) {
    document.getElementById('taskStatus_id').value = taskId;
    document.getElementById('taskStatus_room').textContent = roomNumber;

    const select = document.getElementById('taskStatus_status');
    if (!select) return;
    select.innerHTML = '';

    // Lọc cấu hình dịch chuyển trạng thái hợp lệ để ngăn lỗi chuyển đổi dữ liệu sai
    if (currentStatus === 0) {
        select.innerHTML = '<option value="InProgress">Bắt đầu thực hiện (In Progress)</option>';
    } else if (currentStatus === 1) {
        select.innerHTML = '<option value="Completed">Đã hoàn thành xong (Completed)</option>';
    } else {
        showAlert('warning', 'Tác vụ đã đóng hoàn toàn, không thể dịch chuyển trạng thái máy.');
        return;
    }

    new bootstrap.Modal(document.getElementById('updateTaskStatusModal')).show();
}

/** Gửi yêu cầu PATCH cập nhật trạng thái tiến độ công việc buồng phòng lên endpoint `UpdateTaskStatusAsync`. */
async function submitUpdateTaskStatus() {
    const taskId = document.getElementById('taskStatus_id').value;
    const status = document.getElementById('taskStatus_status').value;

    try {
        const response = await fetch(`/api/tasks/${taskId}/status`, {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ status })
        });

        const result = await response.ok ? await response.json() : null;

        if (response.ok) {
            bootstrap.Modal.getInstance(document.getElementById('updateTaskStatusModal')).hide();
            showAlert('success', (result && result.message) || 'Cập nhật tiến độ trạng thái tác vụ thành công.');
            await loadTasks();
        } else {
            const errResult = await response.json();
            showAlert('danger', errResult.message || 'Thay đổi trạng thái thất bại do vi phạm ràng buộc luồng.');
        }
    } catch (error) {
        console.error('[submitUpdateTaskStatus] Connection fail:', error);
        showAlert('danger', 'Lỗi hệ thống trong quá trình đồng bộ tiến độ công việc.');
    }
}

// ===== WORKFLOW: ĐỒNG BỘ TRẠNG THÁI VẬT LÝ BUỒNG PHÒNG =====
/** Mở cấu hình Modal cho phép nhân viên buồng phòng nghiệm thu cập nhật trạng thái dọn dẹp phòng thực tế. */
function openUpdateRoomStatusModal(roomId, roomNumber) {
    document.getElementById('roomStatus_roomId').value = roomId;
    document.getElementById('roomStatus_room').textContent = roomNumber;

    // Mặc định gán trạng thái dọn dẹp đề xuất tiếp theo là READY (Sẵn sàng đón khách)
    const select = document.getElementById('roomStatus_status');
    if (select) select.value = '2'; // Chỉ số Enum của READY trong thiết lập DB

    new bootstrap.Modal(document.getElementById('updateRoomStatusModal')).show();
}

/** Gửi yêu cầu cập nhật trạng thái vệ sinh buồng phòng vật lý lên API endpoint `UpdateRoomStatusAsync`. */
async function submitUpdateRoomStatus() {
    const roomId = document.getElementById('roomStatus_roomId').value;
    const housekeepingStatus = document.getElementById('roomStatus_status').value;

    try {
        const response = await fetch('/api/tasks/room-status', {
            method: 'PATCH',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                roomId: parseInt(roomId),
                housekeepingStatus: parseInt(housekeepingStatus)
            })
        });

        const result = await response.json();

        if (response.ok) {
            bootstrap.Modal.getInstance(document.getElementById('updateRoomStatusModal')).hide();
            showAlert('success', result.message || 'Đồng bộ trạng thái buồng phòng vật lý thành công.');
            await loadTasks();
        } else {
            showAlert('danger', result.message || 'Không thể cập nhật trạng thái phòng.');
        }
    } catch (error) {
        console.error('[submitUpdateRoomStatus] Error:', error);
        showAlert('danger', 'Gặp sự cố khi kết nối đồng bộ dữ liệu Tiền sảnh.');
    }
}

// ===== WORKFLOW: KHAI BÁO SỰ CỐ KỸ THUẬT THỰC ĐỊA (UC20) =====
/** Kích hoạt cấu trúc biểu mẫu khai báo sự cố hỏng hóc vật chất phát hiện trong ca làm việc. */
async function openReportModal() {
    const select = document.getElementById('report_roomId');
    if (!select) return;

    select.innerHTML = '<option value="">-- Chọn số phòng phát sinh sự cố --</option>';

    // Trích xuất danh mục danh sách phòng độc bản (Unique) từ vùng nhớ đệm tác vụ được gán
    if (activeTasksCache.length === 0) {
        showAlert('warning', 'Hệ thống yêu cầu bạn phải có tác vụ được phân gán tại phòng để thực hiện báo cáo.');
        return;
    }

    const uniqueRooms = [...new Map(activeTasksCache.map(t => [t.roomId, t])).values()];
    uniqueRooms.forEach(t => {
        select.innerHTML += `<option value="${t.roomId}">Phòng ${t.roomNumber}</option>`;
    });

    document.getElementById('report_description').value = '';
    document.getElementById('report_roomId_error').textContent = '';
    document.getElementById('report_description_error').textContent = '';

    new bootstrap.Modal(document.getElementById('reportModal')).show();
}

/** Gửi thông báo khai báo hỏng hóc cơ sở vật chất lên API `ReportMaintenanceAsync` (UC20). */
async function submitReport() {
    const roomId = document.getElementById('report_roomId').value;
    const issueType = document.getElementById('report_issueType').value;
    const description = document.getElementById('report_description').value.trim();

    let isValid = true;

    // Bộ kiểm tra ràng buộc nghiệp vụ phía Client
    if (!roomId) {
        document.getElementById('report_roomId_error').textContent = 'Vui lòng xác định vị trí phòng xảy ra sự cố';
        isValid = false;
    } else {
        document.getElementById('report_roomId_error').textContent = '';
    }

    if (!description) {
        document.getElementById('report_description_error').textContent = 'Nội dung chi tiết mô tả hỏng hóc bắt buộc phải nhập';
        isValid = false;
    } else {
        document.getElementById('report_description_error').textContent = '';
    }

    if (!isValid) return;

    try {
        const response = await fetch('/api/tasks/maintenance', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                roomId: parseInt(roomId),
                issueType,
                description,
                reportedById: staffId
            })
        });

        const result = await response.json();

        if (response.ok) {
            bootstrap.Modal.getInstance(document.getElementById('reportModal')).hide();
            showAlert('success', result.message || 'Ghi nhận khai báo sự cố kỹ thuật thành công, thông tin đã gửi tới Kỹ sư.');
            await loadMaintenance(); // Đồng bộ lại bảng lịch sử báo cáo
        } else {
            showAlert('danger', result.message || 'Lỗi quy trình khai báo sự cố trên Server core.');
        }
    } catch (error) {
        console.error('[submitReport] Exception breakdown:', error);
        showAlert('danger', 'Hệ thống đứt gãy kết nối trong tiến trình chuyển giao dữ liệu bảo trì.');
    }
}

// ===== PRIVATE HELPERS =====
/** Giải mã chỉ số phân loại tác vụ sang cấu trúc Badge tương ứng. */
function getTaskTypeBadge(type) {
    return TASK_TYPE_CONFIG[type] || '<span class="badge bg-dark">Unknown</span>';
}

/** Giải mã mức độ ưu tiên công việc sang cấu trúc màu sắc Badge trực quan. */
function getPriorityBadge(priority) {
    return PRIORITY_CONFIG[priority] || '<span class="badge bg-dark">Unknown</span>';
}

/** Giải mã trạng thái xử lý tác vụ buồng phòng. */
function getStatusBadge(status) {
    return TASK_STATUS_CONFIG[status] || '<span class="badge bg-dark">Unknown</span>';
}

/** Giải mã chỉ số chuỗi máy trạng thái bảo trì. */
function getMaintenanceStatusBadge(status) {
    return MAINTENANCE_STATUS_CONFIG[status] || `<span class="badge bg-dark">${status}</span>`;
}

/** Chuyển hóa chuỗi thời gian thô của máy chủ về cấu trúc hiển thị ngày VN (DD/MM/YYYY). */
function formatDate(dateStr) {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleDateString('vi-VN');
}

/** Kích hoạt hiển thị Banner Alert thông báo nhanh tiến trình vận hành. */
function showAlert(type, message) {
    const alertBox = document.getElementById('alertBox');
    if (!alertBox) return;

    alertBox.className = `alert alert-${type} position-fixed top-0 start-50 translate-middle-x mt-3 z-3 shadow-lg`;
    alertBox.textContent = message;
    alertBox.classList.remove('d-none');

    setTimeout(() => alertBox.classList.add('d-none'), 3000);
}

// Tự động kích hoạt cơ chế nạp dữ liệu ca làm việc ngay khi cấu trúc cây DOM Client hoàn tất lắp dựng
document.addEventListener('DOMContentLoaded', () => {
    if (staffId > 0) {
        loadTasks();
        loadMaintenance();
    } else {
        console.error('[Workspace Setup] Khóa định danh nhân viên StaffId không hợp lệ. Vui lòng đăng nhập lại.');
        showAlert('danger', 'Phiên làm việc hết hạn hoặc không tìm thấy thông tin tài khoản nhân sự.');
    }
});