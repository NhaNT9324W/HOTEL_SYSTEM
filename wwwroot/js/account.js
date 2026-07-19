/**
 * [V.2.1.JS Account Management Frontend]
 * Kịch bản điều khiển giao diện (Client-side) phân hệ Quản lý tài khoản và nhân sự (UC05).
 * Kết nối trực tiếp với API endpoints phân hệ `IAccountService` và `IAuthService` trên nền tảng .NET Core.
 * Sử dụng thuần Vanilla JS kết hợp Bootstrap 5 Modals và Bootstrap Icons.
 */

// ===== MÁY TRẠNG THÁI / CẤU HÌNH ĐỒNG BỘ ENUMS BACKEND =====
const ROLES = ['Admin', 'HotelManager', 'Receptionist', 'RoomStaff'];

const STATUS_BADGES = [
    '<span class="badge bg-success">Active</span>',
    '<span class="badge bg-secondary">Inactive</span>',
    '<span class="badge bg-danger">Locked</span>'
];

// ===== LOAD DATA & RENDERING =====
/**
 * Tải danh sách tài khoản từ Server. 
 * Tự động điều hướng giữa API lấy toàn bộ hoặc API tìm kiếm nâng cao (UC05.1).
 * @param {string} keyword - Từ khóa tra cứu (Tên, email, username).
 */
async function loadAccounts(keyword = '') {
    try {
        const url = keyword
            ? `/api/accounts/search?keyword=${encodeURIComponent(keyword)}`
            : '/api/accounts';

        const response = await fetch(url);
        if (!response.ok) throw new Error('Failed to fetch accounts database.');

        const accounts = await response.json();
        renderTable(accounts);
    } catch (error) {
        console.error('Error on loading accounts:', error);
        showAlert('danger', 'Không thể kết nối tới máy chủ để tải danh mục tài khoản.');
    }
}

/**
 * Kết xuất cấu trúc cây dữ liệu DTO từ Backend lên lưới bảng HTML.
 * @param {Array} accounts - Danh sách mảng chứa các đối tượng AccountDto.
 */
function renderTable(accounts) {
    const tbody = document.getElementById('accountTableBody');
    if (!tbody) return;

    if (accounts.length === 0) {
        tbody.innerHTML = `<tr><td colspan="5" class="text-center text-muted py-3">Không tìm thấy tài khoản nào khớp với bộ lọc</td></tr>`;
        return;
    }

    tbody.innerHTML = accounts.map((a, index) => `
        <tr>
            <td>${index + 1}</td>
            <td class="fw-bold">${a.fullName}</td>
            <td><span class="badge bg-info text-dark">${getRoleName(a.role)}</span></td>
            <td>${getStatusBadge(a.status)}</td>
            <td>
                <div class="btn-group" role="group">
                    <button class="btn btn-sm btn-outline-info me-1" onclick="openDetailModal(${a.id})" title="Xem chi tiết">
                        <i class="bi bi-eye"></i> Chi tiết
                    </button>
                    <button class="btn btn-sm btn-outline-warning me-1" onclick="openEditModal(${a.id})" title="Hiệu chỉnh thông tin">
                        <i class="bi bi-pencil"></i> Sửa
                    </button>
                    <button class="btn btn-sm btn-outline-danger" onclick="openDeleteModal(${a.id}, '${a.fullName}')" title="Xóa tài khoản">
                        <i class="bi bi-trash"></i> Xóa
                    </button>
                </div>
            </td>
        </tr>
    `).join('');
}

// ===== SEARCH ACTIONS =====
/** Thực thi bộ lọc tra cứu tài khoản dựa trên từ khóa client nhập. */
function searchAccounts() {
    const keyword = document.getElementById('searchInput').value.trim();
    loadAccounts(keyword);
}

/** Xóa trạng thái bộ lọc và làm mới lưới dữ liệu về ban đầu. */
function clearSearch() {
    document.getElementById('searchInput').value = '';
    loadAccounts();
}

// ===== CREATE ACCOUNT (UC05.3) =====
/** Khởi tạo biểu mẫu và hiển thị Modal thêm mới nhân sự. */
function openCreateModal() {
    clearCreateForm();
    const modalElement = document.getElementById('createModal');
    if (modalElement) new bootstrap.Modal(modalElement).show();
}

/** Dọn dẹp dữ liệu cũ và reset thông báo lỗi Validate trên Form. */
function clearCreateForm() {
    ['fullName', 'username', 'password', 'email', 'phone'].forEach(f => {
        const input = document.getElementById(`create_${f}`);
        const errorSpan = document.getElementById(`create_${f}_error`);
        if (input) input.value = '';
        if (errorSpan) errorSpan.textContent = '';
    });
    const roleSelect = document.getElementById('create_role');
    if (roleSelect) roleSelect.value = '2'; // Mặc định gán quyền Receptionist
}

/** Gửi yêu cầu khởi tạo tài khoản mới lên endpoint API `CreateAsync`. */
async function submitCreate() {
    let isValid = true;

    const fullName = document.getElementById('create_fullName').value.trim();
    const username = document.getElementById('create_username').value.trim();
    const password = document.getElementById('create_password').value.trim();
    const email = document.getElementById('create_email').value.trim();
    const phone = document.getElementById('create_phone').value.trim();
    const role = parseInt(document.getElementById('create_role').value);

    // Kiểm tra ràng buộc phía Client trước khi gửi request
    if (!fullName) { document.getElementById('create_fullName_error').textContent = 'Họ tên nhân viên không được để trống'; isValid = false; }
    else document.getElementById('create_fullName_error').textContent = '';

    if (!username) { document.getElementById('create_username_error').textContent = 'Tên đăng nhập không được để trống'; isValid = false; }
    else document.getElementById('create_username_error').textContent = '';

    if (!password || password.length < 6) { document.getElementById('create_password_error').textContent = 'Mật khẩu phải chứa ít nhất 6 ký tự'; isValid = false; }
    else document.getElementById('create_password_error').textContent = '';

    if (!email) { document.getElementById('create_email_error').textContent = 'Địa chỉ email bắt buộc nhập'; isValid = false; }
    else document.getElementById('create_email_error').textContent = '';

    if (!isValid) return;

    try {
        const response = await fetch('/api/accounts', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ fullName, username, password, email, phone, role })
        });

        const result = await response.json();

        if (response.ok) {
            bootstrap.Modal.getInstance(document.getElementById('createModal')).hide();
            showAlert('success', result.message || 'Khởi tạo tài khoản nhân sự thành công.');
            loadAccounts();
        } else {
            showAlert('danger', result.message || 'Khởi tạo thất bại do vi phạm ràng buộc dữ liệu.');
        }
    } catch (error) {
        console.error('Error creating account:', error);
        showAlert('danger', 'Lỗi hệ thống trong quá trình xử lý tạo tài khoản.');
    }
}

// ===== EDIT ACCOUNT (UC05.2) =====
/**
 * Tải thông tin tài khoản hiện tại đổ vào Form và kích hoạt giao diện hiệu chỉnh.
 * @param {number} id - Mã primary key của tài khoản cần sửa.
 */
async function openEditModal(id) {
    try {
        const response = await fetch(`/api/accounts/${id}`);
        if (!response.ok) throw new Error('Account record not found.');

        const account = await response.json();

        document.getElementById('edit_id').value = account.id;
        document.getElementById('edit_fullName').value = account.fullName;
        document.getElementById('edit_email').value = account.email;
        document.getElementById('edit_phone').value = account.phone || '';
        document.getElementById('edit_role').value = account.role;
        document.getElementById('edit_status').value = account.status;

        // Xóa sạch dấu vết thông báo lỗi cũ
        document.getElementById('edit_fullName_error').textContent = '';
        document.getElementById('edit_email_error').textContent = '';

        new bootstrap.Modal(document.getElementById('editModal')).show();
    } catch (error) {
        console.error('Error fetching account for edit:', error);
        showAlert('danger', 'Không thể tải thông tin tài khoản để hiệu chỉnh.');
    }
}

/** Đẩy dữ liệu cấu hình cập nhật của nhân sự lên Server qua phương thức HTTP PUT. */
async function submitEdit() {
    const id = document.getElementById('edit_id').value;
    const fullName = document.getElementById('edit_fullName').value.trim();
    const email = document.getElementById('edit_email').value.trim();
    const phone = document.getElementById('edit_phone').value.trim();
    const role = parseInt(document.getElementById('edit_role').value);
    const status = parseInt(document.getElementById('edit_status').value);

    let isValid = true;
    if (!fullName) { document.getElementById('edit_fullName_error').textContent = 'Họ tên không được để trống'; isValid = false; }
    else document.getElementById('edit_fullName_error').textContent = '';

    if (!email) { document.getElementById('edit_email_error').textContent = 'Email không được để trống'; isValid = false; }
    else document.getElementById('edit_email_error').textContent = '';

    if (!isValid) return;

    try {
        const response = await fetch(`/api/accounts/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ id: parseInt(id), fullName, email, phone, role, status })
        });

        const result = await response.json();

        if (response.ok) {
            bootstrap.Modal.getInstance(document.getElementById('editModal')).hide();
            showAlert('success', result.message || 'Cập nhật tài khoản thành công.');
            loadAccounts();
        } else {
            showAlert('danger', result.message || 'Cập nhật tài khoản thất bại.');
        }
    } catch (error) {
        console.error('Error updating account:', error);
        showAlert('danger', 'Gặp lỗi trong quá trình đồng bộ thay đổi lên máy chủ.');
    }
}

// ===== DELETE ACCOUNT =====
/** Kích hoạt hộp thoại cảnh báo xác nhận xóa tài khoản vật lý. */
function openDeleteModal(id, fullName) {
    document.getElementById('delete_id').value = id;
    document.getElementById('delete_name').textContent = fullName;
    new bootstrap.Modal(document.getElementById('deleteModal')).show();
}

/** Gửi lệnh xóa cứng bản ghi tài khoản nhân sự thông qua giao thức HTTP DELETE. */
async function submitDelete() {
    const id = document.getElementById('delete_id').value;

    try {
        const response = await fetch(`/api/accounts/${id}`, {
            method: 'DELETE'
        });

        const result = await response.json();

        if (response.ok) {
            bootstrap.Modal.getInstance(document.getElementById('deleteModal')).hide();
            showAlert('success', result.message || 'Đã loại bỏ tài khoản ra khỏi hệ thống.');
            loadAccounts();
        } else {
            showAlert('danger', result.message || 'Chặn xóa: Bản ghi có liên kết lịch sử nghiệp vụ.');
        }
    } catch (error) {
        console.error('Error deleting account:', error);
        showAlert('danger', 'Quá trình thực thi xóa tài khoản gặp sự cố.');
    }
}

// ===== DETAIL VIEW & MANAGEMENT TOOLS =====
/** Tra cứu toàn bộ thông tin chi tiết cấu trúc AccountDto và hiển thị lên View Modal. */
async function openDetailModal(id) {
    try {
        const response = await fetch(`/api/accounts/${id}`);
        if (!response.ok) throw new Error('Unable to retrieve detail.');

        const account = await response.json();

        document.getElementById('detail_fullName').textContent = account.fullName;
        document.getElementById('detail_username').textContent = account.username;
        document.getElementById('detail_email').textContent = account.email;
        document.getElementById('detail_phone').textContent = account.phone || '-';
        document.getElementById('detail_role').innerHTML = `<span class="badge bg-info text-dark">${getRoleName(account.role)}</span>`;
        document.getElementById('detail_status').innerHTML = getStatusBadge(account.status);

        // Format hiển thị ngày tạo tài khoản theo chuẩn quốc gia Việt Nam
        document.getElementById('detail_createdAt').textContent = new Date(account.createdAt).toLocaleDateString('vi-VN');

        // Lưu trữ thông tin đệm phục vụ tính năng cưỡng bức Reset mật khẩu
        document.getElementById('reset_userId').value = account.id;
        document.getElementById('reset_userName').textContent = account.fullName;

        new bootstrap.Modal(document.getElementById('detailModal')).show();
    } catch (error) {
        console.error('Error showing detail:', error);
        showAlert('danger', 'Không thể mở dữ liệu chi tiết của nhân viên.');
    }
}

// ===== CƯỠNG BỨC ĐẶT LẠI MẬT KHẨU BỞI ADMIN (UC05) =====
/** Chuyển giao diện từ xem chi tiết sang Modal thiết lập lại mật khẩu. */
function openResetPasswordModal() {
    const detailModal = bootstrap.Modal.getInstance(document.getElementById('detailModal'));
    if (detailModal) detailModal.hide();

    document.getElementById('reset_newPassword').value = '';
    document.getElementById('reset_newPassword_error').textContent = '';
    new bootstrap.Modal(document.getElementById('resetPasswordModal')).show();
}

/** Gửi yêu cầu cập nhật mật khẩu mới của nhân viên lên API `ResetPasswordByAdminAsync`. */
async function submitResetPassword() {
    const userId = document.getElementById('reset_userId').value;
    const newPassword = document.getElementById('reset_newPassword').value.trim();

    if (!newPassword || newPassword.length < 6) {
        document.getElementById('reset_newPassword_error').textContent = 'Mật khẩu mới bắt buộc nhập và tối thiểu 6 ký tự';
        return;
    }
    document.getElementById('reset_newPassword_error').textContent = '';

    try {
        const response = await fetch(`/api/auth/reset-password-admin/${userId}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ newPassword })
        });

        const result = await response.json();

        if (response.ok) {
            bootstrap.Modal.getInstance(document.getElementById('resetPasswordModal')).hide();
            showAlert('success', result.message || 'Đặt lại mật khẩu nhân viên thành công.');
        } else {
            showAlert('danger', result.message || 'Không thể thực thi đặt lại mật khẩu.');
        }
    } catch (error) {
        console.error('Error resetting password by admin:', error);
        showAlert('danger', 'Có lỗi xảy ra trong quá trình đặt lại thông tin bảo mật.');
    }
}

// ===== PRIVATE HELPERS =====
/** Giải mã chỉ số Enum trả về từ Backend sang chuỗi text mô tả phân quyền. */
function getRoleName(role) {
    return ROLES[role] || 'Không xác định';
}

/** Chuyển hóa trạng thái số của tài khoản sang thẻ Badge giao diện tương ứng. */
function getStatusBadge(status) {
    return STATUS_BADGES[status] || '<span class="badge bg-dark">Unknown</span>';
}

/**
 * Hiển thị Banner thông báo nhanh dạng Toast/Alert trên thanh tiêu đề ứng dụng.
 * @param {string} type - Phân loại thông báo Bootstrap (success, danger, warning, info).
 * @param {string} message - Nội dung chuỗi văn bản hiển thị.
 */
function showAlert(type, message) {
    const alertBox = document.getElementById('alertBox');
    if (!alertBox) return;

    alertBox.className = `alert alert-${type} position-fixed top-0 start-50 translate-middle-x mt-3 z-3`;
    alertBox.textContent = message;
    alertBox.classList.remove('d-none');

    // Tự động giải phóng biến mất sau 3000ms vận hành
    setTimeout(() => alertBox.classList.add('d-none'), 3000);
}

// Kích hoạt tiến trình tải dữ liệu gốc ngay khi DOM hoàn tất cấu trúc
document.addEventListener('DOMContentLoaded', () => loadAccounts());