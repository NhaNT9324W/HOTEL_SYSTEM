/**
 * [V.2.10.JS Room Management Frontend]
 * Kịch bản điều khiển giao diện (Client-side) phân hệ Quản lý danh mục phòng vật lý (UC07).
 * Kết nối trực tiếp với API endpoints phân hệ `IRoomService` và `IRoomTypeService` trên nền tảng .NET Core.
 * Quản lý vòng đời trạng thái kinh doanh (BookingStatus) và dọn dẹp buồng phòng (HousekeepingStatus).
 */

// ===== CẤU HÌNH ĐỒNG BỘ MÁY TRẠNG THÁI & DANH MỤC ENUMS ENGINES =====
const BOOKING_STATUS_CONFIG = {
    texts: ['AVAILABLE', 'RESERVED', 'OCCUPIED', 'MAINTENANCE'],
    colors: ['success', 'warning', 'danger', 'secondary'],
    vnTexts: ['Còn trống', 'Đã đặt trước', 'Đang ở', 'Bảo trì']
};

const HOUSEKEEPING_STATUS_CONFIG = {
    texts: ['DIRTY', 'CLEAN', 'READY'],
    colors: ['danger', 'info', 'success'],
    vnTexts: ['Chưa dọn phòng', 'Đã dọn dẹp', 'Sẵn sàng đón khách']
};

// Vùng nhớ đệm dữ liệu (Data Cache Pool) để quản lý thực thể an toàn, tránh lỗi cú pháp khi truyền JSON vào HTML
let roomsDataCache = [];

// ===== TẢI DANH MỤC PHỤ TRỢ (DROPDOWN) =====
/**
 * Tải danh sách các hạng phòng đang hoạt động và đổ vào thẻ Select Dropdown.
 * Khớp nối dữ liệu từ `IRoomTypeService.GetAllAsync()`.
 * @param {number|null} selectedId - Mã ID hạng phòng được chọn mặc định (nếu có).
 */
async function loadRoomTypesDropdown(selectedId = null) {
    try {
        const res = await fetch('/api/roomtypes');
        if (!res.ok) throw new Error('Không thể tải danh mục hạng phòng.');

        const data = await res.json();
        const select = document.getElementById('roomTypeId');
        if (!select) return;

        select.innerHTML = data
            .filter(rt => rt.isActive)
            .map(rt => `
                <option value="${rt.id}" ${rt.id == selectedId ? 'selected' : ''}>
                    ${rt.name} (${formatCurrency(rt.basePrice)}/đêm)
                </option>
            `).join('');
    } catch (error) {
        console.error('[loadRoomTypesDropdown] Lỗi hệ thống:', error);
    }
}

// ===== TẢI DANH SÁCH PHÒNG VẬT LÝ (UC07.1) =====
/**
 * Truy xuất danh sách toàn bộ phòng khách sạn từ Server.
 * Khớp nối dữ liệu trực tiếp với cấu trúc dữ liệu RoomDto.
 */
async function loadRooms() {
    try {
        const res = await fetch('/api/rooms');
        if (!res.ok) throw new Error('Không thể tải cơ sở dữ liệu phòng vật lý.');

        const data = await res.json();
        roomsDataCache = data; // Đồng bộ dữ liệu vào vùng nhớ đệm bảo mật

        const tbody = document.querySelector('#roomTable tbody');
        if (!tbody) return;

        if (data.length === 0) {
            tbody.innerHTML = `<tr><td colspan="4" class="text-center text-muted py-4">Hệ thống chưa ghi nhận phòng vật lý nào</td></tr>`;
            return;
        }

        tbody.innerHTML = data.map(r => {
            const statusIdx = r.bookingStatus ?? 0;
            const statusText = BOOKING_STATUS_CONFIG.vnTexts[statusIdx] || BOOKING_STATUS_CONFIG.texts[statusIdx];
            const statusColor = BOOKING_STATUS_CONFIG.colors[statusIdx] || 'dark';

            return `
                <tr class="align-middle">
                    <td class="fw-bold text-dark">Phòng ${r.roomNumber}</td>
                    <td>${r.maxOccupancy ?? '-'} người</td>
                    <td><span class="badge bg-${statusColor}">${statusText}</span></td>
                    <td class="text-end">
                        <div class="btn-group" role="group">
                            <button class="btn btn-sm btn-outline-info me-1" onclick="viewRoomDetail(${r.id})" title="Xem chi tiết">
                                <i class="bi bi-eye"></i>
                            </button>
                            <button class="btn btn-sm btn-outline-warning me-1" onclick="openEditModal(${r.id})" title="Hiệu chỉnh thông tin">
                                <i class="bi bi-pencil"></i>
                            </button>
                            <button class="btn btn-sm btn-outline-danger" onclick="deleteRoom(${r.id})" title="Xóa phòng">
                                <i class="bi bi-trash"></i>
                            </button>
                        </div>
                    </td>
                </tr>`;
        }).join('');
    } catch (error) {
        console.error('[loadRooms] Root cause error:', error);
        alert('Gặp sự cố hệ thống trong quá trình làm mới danh sách phòng.');
    }
}

// ===== XEM CHI TIẾT THÔNG TIN PHÒNG PIVOT =====
/**
 * Trích xuất thực thể phòng từ Cache Pool và hiển thị lên giao diện View Modal chi tiết.
 * @param {number} roomId - Mã Primary Key của phòng vật lý cần xem.
 */
function viewRoomDetail(roomId) {
    const room = roomsDataCache.find(r => r.id === roomId);
    if (!room) {
        alert('Không tìm thấy dữ liệu cấu trúc phòng này.');
        return;
    }

    const bStatus = room.bookingStatus ?? 0;
    const hStatus = room.housekeepingStatus ?? 0;

    const detailContainer = document.getElementById('roomDetailBody');
    if (detailContainer) {
        detailContainer.innerHTML = `
            <p><b>Số hiệu phòng:</b> <span class="text-dark fw-bold">Phòng ${room.roomNumber}</span></p>
            <p><b>Vị trí khu vực:</b> Tầng ${room.floor}</p>
            <p><b>Phân loại hạng phòng:</b> ${room.roomTypeName || 'Chưa liên kết'}</p>
            <p><b>Sức chứa tiêu chuẩn:</b> ${room.maxOccupancy ?? 'Chưa cấu hình'} người</p>
            <p><b>Đơn giá niêm yết:</b> <span class="text-success fw-bold">${formatCurrency(room.basePrice)}</span></p>
            <p><b>Trạng thái đặt phòng:</b> <span class="badge bg-${BOOKING_STATUS_CONFIG.colors[bStatus]}">${BOOKING_STATUS_CONFIG.vnTexts[bStatus]}</span></p>
            <p><b>Trạng thái dọn dẹp:</b> <span class="badge bg-${HOUSEKEEPING_STATUS_CONFIG.colors[hStatus]}">${HOUSEKEEPING_STATUS_CONFIG.vnTexts[hStatus]}</span></p>
        `;
    }

    const modalElement = document.getElementById('roomDetailModal');
    if (modalElement) new bootstrap.Modal(modalElement).show();
}

// ===== KHỞI TẠO & HIỆU CHỈNH BIỂU MẪU DIỀU HÀNH MODAL (UC07.2 / UC07.3) =====
/** Kích hoạt trạng thái Form trống chuẩn bị thêm mới phòng khách sạn. */
async function openCreateModal() {
    document.getElementById('modalTitle').innerText = 'Thêm phòng vật lý mới';
    document.getElementById('roomId').value = '';
    document.getElementById('roomNumber').value = '';
    document.getElementById('roomFloor').value = '1';

    const statusWrapper = document.getElementById('statusWrapper');
    if (statusWrapper) statusWrapper.style.display = 'none'; // Thêm mới thì giấu trạng thái máy để Backend tự gán Default

    await loadRoomTypesDropdown();

    const modalElement = document.getElementById('roomModal');
    if (modalElement) new bootstrap.Modal(modalElement).show();
}

/** Tải dữ liệu thực thể phòng lên biểu mẫu đầu vào để tiến hành sửa đổi. */
async function openEditModal(roomId) {
    const room = roomsDataCache.find(r => r.id === roomId);
    if (!room) {
        alert('Hệ thống không thể trích xuất hồ sơ phòng cần sửa đổi.');
        return;
    }

    document.getElementById('modalTitle').innerText = `Cập nhật cấu hình Phòng ${room.roomNumber}`;
    document.getElementById('roomId').value = room.id;
    document.getElementById('roomNumber').value = room.roomNumber;
    document.getElementById('roomFloor').value = room.floor;

    await loadRoomTypesDropdown(room.roomTypeId);

    const bookingStatusSelect = document.getElementById('bookingStatus');
    const housekeepingStatusSelect = document.getElementById('housekeepingStatus');

    if (bookingStatusSelect) bookingStatusSelect.value = room.bookingStatus;
    if (housekeepingStatusSelect) housekeepingStatusSelect.value = room.housekeepingStatus;

    const statusWrapper = document.getElementById('statusWrapper');
    if (statusWrapper) statusWrapper.style.display = 'flex'; // Hiện trạng thái máy để Quản lý chủ động điều phối nghiệp vụ

    const modalElement = document.getElementById('roomModal');
    if (modalElement) new bootstrap.Modal(modalElement).show();
}

/** Gửi lệnh đồng bộ hoặc lưu mới dữ liệu phòng lên máy chủ điều hướng theo luồng Method HTTP. */
async function saveRoom() {
    const roomNumberInput = document.getElementById('roomNumber');
    const roomNumber = roomNumberInput ? roomNumberInput.value.trim() : '';

    if (!roomNumber) {
        alert('Vui lòng nhập số hiệu danh định phòng khách sạn.');
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

    try {
        const res = await fetch(url, {
            method,
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });

        if (!res.ok) {
            const err = await res.json();
            throw new Error(err.message || 'Lỗi xử lý nghiệp vụ trên Core Server.');
        }

        const modalElement = document.getElementById('roomModal');
        const instance = bootstrap.Modal.getInstance(modalElement);
        if (instance) instance.hide();

        await loadRooms(); // Tải lại lưới bảng để hiển thị trạng thái phòng mới đồng bộ
    } catch (error) {
        console.error('[saveRoom] Exception breakdown:', error);
        alert(error.message || 'Không thể lưu thay đổi do vi phạm quy tắc số phòng hoặc hạng phòng liên kết.');
    }
}

// ===== XÓA CỨNG PHÒNG VẬT LÝ =====
/** Gửi yêu cầu xóa cứng bản ghi phòng ra khỏi Database. Chặn nếu có ràng buộc. */
async function deleteRoom(id) {
    if (!confirm('Bạn có chắc chắn muốn loại bỏ phòng vật lý này ra khỏi hệ thống danh mục khách sạn?')) return;

    try {
        const res = await fetch(`/api/rooms/${id}`, { method: 'DELETE' });
        if (!res.ok) {
            throw new Error('Chặn xóa: Phòng đang nằm trong vòng đời lưu trú hoặc hóa đơn tài chính.');
        }
        await loadRooms();
    } catch (error) {
        console.error('[deleteRoom] Failure details:', error);
        alert(error.message || 'Hệ thống từ chối lệnh xóa nhằm duy trì tính toàn vẹn dữ liệu lịch sử đặt phòng.');
    }
}

// ===== PRIVATE HELPERS TÀI CHÍNH =====
/** Định dạng số thô đầu vào sang chuẩn tiền tệ Việt Nam Đồng thân thiện trực quan. */
function formatCurrency(value) {
    if (value === null || value === undefined) return '0 đ';
    return new Intl.NumberFormat('vi-VN').format(value) + ' đ';
}

// Đăng ký kích hoạt tiến trình nạp cơ sở dữ liệu ngay khi cây cấu trúc DOM Client sẵn sàng
document.addEventListener('DOMContentLoaded', loadRooms);