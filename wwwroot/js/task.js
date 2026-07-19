/**
 * [V.2.19.JS Task Management Admin]
 * Kịch bản điều khiển giao diện (Client-side) phân hệ Quản lý tác vụ và phân công buồng phòng (UC19).
 * Kết nối với API phân hệ `ITaskService` trên nền tảng .NET Core.
 * Chức năng: Điều phối tác vụ vệ sinh, bảo trì và kiểm tra chất lượng phòng vật lý.
 */

// ===== CẤU HÌNH ĐỒNG BỘ DỮ LIỆU (ENUMS) =====
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

const STATUS_CONFIG = [
    '<span class="badge bg-warning text-dark"><i class="bi bi-hourglass-split me-1"></i>Pending</span>',
    '<span class="badge bg-primary"><i class="bi bi-gear-wide-connected spin me-1"></i>In Progress</span>',
    '<span class="badge bg-success"><i class="bi bi-check-circle-fill me-1"></i>Completed</span>'
];

// ===== LOAD DATA & RENDERING =====
/**
 * Tải danh sách tác vụ từ Server. Hỗ trợ tìm kiếm nhanh theo keyword (UC19).
 */
async function loadTasks(keyword = '') {
    try {
        const url = keyword
            ? `/api/tasks/search?keyword=${encodeURIComponent(keyword)}`
            : '/api/tasks';

        const response = await fetch(url);
        if (!response.ok) throw new Error('Failed to fetch tasks.');

        const tasks = await response.json();
        renderTable(tasks);
    } catch (error) {
        console.error('[loadTasks] Error:', error);
        showAlert('danger', 'Không thể kết nối đến máy chủ để tải danh sách tác vụ.');
    }
}

/**
 * Render bảng danh sách tác vụ với các hành động chỉnh sửa/xóa tương ứng trạng thái.
 */
function renderTable(tasks) {
    const tbody = document.getElementById('taskTableBody');
    if (!tbody) return;

    if (tasks.length === 0) {
        tbody.innerHTML = `<tr><td colspan="8" class="text-center text-muted py-3">Không có tác vụ nào được tìm thấy</td></tr>`;
        return;
    }

    tbody.innerHTML = tasks.map((t, index) => `
        <tr class="align-middle">
            <td>${index + 1}</td>
            <td><span class="fw-bold text-dark">Phòng ${t.roomNumber}</span></td>
            <td>${t.assignedToName}</td>
            <td>${getTaskTypeBadge(t.taskType)}</td>
            <td>${getPriorityBadge(t.priority)}</td>
            <td>${getStatusBadge(t.status)}</td>
            <td>${t.dueDate ? new Date(t.dueDate).toLocaleDateString('vi-VN') : '-'}</td>
            <td>
                <div class="btn-group" role="group">
                    <button class="btn btn-sm btn-outline-info me-1" onclick="openDetailModal(${t.id})">
                        <i class="bi bi-eye"></i> Chi tiết
                    </button>
                    ${t.status === 0 ? `
                    <button class="btn btn-sm btn-outline-warning me-1" onclick="openEditModal(${t.id})">
                        <i class="bi bi-pencil"></i> Sửa
                    </button>
                    <button class="btn btn-sm btn-outline-danger" onclick="openDeleteModal(${t.id}, '${t.roomNumber}')">
                        <i class="bi bi-trash"></i> Xóa
                    </button>` : ''}
                </div>
            </td>
        </tr>
    `).join('');
}

// ===== SEARCH ACTIONS =====
function searchTasks() {
    const keyword = document.getElementById('searchInput').value.trim();
    loadTasks(keyword);
}

function clearSearch() {
    document.getElementById('searchInput').value = '';
    loadTasks();
}

// ===== LOAD DROPDOWNS =====
async function loadRooms(selectId) {
    const response = await fetch('/api/rooms/dropdown');
    const rooms = await response.json();
    const select = document.getElementById(selectId);
    if (!select) return;

    select.innerHTML = '<option value="">-- Chọn phòng --</option>';
    rooms.forEach(r => {
        select.innerHTML += `<option value="${r.id}">Phòng ${r.roomNumber} - Tầng ${r.floor} (${r.roomTypeName})</option>`;
    });
}

async function loadStaff(selectId) {
    const response = await fetch('/api/accounts/staff');
    const staffList = await response.json();
    const select = document.getElementById(selectId);
    if (!select) return;

    select.innerHTML = '<option value="">-- Chọn nhân viên --</option>';
    staffList.forEach(s => {
        select.innerHTML += `<option value="${s.id}">${s.fullName}</option>`;
    });
}

// ===== DETAIL VIEW =====
async function openDetailModal(id) {
    const response = await fetch(`/api/tasks/${id}`);
    const task = await response.json();

    document.getElementById('detail_room').textContent = `Phòng ${task.roomNumber}`;
    document.getElementById('detail_assignedTo').textContent = task.assignedToName;
    document.getElementById('detail_createdBy').textContent = task.createdByName;
    document.getElementById('detail_taskType').innerHTML = getTaskTypeBadge(task.taskType);
    document.getElementById('detail_priority').innerHTML = getPriorityBadge(task.priority);
    document.getElementById('detail_status').innerHTML = getStatusBadge(task.status);
    document.getElementById('detail_description').textContent = task.description || '-';
    document.getElementById('detail_dueDate').textContent = task.dueDate ? new Date(task.dueDate).toLocaleString('vi-VN') : '-';
    document.getElementById('detail_completedAt').textContent = task.completedAt ? new Date(task.completedAt).toLocaleString('vi-VN') : '-';
    document.getElementById('detail_createdAt').textContent = new Date(task.createdAt).toLocaleString('vi-VN');

    new bootstrap.Modal(document.getElementById('detailModal')).show();
}

// ===== CREATE & EDIT OPERATIONS =====
async function openCreateModal() {
    await loadRooms('create_roomId');
    await loadStaff('create_staffId');
    document.getElementById('createModal').querySelector('form').reset();
    new bootstrap.Modal(document.getElementById('createModal')).show();
}

async function submitCreate() {
    const roomId = document.getElementById('create_roomId').value;
    const staffId = document.getElementById('create_staffId').value;

    if (!roomId || !staffId) {
        showAlert('warning', 'Vui lòng chọn đầy đủ Phòng và Nhân viên.');
        return;
    }

    const payload = {
        roomId: parseInt(roomId),
        assignedToId: parseInt(staffId),
        createdById: parseInt(sessionStorage.getItem('userId') || '1'),
        taskType: parseInt(document.getElementById('create_taskType').value),
        priority: parseInt(document.getElementById('create_priority').value),
        description: document.getElementById('create_description').value.trim(),
        dueDate: document.getElementById('create_dueDate').value || null
    };

    const response = await fetch('/api/tasks', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    });

    if (response.ok) {
        bootstrap.Modal.getInstance(document.getElementById('createModal')).hide();
        showAlert('success', 'Tạo tác vụ thành công.');
        loadTasks();
    } else {
        const err = await response.json();
        showAlert('danger', err.message || 'Lỗi hệ thống.');
    }
}

async function openEditModal(id) {
    await loadRooms('edit_roomId');
    await loadStaff('edit_staffId');

    const response = await fetch(`/api/tasks/${id}`);
    const task = await response.json();

    document.getElementById('edit_id').value = task.id;
    document.getElementById('edit_roomId').value = task.roomId;
    document.getElementById('edit_staffId').value = task.assignedToId;
    document.getElementById('edit_taskType').value = task.taskType;
    document.getElementById('edit_priority').value = task.priority;
    document.getElementById('edit_description').value = task.description || '';
    document.getElementById('edit_dueDate').value = task.dueDate ? task.dueDate.slice(0, 16) : '';

    new bootstrap.Modal(document.getElementById('editModal')).show();
}

async function submitEdit() {
    const id = document.getElementById('edit_id').value;
    const payload = {
        roomId: parseInt(document.getElementById('edit_roomId').value),
        assignedToId: parseInt(document.getElementById('edit_staffId').value),
        taskType: parseInt(document.getElementById('edit_taskType').value),
        priority: parseInt(document.getElementById('edit_priority').value),
        description: document.getElementById('edit_description').value.trim(),
        dueDate: document.getElementById('edit_dueDate').value || null
    };

    const response = await fetch(`/api/tasks/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    });

    if (response.ok) {
        bootstrap.Modal.getInstance(document.getElementById('editModal')).hide();
        showAlert('success', 'Cập nhật tác vụ thành công.');
        loadTasks();
    } else {
        showAlert('danger', 'Lỗi cập nhật tác vụ.');
    }
}

// ===== DELETE OPERATION =====
function openDeleteModal(id, roomNumber) {
    document.getElementById('delete_id').value = id;
    document.getElementById('delete_room').textContent = roomNumber;
    new bootstrap.Modal(document.getElementById('deleteModal')).show();
}

async function submitDelete() {
    const id = document.getElementById('delete_id').value;
    const response = await fetch(`/api/tasks/${id}`, { method: 'DELETE' });

    if (response.ok) {
        bootstrap.Modal.getInstance(document.getElementById('deleteModal')).hide();
        showAlert('success', 'Đã xóa tác vụ.');
        loadTasks();
    } else {
        showAlert('danger', 'Lỗi khi xóa tác vụ.');
    }
}

// ===== HELPERS =====
function getTaskTypeBadge(type) { return TASK_TYPE_CONFIG[type] || 'Unknown'; }
function getPriorityBadge(priority) { return PRIORITY_CONFIG[priority] || 'Unknown'; }
function getStatusBadge(status) { return STATUS_CONFIG[status] || 'Unknown'; }

function showAlert(type, message) {
    const alertBox = document.getElementById('alertBox');
    alertBox.className = `alert alert-${type} shadow-sm position-fixed top-0 start-50 translate-middle-x mt-3 z-3`;
    alertBox.textContent = message;
    alertBox.classList.remove('d-none');
    setTimeout(() => alertBox.classList.add('d-none'), 3000);
}

document.addEventListener('DOMContentLoaded', () => loadTasks());