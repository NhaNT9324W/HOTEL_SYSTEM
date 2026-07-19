/**
 * [V.2.6.JS Guest Management Frontend]
 * Kịch bản điều khiển giao diện (Client-side) phân hệ Quản lý hồ sơ khách hàng (UC15).
 * Kết nối trực tiếp với API endpoints phân hệ `IGuestService` trên nền tảng .NET Core.
 * Quản lý toàn bộ vòng đời thông tin khách lưu trú, tối ưu hóa tra cứu và hỗ trợ đồng bộ dữ liệu Tiền sảnh.
 */

// ===== TRẠNG THÁI TOÀN CỤC PHÂN HỆ =====
let editingGuestId = null;

// ===== LOAD DATA & RENDERING =====
/**
 * Tải danh sách hồ sơ khách hàng từ Server.
 * Hỗ trợ lọc tìm kiếm động thời gian thực thông qua query tham số (UC15).
 * @param {string} search - Từ khóa tra cứu nhanh (Họ tên hoặc Số điện thoại).
 */
async function loadGuests(search = "") {
    try {
        const url = `/api/guests?search=${encodeURIComponent(search)}`;
        const res = await fetch(url);

        if (!res.ok) throw new Error("Không thể kết nối tới cơ sở dữ liệu khách hàng.");

        const data = await res.json();
        renderTable(data);
    } catch (err) {
        console.error("[loadGuests] Root cause analysis:", err);
        alert("Gặp sự cố hệ thống khi tải danh sách hồ sơ khách hàng.");
    }
}

/**
 * Kết xuất cấu trúc danh sách rút gọn GuestListDto lên lưới bảng HTML.
 * @param {Array} list - Mảng danh sách các thực thể hồ sơ khách hàng.
 */
function renderTable(list) {
    const tbody = document.querySelector("#guestTable tbody");
    if (!tbody) return;

    tbody.innerHTML = "";

    if (list.length === 0) {
        tbody.innerHTML = `<tr><td colspan="2" class="text-center text-muted py-3">Không tìm thấy thông tin khách hàng phù hợp</td></tr>`;
        return;
    }

    list.forEach(g => {
        const row = document.createElement("tr");
        row.style.cursor = "pointer";
        row.className = "align-middle target-row";
        row.innerHTML = `
            <td class="fw-bold">${g.fullName}</td>
            <td><i class="bi bi-telephone text-muted me-2"></i>${g.phone}</td>
        `;
        // Đăng ký sự kiện xem chi tiết hồ sơ khi click vào dòng tương ứng
        row.onclick = () => openDetailCard(g.id);
        tbody.appendChild(row);
    });
}

// ===== CREATE GUEST PROFILE (UC15.1) =====
/** Kích hoạt cấu trúc Form trống tại Panel chi tiết phục vụ tạo mới hồ sơ. */
function openCreateForm() {
    editingGuestId = null;

    const container = document.getElementById("detailCardBody");
    if (!container) return;

    container.innerHTML = `
        <div class="mb-3">
            <label class="form-label fw-bold">Họ và tên <span class="text-danger">*</span></label>
            <input id="editFullName" class="form-control" placeholder="Nhập tên khách hàng..." />
            <div id="error_fullName" class="text-danger small mt-1"></div>
        </div>
        <div class="mb-3">
            <label class="form-label fw-bold">Số điện thoại <span class="text-danger">*</span></label>
            <input id="editPhone" class="form-control" type="tel" placeholder="Nhập số điện thoại liên hệ..." />
            <div id="error_phone" class="text-danger small mt-1"></div>
        </div>
        <div class="mb-3">
            <label class="form-label fw-bold">Số CCCD / Passport <span class="text-danger">*</span></label>
            <input id="editIdNumber" class="form-control" placeholder="Nhập số căn cước hoặc hộ chiếu..." />
            <div id="error_idNumber" class="text-danger small mt-1"></div>
        </div>
        <div class="mb-3">
            <label class="form-label fw-bold">Địa chỉ Email</label>
            <input id="editEmail" class="form-control" type="email" placeholder="example@hotel.com" />
        </div>
        <div class="d-flex justify-content-end gap-2 mt-4">
            <button class="btn btn-secondary" onclick="closeDetailCard()">Hủy bỏ</button>
            <button class="btn btn-success" onclick="submitCreate()"><i class="bi bi-check-circle me-1"></i>Tạo mới</button>
        </div>
    `;

    document.getElementById("detailCard").style.display = "block";
}

/** Gửi yêu cầu thêm mới dữ liệu khách hàng lên Server qua phương thức HTTP POST. */
async function submitCreate() {
    const fullNameInput = document.getElementById("editFullName");
    const phoneInput = document.getElementById("editPhone");
    const idNumberInput = document.getElementById("editIdNumber");
    const emailInput = document.getElementById("editEmail");

    const fullName = fullNameInput.value.trim();
    const phone = phoneInput.value.trim();
    const idNumber = idNumberInput.value.trim();
    const email = emailInput.value.trim() || null;

    // Tiến hành validate dữ liệu nghiêm ngặt phía Client
    let isValid = true;
    if (!fullName) { document.getElementById("error_fullName").textContent = "Họ tên không được để trống"; isValid = false; }
    else document.getElementById("error_fullName").textContent = "";

    if (!phone) { document.getElementById("error_phone").textContent = "Số điện thoại bắt buộc nhập"; isValid = false; }
    else document.getElementById("error_phone").textContent = "";

    if (!idNumber) { document.getElementById("error_idNumber").textContent = "Số CCCD / Passport bắt buộc nhập"; isValid = false; }
    else document.getElementById("error_idNumber").textContent = "";

    if (!isValid) return;

    const body = { fullName, phone, idNumber, email };

    try {
        const res = await fetch("/api/guests", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });

        const result = await res.json();

        if (res.ok) {
            alert("Khởi tạo hồ sơ khách hàng thành công!");
            closeDetailCard();
            loadGuests(document.getElementById("searchBox").value);
        } else {
            alert("Lỗi hệ thống: " + (result.message || "Dữ liệu gửi lên không hợp lệ."));
        }
    } catch (err) {
        console.error("[submitCreate] Exception:", err);
        alert("Hệ thống gặp lỗi trong quá trình thêm mới khách hàng.");
    }
}

// ===== EDIT & UPDATE PROFILE =====
/** Gửi yêu cầu đồng bộ hiệu chỉnh thông tin hồ sơ lên endpoint thông qua HTTP PUT. */
async function submitUpdate() {
    if (!editingGuestId) return;

    const fullName = document.getElementById("editFullName").value.trim();
    const phone = document.getElementById("editPhone").value.trim();
    const idNumber = document.getElementById("editIdNumber").value.trim();
    const email = document.getElementById("editEmail").value.trim() || null;

    if (!fullName || !phone || !idNumber) {
        alert("Vui lòng nhập đầy đủ các thông tin bắt buộc (*)");
        return;
    }

    const body = { fullName, phone, idNumber, email };

    try {
        const res = await fetch(`/api/guests/${editingGuestId}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });

        const result = await res.json();
        alert(result.message);

        if (res.ok) {
            closeDetailCard();
            loadGuests(document.getElementById("searchBox").value);
        }
    } catch (err) {
        console.error("[submitUpdate] Exception:", err);
        alert("Gặp sự cố khi cập nhật hồ sơ khách hàng.");
    }
}

// ===== SOFT DELETE GUEST PROFILE =====
/** Gửi yêu cầu ẩn/xóa mềm hồ sơ khách hàng dựa theo cơ chế thiết lập an toàn DB. */
async function submitDelete() {
    if (!editingGuestId) return;
    if (!confirm("Xác nhận ngắt kích hoạt và xóa mềm hồ sơ khách hàng này? (Lịch sử đặt phòng vẫn được bảo toàn)")) return;

    try {
        const res = await fetch(`/api/guests/${editingGuestId}`, { method: "DELETE" });
        const result = await res.json();
        alert(result.message);

        if (res.ok) {
            closeDetailCard();
            loadGuests(document.getElementById("searchBox").value);
        }
    } catch (err) {
        console.error("[submitDelete] Exception:", err);
        alert("Xảy ra lỗi hệ thống trong quá trình thực thi xóa.");
    }
}

// ===== DETAILS VIEW PANEL =====
/**
 * Truy xuất sâu dữ liệu chi tiết GuestDetailDto từ Server và hiển thị lên Form.
 * @param {number} id - Mã định danh Primary Key khách hàng cần xem thông tin.
 */
async function openDetailCard(id) {
    try {
        const res = await fetch(`/api/guests/${id}`);
        if (!res.ok) throw new Error("Không thể trích xuất hồ sơ chi tiết.");

        const d = await res.json();
        editingGuestId = d.id;

        const container = document.getElementById("detailCardBody");
        if (!container) return;

        // Cập nhật cấu trúc DOM động cho biểu mẫu chi tiết và lịch sử
        container.innerHTML = `
            <div class="mb-3">
                <label class="form-label fw-bold">Họ và tên <span class="text-danger">*</span></label>
                <input id="editFullName" class="form-control" value="${d.fullName}" />
            </div>
            <div class="mb-3">
                <label class="form-label fw-bold">Số điện thoại <span class="text-danger">*</span></label>
                <input id="editPhone" class="form-control" value="${d.phone}" />
            </div>
            <div class="mb-3">
                <label class="form-label fw-bold">Số CCCD / Passport <span class="text-danger">*</span></label>
                <input id="editIdNumber" class="form-control" value="${d.idNumber}" />
            </div>
            <div class="mb-3">
                <label class="form-label fw-bold">Địa chỉ Email</label>
                <input id="editEmail" class="form-control" value="${d.email ?? ""}" />
            </div>
             
            <div class="card bg-light border-0 my-3">
                <div class="card-body p-3 text-secondary small">
                    <div class="d-flex justify-content-between mb-1">
                        <span><i class="bi bi-calendar-check me-2"></i>Tổng lượt đặt phòng:</span>
                        <span class="fw-bold text-dark">${d.totalReservations} lượt</span>
                    </div>
                    <div class="d-flex justify-content-between">
                        <span><i class="bi bi-clock-history me-2"></i>Ngày tạo hồ sơ:</span>
                        <span class="fw-bold text-dark">${new Date(d.createdAt).toLocaleDateString("vi-VN")}</span>
                    </div>
                </div>
            </div>

            <div class="d-flex justify-content-between mt-4">
                <button class="btn btn-outline-danger" onclick="submitDelete()"><i class="bi bi-trash me-1"></i>Xóa hồ sơ</button>
                <div class="d-flex gap-2">
                    <button class="btn btn-secondary" onclick="closeDetailCard()">Đóng</button>
                    <button class="btn btn-success" onclick="submitUpdate()"><i class="bi bi-save me-1"></i>Lưu thay đổi</button>
                </div>
            </div>
        `;

        document.getElementById("detailCard").style.display = "block";
    } catch (err) {
        console.error("[openDetailCard] Exception:", err);
        alert("Không tìm thấy thông tin chi tiết của khách hàng này.");
    }
}

/** Đóng Panel chi tiết và giải phóng biến lưu trữ con trỏ ID. */
function closeDetailCard() {
    const card = document.getElementById("detailCard");
    if (card) card.style.display = "none";  
    editingGuestId = null;
}

// ===== BẬT LẮNG NGHE SỰ KIỆN HỆ THỐNG =====
document.addEventListener("DOMContentLoaded", () => {
    const searchBox = document.getElementById("searchBox");
    if (searchBox) {
        // Áp dụng cơ chế bắt sự kiện Input để tự động lọc kết quả thời gian thực
        searchBox.addEventListener("input", (e) => loadGuests(e.target.value));
    }

    // Kích hoạt tiến trình tải dữ liệu ban đầu
    loadGuests();
});