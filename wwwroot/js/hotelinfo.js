// ===== LOAD DATA =====
async function loadHotelInfo() {
    const response = await fetch('/api/hotelinfo');
    const info = await response.json();
    renderInfo(info);
}

function renderInfo(info) {
    document.getElementById('info_hotelName').textContent = info.hotelName;
    document.getElementById('info_address').textContent = info.address;
    document.getElementById('info_phone').textContent = info.phone;
    document.getElementById('info_email').textContent = info.email;
    document.getElementById('info_website').innerHTML = info.website
        ? `<a href="${info.website}" target="_blank">${info.website}</a>`
        : '-';
    document.getElementById('info_description').textContent = info.description || '-';
    document.getElementById('info_updatedAt').textContent =
        new Date(info.updatedAt).toLocaleString('vi-VN');
}

// ===== EDIT =====
async function openEditModal() {
    const response = await fetch('/api/hotelinfo');
    const info = await response.json();

    document.getElementById('edit_hotelName').value = info.hotelName;
    document.getElementById('edit_address').value = info.address;
    document.getElementById('edit_phone').value = info.phone;
    document.getElementById('edit_email').value = info.email;
    document.getElementById('edit_website').value = info.website || '';
    document.getElementById('edit_description').value = info.description || '';

    // Clear errors
    ['hotelName', 'address', 'phone', 'email'].forEach(f => {
        const err = document.getElementById(`edit_${f}_error`);
        if (err) err.textContent = '';
    });

    new bootstrap.Modal(document.getElementById('editModal')).show();
}

async function submitEdit() {
    const hotelName = document.getElementById('edit_hotelName').value.trim();
    const address = document.getElementById('edit_address').value.trim();
    const phone = document.getElementById('edit_phone').value.trim();
    const email = document.getElementById('edit_email').value.trim();
    const website = document.getElementById('edit_website').value.trim();
    const description = document.getElementById('edit_description').value.trim();

    // Validate
    let isValid = true;

    if (!hotelName) { document.getElementById('edit_hotelName_error').textContent = 'Hotel name is required'; isValid = false; }
    else document.getElementById('edit_hotelName_error').textContent = '';

    if (!address) { document.getElementById('edit_address_error').textContent = 'Address is required'; isValid = false; }
    else document.getElementById('edit_address_error').textContent = '';

    if (!phone) { document.getElementById('edit_phone_error').textContent = 'Phone is required'; isValid = false; }
    else document.getElementById('edit_phone_error').textContent = '';

    if (!email) { document.getElementById('edit_email_error').textContent = 'Email is required'; isValid = false; }
    else document.getElementById('edit_email_error').textContent = '';

    if (!isValid) return;

    const response = await fetch('/api/hotelinfo', {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ hotelName, address, phone, email, website, description })
    });

    const result = await response.json();

    if (response.ok) {
        bootstrap.Modal.getInstance(document.getElementById('editModal')).hide();
        showAlert('success', result.message);
        loadHotelInfo();
    } else {
        showAlert('danger', result.message);
    }
}

// ===== HELPERS =====
function showAlert(type, message) {
    const alertBox = document.getElementById('alertBox');
    alertBox.className = `alert alert-${type}`;
    alertBox.textContent = message;
    alertBox.classList.remove('d-none');
    setTimeout(() => alertBox.classList.add('d-none'), 3000);
}

// Load khi trang khởi động
document.addEventListener('DOMContentLoaded', () => loadHotelInfo());