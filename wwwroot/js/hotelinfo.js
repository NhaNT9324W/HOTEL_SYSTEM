/**
 * [V.2.7.JS Hotel Information Frontend]
 * Kịch bản điều khiển giao diện (Client-side) phân hệ Quản lý thông tin khách sạn (UC06).
 * Kết nối trực tiếp với API endpoints phân hệ `IHotelInfoService` trên nền tảng .NET Core.
 * Quản lý thông tin cấu hình gốc của cơ sở lưu trú, phục vụ hiển thị hệ thống và trích xuất in ấn Hóa đơn (UC18).
 */

// ===== LOAD DATA & RENDERING =====
/**
 * Tải thông tin cấu hình duy nhất của khách sạn từ Server.
 * Khớp nối trực tiếp cấu trúc dữ liệu với HotelInfoDto từ Backend (UC06.1).
 */
async function loadHotelInfo() {
    try {
        const response = await fetch('/api/hotelinfo');
        if (!response.ok) throw new Error('Failed to fetch hotel metadata from server.');

        const info = await response.json();
        renderInfo(info);
    } catch (error) {
        console.error('[loadHotelInfo] Root cause analysis:', error);
        showAlert('danger', 'Không thể kết nối tới máy chủ để tải thông tin hồ sơ khách sạn.');
    }
}

/**
 * Kết xuất chi tiết thông tin thực thể khách sạn lên các thẻ giao diện HTML.
 * @param {Object} info - Đối tượng chứa cấu trúc dữ liệu HotelInfoDto.
 */
function renderInfo(info) {
    if (!info) return;

    // Cập nhật text content an toàn để tránh lỗ hổng XSS
    const nameEl = document.getElementById('info_hotelName');
    const addrEl = document.getElementById('info_address');
    const phoneEl = document.getElementById('info_phone');
    const emailEl = document.getElementById('info_email');
    const webEl = document.getElementById('info_website');
    const descEl = document.getElementById('info_description');
    const updateEl = document.getElementById('info_updatedAt');

    if (nameEl) nameEl.textContent = info.hotelName;
    if (addrEl) addrEl.textContent = info.address;
    if (phoneEl) phoneEl.textContent = info.phone;
    if (emailEl) emailEl.textContent = info.email;

    if (webEl) {
        webEl.innerHTML = info.website
            ? `<a href="${encodeURI(info.website)}" target="_blank" class="text-decoration-none"><i class="bi bi-globe me-1"></i>${info.website}</a>`
            : '<span class="text-muted">-</span>';
    }

    if (descEl) descEl.textContent = info.description || '-';

    if (updateEl && info.updatedAt) {
        updateEl.textContent = new Date(info.updatedAt).toLocaleString('vi-VN', {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit'
        });
    }
}

// ===== EDIT & CONFIGURATION (UC06.2) =====
/** Tải lại thông tin gốc hiện tại, đổ vào Form đầu vào và kích hoạt Modal hiệu chỉnh. */
async function openEditModal() {
    try {
        const response = await fetch('/api/hotelinfo');
        if (!response.ok) throw new Error('Unable to retrieve current configuration data.');

        const info = await response.json();

        // Điền dữ liệu thực tế vào các trường biểu mẫu
        document.getElementById('edit_hotelName').value = info.hotelName || '';
        document.getElementById('edit_address').value = info.address || '';
        document.getElementById('edit_phone').value = info.phone || '';
        document.getElementById('edit_email').value = info.email || '';
        document.getElementById('edit_website').value = info.website || '';
        document.getElementById('edit_description').value = info.description || '';

        // Dọn dẹp sạch sẽ toàn bộ thông báo lỗi cũ
        ['hotelName', 'address', 'phone', 'email'].forEach(f => {
            const errSpan = document.getElementById(`edit_${f}_error`);
            if (errSpan) errSpan.textContent = '';
        });

        const modalElement = document.getElementById('editModal');
        if (modalElement) {
            new bootstrap.Modal(modalElement).show();
        }
    } catch (error) {
        console.error('[openEditModal] Exception details:', error);
        showAlert('danger', 'Không thể tải dữ liệu cấu hình để thực hiện hiệu chỉnh.');
    }
}

/** Đẩy dữ liệu cập nhật hồ sơ khách sạn lên hệ thống thông qua giao thức HTTP PUT. */
async function submitEdit() {
    const hotelName = document.getElementById('edit_hotelName').value.trim();
    const address = document.getElementById('edit_address').value.trim();
    const phone = document.getElementById('edit_phone').value.trim();
    const email = document.getElementById('edit_email').value.trim();
    const website = document.getElementById('edit_website').value.trim();
    const description = document.getElementById('edit_description').value.trim();

    // Thực hiện cơ chế kiểm tra ràng buộc phía Client (Front-end Validation)
    let isValid = true;

    if (!hotelName) { document.getElementById('edit_hotelName_error').textContent = 'Tên khách sạn bắt buộc phải nhập'; isValid = false; }
    else document.getElementById('edit_hotelName_error').textContent = '';

    if (!address) { document.getElementById('edit_address_error').textContent = 'Địa chỉ cơ sở không được để trống'; isValid = false; }
    else document.getElementById('edit_address_error').textContent = '';

    if (!phone) { document.getElementById('edit_phone_error').textContent = 'Số điện thoại liên hệ bắt buộc nhập'; isValid = false; }
    else document.getElementById('edit_phone_error').textContent = '';

    if (!email) { document.getElementById('edit_email_error').textContent = 'Địa chỉ Email đại diện bắt buộc nhập'; isValid = false; }
    else document.getElementById('edit_email_error').textContent = '';

    if (!isValid) return;

    try {
        const response = await fetch('/api/hotelinfo', {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ hotelName, address, phone, email, website, description })
        });

        const result = await response.json();

        if (response.ok) {
            const modalElement = document.getElementById('editModal');
            const instance = bootstrap.Modal.getInstance(modalElement);
            if (instance) instance.hide();

            showAlert('success', result.message || 'Cập nhật hồ sơ thông tin khách sạn thành công.');
            loadHotelInfo();
        } else {
            showAlert('danger', result.message || 'Cập nhật thất bại do vi phạm ràng buộc dữ liệu.');
        }
    } catch (error) {
        console.error('[submitEdit] Connection breakdown:', error);
        showAlert('danger', 'Có lỗi xảy ra trong quá trình đồng bộ thông tin lên máy chủ.');
    }
}

// ===== PRIVATE HELPERS =====
/**
 * Hiển thị Banner thông báo nhanh trạng thái dạng Banner/Toast trên thanh điều đề.
 * @param {string} type - Phân loại lớp định dạng Bootstrap (success, danger, warning, info).
 * @param {string} message - Nội dung chuỗi văn bản thông báo.
 */
function showAlert(type, message) {
    const alertBox = document.getElementById('alertBox');
    if (!alertBox) return;

    alertBox.className = `alert alert-${type} position-fixed top-0 start-50 translate-middle-x mt-3 z-3 shadow`;
    alertBox.textContent = message;
    alertBox.classList.remove('d-none');

    // Tự động giải phóng ẩn hộp thoại sau 3000ms vận hành ổn định
    setTimeout(() => alertBox.classList.add('d-none'), 3000);
}

// Kích hoạt tiến trình tải cấu hình gốc ngay khi cấu trúc DOM hoàn tất lập trình
document.addEventListener('DOMContentLoaded', () => loadHotelInfo());