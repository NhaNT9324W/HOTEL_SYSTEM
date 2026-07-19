using Hotel_System.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hotel_System.Repositories.Interfaces
{
    /**
     * [V.1.4.I IReservationRepository Interface]
     * Giao diện quy định các phương thức thao tác dữ liệu cho thực thể Reservation (Đặt phòng).
     * Phục vụ lõi các nghiệp vụ tiền sảnh: Đặt phòng (UC13), Nhận phòng (UC14), Đổi phòng (UC16) và Trả phòng (UC18).
     */
    public interface IReservationRepository
    {
        /** Lấy toàn bộ danh sách đơn đặt phòng kèm thông tin Guest và Room để hiển thị (UC13.1). */
        Task<IEnumerable<Reservation>> GetAllAsync();

        /** Tìm đơn đặt phòng theo ID kèm đầy đủ liên kết để phục vụ tính toán hóa đơn folio (UC18). */
        Task<Reservation?> GetByIdAsync(int id);

        /** Tìm kiếm đơn đặt phòng theo từ khóa linh hoạt (Tên khách, Số điện thoại, Số phòng). */
        Task<IEnumerable<Reservation>> SearchAsync(string keyword);

        /** Thuật toán kiểm tra trùng lịch đặt phòng (BR-15), ngăn chặn tuyệt đối lỗi Double-booking. */
        Task<bool> HasOverlappingReservationAsync(int roomId, DateTime checkIn, DateTime checkOut, int? excludeId = null);

        /** Thêm mới một đơn đặt phòng và xác nhận lưu trực tiếp xuống cơ cơ dữ liệu (UC13). */
        Task AddAsync(Reservation reservation);

        /** Cập nhật toàn diện thông tin đơn đặt phòng (thay đổi ngày, đổi phòng), ghi vết UpdatedAt. */
        Task UpdateAsync(Reservation reservation);

        /** Cập nhật nhanh trạng thái đơn đặt phòng (CONFIRMED, CHECKED_IN, CANCELED) để tối ưu hiệu năng. */
        Task UpdateStatusAsync(int id, Entities.Enums.ReservationStatus status);
    }
}