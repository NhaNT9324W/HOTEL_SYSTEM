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
            ${r.status === 'CONFIRMED' ? `
            <button class="btn btn-sm btn-success me-1"
                    onclick="checkIn(${r.id}, '${r.guestName}')">
                <i class="bi bi-box-arrow-in-right"></i> Check-in
            </button>
            <button class="btn btn-sm btn-warning me-1"
                    onclick="openEditModal(${r.id})">
                <i class="bi bi-pencil"></i> Edit
            </button>
            <button class="btn btn-sm btn-danger"
                    onclick="openCancelModal(${r.id}, '${r.guestName}')">
                <i class="bi bi-x-circle"></i> Cancel
            </button>` : ''}
            ${r.status === 'CHECKED_IN' ? `
            <button class="btn btn-sm btn-primary me-1"
                    onclick="openAddServiceModal(${r.id}, '${r.guestName}')">
                <i class="bi bi-plus-circle"></i> Add Service
            </button>
            <button class="btn btn-sm btn-danger"
                    onclick="openCheckOutModal(${r.id}, '${r.guestName}')">
                <i class="bi bi-box-arrow-right"></i> Check-out
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

// ===== LOAD ROOM TYPES =====
async function loadRoomTypes(selectId) {
    const response = await fetch('/api/roomtypes');
    const roomTypes = await response.json();
    const select = document.getElementById(selectId);

    select.innerHTML = '<option value="">-- All Room Types --</option>';
    roomTypes
        .filter(rt => rt.isActive)
        .forEach(rt => {
            select.innerHTML += `<option value="${rt.id}">
                ${rt.name} - ${formatPrice(rt.basePrice)}/night
                (Max: ${rt.maxOccupancy} guests)
            </option>`;
        });
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
async function openCreateModal() {
    await loadRoomTypes('create_roomTypeId');

    document.getElementById('create_guestName').value = '';
    document.getElementById('create_guestPhone').value = '';
    document.getElementById('create_guestIdNumber').value = '';
    document.getElementById('create_guestEmail').value = '';
    document.getElementById('create_checkIn').value = '';
    document.getElementById('create_checkOut').value = '';
    document.getElementById('create_roomTypeId').value = '';
    document.getElementById('create_roomId').innerHTML =
        '<option value="">-- Check availability first --</option>';
    document.getElementById('create_roomId').disabled = true;

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
    const roomTypeId = document.getElementById('create_roomTypeId').value;

    if (!checkIn || !checkOut) {
        showAlert('warning', 'Please select check-in and check-out dates first');
        return;
    }

    if (checkIn >= checkOut) {
        showAlert('warning', 'Check-out date must be after check-in date');
        return;
    }

    let url = `/api/reservations/available-rooms?checkIn=${checkIn}&checkOut=${checkOut}`;
    if (roomTypeId) url += `&roomTypeId=${roomTypeId}`;

    const response = await fetch(url);
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
        `<option value="${r.roomId}">Room ${r.roomNumber} (current)</option>`;
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

    if (checkIn >= checkOut) {
        showAlert('warning', 'Check-out date must be after check-in date');
        return;
    }

    const response = await fetch(
        `/api/reservations/available-rooms?checkIn=${checkIn}&checkOut=${checkOut}`);
    const rooms = await response.json();

    const select = document.getElementById('edit_roomId');
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

// ===== CHECK-IN =====
async function checkIn(id, guestName) {
    if (!confirm(`Check-in for ${guestName}?`)) return;

    const response = await fetch(`/api/reservations/${id}/checkin`, {
        method: 'PATCH'
    });

    const result = await response.json();

    if (response.ok) {
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

// ===== ADD SERVICE MODAL =====
async function openAddServiceModal(reservationId, guestName) {
    document.getElementById('addService_reservationId').value = reservationId;
    document.getElementById('addService_guestName').textContent = guestName;

    // Load services dropdown
    const response = await fetch('/api/services');
    const services = await response.json();
    const select = document.getElementById('addService_serviceId');
    select.innerHTML = '<option value="">-- Select Service --</option>';
    services
        .filter(s => s.status === 0) // Active only
        .forEach(s => {
            select.innerHTML += `<option value="${s.id}">
                ${s.serviceName} - ${formatPrice(s.price)}
            </option>`;
        });

    document.getElementById('addService_quantity').value = 1;

    // Load existing service usages
    await loadServiceUsages(reservationId);

    new bootstrap.Modal(document.getElementById('addServiceModal')).show();
}

async function loadServiceUsages(reservationId) {
    const response = await fetch(`/api/checkout/${reservationId}/services`);
    const usages = await response.json();

    const tbody = document.getElementById('serviceUsageTableBody');
    if (usages.length === 0) {
        tbody.innerHTML = `<tr><td colspan="6" class="text-center text-muted">
            No services added</td></tr>`;
        document.getElementById('totalServiceCharge').textContent = '0 VND';
        return;
    }

    tbody.innerHTML = usages.map(u => `
        <tr>
            <td>${u.serviceName}</td>
            <td>${u.quantity}</td>
            <td>${formatPrice(u.unitPrice)}</td>
            <td>${formatPrice(u.totalPrice)}</td>
            <td>${new Date(u.usedAt).toLocaleDateString('vi-VN')}</td>
            <td>
                <button class="btn btn-sm btn-danger"
                        onclick="removeService(${u.id}, ${reservationId})">
                    <i class="bi bi-trash"></i>
                </button>
            </td>
        </tr>
    `).join('');

    const total = usages.reduce((sum, u) => sum + u.totalPrice, 0);
    document.getElementById('totalServiceCharge').textContent = formatPrice(total);
}

async function submitAddService() {
    const reservationId = parseInt(
        document.getElementById('addService_reservationId').value);
    const serviceId = document.getElementById('addService_serviceId').value;
    const quantity = parseInt(document.getElementById('addService_quantity').value);

    if (!serviceId) {
        showAlert('warning', 'Please select a service');
        return;
    }

    if (quantity < 1) {
        showAlert('warning', 'Quantity must be at least 1');
        return;
    }

    const response = await fetch('/api/checkout/services', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ reservationId, serviceId: parseInt(serviceId), quantity })
    });

    const result = await response.json();

    if (response.ok) {
        showAlert('success', result.message);
        await loadServiceUsages(reservationId);
        document.getElementById('addService_serviceId').value = '';
        document.getElementById('addService_quantity').value = 1;
    } else {
        showAlert('danger', result.message);
    }
}

async function removeService(serviceUsageId, reservationId) {
    if (!confirm('Remove this service?')) return;

    const response = await fetch(`/api/checkout/services/${serviceUsageId}`, {
        method: 'DELETE'
    });

    const result = await response.json();

    if (response.ok) {
        showAlert('success', result.message);
        await loadServiceUsages(reservationId);
    } else {
        showAlert('danger', result.message);
    }
}

// ===== CHECK-OUT MODAL =====
async function openCheckOutModal(reservationId, guestName) {
    document.getElementById('checkout_reservationId').value = reservationId;

    // Load invoice preview
    const response = await fetch(`/api/checkout/${reservationId}/preview`);
    const invoice = await response.json();

    document.getElementById('co_guestName').textContent = invoice.guestName;
    document.getElementById('co_roomNumber').textContent = `Room ${invoice.roomNumber}`;
    document.getElementById('co_roomType').textContent = invoice.roomTypeName;
    document.getElementById('co_checkIn').textContent = formatDate(invoice.checkInDate);
    document.getElementById('co_checkOut').textContent = formatDate(invoice.checkOutDate);
    document.getElementById('co_nights').textContent = `${invoice.nights} nights`;
    document.getElementById('co_nights2').textContent = invoice.nights;
    document.getElementById('co_roomCharge').textContent = formatPrice(invoice.roomCharge);
    document.getElementById('co_serviceCharge').textContent =
        formatPrice(invoice.serviceCharge);
    document.getElementById('co_totalAmount').textContent = formatPrice(invoice.totalAmount);

    // Service details
    const tbody = document.getElementById('co_serviceDetails');
    if (invoice.services.length === 0) {
        tbody.innerHTML = `<tr><td colspan="2" class="text-center text-muted">
            No services used</td></tr>`;
    } else {
        tbody.innerHTML = invoice.services.map(s => `
            <tr>
                <td>${s.serviceName} x${s.quantity}</td>
                <td class="text-end">${formatPrice(s.totalPrice)}</td>
            </tr>
        `).join('');
    }

    new bootstrap.Modal(document.getElementById('checkOutModal')).show();
}

async function submitCheckOut() {
    const reservationId = document.getElementById('checkout_reservationId').value;

    const response = await fetch(`/api/checkout/${reservationId}/confirm`, {
        method: 'POST'
    });

    const result = await response.json();

    if (response.ok) {
        bootstrap.Modal.getInstance(document.getElementById('checkOutModal')).hide();
        showAlert('success', 'Check-out successful!');
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