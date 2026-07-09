// ===== SET DEFAULT DATES =====
document.addEventListener('DOMContentLoaded', () => {
    const today = new Date();
    const firstDay = new Date(today.getFullYear(), today.getMonth(), 1);

    document.getElementById('fromDate').value = firstDay.toISOString().split('T')[0];
    document.getElementById('toDate').value = today.toISOString().split('T')[0];
});

// ===== GENERATE REPORT =====
async function generateReport() {
    const fromDate = document.getElementById('fromDate').value;
    const toDate = document.getElementById('toDate').value;
    const reportType = document.getElementById('reportType').value;

    if (!fromDate || !toDate) {
        showAlert('warning', 'Please select date range');
        return;
    }

    if (fromDate > toDate) {
        showAlert('warning', 'From date must be before to date');
        return;
    }

    // Hide all reports
    hideAllReports();

    const response = await fetch(`/api/reports/${reportType}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ fromDate, toDate })
    });

    if (!response.ok) {
        showAlert('danger', 'Failed to generate report');
        return;
    }

    const data = await response.json();
    document.getElementById('reportContent').classList.remove('d-none');

    switch (reportType) {
        case 'occupancy': renderOccupancy(data); break;
        case 'revenue': renderRevenue(data); break;
        case 'financial': renderFinancial(data); break;
        case 'staffperformance': renderStaffPerformance(data); break;
    }
}

// ===== HIDE ALL REPORTS =====
function hideAllReports() {
    ['occupancyReport', 'revenueReport', 'financialReport', 'staffReport']
        .forEach(id => document.getElementById(id).classList.add('d-none'));
}

// ===== RENDER OCCUPANCY =====
function renderOccupancy(data) {
    document.getElementById('occupancyReport').classList.remove('d-none');
    document.getElementById('occ_totalRooms').textContent = data.totalRooms;
    document.getElementById('occ_occupiedRooms').textContent = data.occupiedRooms;
    document.getElementById('occ_rate').textContent = `${data.occupancyRate}%`;

    document.getElementById('occ_tableBody').innerHTML = data.details.length === 0
        ? `<tr><td colspan="5" class="text-center text-muted">No data</td></tr>`
        : data.details.map((d, i) => `
            <tr>
                <td>${i + 1}</td>
                <td>Room ${d.roomNumber}</td>
                <td>${d.roomTypeName}</td>
                <td>${d.totalNights} nights</td>
                <td>${getStatusBadge(d.status)}</td>
            </tr>
        `).join('');
}

// ===== RENDER REVENUE =====
function renderRevenue(data) {
    document.getElementById('revenueReport').classList.remove('d-none');
    document.getElementById('rev_totalRevenue').textContent = formatPrice(data.totalRevenue);
    document.getElementById('rev_totalReservations').textContent = data.totalReservations;
    document.getElementById('rev_avgRevenue').textContent = formatPrice(data.averageRevenuePerReservation);

    document.getElementById('rev_tableBody').innerHTML = data.details.length === 0
        ? `<tr><td colspan="7" class="text-center text-muted">No data</td></tr>`
        : data.details.map((d, i) => `
            <tr>
                <td>${i + 1}</td>
                <td>${d.guestName}</td>
                <td>Room ${d.roomNumber}</td>
                <td>${formatDate(d.checkInDate)}</td>
                <td>${formatDate(d.checkOutDate)}</td>
                <td>${d.nights} nights</td>
                <td>${formatPrice(d.revenue)}</td>
            </tr>
        `).join('');
}

// ===== RENDER FINANCIAL =====
function renderFinancial(data) {
    document.getElementById('financialReport').classList.remove('d-none');
    document.getElementById('fin_totalRevenue').textContent = formatPrice(data.totalRevenue);
    document.getElementById('fin_totalReservations').textContent = data.totalReservations;
    document.getElementById('fin_cancelledReservations').textContent = data.cancelledReservations;
    document.getElementById('fin_roomRevenue').textContent = formatPrice(data.roomRevenue);

    document.getElementById('fin_tableBody').innerHTML = data.details.length === 0
        ? `<tr><td colspan="4" class="text-center text-muted">No data</td></tr>`
        : data.details.map((d, i) => `
            <tr>
                <td>${i + 1}</td>
                <td>${d.roomTypeName}</td>
                <td>${d.totalReservations}</td>
                <td>${formatPrice(d.revenue)}</td>
            </tr>
        `).join('');
}

// ===== RENDER STAFF PERFORMANCE =====
function renderStaffPerformance(data) {
    document.getElementById('staffReport').classList.remove('d-none');
    document.getElementById('staff_totalStaff').textContent = data.totalStaff;
    document.getElementById('staff_totalCompleted').textContent = data.totalTasksCompleted;

    document.getElementById('staff_tableBody').innerHTML = data.details.length === 0
        ? `<tr><td colspan="8" class="text-center text-muted">No data</td></tr>`
        : data.details.map((d, i) => `
            <tr>
                <td>${i + 1}</td>
                <td>${d.staffName}</td>
                <td>${d.role}</td>
                <td>${d.totalTasks}</td>
                <td><span class="badge bg-success">${d.completedTasks}</span></td>
                <td><span class="badge bg-warning text-dark">${d.pendingTasks}</span></td>
                <td><span class="badge bg-primary">${d.inProgressTasks}</span></td>
                <td>
                    <div class="progress" style="height: 20px;">
                        <div class="progress-bar bg-success" style="width: ${d.completionRate}%">
                            ${d.completionRate}%
                        </div>
                    </div>
                </td>
            </tr>
        `).join('');
}

// ===== EXPORT EXCEL =====
async function exportReport() {
    const fromDate = document.getElementById('fromDate').value;
    const toDate = document.getElementById('toDate').value;
    const reportType = document.getElementById('reportType').value;

    if (!fromDate || !toDate) {
        showAlert('warning', 'Please select date range');
        return;
    }

    const url = `/api/reports/export?type=${reportType}&fromDate=${fromDate}&toDate=${toDate}`;
    window.location.href = url;
}

// ===== HELPERS =====
function getStatusBadge(status) {
    const badges = {
        'PENDING': '<span class="badge bg-warning text-dark">Pending</span>',
        'CONFIRMED': '<span class="badge bg-primary">Confirmed</span>',
        'CHECKED_IN': '<span class="badge bg-success">Checked In</span>',
        'CHECKED_OUT': '<span class="badge bg-secondary">Checked Out</span>',
        'CANCELLED': '<span class="badge bg-danger">Cancelled</span>'
    };
    return badges[status] || status;
}

function formatDate(dateStr) {
    return new Date(dateStr).toLocaleDateString('vi-VN');
}

function formatPrice(price) {
    return new Intl.NumberFormat('vi-VN', {
        style: 'currency',
        currency: 'VND'
    }).format(price);
}

function showAlert(type, message) {
    const alertBox = document.getElementById('alertBox');
    alertBox.className = `alert alert-${type}`;
    alertBox.textContent = message;
    alertBox.classList.remove('d-none');
    setTimeout(() => alertBox.classList.add('d-none'), 3000);
}