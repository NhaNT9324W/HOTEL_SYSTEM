// ===== LOAD DATA =====
async function loadMaintenanceIssues() {
    const response = await fetch('/api/tasks/maintenance');
    const issues = await response.json();
    renderTable(issues);
}

function renderTable(issues) {
    const tbody = document.getElementById('maintenanceTableBody');

    if (issues.length === 0) {
        tbody.innerHTML = `<tr><td colspan="8" class="text-center text-muted">
            No maintenance issues found</td></tr>`;
        return;
    }

    tbody.innerHTML = issues.map((m, index) => `
        <tr>
            <td>${index + 1}</td>
            <td><span class="fw-bold">Room ${m.roomNumber}</span></td>
            <td>${m.reportedByName}</td>
            <td><span class="badge bg-warning text-dark">${m.issueType}</span></td>
            <td>${m.description}</td>
            <td>${getStatusBadge(m.status)}</td>
            <td>${new Date(m.createdAt).toLocaleDateString('vi-VN')}</td>
            <td>
                ${m.status !== 'RESOLVED' ? `
                <button class="btn btn-sm btn-warning"
                        onclick="openUpdateStatusModal(${m.id}, '${m.roomNumber}', '${m.status}')">
                    <i class="bi bi-arrow-repeat"></i> Update
                </button>` : '<span class="text-success"><i class="bi bi-check-circle"></i> Resolved</span>'}
            </td>
        </tr>
    `).join('');
}

// ===== UPDATE STATUS =====
function openUpdateStatusModal(id, roomNumber, currentStatus) {
    document.getElementById('update_id').value = id;
    document.getElementById('update_room').textContent = roomNumber;
    document.getElementById('update_status').value = currentStatus;
    new bootstrap.Modal(document.getElementById('updateStatusModal')).show();
}

async function submitUpdateStatus() {
    const id = document.getElementById('update_id').value;
    const status = document.getElementById('update_status').value;

    const response = await fetch(`/api/tasks/maintenance/${id}/status`, {
        method: 'PATCH',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ status })
    });

    const result = await response.json();

    if (response.ok) {
        bootstrap.Modal.getInstance(
            document.getElementById('updateStatusModal')).hide();
        showAlert('success', result.message);
        loadMaintenanceIssues();
    } else {
        showAlert('danger', result.message);
    }
}

// ===== HELPERS =====
function getStatusBadge(status) {
    const badges = {
        'PENDING': '<span class="badge bg-warning text-dark">Pending</span>',
        'IN_PROGRESS': '<span class="badge bg-primary">In Progress</span>',
        'RESOLVED': '<span class="badge bg-success">Resolved</span>'
    };
    return badges[status] || status;
}

function showAlert(type, message) {
    const alertBox = document.getElementById('alertBox');
    alertBox.className = `alert alert-${type}`;
    alertBox.textContent = message;
    alertBox.classList.remove('d-none');
    setTimeout(() => alertBox.classList.add('d-none'), 3000);
}

// Load khi trang khởi động
document.addEventListener('DOMContentLoaded', () => loadMaintenanceIssues());