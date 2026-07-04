async function loadReservations(search = "") {
    try {
        const res = await fetch(`/api/reservations?search=${encodeURIComponent(search)}`);
        if (!res.ok) throw new Error("Load thất bại");
        const data = await res.json();
        renderTable(data);
    } catch (err) {
        console.error("[loadReservations] Root cause:", err);
    }
}

function renderTable(list) {
    const tbody = document.querySelector("#reservationTable tbody");
    tbody.innerHTML = "";
    list.forEach(r => {
        const row = document.createElement("tr");
        row.style.cursor = "pointer";
        row.innerHTML = `<td>${r.roomNumber}</td><td>${r.guestName}</td><td>${new Date(r.checkInDate).toLocaleDateString()}</td><td>${new Date(r.checkOutDate).toLocaleDateString()}</td><td>${r.status}</td>`;
        row.onclick = () => openDetailCard(r.id);
        tbody.appendChild(row);
    });
}

async function openDetailCard(id) {
    try {
        const res = await fetch(`/api/reservations/${id}`);
        if (!res.ok) throw new Error("Không tìm thấy reservation");
        const d = await res.json();
        document.getElementById("detailCardBody").innerHTML = `
            <p><b>Phòng:</b> ${d.roomNumber} (${d.roomTypeName})</p>
            <p><b>Khách:</b> ${d.guestName} - ${d.guestPhone} - ${d.guestEmail ?? ""}</p>
            <p><b>Check-in:</b> ${new Date(d.checkInDate).toLocaleString()}</p>
            <p><b>Check-out:</b> ${new Date(d.checkOutDate).toLocaleString()}</p>
            <p><b>Trạng thái:</b> ${d.status}</p>
            <p><b>Tạo lúc:</b> ${new Date(d.createdAt).toLocaleString()}</p>
        `;
        document.getElementById("detailCard").style.display = "block";
    } catch (err) {
        console.error("[openDetailCard] Root cause:", err);
    }
}

function closeDetailCard() {
    document.getElementById("detailCard").style.display = "none";
}

document.getElementById("searchBox").addEventListener("input", (e) => loadReservations(e.target.value));

window.onload = () => loadReservations();