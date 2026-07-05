// ===== LOAD DATA =====
async function loadReservations(keyword = '') {
    const url = keyword
        ? `/api/reservations/search?keyword=${encodeURIComponent(keyword)}`
        : '/api/reservations';

    const response = await fetch(url);
    const reservations = await response.json();
    renderTable(reservations);
}

function renderTable(reservations) {
    const tbody = document.getElementById('reservationTableBody');

    if (reservations.length === 0) {
        tbody.innerHTML = `<tr><td colspan="8" class="text-center text-muted">No reservations found</td></tr>`;
        return;
    }

    tbody.innerHTML = reservations.map((r, index) => `
        <tr>
            <td>${index + 1}</td>
            <td>${r.guestName}</td>
            <td>${r.guestPhone}</td>
            <td><span class="fw-bold">Room ${r.roomNumber}</span></td>
            <td>${formatDate(r.checkInDate)}</td>
            <td>${formatDate(r.checkOutDate)}</td>
            <td>${getStatusBadge(r.status)}</td>
            <td>
                <button class="btn btn-sm btn-info me-1"
                        onclick="openDetailModal(${r.id})">
                    <i class="bi bi-eye"></i> Detail
                </button>
                ${r.status === 'CONFIRMED' || r.status === 'PENDING' ? `
                <button class="btn btn-sm btn-warning me-1"
                        onclick="openEditModal(${r.id})">
                    <i class="bi bi-pencil"></i> Edit
                </button>
                <button class="btn btn-sm btn-danger"
                        onclick="openCancelModal(${r.id}, '${r.guestName}')">
                    <i class="bi bi-x-circle"></i> Cancel
                </button>` : ''}
            </td>
        </tr>
    `).join('');
}

// ===== SEARCH =====
function searchReservations() {
    const keyword = document.getElementById('searchInput').value.trim();
    loadReservations(keyword);
}

function clearSearch() {
    document.getElementById('searchInput').value = '';
    loadReservations();
}

// ===== DETAIL =====
async function openDetailModal(id) {
    const response = await fetch(`/api/reservations/${id}`);
    const r = await response.json();

    document.getElementById('detail_guestName').textContent = r.guestName;
    document.getElementById('detail_guestPhone').textContent = r.guestPhone;
    document.getElementById('detail_guestIdNumber').textContent = r.guestIdNumber;
    document.getElementById('detail_guestEmail').textContent = r.guestEmail || '-';
    document.getElementById('detail_roomNumber').textContent = `Room ${r.roomNumber}`;
    document.getElementById('detail_roomType').textContent = r.roomTypeName;
    document.getElementById('detail_floor').textContent = `Floor ${r.floor}`;
    document.getElementById('detail_checkIn').textContent = formatDate(r.checkInDate);
    document.getElementById('detail_checkOut').textContent = formatDate(r.checkOutDate);
    document.getElementById('detail_status').innerHTML = getStatusBadge(r.status);
    document.getElementById('detail_createdAt').textContent =
        new Date(r.createdAt).toLocaleString('vi-VN');

    new bootstrap.Modal(document.getElementById('detailModal')).show();
}

// ===== CREATE =====
function openCreateModal() {
    document.getElementById('create_guestName').value = '';
    document.getElementById('create_guestPhone').value = '';
    document.getElementById('create_guestIdNumber').value = '';
    document.getElementById('create_guestEmail').value = '';
    document.getElementById('create_checkIn').value = '';
    document.getElementById('create_checkOut').value = '';
    document.getElementById('create_roomId').innerHTML =
        '<option value="">-- Check availability first --</option>';
    document.getElementById('create_roomId').disabled = true;

    // Clear errors
    ['guestName', 'guestPhone', 'guestIdNumber', 'checkIn', 'checkOut', 'roomId']
        .forEach(f => {
            const err = document.getElementById(`create_${f}_error`);
            if (err) err.textContent = '';
        });

    new bootstrap.Modal(document.getElementById('createModal')).show();
}

async function checkAvailability() {
    const checkIn = document.getElementById('create_checkIn').value;
    const checkOut = document.getElementById('create_checkOut').value;

    if (!checkIn || !checkOut) {
        showAlert('warning', 'Please select check-in and check-out dates first');
        return;
    }

    if (checkIn >= checkOut) {
        showAlert('warning', 'Check-out date must be after check-in date');
        return;
    }

    const response = await fetch(
        `/api/reservations/available-rooms?checkIn=${checkIn}&checkOut=${checkOut}`);
    const rooms = await response.json();

    const select = document.getElementById('create_roomId');
    if (rooms.length === 0) {
        select.innerHTML = '<option value="">No rooms available</option>';
        select.disabled = true;
        showAlert('warning', 'No rooms available for selected dates');
    } else {
        select.innerHTML = '<option value="">-- Select Room --</option>';
        rooms.forEach(r => {
            select.innerHTML += `<option value="${r.id}">
                Room ${r.roomNumber} - Floor ${r.floor} 
                (${r.roomTypeName}) - ${formatPrice(r.price)}/night
            </option>`;
        });
        select.disabled = false;
        showAlert('success', `${rooms.length} room(s) available`);
    }
}

async function submitCreate() {
    const guestName = document.getElementById('create_guestName').value.trim();
    const guestPhone = document.getElementById('create_guestPhone').value.trim();
    const guestIdNumber = document.getElementById('create_guestIdNumber').value.trim();
    const guestEmail = document.getElementById('create_guestEmail').value.trim();
    const checkIn = document.getElementById('create_checkIn').value;
    const checkOut = document.getElementById('create_checkOut').value;
    const roomId = document.getElementById('create_roomId').value;

    // Validate
    let isValid = true;

    if (!guestName) {
        document.getElementById('create_guestName_error').textContent = 'Guest name is required';
        isValid = false;
    } else document.getElementById('create_guestName_error').textContent = '';

    if (!guestPhone) {
        document.getElementById('create_guestPhone_error').textContent = 'Phone is required';
        isValid = false;
    } else document.getElementById('create_guestPhone_error').textContent = '';

    if (!guestIdNumber) {
        document.getElementById('create_guestIdNumber_error').textContent = 'ID Number is required';
        isValid = false;
    } else document.getElementById('create_guestIdNumber_error').textContent = '';

    if (!checkIn) {
        document.getElementById('create_checkIn_error').textContent = 'Check-in date is required';
        isValid = false;
    } else document.getElementById('create_checkIn_error').textContent = '';

    if (!checkOut) {
        document.getElementById('create_checkOut_error').textContent = 'Check-out date is required';
        isValid = false;
    } else document.getElementById('create_checkOut_error').textContent = '';

    if (!roomId) {
        document.getElementById('create_roomId_error').textContent = 'Please select a room';
        isValid = false;
    } else document.getElementById('create_roomId_error').textContent = '';

    if (!isValid) return;

    const response = await fetch('/api/reservations', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            guestFullName: guestName,
            guestPhone,
            guestIdNumber,
            guestEmail: guestEmail || null,
            roomId: parseInt(roomId),
            checkInDate: checkIn,
            checkOutDate: checkOut
        })
    });

    const result = await response.json();

    if (response.ok) {
        bootstrap.Modal.getInstance(document.getElementById('createModal')).hide();
        showAlert('success', result.message);
        loadReservations();
    } else {
        showAlert('danger', result.message);
    }
}

// ===== EDIT =====
async function openEditModal(id) {
    const response = await fetch(`/api/reservations/${id}`);
    const r = await response.json();

    document.getElementById('edit_id').value = r.id;
    document.getElementById('edit_checkIn').value = r.checkInDate.split('T')[0];
    document.getElementById('edit_checkOut').value = r.checkOutDate.split('T')[0];
    document.getElementById('edit_roomId').innerHTML =
        `<option value="${r.id}">Room ${r.roomNumber} (current)</option>`;
    document.getElementById('edit_roomId').disabled = false;
    document.getElementById('edit_checkIn_error').textContent = '';
    document.getElementById('edit_checkOut_error').textContent = '';
    document.getElementById('edit_roomId_error').textContent = '';

    new bootstrap.Modal(document.getElementById('editModal')).show();
}

async function checkAvailabilityEdit() {
    const checkIn = document.getElementById('edit_checkIn').value;
    const checkOut = document.getElementById('edit_checkOut').value;

    if (!checkIn || !checkOut) {
        showAlert('warning', 'Please select check-in and check-out dates first');
        return;
    }

    const response = await fetch(
        `/api/reservations/available-rooms?checkIn=${checkIn}&checkOut=${checkOut}`);
    const rooms = await response.json();

    const select = document.getElementById('edit_roomId');
    if (rooms.length === 0) {
        select.innerHTML = '<option value="">No rooms available</option>';
        select.disabled = true;
    } else {
        select.innerHTML = '<option value="">-- Select Room --</option>';
        rooms.forEach(r => {
            select.innerHTML += `<option value="${r.id}">
                Room ${r.roomNumber} - Floor ${r.floor}
                (${r.roomTypeName}) - ${formatPrice(r.price)}/night
            </option>`;
        });
        select.disabled = false;
    }
}

async function submitEdit() {
    const id = document.getElementById('edit_id').value;
    const checkIn = document.getElementById('edit_checkIn').value;
    const checkOut = document.getElementById('edit_checkOut').value;
    const roomId = document.getElementById('edit_roomId').value;

    let isValid = true;

    if (!checkIn) {
        document.getElementById('edit_checkIn_error').textContent = 'Check-in date is required';
        isValid = false;
    } else document.getElementById('edit_checkIn_error').textContent = '';

    if (!checkOut) {
        document.getElementById('edit_checkOut_error').textContent = 'Check-out date is required';
        isValid = false;
    } else document.getElementById('edit_checkOut_error').textContent = '';

    if (!roomId) {
        document.getElementById('edit_roomId_error').textContent = 'Please select a room';
        isValid = false;
    } else document.getElementById('edit_roomId_error').textContent = '';

    if (!isValid) return;

    const response = await fetch(`/api/reservations/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
            roomId: parseInt(roomId),
            checkInDate: checkIn,
            checkOutDate: checkOut
        })
    });

    const result = await response.json();

    if (response.ok) {
        bootstrap.Modal.getInstance(document.getElementById('editModal')).hide();
        showAlert('success', result.message);
        loadReservations();
    } else {
        showAlert('danger', result.message);
    }
}

// ===== CANCEL =====
function openCancelModal(id, guestName) {
    document.getElementById('cancel_id').value = id;
    document.getElementById('cancel_guestName').textContent = guestName;
    new bootstrap.Modal(document.getElementById('cancelModal')).show();
}

async function submitCancel() {
    const id = document.getElementById('cancel_id').value;

    const response = await fetch(`/api/reservations/${id}/cancel`, {
        method: 'PATCH'
    });

    const result = await response.json();

    if (response.ok) {
        bootstrap.Modal.getInstance(document.getElementById('cancelModal')).hide();
        showAlert('success', result.message);
        loadReservations();
    } else {
        showAlert('danger', result.message);
    }
}

// ===== HELPERS =====
function getStatusBadge(status) {
    const badges = {
        'PENDING': '<span class="badge bg-warning text-dark">Pending</span>',
        'CONFIRMED': '<span class="badge bg-primary">Confirmed</span>',
        'CHECKED_IN': '<span class="badge bg-success">Checked In</span>',
        'CHECKED_OUT': '<span class="badge bg-secondary">Checked Out</span>',
        'CANCELLED': '<span class="badge bg-danger">Cancelled</span>'
    };
    return badges[status] || status;
}

function formatDate(dateStr) {
    return new Date(dateStr).toLocaleDateString('vi-VN');
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
document.addEventListener('DOMContentLoaded', () => loadReservations());