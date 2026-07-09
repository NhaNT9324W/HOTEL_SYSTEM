const staffId = parseInt(sessionStorage.getItem('userId') || '0');

// ===== LOAD DATA =====
async function loadTasks() {
    const response = await fetch(`/api/tasks/staff/${staffId}`);
    const tasks = await response.json();
    renderTaskTable(tasks);
}

async function loadMaintenance() {
    const response = await fetch(`/api/tasks/maintenance/staff/${staffId}`);
    const issues = await response.json();
    renderMaintenanceTable(issues);
}

// ===== RENDER TASKS =====
function renderTaskTable(tasks) {
    const tbody = document.getElementById('taskTableBody');

    if (tasks.length === 0) {
        tbody.innerHTML = `<tr><td colspan="7" class="text-center text-muted">
            No tasks assigned</td></tr>`;
        return;
    }

    tbody.innerHTML = tasks.map((t, index) => `
        <tr>
            <td>${index + 1}</td>
            <td><span class="fw-bold">Room ${t.roomNumber}</span></td>
            <td>${getTaskTypeBadge(t.taskType)}</td>
            <td>${getPriorityBadge(t.priority)}</td>
            <td>${getStatusBadge(t.status)}</td>
            <td>${t.dueDate ? formatDate(t.dueDate) : '-'}</td>
            <td>
                <button class="btn btn-sm btn-info me-1"
                        onclick="openDetailModal(${t.id})">
                    <i class="bi bi-eye"></i> Detail
                </button>
                ${t.status !== 2 ? `
                <button class="btn btn-sm btn-primary me-1"
                        onclick="openUpdateTaskStatusModal(${t.id}, '${t.roomNumber}', ${t.status})">
                    <i class="bi bi-arrow-repeat"></i> Task Status
                </button>
                <button class="btn btn-sm btn-success"
                        onclick="openUpdateRoomStatusModal(${t.roomId}, '${t.roomNumber}')">
                    <i class="bi bi-door-open"></i> Room Status
                </button>` : ''}
            </td>
        </tr>
    `).join('');
}

// ===== RENDER MAINTENANCE =====
function renderMaintenanceTable(issues) {
    const tbody = document.getElementById('maintenanceTableBody');

    if (issues.length === 0) {
        tbody.innerHTML = `<tr><td colspan="6" class="text-center text-muted">
            No maintenance issues reported</td></tr>`;
        return;
    }

    tbody.innerHTML = issues.map((m, index) => `
        <tr>
            <td>${index + 1}</td>
            <td><span class="fw-bold">Room ${m.roomNumber}</span></td>
            <td><span class="badge bg-warning text-dark">${m.issueType}</span></td>
            <td>${m.description}</td>
            <td>${getMaintenanceStatusBadge(m.status)}</td>
            <td>${formatDate(m.createdAt)}</td>
        </tr>
    `).join('');
}

// ===== DETAIL =====
async function openDetailModal(id) {
    const response = await fetch(`/api/tasks/${id}`);
    const task = await response.json();

    document.getElementById('detail_room').textContent = `Room ${task.roomNumber}`;
    document.getElementById('detail_taskType').innerHTML = getTaskTypeBadge(task.taskType);
    document.getElementById('detail_priority').innerHTML = getPriorityBadge(task.priority);
    document.getElementById('detail_status').innerHTML = getStatusBadge(task.status);
    document.getElementById('detail_description').textContent = task.description || '-';
    document.getElementById('detail_createdBy').textContent = task.createdByName;
    document.getElementById('detail_dueDate').textContent =
        task.dueDate ? formatDate(task.dueDate) : '-';
    document.getElementById('detail_completedAt').textContent =
        task.completedAt ? formatDate(task.completedAt) : '-';

    new bootstrap.Modal(document.getElementById('detailModal')).show();
}

// ===== UPDATE TASK STATUS =====
function openUpdateTaskStatusModal(taskId, roomNumber, currentStatus) {
    document.getElementById('taskStatus_id').value = taskId;
    document.getElementById('taskStatus_room').textContent = roomNumber;

    const select = document.getElementById('taskStatus_status');
    select.innerHTML = '';

    // Chỉ hiện option hợp lệ theo transition
    if (currentStatus === 0) { // Pending
        select.innerHTML = '<option value="InProgress">In Progress</option>';
    } else if (currentStatus === 1) { // InProgress
        select.innerHTML = '<option value="Completed">Completed</option>';
    }

    new bootstrap.Modal(document.getElementById('updateTaskStatusModal')).show();
}

async function submitUpdateTaskStatus() {
    const taskId = document.getElementById('taskStatus_id').value;
    const status = document.getElementById('taskStatus_status').value;

    const response = await fetch(`/api/tasks/${taskId}/status`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ status })
    });

    const result = await response.json();

    if (response.ok) {
        bootstrap.Modal.getInstance(
            document.getElementById('updateTaskStatusModal')).hide();
        showAlert('success', result.message);
        loadTasks();
    } else {
        showAlert('danger', result.message);
    }
}

// ===== UPDATE ROOM STATUS =====
function openUpdateRoomStatusModal(roomId, roomNumber) {
    document.getElementById('roomStatus_roomId').value = roomId;
    document.getElementById('roomStatus_room').textContent = roomNumber;
    new bootstrap.Modal(document.getElementById('updateRoomStatusModal')).show();
}

async function submitUpdateRoomStatus() {
    const roomId = document.getElementById('roomStatus_roomId').value;
    const housekeepingStatus = document.getElementById('roomStatus_status').value;

    const response = await fetch('/api/tasks/room-status', {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            roomId: parseInt(roomId),
            housekeepingStatus
        })
    });

    const result = await response.json();

    if (response.ok) {
        bootstrap.Modal.getInstance(
            document.getElementById('updateRoomStatusModal')).hide();
        showAlert('success', result.message);
        loadTasks();
    } else {
        showAlert('danger', result.message);
    }
}

// ===== REPORT MAINTENANCE =====
async function openReportModal() {
    // Load rooms từ tasks của staff
    const response = await fetch(`/api/tasks/staff/${staffId}`);
    const tasks = await response.json();

    const select = document.getElementById('report_roomId');
    select.innerHTML = '<option value="">-- Select Room --</option>';

    // Lấy danh sách phòng unique từ tasks
    const rooms = [...new Map(tasks.map(t => [t.roomId, t])).values()];
    rooms.forEach(t => {
        select.innerHTML += `<option value="${t.roomId}">Room ${t.roomNumber}</option>`;
    });

    document.getElementById('report_description').value = '';
    document.getElementById('report_roomId_error').textContent = '';
    document.getElementById('report_description_error').textContent = '';

    new bootstrap.Modal(document.getElementById('reportModal')).show();
}

async function submitReport() {
    const roomId = document.getElementById('report_roomId').value;
    const issueType = document.getElementById('report_issueType').value;
    const description = document.getElementById('report_description').value.trim();

    let isValid = true;

    if (!roomId) {
        document.getElementById('report_roomId_error').textContent = 'Room is required';
        isValid = false;
    } else {
        document.getElementById('report_roomId_error').textContent = '';
    }

    if (!description) {
        document.getElementById('report_description_error').textContent = 'Description is required';
        isValid = false;
    } else {
        document.getElementById('report_description_error').textContent = '';
    }

    if (!isValid) return;

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
        showAlert('success', result.message);
        loadMaintenance();
    } else {
        showAlert('danger', result.message);
    }
}

// ===== HELPERS =====
function getTaskTypeBadge(type) {
    const types = [
        '<span class="badge bg-primary">Cleaning</span>',
        '<span class="badge bg-warning text-dark">Maintenance</span>',
        '<span class="badge bg-info text-dark">Inspection</span>'
    ];
    return types[type] || 'Unknown';
}

function getPriorityBadge(priority) {
    const priorities = [
        '<span class="badge bg-secondary">Low</span>',
        '<span class="badge bg-primary">Medium</span>',
        '<span class="badge bg-danger">High</span>'
    ];
    return priorities[priority] || 'Unknown';
}

function getStatusBadge(status) {
    const statuses = [
        '<span class="badge bg-warning text-dark">Pending</span>',
        '<span class="badge bg-primary">In Progress</span>',
        '<span class="badge bg-success">Completed</span>'
    ];
    return statuses[status] || 'Unknown';
}

function getMaintenanceStatusBadge(status) {
    const badges = {
        'PENDING': '<span class="badge bg-warning text-dark">Pending</span>',
        'IN_PROGRESS': '<span class="badge bg-primary">In Progress</span>',
        'RESOLVED': '<span class="badge bg-success">Resolved</span>'
    };
    return badges[status] || status;
}

function formatDate(dateStr) {
    return new Date(dateStr).toLocaleDateString('vi-VN');
}

function showAlert(type, message) {
    const alertBox = document.getElementById('alertBox');
    alertBox.className = `alert alert-${type}`;
    alertBox.textContent = message;
    alertBox.classList.remove('d-none');
    setTimeout(() => alertBox.classList.add('d-none'), 3000);
}

// Load khi trang khởi động
document.addEventListener('DOMContentLoaded', () => {
    loadTasks();
    loadMaintenance();
});