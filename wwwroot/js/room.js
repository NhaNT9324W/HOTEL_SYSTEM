const bookingStatusText = ['AVAILABLE', 'RESERVED', 'OCCUPIED', 'MAINTENANCE'];
const bookingStatusColor = ['success', 'warning', 'danger', 'secondary'];
const housekeepingStatusText = ['DIRTY', 'CLEAN', 'READY'];
const housekeepingStatusColor = ['danger', 'info', 'success'];

async function loadRoomTypesDropdown(selectedId = null) {
    const res = await fetch('/api/roomtypes');
    const data = await res.json();
    const select = document.getElementById('roomTypeId');
    select.innerHTML = data
        .filter(rt => rt.isActive)
        .map(rt => `<option value="${rt.id}" ${rt.id == selectedId ? 'selected' : ''}>${rt.name} (${formatCurrency(rt.basePrice)})</option>`)
        .join('');
}

async function loadRooms() {
    const res = await fetch('/api/rooms');
    const data = await res.json();
    const tbody = document.querySelector('#roomTable tbody');

    if (data.length === 0) {
        tbody.innerHTML = `<tr><td colspan="4" class="text-center text-muted py-4">Chưa có phòng nào</td></tr>`;
        return;
    }

    // Ghi chú Root Cause: field "capacity" chưa có trong API response hiện tại
    // (Room.cs / RoomTypeDto chưa expose Capacity) -> tạm hiển thị "-" chờ xác nhận schema
    tbody.innerHTML = data.map(r => `
        <tr>
            <td class="fw-semibold">${r.roomNumber}</td>
            <td>${r.maxOccupancy ?? '-'}${r.maxOccupancy ? ' người' : ''}</td>
            <td><span class="badge bg-${bookingStatusColor[r.bookingStatus]}">${bookingStatusText[r.bookingStatus]}</span></td>
            <td class="text-end">
                <button class="btn btn-sm btn-outline-info" onclick='viewRoomDetail(${JSON.stringify(r)})'>
                    <i class="bi bi-eye"></i>
                </button>
                <button class="btn btn-sm btn-outline-warning" onclick='editRoom(${JSON.stringify(r)})'>
                    <i class="bi bi-pencil"></i>
                </button>
                <button class="btn btn-sm btn-outline-danger" onclick="deleteRoom(${r.id})">
                    <i class="bi bi-trash"></i>
                </button>
            </td>
        </tr>`).join('');
}

function viewRoomDetail(r) {
    document.getElementById('roomDetailBody').innerHTML = `
        <p><b>Số phòng:</b> ${r.roomNumber}</p>
        <p><b>Tầng:</b> ${r.floor}</p>
        <p><b>Loại phòng:</b> ${r.roomTypeName}</p>
        <p><b>Sức chứa:</b> ${r.maxOccupancy ?? '- (chưa có dữ liệu)'} người</p>
        <p><b>Giá:</b> ${formatCurrency(r.basePrice)}</p>
        <p><b>Trạng thái đặt phòng:</b> <span class="badge bg-${bookingStatusColor[r.bookingStatus]}">${bookingStatusText[r.bookingStatus]}</span></p>
        <p><b>Trạng thái vệ sinh:</b> <span class="badge bg-${housekeepingStatusColor[r.housekeepingStatus]}">${housekeepingStatusText[r.housekeepingStatus]}</span></p>
    `;
    new bootstrap.Modal(document.getElementById('roomDetailModal')).show();
}

function formatCurrency(value) {
    return new Intl.NumberFormat('vi-VN').format(value) + ' đ';
}

async function openCreateModal() {
    document.getElementById('modalTitle').innerText = 'Thêm phòng';
    document.getElementById('roomId').value = '';
    document.getElementById('roomNumber').value = '';
    document.getElementById('roomFloor').value = '';
    document.getElementById('statusWrapper').style.display = 'none';
    await loadRoomTypesDropdown();
    new bootstrap.Modal(document.getElementById('roomModal')).show();
}

async function editRoom(r) {
    document.getElementById('modalTitle').innerText = 'Sửa phòng';
    document.getElementById('roomId').value = r.id;
    document.getElementById('roomNumber').value = r.roomNumber;
    document.getElementById('roomFloor').value = r.floor;
    await loadRoomTypesDropdown(r.roomTypeId);
    document.getElementById('bookingStatus').value = r.bookingStatus;
    document.getElementById('housekeepingStatus').value = r.housekeepingStatus;
    document.getElementById('statusWrapper').style.display = 'flex';
    new bootstrap.Modal(document.getElementById('roomModal')).show();
}

async function saveRoom() {
    const roomNumber = document.getElementById('roomNumber').value.trim();
    if (!roomNumber) {
        alert('Vui lòng nhập số phòng');
        return;
    }

    const id = document.getElementById('roomId').value;
    const payload = {
        roomNumber: roomNumber,
        floor: parseInt(document.getElementById('roomFloor').value) || 0,
        roomTypeId: parseInt(document.getElementById('roomTypeId').value),
        bookingStatus: parseInt(document.getElementById('bookingStatus')?.value || 0),
        housekeepingStatus: parseInt(document.getElementById('housekeepingStatus')?.value || 0)
    };

    const url = id ? `/api/rooms/${id}` : '/api/rooms';
    const method = id ? 'PUT' : 'POST';

    const res = await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    });

    if (!res.ok) {
        const err = await res.json();
        alert(err.message || 'Có lỗi xảy ra');
        return;
    }

    bootstrap.Modal.getInstance(document.getElementById('roomModal')).hide();
    loadRooms();
}

async function deleteRoom(id) {
    if (!confirm('Bạn có chắc muốn xóa phòng này?')) return;
    const res = await fetch(`/api/rooms/${id}`, { method: 'DELETE' });
    if (!res.ok) {
        alert('Không thể xóa phòng này (có thể đang có đặt phòng liên quan).');
        return;
    }
    loadRooms();
}

document.addEventListener('DOMContentLoaded', loadRooms);