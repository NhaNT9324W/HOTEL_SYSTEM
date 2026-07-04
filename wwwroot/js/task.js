// ===== LOAD DATA =====
async function loadTasks(keyword = '') {
    const url = keyword
        ? `/api/tasks/search?keyword=${encodeURIComponent(keyword)}`
        : '/api/tasks';

    const response = await fetch(url);
    const tasks = await response.json();
    renderTable(tasks);
}

function renderTable(tasks) {
    const tbody = document.getElementById('taskTableBody');

    if (tasks.length === 0) {
        tbody.innerHTML = `<tr><td colspan="8" class="text-center text-muted">No tasks found</td></tr>`;
        return;
    }

    tbody.innerHTML = tasks.map((t, index) => `
        <tr>
            <td>${index + 1}</td>
            <td><span class="fw-bold">Room ${t.roomNumber}</span></td>
            <td>${t.assignedToName}</td>
            <td>${getTaskTypeBadge(t.taskType)}</td>
            <td>${getPriorityBadge(t.priority)}</td>
            <td>${getStatusBadge(t.status)}</td>
            <td>${t.dueDate ? new Date(t.dueDate).toLocaleDateString('vi-VN') : '-'}</td>
            <td>
                <button class="btn btn-sm btn-info me-1" onclick="openDetailModal(${t.id})">
                    <i class="bi bi-eye"></i> Detail
                </button>
                ${t.status === 0 ? `
                <button class="btn btn-sm btn-warning me-1" onclick="openEditModal(${t.id})">
                    <i class="bi bi-pencil"></i> Edit
                </button>
                <button class="btn btn-sm btn-danger" onclick="openDeleteModal(${t.id}, '${t.roomNumber}')">
                    <i class="bi bi-trash"></i> Delete
                </button>` : ''}
            </td>
        </tr>
    `).join('');
}

// ===== SEARCH =====
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
    const currentVal = select.value;

    select.innerHTML = '<option value="">-- Select Room --</option>';
    rooms.forEach(r => {
        select.innerHTML += `<option value="${r.id}">
            Room ${r.roomNumber} - Floor ${r.floor} (${r.roomTypeName})
        </option>`;
    });

    if (currentVal) select.value = currentVal;
}

async function loadStaff(selectId) {
    const response = await fetch('/api/accounts/staff');
    const staffList = await response.json();
    const select = document.getElementById(selectId);
    const currentVal = select.value;

    select.innerHTML = '<option value="">-- Select Staff --</option>';
    staffList.forEach(s => {
        select.innerHTML += `<option value="${s.id}">${s.fullName}</option>`;
    });

    if (currentVal) select.value = currentVal;
}

// ===== DETAIL =====
async function openDetailModal(id) {
    const response = await fetch(`/api/tasks/${id}`);
    const task = await response.json();

    document.getElementById('detail_room').textContent = `Room ${task.roomNumber}`;
    document.getElementById('detail_assignedTo').textContent = task.assignedToName;
    document.getElementById('detail_createdBy').textContent = task.createdByName;
    document.getElementById('detail_taskType').innerHTML = getTaskTypeBadge(task.taskType);
    document.getElementById('detail_priority').innerHTML = getPriorityBadge(task.priority);
    document.getElementById('detail_status').innerHTML = getStatusBadge(task.status);
    document.getElementById('detail_description').textContent = task.description || '-';
    document.getElementById('detail_dueDate').textContent =
        task.dueDate ? new Date(task.dueDate).toLocaleString('vi-VN') : '-';
    document.getElementById('detail_completedAt').textContent =
        task.completedAt ? new Date(task.completedAt).toLocaleString('vi-VN') : '-';
    document.getElementById('detail_createdAt').textContent =
        new Date(task.createdAt).toLocaleString('vi-VN');

    new bootstrap.Modal(document.getElementById('detailModal')).show();
}

// ===== CREATE =====
async function openCreateModal() {
    await loadRooms('create_roomId');
    await loadStaff('create_staffId');

    document.getElementById('create_roomId').value = '';
    document.getElementById('create_staffId').value = '';
    document.getElementById('create_taskType').value = '0';
    document.getElementById('create_priority').value = '1';
    document.getElementById('create_description').value = '';
    document.getElementById('create_dueDate').value = '';
    document.getElementById('create_roomId_error').textContent = '';
    document.getElementById('create_staffId_error').textContent = '';

    new bootstrap.Modal(document.getElementById('createModal')).show();
}

async function submitCreate() {
    const roomId = document.getElementById('create_roomId').value;
    const staffId = document.getElementById('create_staffId').value;
    const taskType = parseInt(document.getElementById('create_taskType').value);
    const priority = parseInt(document.getElementById('create_priority').value);
    const description = document.getElementById('create_description').value.trim();
    const dueDate = document.getElementById('create_dueDate').value;

    // Validate
    let isValid = true;
    if (!roomId) {
        document.getElementById('create_roomId_error').textContent = 'Room is required';
        isValid = false;
    } else {
        document.getElementById('create_roomId_error').textContent = '';
    }

    if (!staffId) {
        document.getElementById('create_staffId_error').textContent = 'Staff is required';
        isValid = false;
    } else {
        document.getElementById('create_staffId_error').textContent = '';
    }

    if (!isValid) return;

    const response = await fetch('/api/tasks', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            roomId: parseInt(roomId),
            assignedToId: parseInt(staffId),
            createdById: 1, // TODO: thay bằng ID user đang đăng nhập
            taskType,
            priority,
            description,
            dueDate: dueDate || null
        })
    });

    const result = await response.json();

    if (response.ok) {
        bootstrap.Modal.getInstance(document.getElementById('createModal')).hide();
        showAlert('success', result.message);
        loadTasks();
    } else {
        showAlert('danger', result.message);
    }
}

// ===== EDIT =====
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
    document.getElementById('edit_dueDate').value = task.dueDate
        ? new Date(task.dueDate).toISOString().slice(0, 16)
        : '';
    document.getElementById('edit_roomId_error').textContent = '';
    document.getElementById('edit_staffId_error').textContent = '';

    new bootstrap.Modal(document.getElementById('editModal')).show();
}

async function submitEdit() {
    const id = document.getElementById('edit_id').value;
    const roomId = document.getElementById('edit_roomId').value;
    const staffId = document.getElementById('edit_staffId').value;
    const taskType = parseInt(document.getElementById('edit_taskType').value);
    const priority = parseInt(document.getElementById('edit_priority').value);
    const description = document.getElementById('edit_description').value.trim();
    const dueDate = document.getElementById('edit_dueDate').value;

    // Validate
    let isValid = true;
    if (!roomId) {
        document.getElementById('edit_roomId_error').textContent = 'Room is required';
        isValid = false;
    } else {
        document.getElementById('edit_roomId_error').textContent = '';
    }

    if (!staffId) {
        document.getElementById('edit_staffId_error').textContent = 'Staff is required';
        isValid = false;
    } else {
        document.getElementById('edit_staffId_error').textContent = '';
    }

    if (!isValid) return;

    const response = await fetch(`/api/tasks/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            roomId: parseInt(roomId),
            assignedToId: parseInt(staffId),
            taskType,
            priority,
            description,
            dueDate: dueDate || null
        })
    });

    const result = await response.json();

    if (response.ok) {
        bootstrap.Modal.getInstance(document.getElementById('editModal')).hide();
        showAlert('success', result.message);
        loadTasks();
    } else {
        showAlert('danger', result.message);
    }
}

// ===== DELETE =====
function openDeleteModal(id, roomNumber) {
    document.getElementById('delete_id').value = id;
    document.getElementById('delete_room').textContent = roomNumber;
    new bootstrap.Modal(document.getElementById('deleteModal')).show();
}

async function submitDelete() {
    const id = document.getElementById('delete_id').value;

    const response = await fetch(`/api/tasks/${id}`, {
        method: 'DELETE'
    });

    const result = await response.json();

    if (response.ok) {
        bootstrap.Modal.getInstance(document.getElementById('deleteModal')).hide();
        showAlert('success', result.message);
        loadTasks();
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

function showAlert(type, message) {
    const alertBox = document.getElementById('alertBox');
    alertBox.className = `alert alert-${type}`;
    alertBox.textContent = message;
    alertBox.classList.remove('d-none');
    setTimeout(() => alertBox.classList.add('d-none'), 3000);
}

// Load khi trang khởi động
document.addEventListener('DOMContentLoaded', () => loadTasks());