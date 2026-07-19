/**
 * [V.2.11.JS Room Type Configuration Frontend]
 * Kịch bản điều khiển giao diện (Client-side) phân hệ Quản lý cấu hình và biểu giá hạng phòng (UC09).
 * Kết nối trực tiếp với các API endpoints của `IRoomTypeService` trên nền tảng .NET Core.
 * Quản lý chính sách danh mục hạng phòng, sức chứa tiêu chuẩn và áp dụng cơ chế xóa mềm bảo toàn dữ liệu liên kết.
 */

// ===== VÙNG NHỚ ĐỆM DỮ LIỆU TOÀN CỤC (DATA CACHE POOL) =====
// Ngăn ngừa lỗi cú pháp trích xuất chuỗi JSON trực tiếp trên cây thuộc tính HTML DOM
let roomTypesDataCache = [];

// ===== LOAD DATA & RENDERING =====
/**
 * Tải danh sách toàn bộ các hạng phòng từ máy chủ hệ thống.
 * Khớp nối trực tiếp cấu trúc dữ liệu trả về với RoomTypeDto (UC09.1).
 */
async function loadRoomTypes() {
    try {
        const res = await fetch('/api/roomtypes');
        if (!res.ok) throw new Error('Không thể kết nối cơ sở dữ liệu danh mục hạng phòng.');

        const data = await res.json();
        roomTypesDataCache = data; // Đồng bộ hóa dữ liệu vào vùng đệm an toàn

        const tbody = document.querySelector('#roomTypeTable tbody');
        if (!tbody) return;

        if (data.length === 0) {
            tbody.innerHTML = `<tr><td colspan="4" class="text-center text-muted py-4">Chưa có cấu hình hạng phòng nào trên hệ thống</td></tr>`;
            return;
        }

        tbody.innerHTML = data.map(rt => `
            <tr class="align-middle">
                <td class="fw-bold text-dark">${rt.name}</td>
                <td><i class="bi bi-people me-2 text-secondary"></i>${rt.maxOccupancy} người</td>
                <td>
                    <span class="badge ${rt.isActive ? 'bg-success' : 'bg-secondary'}">
                        ${rt.isActive ? 'Đang hoạt động' : 'Ngừng kích hoạt'}
                    </span>
                </td>
                <td class="text-end">
                    <div class="btn-group" role="group">
                        <button class="btn btn-sm btn-outline-info me-1" onclick="viewRoomTypeDetail(${rt.id})" title="Xem chi tiết">
                            <i class="bi bi-eye"></i> Chi tiết
                        </button>
                        <button class="btn btn-sm btn-outline-warning me-1" onclick="openEditModal(${rt.id})" title="Hiệu chỉnh cấu hình">
                            <i class="bi bi-pencil"></i> Sửa
                        </button>
                        <button class="btn btn-sm btn-outline-danger" onclick="deleteRoomType(${rt.id})" title="Xóa/Vô hiệu hóa">
                            <i class="bi bi-trash"></i> Xóa
                        </button>
                    </div>
                </td>
            </tr>`).join('');
    } catch (error) {
        console.error('[loadRoomTypes] Root cause error:', error);
        alert('Gặp sự cố hệ thống khi tải danh mục hạng phòng.');
    }
}

/**
 * Trích xuất thực thể hạng phòng từ Cache Pool và hiển thị chi tiết Folio thuộc tính (UC09.2).
 * @param {number} id - Mã định danh Primary Key của hạng phòng cần xem.
 */
function viewRoomTypeDetail(id) {
    const rt = roomTypesDataCache.find(item => item.id === id);
    if (!rt) {
        alert('Không tìm thấy dữ liệu hạng phòng yêu cầu.');
        return;
    }

    const detailContainer = document.getElementById('roomTypeDetailBody');
    if (detailContainer) {
        detailContainer.innerHTML = `
            <p><b>Tên loại hạng phòng:</b> <span class="text-dark fw-bold">${rt.name}</span></p>
            <p><b>Mô tả chính sách:</b> ${rt.description ?? '<span class="text-muted">Không có mô tả</span>'}</p>
            <p><b>Giá sàn cơ bản:</b> <span class="text-success fw-bold">${formatCurrency(rt.basePrice)}</span></p>
            <p><b>Sức chứa tiêu chuẩn:</b> ${rt.maxOccupancy} người / phòng</p>
            <p><b>Trạng thái hệ thống:</b>
                <span class="badge ${rt.isActive ? 'bg-success' : 'bg-secondary'}">
                    ${rt.isActive ? 'Đang hoạt động (Active)' : 'Ngừng kích hoạt (Inactive)'}
                </span>
            </p>
        `;
    }

    const modalElement = document.getElementById('roomTypeDetailModal');
    if (modalElement) new bootstrap.Modal(modalElement).show();
}

// ===== KHỞI TẠO BIỂU MẪU CẤU HÌNH MODAL (UC09.3) =====
/** Dọn dẹp dữ liệu cũ, thiết lập trạng thái mặc định chuẩn bị thêm mới hạng phòng. */
function openCreateModal() {
    document.getElementById('modalTitle').innerText = 'Thêm loại hạng phòng mới';
    document.getElementById('rtId').value = '';
    document.getElementById('rtName').value = '';
    document.getElementById('rtDesc').value = '';
    document.getElementById('rtPrice').value = '';
    document.getElementById('rtOccupancy').value = '2';

    const statusWrapper = document.getElementById('statusWrapper');
    if (statusWrapper) statusWrapper.style.display = 'none'; // Thêm mới mặc định hệ thống tự kích hoạt Active

    const modalElement = document.getElementById('roomTypeModal');
    if (modalElement) new bootstrap.Modal(modalElement).show();
}

/**
 * Tải dữ liệu thực thể hạng phòng từ Cache Pool đổ lên Form điều hành để sửa đổi.
 * @param {number} id - Mã định danh hạng phòng cần hiệu chỉnh.
 */
function openEditModal(id) {
    const rt = roomTypesDataCache.find(item => item.id === id);
    if (!rt) {
        alert('Hệ thống không thể trích xuất hồ sơ hạng phòng cần sửa đổi.');
        return;
    }

    document.getElementById('modalTitle').innerText = `Cập nhật cấu hình hạng phòng: ${rt.name}`;
    document.getElementById('rtId').value = rt.id;
    document.getElementById('rtName').value = rt.name;
    document.getElementById('rtDesc').value = rt.description ?? '';
    document.getElementById('rtPrice').value = rt.basePrice;
    document.getElementById('rtOccupancy').value = rt.maxOccupancy;

    const statusSelect = document.getElementById('rtIsActive');
    if (statusSelect) statusSelect.value = rt.isActive.toString();

    const statusWrapper = document.getElementById('statusWrapper');
    if (statusWrapper) statusWrapper.style.display = 'block';

    const modalElement = document.getElementById('roomTypeModal');
    if (modalElement) new bootstrap.Modal(modalElement).show();
}

/** Gửi yêu cầu đồng bộ cấu hình (POST/PUT) lên Server Core Core API. */
async function saveRoomType() {
    const nameInput = document.getElementById('rtName');
    const name = nameInput ? nameInput.value.trim() : '';

    if (!name) {
        alert('Vui lòng điền thông tin tên phân loại hạng phòng.');
        return;
    }

    const id = document.getElementById('rtId').value;
    const payload = {
        name: name,
        description: document.getElementById('rtDesc').value.trim() || null,
        basePrice: parseFloat(document.getElementById('rtPrice').value) || 0,
        maxOccupancy: parseInt(document.getElementById('rtOccupancy').value) || 1,
        isActive: id ? (document.getElementById('rtIsActive').value === 'true') : true
    };

    const url = id ? `/api/roomtypes/${id}` : '/api/roomtypes';
    const method = id ? 'PUT' : 'POST';

    try {
        const res = await fetch(url, {
            method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (!res.ok) {
            const errData = await res.json().catch(() => ({}));
            throw new Error(errData.message || 'Lỗi xử lý ràng buộc nghiệp vụ cấu hình trên Core Server.');
        }

        const modalElement = document.getElementById('roomTypeModal');
        const instance = bootstrap.Modal.getInstance(modalElement);
        if (instance) instance.hide();

        await loadRoomTypes(); // Làm mới lưới dữ liệu sau khi đồng bộ thành công
    } catch (error) {
        console.error('[saveRoomType] Exception analysis:', error);
        alert(error.message || 'Không thể lưu thay đổi. Vui lòng kiểm tra lại tính hợp lệ của dữ liệu đầu vào.');
    }
}

// ===== XÓA MỀM / NGỪNG KÍCH HOẠT HẠNG PHÒNG =====
/**
 * Thực thi cơ chế Xóa mềm (Soft Delete) hạng phòng qua giao thức HTTP DELETE.
 * Bảo toàn tính toàn vẹn dữ liệu cho các phòng vật lý đang liên kết thực địa.
 * @param {number} id - Mã định danh hạng phòng cần xóa mềm.
 */
async function deleteRoomType(id) {
    if (!confirm('Xác nhận ngắt kích hoạt và áp dụng cơ chế xóa mềm cho loại phòng này? (Các phòng vật lý liên quan sẽ bị ảnh hưởng nếu đang hoạt động)')) return;

    try {
        const res = await fetch(`/api/roomtypes/${id}`, { method: 'DELETE' });
        if (!res.ok) {
            throw new Error('Hệ thống từ chối lệnh xóa do vi phạm toàn vẹn dữ liệu lịch sử đặt phòng.');
        }
        await loadRoomTypes();
    } catch (error) {
        console.error('[deleteRoomType] Execution fail:', error);
        alert(error.message || 'Không thể thực thi xóa mềm hạng phòng mục tiêu.');
    }
}

// ===== PRIVATE HELPERS =====
/** Định dạng giá trị số thô sang chuẩn hiển thị tiền tệ Việt Nam Đồng (VND). */
function formatCurrency(value) {
    if (value === null || value === undefined) return '0 đ';
    return new Intl.NumberFormat('vi-VN').format(value) + ' đ';
}

// Đăng ký lắng nghe sự kiện khởi tạo hệ thống tải danh mục gốc
document.addEventListener('DOMContentLoaded', loadRoomTypes);