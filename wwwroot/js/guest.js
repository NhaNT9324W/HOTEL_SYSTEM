let editingGuestId = null;

async function loadGuests(search = "") {
    try {
        const res = await fetch(`/api/guests?search=${encodeURIComponent(search)}`);
        if (!res.ok) throw new Error("Load thất bại");
        renderTable(await res.json());
    } catch (err) {
        console.error("[loadGuests] Root cause:", err);
    }
}

function renderTable(list) {
    const tbody = document.querySelector("#guestTable tbody");
    tbody.innerHTML = "";
    list.forEach(g => {
        const row = document.createElement("tr");
        row.style.cursor = "pointer";
        row.innerHTML = `<td>${g.fullName}</td><td>${g.phone}</td>`;
        row.onclick = () => openDetailCard(g.id);
        tbody.appendChild(row);
    });
}

async function openDetailCard(id) {
    try {
        const res = await fetch(`/api/guests/${id}`);
        if (!res.ok) throw new Error("Không tìm thấy khách hàng");
        const d = await res.json();
        editingGuestId = d.id;
        document.getElementById("detailCardBody").innerHTML = `
            <p><b>Họ tên:</b> <input id="editFullName" class="form-control" value="${d.fullName}" /></p>
            <p><b>SĐT:</b> <input id="editPhone" class="form-control" value="${d.phone}" /></p>
            <p><b>Email:</b> <input id="editEmail" class="form-control" value="${d.email ?? ""}" /></p>
            <p><b>Tổng số đặt phòng:</b> ${d.totalReservations}</p>
            <p><b>Tạo lúc:</b> ${new Date(d.createdAt).toLocaleString()}</p>
            <button class="btn btn-success" onclick="submitUpdate()">Lưu</button>
            <button class="btn btn-danger" onclick="submitDelete()">Xóa</button>
            <button class="btn btn-secondary" onclick="closeDetailCard()">Đóng</button>
        `;
        document.getElementById("detailCard").style.display = "block";
    } catch (err) {
        console.error("[openDetailCard] Root cause:", err);
    }
}

function openCreateForm() {
    editingGuestId = null;
    document.getElementById("detailCardBody").innerHTML = `
        <p><b>Họ tên:</b> <input id="editFullName" class="form-control" /></p>
        <p><b>SĐT:</b> <input id="editPhone" class="form-control" /></p>
        <p><b>Email:</b> <input id="editEmail" class="form-control" /></p>
        <button class="btn btn-success" onclick="submitCreate()">Tạo mới</button>
        <button class="btn btn-secondary" onclick="closeDetailCard()">Đóng</button>
    `;
    document.getElementById("detailCard").style.display = "block";
}

async function submitCreate() {
    const body = {
        fullName: document.getElementById("editFullName").value,
        phone: document.getElementById("editPhone").value,
        email: document.getElementById("editEmail").value || null
    };
    try {
        const res = await fetch("/api/guests", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });
        const result = await res.json();
        alert(result.message);
        if (res.ok) { closeDetailCard(); loadGuests(); }
    } catch (err) {
        console.error("[submitCreate] Root cause:", err);
    }
}

async function submitUpdate() {
    const body = {
        fullName: document.getElementById("editFullName").value,
        phone: document.getElementById("editPhone").value,
        email: document.getElementById("editEmail").value || null
    };
    try {
        const res = await fetch(`/api/guests/${editingGuestId}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify(body)
        });
        const result = await res.json();
        alert(result.message);
        if (res.ok) { closeDetailCard(); loadGuests(); }
    } catch (err) {
        console.error("[submitUpdate] Root cause:", err);
    }
}

async function submitDelete() {
    if (!confirm("Xác nhận xóa khách hàng này?")) return;
    try {
        const res = await fetch(`/api/guests/${editingGuestId}`, { method: "DELETE" });
        const result = await res.json();
        alert(result.message);
        if (res.ok) { closeDetailCard(); loadGuests(); }
    } catch (err) {
        console.error("[submitDelete] Root cause:", err);
    }
}

function closeDetailCard() {
    document.getElementById("detailCard").style.display = "none";
    editingGuestId = null;
}

document.getElementById("searchBox").addEventListener("input", (e) => loadGuests(e.target.value));
window.onload = () => loadGuests();