using Hotel_System.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hotel_System.Services.Interfaces
{
    /**
     * [V.2.9.I IReservationService Interface]
     * Giao diện quy định các phương thức xử lý nghiệp vụ lõi điều phối vòng đời lưu trú của khách hàng.
     * Làm hợp đồng cho ReservationService thực thi, quản lý chặt chẽ luồng Đặt phòng (UC13), Nhận phòng (UC14) và giải phóng phòng vật lý.
     */
    public interface IReservationService
    {
        /** Lấy toàn bộ danh sách đơn đặt phòng dưới dạng DTO rút gọn, phục vụ màn hình tổng quan tiền sảnh (UC13.1). */
        Task<IEnumerable<ReservationListDto>> GetAllAsync();

        /** Tra cứu thông tin cấu trúc chi tiết của một đơn đặt phòng cụ thể dựa trên ID phục vụ thủ tục Check-in hoặc Folio kế toán. */
        Task<ReservationDetailDto?> GetByIdAsync(int id);

        /** Tìm kiếm đơn đặt phòng theo từ khóa linh hoạt (Tên khách, Số điện thoại, Số phòng) tại bộ lọc tìm nhanh. */
        Task<IEnumerable<ReservationListDto>> SearchAsync(string keyword);

        /** 
         * Thuật toán lọc tra cứu danh sách phòng trống khả dụng thời gian thực (Real-time Availability).
         * Thực hiện đối chiếu trùng lịch đặt phòng vật lý, trạng thái phòng sẵn sàng đón khách (READY) và không vướng sự cố kỹ thuật bảo trì.
         */
        Task<IEnumerable<object>> GetAvailableRoomsAsync(DateTime checkIn, DateTime checkOut, int? roomTypeId = null);

        /** 
         * Xử lý tạo mới một đơn đặt phòng tại quầy Tiền sảnh (UC13).
         * Áp dụng các ràng buộc nghiệp vụ về thời gian lưu trú, chặn double-booking và tự động đồng bộ hồ sơ khách hàng (Guest Folio).
         */
        Task CreateAsync(CreateReservationDto dto);

        /** Hiệu chỉnh thông tin, thay đổi thời gian hoặc chuyển đổi phòng vật lý trước khi khách làm thủ tục Check-in. */
        Task UpdateAsync(int id, UpdateReservationDto dto);

        /** Hủy bỏ đơn đặt phòng (CANCELED) phục vụ luồng xử lý yêu cầu hủy của khách, đảm bảo điều kiện trạng thái lưu trú hợp lệ. */
        Task CancelAsync(int id);

        /** 
         * Quy trình làm thủ tục Nhận phòng chính thức (UC14 - Check-in).
         * Kích hoạt xác nhận trạng thái đơn đặt sang CHECKED_IN và đồng thời chuyển BookingStatus của phòng vật lý sang OCCUPIED.
         */
        Task CheckInAsync(int reservationId);
    }
}