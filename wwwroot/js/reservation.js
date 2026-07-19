/**
 * [V.2.9.JS Reservation & Stay Operations Frontend]
 * Kịch bản điều khiển giao diện (Client-side) phân hệ Quản lý Đặt phòng, Lưu trú và Quyết toán Folio (UC13, UC14, UC17, UC18).
 * Kết nối trực tiếp với API endpoints của `IReservationService` và `ICheckOutService` trên nền .NET Core.
 * Quản lý chặt chẽ vòng đời lưu trú của khách hàng từ khi đặt phòng, Check-in, tiêu dùng dịch vụ cho đến khi Check-out xuất hóa đơn.
 */

// ===== TẢI DANH SÁCH & ĐỔ DỮ LIỆU =====
/**
 * Tải danh sách đơn đặt phòng từ máy chủ.
 * Tự động điều hướng giữa API lấy toàn bộ và API tìm kiếm nâng cao dựa trên từ khóa (UC13.1).
 * @param {string} keyword - Từ khóa tìm kiếm (Tên khách hàng, Số điện thoại, Số phòng).
 */
async function loadReservations(keyword = '') {
    try {
        const url = keyword
            ? `/api/reservations/search?keyword=${encodeURIComponent(keyword)}`
            : '/api/reservations';

        const response = await fetch(url);
        if (!response.ok) throw new Error('Failed to fetch reservations database.');

        const reservations = await response.json();
        renderTable(reservations);
    } catch (error) {
        console.error('[loadReservations] Root cause:', error);
        showAlert('danger', 'Không thể kết nối tới máy chủ để tải danh sách đặt phòng.');
    }
}

/**
 * Kết xuất cấu trúc danh sách đơn đặt phòng từ mảng DTO lên lưới bảng HTML.
 * @param {Array} reservations - Mảng chứa các đối tượng ReservationListDto.
 */
function renderTable(reservations) {
    const tbody = document.getElementById('reservationTableBody');
    if (!tbody) return;

    if (reservations.length === 0) {
        tbody.innerHTML = `<tr><td colspan="8" class="text-center text-muted py-3">Không tìm thấy đơn đặt phòng nào phù hợp</td></tr>`;
        return;
    }

    tbody.innerHTML = reservations.map((r, index) => `
        <tr>
            <td>${index + 1}</td>
            <td class="fw-bold text-dark">${r.guestName}</td>
            <td>${r.guestPhone}</td>
            <td><span class="badge bg-light text-dark border fw-bold">Phòng ${r.roomNumber}</span></td>
            <td>${formatDate(r.checkInDate)}</td>
            <td>${formatDate(r.checkOutDate)}</td>
            <td>${getStatusBadge(r.status)}</td>
            <td>
                <div class="btn-group" role="group">
                    <button class="btn btn-sm btn-outline-info me-1" onclick="openDetailModal(${r.id})" title="Xem chi tiết đơn">
                        <i class="bi bi-eye"></i> Chi tiết
                    </button>
                    ${r.status === 'CONFIRMED' ? `
                    <button class="btn btn-sm btn-success me-1" onclick="checkIn(${r.id}, '${r.guestName}')" title="Làm thủ tục nhận phòng">
                        <i class="bi bi-box-arrow-in-right"></i> Check-in
                    </button>
                    <button class="btn btn-sm btn-warning me-1" onclick="openEditModal(${r.id})" title="Thay đổi lịch đặt">
                        <i class="bi bi-pencil"></i> Sửa
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="openCancelModal(${r.id}, '${r.guestName}')" title="Hủy đơn đặt phòng">
                        <i class="bi bi-x-circle"></i> Hủy đơn
                    </button>` : ''}
                    ${r.status === 'CHECKED_IN' ? `
                    <button class="btn btn-sm btn-primary me-1" onclick="openAddServiceModal(${r.id}, '${r.guestName}')" title="Ghi nhận tiêu dùng tiện ích">
                        <i class="bi bi-plus-circle"></i> Thêm dịch vụ
                    </button>
                    <button class="btn btn-sm btn-outline-danger" onclick="openCheckOutModal(${r.id}, '${r.guestName}')" title="Quyết toán hóa đơn & trả phòng">
                        <i class="bi bi-box-arrow-right"></i> Check-out
                    </button>` : ''}    
                </div>
            </td>
        </tr>
    `).join('');
}

// ===== TÌM KIẾM =====
/** Thực thi bộ lọc tra cứu đơn đặt phòng dựa trên từ khóa client nhập. */
function searchReservations() {
    const keyword = document.getElementById('searchInput').value.trim();
    loadReservations(keyword);
}

/** Xóa bộ lọc tìm kiếm và đưa lưới dữ liệu về trạng thái ban đầu. */
function clearSearch() {
    document.getElementById('searchInput').value = '';
    loadReservations();
}

// ===== TẢI DANH MỤC PHỤ TRỢ =====
/** Tải danh sách cấu hình các hạng phòng đang hoạt động đổ vào thẻ Select Dropdown. */
async function loadRoomTypes(selectId) {
    try {
        const response = await fetch('/api/roomtypes');
        const roomTypes = await response.json();
        const select = document.getElementById(selectId);
        if (!select) return;

        select.innerHTML = '<option value="">-- Chọn loại hạng phòng --</option>';
        roomTypes
            .filter(rt => rt.isActive)
            .forEach(rt => {
                select.innerHTML += `<option value="${rt.id}">
                    ${rt.name} - ${formatPrice(rt.basePrice)}/đêm (Tối đa: ${rt.maxOccupancy} khách)
                </option>`;
            });
    } catch (error) {
        console.error('[loadRoomTypes] Error:', error);
    }
}

// ===== XEM CHI TIẾT ĐƠN ĐẶT PHÒNG =====
/** Tra cứu chi tiết cấu trúc ReservationDetailDto và hiển thị lên View Modal phục vụ lễ tân. */
async function openDetailModal(id) {
    try {
        const response = await fetch(`/api/reservations/${id}`);
        if (!response.ok) throw new Error('Cannot query reservation details.');

        const r = await response.json();

        document.getElementById('detail_guestName').textContent = r.guestName;
        document.getElementById('detail_guestPhone').textContent = r.guestPhone;
        document.getElementById('detail_guestIdNumber').textContent = r.guestIdNumber;
        document.getElementById('detail_guestEmail').textContent = r.guestEmail || '-';
        document.getElementById('detail_roomNumber').textContent = `Phòng ${r.roomNumber}`;
        document.getElementById('detail_roomType').textContent = r.roomTypeName;
        document.getElementById('detail_floor').textContent = `Tầng ${r.floor}`;
        document.getElementById('detail_checkIn').textContent = formatDate(r.checkInDate);
        document.getElementById('detail_checkOut').textContent = formatDate(r.checkOutDate);
        document.getElementById('detail_status').innerHTML = getStatusBadge(r.status);
        document.getElementById('detail_createdAt').textContent = new Date(r.createdAt).toLocaleString('vi-VN');

        new bootstrap.Modal(document.getElementById('detailModal')).show();
    } catch (error) {
        console.error('[openDetailModal] Error:', error);
        showAlert('danger', 'Không thể tải thông tin chi tiết của đơn đặt phòng.');
    }
}

// ===== KHỞI TẠO ĐƠN ĐẶT PHÒNG MỚI (UC13) =====
/** Chuẩn bị dữ liệu form trống và hiển thị Modal lập đơn đặt phòng tại quầy. */
async function openCreateModal() {
    await loadRoomTypes('create_roomTypeId');

    document.getElementById('create_guestName').value = '';
    document.getElementById('create_guestPhone').value = '';
    document.getElementById('create_guestIdNumber').value = '';
    document.getElementById('create_guestEmail').value = '';
    document.getElementById('create_checkIn').value = '';
    document.getElementById('create_checkOut').value = '';
    document.getElementById('create_roomTypeId').value = '';
    document.getElementById('create_roomId').innerHTML = '<option value="">-- Vui lòng kiểm tra phòng trống trước --</option>';
    document.getElementById('create_roomId').disabled = true;

    // Reset thông báo lỗi Front-end Validation
    ['guestName', 'guestPhone', 'guestIdNumber', 'checkIn', 'checkOut', 'roomId']
        .forEach(f => {
            const err = document.getElementById(`create_${f}_error`);
            if (err) err.textContent = '';
        });

    new bootstrap.Modal(document.getElementById('createModal')).show();
}

/** Thuật toán kiểm tra và lọc phòng trống khả dụng thời gian thực dựa trên khoảng thời gian lưu trú. */
async function checkAvailability() {
    const checkIn = document.getElementById('create_checkIn').value;
    const checkOut = document.getElementById('create_checkOut').value;
    const roomTypeId = document.getElementById('create_roomTypeId').value;

    if (!checkIn || !checkOut) {
        showAlert('warning', 'Vui lòng chọn đầy đủ ngày nhận (Check-in) và ngày trả (Check-out).');
        return;
    }

    if (checkIn >= checkOut) {
        showAlert('warning', 'Ngày trả phòng (Check-out) phải sau ngày nhận phòng (Check-in).');
        return;
    }

    let url = `/api/reservations/available-rooms?checkIn=${checkIn}&checkOut=${checkOut}`;
    if (roomTypeId) url += `&roomTypeId=${roomTypeId}`;

    try {
        const response = await fetch(url);
        const rooms = await response.json();
        const select = document.getElementById('create_roomId');

        if (rooms.length === 0) {
            select.innerHTML = '<option value="">Không có phòng trống phù hợp</option>';
            select.disabled = true;
            showAlert('warning', 'Không còn phòng trống khả dụng thuộc hạng phòng này trong khoảng thời gian đã chọn.');
        } else {
            select.innerHTML = '<option value="">-- Chọn số phòng vật lý --</option>';
            rooms.forEach(r => {
                select.innerHTML += `<option value="${r.id}">
                    Phòng ${r.roomNumber} - Tầng ${r.floor} (${r.roomTypeName}) - ${formatPrice(r.price)}/đêm
                </option>`;
            });
            select.disabled = false;
            showAlert('success', `Tìm thấy ${rooms.length} phòng trống sẵn sàng đón khách.`);
        }
    } catch (error) {
        console.error('[checkAvailability] Error:', error);
        showAlert('danger', 'Lỗi hệ thống khi tra cứu dữ liệu phòng trống.');
    }
}

/** Gửi yêu cầu lập đơn đặt phòng chính thức lên endpoint dịch vụ `CreateAsync`. */
async function submitCreate() {
    const guestName = document.getElementById('create_guestName').value.trim();
    const guestPhone = document.getElementById('create_guestPhone').value.trim();
    const guestIdNumber = document.getElementById('create_guestIdNumber').value.trim();
    const guestEmail = document.getElementById('create_guestEmail').value.trim();
    const checkIn = document.getElementById('create_checkIn').value;
    const checkOut = document.getElementById('create_checkOut').value;
    const roomId = document.getElementById('create_roomId').value;

    let isValid = true;

    // Bộ kiểm tra điều kiện Front-end
    if (!guestName) { document.getElementById('create_guestName_error').textContent = 'Họ tên khách hàng bắt buộc nhập'; isValid = false; }
    else document.getElementById('create_guestName_error').textContent = '';

    if (!guestPhone) { document.getElementById('create_guestPhone_error').textContent = 'Số điện thoại liên hệ không được để trống'; isValid = false; }
    else document.getElementById('create_guestPhone_error').textContent = '';

    if (!guestIdNumber) { document.getElementById('create_guestIdNumber_error').textContent = 'Số CCCD/Passport là bắt buộc'; isValid = false; }
    else document.getElementById('create_guestIdNumber_error').textContent = '';

    if (!checkIn) { document.getElementById('create_checkIn_error').textContent = 'Vui lòng chọn ngày nhận phòng'; isValid = false; }
    else document.getElementById('create_checkIn_error').textContent = '';

    if (!checkOut) { document.getElementById('create_checkOut_error').textContent = 'Vui lòng chọn ngày trả phòng'; isValid = false; }
    else document.getElementById('create_checkOut_error').textContent = '';

    if (!roomId) { document.getElementById('create_roomId_error').textContent = 'Vui lòng chọn phòng vật lý cụ thể'; isValid = false; }
    else document.getElementById('create_roomId_error').textContent = '';

    if (!isValid) return;

    try {
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
            showAlert('success', result.message || 'Khởi tạo đơn đặt phòng thành công.');
            loadReservations();
        } else {
            showAlert('danger', result.message || 'Lập đơn thất bại do xung đột dữ liệu.');
        }
    } catch (error) {
        console.error('[submitCreate] Connection breakdown:', error);
        showAlert('danger', 'Gặp sự cố kết nối trong quá trình xử lý lưu đơn đặt phòng.');
    }
}

// ===== HIỆU CHỈNH ĐƠN ĐẶT PHÒNG CHƯA CHECK-IN =====
/** Tải thông tin đơn đặt phòng hiện tại để tiến hành sửa đổi thời gian lưu trú hoặc đổi phòng vật lý. */
async function openEditModal(id) {
    try {
        const response = await fetch(`/api/reservations/${id}`);
        const r = await response.json();

        document.getElementById('edit_id').value = r.id;
        document.getElementById('edit_checkIn').value = r.checkInDate.split('T')[0];
        document.getElementById('edit_checkOut').value = r.checkOutDate.split('T')[0];
        document.getElementById('edit_roomId').innerHTML = `<option value="${r.roomId}">Phòng ${r.roomNumber} (Hiện tại)</option>`;
        document.getElementById('edit_roomId').disabled = false;

        ['checkIn', 'checkOut', 'roomId'].forEach(f => {
            const err = document.getElementById(`edit_${f}_error`);
            if (err) err.textContent = '';
        });

        new bootstrap.Modal(document.getElementById('editModal')).show();
    } catch (error) {
        console.error('[openEditModal] Error:', error);
        showAlert('danger', 'Không thể truy xuất dữ liệu đơn đặt phòng để hiệu chỉnh.');
    }
}

/** Tra cứu phòng trống khả dụng phục vụ riêng cho luồng sửa đổi đơn đặt phòng. */
async function checkAvailabilityEdit() {
    const checkIn = document.getElementById('edit_checkIn').value;
    const checkOut = document.getElementById('edit_checkOut').value;

    if (!checkIn || !checkOut) {
        showAlert('warning', 'Vui lòng điền mốc thời gian nhận/trả phòng.');
        return;
    }

    if (checkIn >= checkOut) {
        showAlert('warning', 'Ngày trả phòng (Check-out) phải sau ngày nhận phòng (Check-in).');
        return;
    }

    try {
        const response = await fetch(`/api/reservations/available-rooms?checkIn=${checkIn}&checkOut=${checkOut}`);
        const rooms = await response.json();
        const select = document.getElementById('edit_roomId');

        if (rooms.length === 0) {
            select.innerHTML = '<option value="">Không có phòng khả dụng</option>';
            select.disabled = true;
            showAlert('warning', 'Không tìm thấy phòng trống phù hợp với mốc thời gian điều chỉnh mới.');
        } else {
            select.innerHTML = '<option value="">-- Chọn phòng thay thế --</option>';
            rooms.forEach(r => {
                select.innerHTML += `<option value="${r.id}">
                    Phòng ${r.roomNumber} - Tầng ${r.floor} (${r.roomTypeName}) - ${formatPrice(r.price)}/đêm
                </option>`;
            });
            select.disabled = false;
            showAlert('success', `Tìm thấy ${rooms.length} sự lựa chọn thay thế khả dụng.`);
        }
    } catch (error) {
        console.error('[checkAvailabilityEdit] Error:', error);
    }
}

/** Đẩy dữ liệu hiệu chỉnh lịch trình đơn lưu trú lên Server bằng phương thức HTTP PUT. */
async function submitEdit() {
    const id = document.getElementById('edit_id').value;
    const checkIn = document.getElementById('edit_checkIn').value;
    const checkOut = document.getElementById('edit_checkOut').value;
    const roomId = document.getElementById('edit_roomId').value;

    let isValid = true;
    if (!checkIn) { document.getElementById('edit_checkIn_error').textContent = 'Mốc nhận phòng bắt buộc chọn'; isValid = false; }
    else document.getElementById('edit_checkIn_error').textContent = '';

    if (!checkOut) { document.getElementById('edit_checkOut_error').textContent = 'Mốc trả phòng bắt buộc chọn'; isValid = false; }
    else document.getElementById('edit_checkOut_error').textContent = '';

    if (!roomId) { document.getElementById('edit_roomId_error').textContent = 'Vui lòng chọn số phòng vật lý'; isValid = false; }
    else document.getElementById('edit_roomId_error').textContent = '';

    if (!isValid) return;

    try {
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
            showAlert('success', result.message || 'Cập nhật thay đổi đơn đặt phòng thành công.');
            loadReservations();
        } else {
            showAlert('danger', result.message || 'Cập nhật thất bại do vi phạm ràng buộc phòng trống.');
        }
    } catch (error) {
        console.error('[submitEdit] Error:', error);
        showAlert('danger', 'Lỗi hệ thống khi truyền thông tin chỉnh sửa đơn.');
    }
}

// ===== QUY TRÌNH CHECK-IN (UC14) =====
/** Làm thủ tục nhận phòng chính thức, chuyển BookingStatus của phòng sang OCCUPIED thời gian thực. */
async function checkIn(id, guestName) {
    if (!confirm(`Xác nhận thực hiện thủ tục Check-in nhận phòng cho khách hàng: ${guestName}?`)) return;

    try {
        const response = await fetch(`/api/reservations/${id}/checkin`, {
            method: 'PATCH'
        });

        const result = await response.json();

        if (response.ok) {
            showAlert('success', result.message || 'Đã hoàn tất Check-in. Phòng chuyển sang trạng thái đang lưu trú.');
            loadReservations();
        } else {
            showAlert('danger', result.message || 'Không thể Check-in. Kiểm tra lại trạng thái dọn dẹp (READY) của phòng vật lý.');
        }
    } catch (error) {
        console.error('[checkIn] Connection fail:', error);
        showAlert('danger', 'Gặp lỗi trong quy trình gửi lệnh nhận phòng.');
    }
}

// ===== HỦY ĐƠN ĐẶT PHÒNG =====
/** Mở hộp thoại xác nhận hủy bỏ đơn đặt phòng từ xa trước kỳ lưu trú. */
function openCancelModal(id, guestName) {
    document.getElementById('cancel_id').value = id;
    document.getElementById('cancel_guestName').textContent = guestName;
    new bootstrap.Modal(document.getElementById('cancelModal')).show();
}

/** Gửi lệnh PATCH cưỡng bức chuyển trạng thái đơn đặt sang `CANCELLED`. */
async function submitCancel() {
    const id = document.getElementById('cancel_id').value;

    try {
        const response = await fetch(`/api/reservations/${id}/cancel`, {
            method: 'PATCH'
        });

        const result = await response.json();

        if (response.ok) {
            bootstrap.Modal.getInstance(document.getElementById('cancelModal')).hide();
            showAlert('success', result.message || 'Đã hủy đơn đặt phòng thành công và giải phóng lịch phòng đệm.');
            loadReservations();
        } else {
            showAlert('danger', result.message || 'Hủy đơn thất bại do trạng thái đơn hiện tại không hợp lệ.');
        }
    } catch (error) {
        console.error('[submitCancel] Error:', error);
        showAlert('danger', 'Gặp sự cố khi kết nối lệnh hủy đơn đặt phòng.');
    }
}

// ===== QUAN SÁT VÀ GHI NHẬN TIÊU DÙNG DỊCH VỤ (UC17) =====
/** Mở bảng Folio quản lý dịch vụ, hiển thị menu tiện ích tích hợp mini-bar/quầy và lịch sử tiêu dùng (UC17.1). */
async function openAddServiceModal(reservationId, guestName) {
    document.getElementById('addService_reservationId').value = reservationId;
    document.getElementById('addService_guestName').textContent = guestName;

    try {
        // Tải danh mục tất cả dịch vụ tiện ích khách sạn đang cung cấp
        const response = await fetch('/api/services');
        const services = await response.json();
        const select = document.getElementById('addService_serviceId');
        if (!select) return;

        select.innerHTML = '<option value="">-- Chọn dịch vụ/tiện ích --</option>';
        services
            .filter(s => s.status === 0) // Chỉ lọc lấy các dịch vụ khả dụng (Active)
            .forEach(s => {
                select.innerHTML += `<option value="${s.id}">
                    ${s.serviceName} - ${formatPrice(s.price)}
                </option>`;
            });

        document.getElementById('addService_quantity').value = 1;

        // Đồng bộ kết xuất danh sách các dịch vụ khách đã tiêu dùng trước đó lên lưới bảng
        await loadServiceUsages(reservationId);

        new bootstrap.Modal(document.getElementById('addServiceModal')).show();
    } catch (error) {
        console.error('[openAddServiceModal] Error:', error);
        showAlert('danger', 'Không thể khởi tạo danh mục Folio dịch vụ phụ trợ.');
    }
}

/** Tải danh sách chi tiết các tiện ích mà khách hàng tại phòng đã sử dụng (UC17.1). */
async function loadServiceUsages(reservationId) {
    try {
        const response = await fetch(`/api/checkout/${reservationId}/services`);
        const usages = await response.json();
        const tbody = document.getElementById('serviceUsageTableBody');
        if (!tbody) return;

        if (usages.length === 0) {
            tbody.innerHTML = `<tr><td colspan="6" class="text-center text-muted py-3">Chưa ghi nhận tiêu dùng dịch vụ phụ trợ tại phòng này</td></tr>`;
            document.getElementById('totalServiceCharge').textContent = '0 VND';
            return;
        }

        tbody.innerHTML = usages.map(u => `
            <tr>
                <td class="fw-bold">${u.serviceName}</td>
                <td>${u.quantity}</td>
                <td>${formatPrice(u.unitPrice)}</td>
                <td class="text-dark fw-bold">${formatPrice(u.totalPrice)}</td>
                <td>${new Date(u.usedAt).toLocaleDateString('vi-VN')}</td>
                <td>
                    <button class="btn btn-sm btn-outline-danger" onclick="removeService(${u.id}, ${reservationId})" title="Xóa bản ghi nhập sai">
                        <i class="bi bi-trash"></i>
                    </button>
                </td>
            </tr>
        `).join('');

        // Tính toán tổng tiền dịch vụ phát sinh lũy kế trong hóa đơn đệm
        const total = usages.reduce((sum, u) => sum + u.totalPrice, 0);
        document.getElementById('totalServiceCharge').textContent = formatPrice(total);
    } catch (error) {
        console.error('[loadServiceUsages] Error:', error);
    }
}

/** Thêm mới một lượt tiêu dùng dịch vụ phụ trợ cho phòng lưu trú vào hóa đơn (UC17.2). */
async function submitAddService() {
    const reservationId = parseInt(document.getElementById('addService_reservationId').value);
    const serviceId = document.getElementById('addService_serviceId').value;
    const quantity = parseInt(document.getElementById('addService_quantity').value);

    if (!serviceId) {
        showAlert('warning', 'Vui lòng chọn loại dịch vụ hoặc vật phẩm tiêu dùng.');
        return;
    }

    if (quantity < 1) {
        showAlert('warning', 'Số lượng cung cấp/tiêu dùng tối thiểu từ 1 đơn vị.');
        return;
    }

    try {
        const response = await fetch('/api/checkout/services', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ reservationId, serviceId: parseInt(serviceId), quantity })
        });

        const result = await response.json();

        if (response.ok) {
            showAlert('success', result.message || 'Ghi nhận tiêu dùng dịch vụ thành công.');
            await loadServiceUsages(reservationId); // Đồng bộ lại bảng kê Folio phụ trợ
            document.getElementById('addService_serviceId').value = '';
            document.getElementById('addService_quantity').value = 1;
        } else {
            showAlert('danger', result.message || 'Ghi nhận thất bại do trạng thái đơn lưu trú không hợp lệ.');
        }
    } catch (error) {
        console.error('[submitAddService] Error:', error);
    }
}

/** Loại bỏ một bản ghi tiêu dùng dịch vụ phụ trợ ra khỏi danh mục Folio trước khi chốt hóa đơn. */
async function removeService(serviceUsageId, reservationId) {
    if (!confirm('Xác nhận loại bỏ bản ghi tiêu dùng dịch vụ này ra khỏi hóa đơn chi tiết phòng?')) return;

    try {
        const response = await fetch(`/api/checkout/services/${serviceUsageId}`, {
            method: 'DELETE'
        });

        const result = await response.json();

        if (response.ok) {
            showAlert('success', result.message || 'Đã xóa bản ghi tiêu dùng dịch vụ.');
            await loadServiceUsages(reservationId);
        } else {
            showAlert('danger', result.message || 'Không thể loại bỏ dòng chi phí này.');
        }
    } catch (error) {
        console.error('[removeService] Error:', error);
    }
}

// ===== QUY TRÌNH TRẢ PHÒNG & XUẤT HÓA ĐƠN QUYẾT TOÁN (UC18) =====
/** Tải bảng Folio xem trước toàn bộ chi phí (Tiền phòng, Tiền dịch vụ phát sinh) trước khi quyết toán chốt hóa đơn (UC18.1). */
async function openCheckOutModal(reservationId, guestName) {
    document.getElementById('checkout_reservationId').value = reservationId;

    try {
        // Gọi API nén dữ liệu PreviewInvoiceAsync từ máy chủ phục vụ đối chiếu bill khách hàng
        const response = await fetch(`/api/checkout/${reservationId}/preview`);
        if (!response.ok) throw new Error('Invoice preview generation collapsed.');

        const invoice = await response.json();

        document.getElementById('co_guestName').textContent = invoice.guestName;
        document.getElementById('co_roomNumber').textContent = `Phòng ${invoice.roomNumber}`;
        document.getElementById('co_roomType').textContent = invoice.roomTypeName;
        document.getElementById('co_checkIn').textContent = formatDate(invoice.checkInDate);
        document.getElementById('co_checkOut').textContent = formatDate(invoice.checkOutDate);
        document.getElementById('co_nights').textContent = `${invoice.nights} đêm`;
        document.getElementById('co_nights2').textContent = invoice.nights;
        document.getElementById('co_roomCharge').textContent = formatPrice(invoice.roomCharge);
        document.getElementById('co_serviceCharge').textContent = formatPrice(invoice.serviceCharge);
        document.getElementById('co_totalAmount').textContent = formatPrice(invoice.totalAmount);

        // Kết xuất bảng chiết tính chi tiết dịch vụ đi kèm trong tệp in hóa đơn
        const tbody = document.getElementById('co_serviceDetails');
        if (invoice.services.length === 0) {
            tbody.innerHTML = `<tr><td colspan="2" class="text-center text-muted small py-2">Không sử dụng dịch vụ phụ trợ ngoài</td></tr>`;
        } else {
            tbody.innerHTML = invoice.services.map(s => `
                <tr>
                    <td class="small">${s.serviceName} <span class="text-muted">x${s.quantity}</span></td>
                    <td class="text-end small fw-bold">${formatPrice(s.totalPrice)}</td>
                </tr>
            `).join('');
        }

        new bootstrap.Modal(document.getElementById('checkOutModal')).show();
    } catch (error) {
        console.error('[openCheckOutModal] Error:', error);
        showAlert('danger', 'Không thể khởi tạo bảng in xem trước hóa đơn quyết toán folio.');
    }
}

/** Chốt thủ tục trả phòng, thực hiện thanh toán, xuất hóa đơn tài chính và chuyển phòng sang dọn dẹp DIRTY (UC18.2). */
async function submitCheckOut() {
    const reservationId = document.getElementById('checkout_reservationId').value;

    try {
        const response = await fetch(`/api/checkout/${reservationId}/confirm`, {
            method: 'POST'
        });

        const result = await response.json();

        if (response.ok) {
            bootstrap.Modal.getInstance(document.getElementById('checkOutModal')).hide();
            showAlert('success', 'Đã hoàn tất thủ tục quyết toán thanh toán và trả phòng thành công!');
            loadReservations();
        } else {
            showAlert('danger', result.message || 'Lỗi quy trình trả phòng trên hệ thống core.');
        }
    } catch (error) {
        console.error('[submitCheckOut] Connection crash:', error);
        showAlert('danger', 'Hệ thống đứt kết nối trong tiến trình chốt hóa đơn.');
    }
}

// ===== TRỢ THỦ ĐỊNH DẠNG DỮ LIỆU CỤ CỤC =====
/** Áp dụng cấu trúc định dạng màu sắc nhãn Badge đồng bộ máy trạng thái luồng lưu trú. */
function getStatusBadge(status) {
    const badges = {
        'PENDING': '<span class="badge bg-warning text-dark"><i class="bi bi-clock me-1"></i>Chờ xác nhận</span>',
        'CONFIRMED': '<span class="badge bg-primary"><i class="bi bi-bookmark-check me-1"></i>Đã đặt trước</span>',
        'CHECKED_IN': '<span class="badge bg-success"><i class="bi bi-door-open me-1"></i>Đang lưu trú</span>',
        'CHECKED_OUT': '<span class="badge bg-secondary"><i class="bi bi-receipt me-1"></i>Checked Out</span>',
        'CANCELLED': '<span class="badge bg-danger"><i class="bi bi-x-circle me-1"></i>Đã hủy đơn</span>'
    };
    return badges[status] || `<span class="badge bg-dark">${status}</span>`;
}

/** Format chuỗi ngày tháng sang dạng hiển thị quốc gia Việt Nam (DD/MM/YYYY). */
function formatDate(dateStr) {
    if (!dateStr) return '-';
    return new Date(dateStr).toLocaleDateString('vi-VN');
}

/** Format dữ liệu số thô sang chuỗi tiền tệ Việt Nam Đồng (VND). */
function formatPrice(price) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(price);
}

/** Kích hoạt hiển thị Banner Alert thông báo nhanh trạng thái nghiệp vụ nổi trên màn hình. */
function showAlert(type, message) {
    const alertBox = document.getElementById('alertBox');
    if (!alertBox) return;

    alertBox.className = `alert alert-${type} position-fixed top-0 start-50 translate-middle-x mt-3 z-3 shadow-lg`;
    alertBox.textContent = message;
    alertBox.classList.remove('d-none');

    setTimeout(() => alertBox.classList.add('d-none'), 3000);
}

// Đăng ký kích hoạt luồng tải dữ liệu mặc định ngay khi cây DOM Client hoàn tất lắp dựng
document.addEventListener('DOMContentLoaded', () => loadReservations());