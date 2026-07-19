/**
 * [V.2.12.JS Service Management Frontend]
 * Kịch bản điều khiển giao diện (Client-side) phân hệ Quản lý danh mục dịch vụ tiện ích khách sạn (UC10).
 * Kết nối trực tiếp với API endpoints phân hệ `IServiceManager` trên nền tảng .NET Core.
 * Đảm bảo tính nhất quán dữ liệu kế toán, quản lý chặt chẽ biểu giá dịch vụ niêm yết và trạng thái khả dụng.
 */

// ===== MÁY TRẠNG THÁI / CẤU HÌNH ĐỒNG BỘ ENUMS BACKEND =====
const SERVICE_STATUS_CONFIG = {
    texts: ['Active', 'Inactive'],
    colors: ['success', 'secondary'],
    vnTexts: ['Đang hoạt động', 'Ngừng kích hoạt']
};

// Vùng nhớ đệm dữ liệu (Data Cache Pool) quản lý thực thể an toàn hệ thống
let servicesDataCache = [];

// ===== LOAD DATA & RENDERING (UC10.1) =====
/**
 * Tải danh sách danh mục dịch vụ tiện ích từ cơ sở dữ liệu.
 * Tự động phân phối giữa luồng tải mặc định và luồng tìm kiếm nâng cao (UC10.2).
 * @param {string} keyword - Từ khóa tra cứu nhanh danh mục.
 */
async function loadServices(keyword = '') {
    try {
        const url = keyword
            ? `/api/services/search?keyword=${encodeURIComponent(keyword)}`
            : '/api/services';

        const response = await fetch(url);
        if (!response.ok) throw new Error('Không thể kết nối cơ sở dữ liệu dịch vụ.');

        const services = await response.json();
        servicesDataCache = services; // Đồng bộ dữ liệu vào vùng đệm an toàn
        renderTable(services);
    } catch (error) {
        console.error('[loadServices] Root cause details:', error);
        showAlert('danger', 'Gặp sự cố hệ thống khi tải danh mục dịch vụ tiện ích.');
    }
}

/**
 * Kết xuất cây cấu trúc dữ liệu ServiceDto từ Backend lên lưới bảng HTML.
 * @param {Array} services - Mảng chứa danh sách đối tượng thực thể dịch vụ.
 */
function renderTable(services) {
    const tbody = document.getElementById('serviceTableBody');
    if (!tbody) return;

    if (services.length === 0) {
        tbody.innerHTML = `<tr><td colspan="5" class="text-center text-muted py-3">Không tìm thấy dịch vụ tiện ích nào phù hợp</td></tr>`;
        return;
    }

    tbody.innerHTML = services.map((s, index) => `
        <tr class="align-middle">
            <td>${index + 1}</td>
            <td class="fw-bold text-dark">${s.serviceName}</td>
            <td class="fw-semibold text-success">${formatPrice(s.price)}</td>
            <td>${getStatusBadge(s.status)}</td>
            <td class="text-end">
                <div class="btn-group" role="group">
                    <button class="btn btn-sm btn-outline-info me-1" onclick="openDetailModal(${s.id})" title="Xem hồ sơ chi tiết">
                        <i class="bi bi-eye"></i> Chi tiết
                    </button>
                    <button class="btn btn-sm btn-outline-warning me-1" onclick="openEditModal(${s.id})" title="Hiệu chỉnh biểu giá cấu hình">
                        <i class="bi bi-pencil"></i> Sửa
                    </button>
                    <button class="btn btn-sm btn-outline-danger" onclick="openDeleteModal(${s.id}, '${s.serviceName.replace(/'/g, "\\'")}')" title="Xóa dịch vụ">
                        <i class="bi bi-trash"></i> Xóa
                    </button>
                </div>
            </td>
        </tr>
    `).join('');
}

// ===== SEARCH ACTIONS =====
/** Thực thi bộ lọc tra cứu danh mục dịch vụ tiện ích dựa trên từ khóa client nhập. */
function searchServices() {
    const keyword = document.getElementById('searchInput').value.trim();
    loadServices(keyword);
}

/** Xóa trạng thái bộ lọc tra cứu và tải lại lưới dữ liệu ban đầu. */
function clearSearch() {
    document.getElementById('searchInput').value = '';
    loadServices();
}

// ===== DETAIL VIEW =====
/**
 * Tra cứu thông tin chi tiết cấu trúc dịch vụ và hiển thị lên giao diện View Modal.
 * @param {number} id - Mã Primary Key định danh thực thể dịch vụ.
 */
async function openDetailModal(id) {
    try {
        const response = await fetch(`/api/services/${id}`);
        if (!response.ok) throw new Error('Service entity not found.');

        const service = await response.json();

        document.getElementById('detail_serviceName').textContent = service.serviceName;
        document.getElementById('detail_description').textContent = service.description || 'Không có mô tả chi tiết.';
        document.getElementById('detail_price').textContent = formatPrice(service.price);
        document.getElementById('detail_status').innerHTML = getStatusBadge(service.status);

        // Đồng bộ chuẩn hiển thị ngày tạo danh mục quốc gia Việt Nam
        document.getElementById('detail_createdAt').textContent = new Date(service.createdAt).toLocaleDateString('vi-VN');

        new bootstrap.Modal(document.getElementById('detailModal')).show();
    } catch (error) {
        console.error('[openDetailModal] Error:', error);
        showAlert('danger', 'Hệ thống không thể tải hồ sơ chi tiết của dịch vụ này.');
    }
}

// ===== CREATE SERVICE (UC10.3) =====
/** Khởi tạo cấu trúc biểu mẫu trống và hiển thị Modal thêm mới dịch vụ tiện ích. */
function openCreateModal() {
    document.getElementById('create_serviceName').value = '';
    document.getElementById('create_description').value = '';
    document.getElementById('create_price').value = '';

    // Xóa sạch dấu vết thông báo lỗi Front-end Validation cũ
    document.getElementById('create_serviceName_error').textContent = '';
    document.getElementById('create_price_error').textContent = '';

    new bootstrap.Modal(document.getElementById('createModal')).show();
}

/** Gửi yêu cầu khởi tạo dịch vụ mới lên API `CreateAsync` kèm ràng buộc nghiệp vụ. */
async function submitCreate() {
    const serviceName = document.getElementById('create_serviceName').value.trim();
    const description = document.getElementById('create_description').value.trim();
    const price = document.getElementById('create_price').value;

    let isValid = true;

    // Bộ kiểm tra ràng buộc nghiêm ngặt dữ liệu đầu vào phía Client
    if (!serviceName) {
        document.getElementById('create_serviceName_error').textContent = 'Tên dịch vụ tiện ích không được để trống';
        isValid = false;
    } else {
        document.getElementById('create_serviceName_error').textContent = '';
    }

    if (!price || parseFloat(price) < 0) {
        document.getElementById('create_price_error').textContent = 'Đơn giá niêm yết phải lớn hơn hoặc bằng 0 đ';
        isValid = false;
    } else {
        document.getElementById('create_price_error').textContent = '';
    }

    if (!isValid) return;

    try {
        const response = await fetch('/api/services', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ serviceName, description: description || null, price: parseFloat(price) })
        });

        const result = await response.json();

        if (response.ok) {
            bootstrap.Modal.getInstance(document.getElementById('createModal')).hide();
            showAlert('success', result.message || 'Khởi tạo danh mục dịch vụ thành công.');
            loadServices();
        } else {
            showAlert('danger', result.message || 'Khởi tạo thất bại do trùng tên dịch vụ.');
        }
    } catch (error) {
        console.error('[submitCreate] Connection fail:', error);
        showAlert('danger', 'Lỗi hệ thống trong quá trình xử lý lưu dịch vụ mới.');
    }
}

// ===== EDIT & UPDATE SERVICE =====
/**
 * Tải thông tin thực thể hiện tại đổ vào Form và kích hoạt giao diện hiệu chỉnh biểu giá.
 * @param {number} id - Mã định danh dịch vụ cần sửa đổi.
 */
async function openEditModal(id) {
    try {
        const response = await fetch(`/api/services/${id}`);
        if (!response.ok) throw new Error('Unable to retrieve current configuration.');

        const service = await response.json();

        document.getElementById('edit_id').value = service.id;
        document.getElementById('edit_serviceName').value = service.serviceName;
        document.getElementById('edit_description').value = service.description || '';
        document.getElementById('edit_price').value = service.price;
        document.getElementById('edit_status').value = service.status;

        document.getElementById('edit_serviceName_error').textContent = '';
        document.getElementById('edit_price_error').textContent = '';

        new bootstrap.Modal(document.getElementById('editModal')).show();
    } catch (error) {
        console.error('[openEditModal] Error:', error);
        showAlert('danger', 'Không thể tải cấu hình dịch vụ tiện ích để thực hiện hiệu chỉnh.');
    }
}

/** Đẩy dữ liệu hiệu chỉnh biểu giá cấu hình dịch vụ lên máy chủ bằng phương thức HTTP PUT. */
async function submitEdit() {
    const id = document.getElementById('edit_id').value;
    const serviceName = document.getElementById('edit_serviceName').value.trim();
    const description = document.getElementById('edit_description').value.trim();
    const price = document.getElementById('edit_price').value;
    const status = parseInt(document.getElementById('edit_status').value);

    let isValid = true;

    if (!serviceName) {
        document.getElementById('edit_serviceName_error').textContent = 'Tên dịch vụ không được để trống';
        isValid = false;
    } else {
        document.getElementById('edit_serviceName_error').textContent = '';
    }

    if (!price || parseFloat(price) < 0) {
        document.getElementById('edit_price_error').textContent = 'Đơn giá cấu hình phải lớn hơn hoặc bằng 0 đ';
        isValid = false;
    } else {
        document.getElementById('edit_price_error').textContent = '';
    }

    if (!isValid) return;

    try {
        const response = await fetch(`/api/services/${id}`, {
            method: 'PUT',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ id: parseInt(id), serviceName, description: description || null, price: parseFloat(price), status })
        });

        const result = await response.json();

        if (response.ok) {
            bootstrap.Modal.getInstance(document.getElementById('editModal')).hide();
            showAlert('success', result.message || 'Cập nhật cấu hình dịch vụ thành công.');
            loadServices();
        } else {
            showAlert('danger', result.message || 'Cập nhật thất bại do vi phạm ràng buộc dữ liệu trùng tên.');
        }
    } catch (error) {
        console.error('[submitEdit] Breakdown details:', error);
        showAlert('danger', 'Gặp lỗi trong quá trình đồng bộ thay đổi lên máy chủ.');
    }
}

// ===== DELETE SERVICE =====
/** Kích hoạt hộp thoại cảnh báo xác nhận xóa vật lý thực thể danh mục. */
function openDeleteModal(id, serviceName) {
    document.getElementById('delete_id').value = id;
    document.getElementById('delete_name').textContent = serviceName;
    new bootstrap.Modal(document.getElementById('deleteModal')).show();
}

/** Gửi lệnh loại bỏ danh mục dịch vụ ra khỏi hệ thống qua giao thức HTTP DELETE. */
async function submitDelete() {
    const id = document.getElementById('delete_id').value;

    try {
        const response = await fetch(`/api/services/${id}`, {
            method: 'DELETE'
        });

        const result = await response.json();

        if (response.ok) {
            bootstrap.Modal.getInstance(document.getElementById('deleteModal')).hide();
            showAlert('success', result.message || 'Đã loại bỏ dịch vụ tiện ích ra khỏi hệ thống.');
            loadServices();
        } else {
            showAlert('danger', result.message || 'Chặn xóa: Dịch vụ đã phát sinh dữ liệu tiêu dùng lịch sử Folio.');
        }
    } catch (error) {
        console.error('[submitDelete] Failure details:', error);
        showAlert('danger', 'Hệ thống từ chối lệnh xóa nhằm duy trì toàn vẹn dữ liệu kế toán.');
    }
}

// ===== PRIVATE HELPERS =====
/** 
 * Chuyển đổi mã trạng thái số nguyên sang thẻ Badge giao diện tương ứng. 
 * @param {number} status - Chỉ số trạng thái hệ thống (0 hoặc 1).
 */
function getStatusBadge(status) {
    const vnText = SERVICE_STATUS_CONFIG.vnTexts[status] || SERVICE_STATUS_CONFIG.texts[status];
    const color = SERVICE_STATUS_CONFIG.colors[status] || 'dark';
    return `<span class="badge bg-${color}">${vnText}</span>`;
}

/** Định dạng số thô đầu vào sang chuỗi tiền tệ hiển thị Việt Nam Đồng (VND). */
function formatPrice(price) {
    if (price === null || price === undefined) return '0 đ';
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(price);
}

/**
 * Hiển thị Banner thông báo nhanh tiến trình dạng Alert nổi trên màn hình.
 * @param {string} type - Phân loại lớp alert Bootstrap (success, danger, warning, info).
 * @param {string} message - Nội dung chuỗi văn bản thông báo.
 */
function showAlert(type, message) {
    const alertBox = document.getElementById('alertBox');
    if (!alertBox) return;

    alertBox.className = `alert alert-${type} position-fixed top-0 start-50 translate-middle-x mt-3 z-3 shadow`;
    alertBox.textContent = message;
    alertBox.classList.remove('d-none');

    // Tự động giải phóng ẩn hộp thoại sau 3000ms vận hành định sẵn
    setTimeout(() => alertBox.classList.add('d-none'), 3000);
}

// Kích hoạt tiến trình tải dữ liệu gốc danh mục ngay khi cấu trúc cây DOM hoàn tất
document.addEventListener('DOMContentLoaded', () => loadServices());