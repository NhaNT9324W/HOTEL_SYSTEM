// ===== LOAD DATA =====
async function loadAccounts(keyword = '') {
    const url = keyword
        ? `/api/accounts/search?keyword=${encodeURIComponent(keyword)}`
        : '/api/accounts';

    const response = await fetch(url);
    const accounts = await response.json();
    renderTable(accounts);
}

function renderTable(accounts) {
    const tbody = document.getElementById('accountTableBody');

    if (accounts.length === 0) {
        tbody.innerHTML = `<tr><td colspan="5" class="text-center text-muted">No accounts found</td></tr>`;
        return;
    }

    tbody.innerHTML = accounts.map((a, index) => `
        <tr>
            <td>${index + 1}</td>
            <td>${a.fullName}</td>
            <td><span class="badge bg-info">${getRoleName(a.role)}</span></td>
            <td>${getStatusBadge(a.status)}</td>
            <td>
                <button class="btn btn-sm btn-info me-1" onclick="openDetailModal(${a.id})">
                    <i class="bi bi-eye"></i> Detail
                </button>
                <button class="btn btn-sm btn-warning me-1" onclick="openEditModal(${a.id})">
                    <i class="bi bi-pencil"></i> Edit
                </button>
                <button class="btn btn-sm btn-danger" onclick="openDeleteModal(${a.id}, '${a.fullName}')">
                    <i class="bi bi-trash"></i> Delete
                </button>
            </td>
        </tr>
    `).join('');
}

// ===== SEARCH =====
function searchAccounts() {
    const keyword = document.getElementById('searchInput').value.trim();
    loadAccounts(keyword);
}

function clearSearch() {
    document.getElementById('searchInput').value = '';
    loadAccounts();
}

// ===== CREATE =====
function openCreateModal() {
    clearCreateForm();
    new bootstrap.Modal(document.getElementById('createModal')).show();
}

function clearCreateForm() {
    ['fullName', 'username', 'password', 'email', 'phone'].forEach(f => {
        document.getElementById(`create_${f}`).value = '';
        const err = document.getElementById(`create_${f}_error`);
        if (err) err.textContent = '';
    });
    document.getElementById('create_role').value = '2';
}

async function submitCreate() {
    // Validate
    let isValid = true;

    const fullName = document.getElementById('create_fullName').value.trim();
    const username = document.getElementById('create_username').value.trim();
    const password = document.getElementById('create_password').value.trim();
    const email = document.getElementById('create_email').value.trim();
    const phone = document.getElementById('create_phone').value.trim();
    const role = parseInt(document.getElementById('create_role').value);

    if (!fullName) { document.getElementById('create_fullName_error').textContent = 'Full name is required'; isValid = false; }
    else document.getElementById('create_fullName_error').textContent = '';

    if (!username) { document.getElementById('create_username_error').textContent = 'Username is required'; isValid = false; }
    else document.getElementById('create_username_error').textContent = '';

    if (!password || password.length < 6) { document.getElementById('create_password_error').textContent = 'Password must be at least 6 characters'; isValid = false; }
    else document.getElementById('create_password_error').textContent = '';

    if (!email) { document.getElementById('create_email_error').textContent = 'Email is required'; isValid = false; }
    else document.getElementById('create_email_error').textContent = '';

    if (!isValid) return;

    const response = await fetch('/api/accounts', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ fullName, username, password, email, phone, role })
    });

    const result = await response.json();

    if (response.ok) {
        bootstrap.Modal.getInstance(document.getElementById('createModal')).hide();
        showAlert('success', result.message);
        loadAccounts();
    } else {
        showAlert('danger', result.message);
    }
}

// ===== EDIT =====
async function openEditModal(id) {
    const response = await fetch(`/api/accounts/${id}`);
    const account = await response.json();

    document.getElementById('edit_id').value = account.id;
    document.getElementById('edit_fullName').value = account.fullName;
    document.getElementById('edit_email').value = account.email;
    document.getElementById('edit_phone').value = account.phone || '';
    document.getElementById('edit_role').value = account.role;
    document.getElementById('edit_status').value = account.status;

    new bootstrap.Modal(document.getElementById('editModal')).show();
}

async function submitEdit() {
    const id = document.getElementById('edit_id').value;
    const fullName = document.getElementById('edit_fullName').value.trim();
    const email = document.getElementById('edit_email').value.trim();
    const phone = document.getElementById('edit_phone').value.trim();
    const role = parseInt(document.getElementById('edit_role').value);
    const status = parseInt(document.getElementById('edit_status').value);

    // Validate
    let isValid = true;
    if (!fullName) { document.getElementById('edit_fullName_error').textContent = 'Full name is required'; isValid = false; }
    else document.getElementById('edit_fullName_error').textContent = '';

    if (!email) { document.getElementById('edit_email_error').textContent = 'Email is required'; isValid = false; }
    else document.getElementById('edit_email_error').textContent = '';

    if (!isValid) return;

    const response = await fetch(`/api/accounts/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ id, fullName, email, phone, role, status })
    });

    const result = await response.json();

    if (response.ok) {
        bootstrap.Modal.getInstance(document.getElementById('editModal')).hide();
        showAlert('success', result.message);
        loadAccounts();
    } else {
        showAlert('danger', result.message);
    }
}

// ===== DELETE =====
function openDeleteModal(id, fullName) {
    document.getElementById('delete_id').value = id;
    document.getElementById('delete_name').textContent = fullName;
    new bootstrap.Modal(document.getElementById('deleteModal')).show();
}

async function submitDelete() {
    const id = document.getElementById('delete_id').value;

    const response = await fetch(`/api/accounts/${id}`, {
        method: 'DELETE'
    });

    const result = await response.json();

    if (response.ok) {
        bootstrap.Modal.getInstance(document.getElementById('deleteModal')).hide();
        showAlert('success', result.message);
        loadAccounts();
    } else {
        showAlert('danger', result.message);
    }
}

// ===== DETAIL =====
async function openDetailModal(id) {
    const response = await fetch(`/api/accounts/${id}`);
    const account = await response.json();

    document.getElementById('detail_fullName').textContent = account.fullName;
    document.getElementById('detail_username').textContent = account.username;
    document.getElementById('detail_email').textContent = account.email;
    document.getElementById('detail_phone').textContent = account.phone || '-';
    document.getElementById('detail_role').innerHTML = `<span class="badge bg-info">${getRoleName(account.role)}</span>`;
    document.getElementById('detail_status').innerHTML = getStatusBadge(account.status);
    document.getElementById('detail_createdAt').textContent = new Date(account.createdAt).toLocaleDateString('vi-VN');

    // Set ID cho reset password
    document.getElementById('reset_userId').value = account.id;
    document.getElementById('reset_userName').textContent = account.fullName;

    new bootstrap.Modal(document.getElementById('detailModal')).show();
}

// ===== RESET PASSWORD BY ADMIN =====
function openResetPasswordModal() {
    bootstrap.Modal.getInstance(document.getElementById('detailModal')).hide();
    document.getElementById('reset_newPassword').value = '';
    document.getElementById('reset_newPassword_error').textContent = '';
    new bootstrap.Modal(document.getElementById('resetPasswordModal')).show();
}

async function submitResetPassword() {
    const userId = document.getElementById('reset_userId').value;
    const newPassword = document.getElementById('reset_newPassword').value.trim();

    if (!newPassword || newPassword.length < 6) {
        document.getElementById('reset_newPassword_error').textContent = 'Password must be at least 6 characters';
        return;
    }
    document.getElementById('reset_newPassword_error').textContent = '';

    const response = await fetch(`/api/auth/reset-password-admin/${userId}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ newPassword })
    });

    const result = await response.json();

    if (response.ok) {
        bootstrap.Modal.getInstance(document.getElementById('resetPasswordModal')).hide();
        showAlert('success', result.message);
    } else {
        showAlert('danger', result.message);
    }
}


// ===== HELPERS =====
function getRoleName(role) {
    const roles = ['Admin', 'HotelManager', 'Receptionist', 'RoomStaff'];
    return roles[role] || 'Unknown';
}

function getStatusBadge(status) {
    const badges = [
        '<span class="badge bg-success">Active</span>',
        '<span class="badge bg-secondary">Inactive</span>',
        '<span class="badge bg-danger">Locked</span>'
    ];
    return badges[status] || 'Unknown';
}

function showAlert(type, message) {
    const alertBox = document.getElementById('alertBox');
    alertBox.className = `alert alert-${type}`;
    alertBox.textContent = message;
    alertBox.classList.remove('d-none');
    setTimeout(() => alertBox.classList.add('d-none'), 3000);
}

// Load khi trang khởi động
document.addEventListener('DOMContentLoaded', () => loadAccounts());