// ===== LOAD DATA =====
async function loadServices(keyword = '') {
    const url = keyword
        ? `/api/services/search?keyword=${encodeURIComponent(keyword)}`
        : '/api/services';

    const response = await fetch(url);
    const services = await response.json();
    renderTable(services);
}

function renderTable(services) {
    const tbody = document.getElementById('serviceTableBody');

    if (services.length === 0) {
        tbody.innerHTML = `<tr><td colspan="5" class="text-center text-muted">No services found</td></tr>`;
        return;
    }

    tbody.innerHTML = services.map((s, index) => `
        <tr>
            <td>${index + 1}</td>
            <td>${s.serviceName}</td>
            <td>${formatPrice(s.price)}</td>
            <td>${getStatusBadge(s.status)}</td>
            <td>
                <button class="btn btn-sm btn-info me-1" onclick="openDetailModal(${s.id})">
                    <i class="bi bi-eye"></i> Detail
                </button>
                <button class="btn btn-sm btn-warning me-1" onclick="openEditModal(${s.id})">
                    <i class="bi bi-pencil"></i> Edit
                </button>
                <button class="btn btn-sm btn-danger" onclick="openDeleteModal(${s.id}, '${s.serviceName}')">
                    <i class="bi bi-trash"></i> Delete
                </button>
            </td>
        </tr>
    `).join('');
}

// ===== SEARCH =====
function searchServices() {
    const keyword = document.getElementById('searchInput').value.trim();
    loadServices(keyword);
}

function clearSearch() {
    document.getElementById('searchInput').value = '';
    loadServices();
}

// ===== DETAIL =====
async function openDetailModal(id) {
    const response = await fetch(`/api/services/${id}`);
    const service = await response.json();

    document.getElementById('detail_serviceName').textContent = service.serviceName;
    document.getElementById('detail_description').textContent = service.description || '-';
    document.getElementById('detail_price').textContent = formatPrice(service.price);
    document.getElementById('detail_status').innerHTML = getStatusBadge(service.status);
    document.getElementById('detail_createdAt').textContent =
        new Date(service.createdAt).toLocaleDateString('vi-VN');

    new bootstrap.Modal(document.getElementById('detailModal')).show();
}

// ===== CREATE =====
function openCreateModal() {
    document.getElementById('create_serviceName').value = '';
    document.getElementById('create_description').value = '';
    document.getElementById('create_price').value = '';
    document.getElementById('create_serviceName_error').textContent = '';
    document.getElementById('create_price_error').textContent = '';

    new bootstrap.Modal(document.getElementById('createModal')).show();
}

async function submitCreate() {
    const serviceName = document.getElementById('create_serviceName').value.trim();
    const description = document.getElementById('create_description').value.trim();
    const price = document.getElementById('create_price').value;

    // Validate
    let isValid = true;

    if (!serviceName) {
        document.getElementById('create_serviceName_error').textContent = 'Service name is required';
        isValid = false;
    } else {
        document.getElementById('create_serviceName_error').textContent = '';
    }

    if (!price || parseFloat(price) < 0) {
        document.getElementById('create_price_error').textContent = 'Price must be greater than 0';
        isValid = false;
    } else {
        document.getElementById('create_price_error').textContent = '';
    }

    if (!isValid) return;

    const response = await fetch('/api/services', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ serviceName, description, price: parseFloat(price) })
    });

    const result = await response.json();

    if (response.ok) {
        bootstrap.Modal.getInstance(document.getElementById('createModal')).hide();
        showAlert('success', result.message);
        loadServices();
    } else {
        showAlert('danger', result.message);
    }
}

// ===== EDIT =====
async function openEditModal(id) {
    const response = await fetch(`/api/services/${id}`);
    const service = await response.json();

    document.getElementById('edit_id').value = service.id;
    document.getElementById('edit_serviceName').value = service.serviceName;
    document.getElementById('edit_description').value = service.description || '';
    document.getElementById('edit_price').value = service.price;
    document.getElementById('edit_status').value = service.status;
    document.getElementById('edit_serviceName_error').textContent = '';
    document.getElementById('edit_price_error').textContent = '';

    new bootstrap.Modal(document.getElementById('editModal')).show();
}

async function submitEdit() {
    const id = document.getElementById('edit_id').value;
    const serviceName = document.getElementById('edit_serviceName').value.trim();
    const description = document.getElementById('edit_description').value.trim();
    const price = document.getElementById('edit_price').value;
    const status = parseInt(document.getElementById('edit_status').value);

    // Validate
    let isValid = true;

    if (!serviceName) {
        document.getElementById('edit_serviceName_error').textContent = 'Service name is required';
        isValid = false;
    } else {
        document.getElementById('edit_serviceName_error').textContent = '';
    }

    if (!price || parseFloat(price) < 0) {
        document.getElementById('edit_price_error').textContent = 'Price must be greater than 0';
        isValid = false;
    } else {
        document.getElementById('edit_price_error').textContent = '';
    }

    if (!isValid) return;

    const response = await fetch(`/api/services/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ serviceName, description, price: parseFloat(price), status })
    });

    const result = await response.json();

    if (response.ok) {
        bootstrap.Modal.getInstance(document.getElementById('editModal')).hide();
        showAlert('success', result.message);
        loadServices();
    } else {
        showAlert('danger', result.message);
    }
}

// ===== DELETE =====
function openDeleteModal(id, serviceName) {
    document.getElementById('delete_id').value = id;
    document.getElementById('delete_name').textContent = serviceName;
    new bootstrap.Modal(document.getElementById('deleteModal')).show();
}

async function submitDelete() {
    const id = document.getElementById('delete_id').value;

    const response = await fetch(`/api/services/${id}`, {
        method: 'DELETE'
    });

    const result = await response.json();

    if (response.ok) {
        bootstrap.Modal.getInstance(document.getElementById('deleteModal')).hide();
        showAlert('success', result.message);
        loadServices();
    } else {
        showAlert('danger', result.message);
    }
}

// ===== HELPERS =====
function getStatusBadge(status) {
    return status === 0
        ? '<span class="badge bg-success">Active</span>'
        : '<span class="badge bg-secondary">Inactive</span>';
}

function formatPrice(price) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(price);
}

function showAlert(type, message) {
    const alertBox = document.getElementById('alertBox');
    alertBox.className = `alert alert-${type}`;
    alertBox.textContent = message;
    alertBox.classList.remove('d-none');
    setTimeout(() => alertBox.classList.add('d-none'), 3000);
}

// Load khi trang khởi động
document.addEventListener('DOMContentLoaded', () => loadServices());