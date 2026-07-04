async function loadRoomTypes() {
    const res = await fetch('/api/roomtypes');
    const data = await res.json();
    const tbody = document.querySelector('#roomTypeTable tbody');

    if (data.length === 0) {
        tbody.innerHTML = `<tr><td colspan="4" class="text-center text-muted py-4">Chưa có loại phòng nào</td></tr>`;
        return;
    }

    tbody.innerHTML = data.map(rt => `
        <tr>
            <td class="fw-semibold">${rt.name}</td>
            <td>${rt.maxOccupancy} người</td>
            <td>
                <span class="badge ${rt.isActive ? 'bg-success' : 'bg-secondary'}">
                    ${rt.isActive ? 'Active' : 'Inactive'}
                </span>
            </td>
            <td class="text-end">
                <button class="btn btn-sm btn-outline-info" onclick='viewRoomTypeDetail(${JSON.stringify(rt)})'>
                    <i class="bi bi-eye"></i>
                </button>
                <button class="btn btn-sm btn-outline-warning" onclick='editRoomType(${JSON.stringify(rt)})'>
                    <i class="bi bi-pencil"></i>
                </button>
                <button class="btn btn-sm btn-outline-danger" onclick="deleteRoomType(${rt.id})">
                    <i class="bi bi-trash"></i>
                </button>
            </td>
        </tr>`).join('');
}

function viewRoomTypeDetail(rt) {
    document.getElementById('roomTypeDetailBody').innerHTML = `
        <p><b>Tên loại phòng:</b> ${rt.name}</p>
        <p><b>Mô tả:</b> ${rt.description ?? '-'}</p>
        <p><b>Giá cơ bản:</b> ${formatCurrency(rt.basePrice)}</p>
        <p><b>Sức chứa:</b> ${rt.maxOccupancy} người</p>
        <p><b>Trạng thái:</b>
            <span class="badge ${rt.isActive ? 'bg-success' : 'bg-secondary'}">
                ${rt.isActive ? 'Active' : 'Inactive'}
            </span>
        </p>
    `;
    new bootstrap.Modal(document.getElementById('roomTypeDetailModal')).show();
}

function formatCurrency(value) {
    return new Intl.NumberFormat('vi-VN').format(value) + ' đ';
}

function openCreateModal() {
    document.getElementById('modalTitle').innerText = 'Thêm loại phòng';
    document.getElementById('rtId').value = '';
    document.getElementById('rtName').value = '';
    document.getElementById('rtDesc').value = '';
    document.getElementById('rtPrice').value = '';
    document.getElementById('rtOccupancy').value = '';
    document.getElementById('statusWrapper').style.display = 'none';
    new bootstrap.Modal(document.getElementById('roomTypeModal')).show();
}

function editRoomType(rt) {
    document.getElementById('modalTitle').innerText = 'Sửa loại phòng';
    document.getElementById('rtId').value = rt.id;
    document.getElementById('rtName').value = rt.name;
    document.getElementById('rtDesc').value = rt.description ?? '';
    document.getElementById('rtPrice').value = rt.basePrice;
    document.getElementById('rtOccupancy').value = rt.maxOccupancy;
    document.getElementById('rtIsActive').value = rt.isActive.toString();
    document.getElementById('statusWrapper').style.display = 'block';
    new bootstrap.Modal(document.getElementById('roomTypeModal')).show();
}

async function saveRoomType() {
    const name = document.getElementById('rtName').value.trim();
    if (!name) {
        alert('Vui lòng nhập tên loại phòng');
        return;
    }

    const id = document.getElementById('rtId').value;
    const payload = {
        name: name,
        description: document.getElementById('rtDesc').value,
        basePrice: parseFloat(document.getElementById('rtPrice').value) || 0,
        maxOccupancy: parseInt(document.getElementById('rtOccupancy').value) || 1,
        isActive: id ? (document.getElementById('rtIsActive').value === 'true') : true
    };

    const url = id ? `/api/roomtypes/${id}` : '/api/roomtypes';
    const method = id ? 'PUT' : 'POST';

    const res = await fetch(url, {
        method,
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(payload)
    });

    if (!res.ok) {
        alert('Có lỗi xảy ra, vui lòng thử lại.');
        return;
    }

    bootstrap.Modal.getInstance(document.getElementById('roomTypeModal')).hide();
    loadRoomTypes();
}

async function deleteRoomType(id) {
    if (!confirm('Bạn có chắc muốn xóa (vô hiệu hóa) loại phòng này?')) return;
    await fetch(`/api/roomtypes/${id}`, { method: 'DELETE' });
    loadRoomTypes();
}

document.addEventListener('DOMContentLoaded', loadRoomTypes);