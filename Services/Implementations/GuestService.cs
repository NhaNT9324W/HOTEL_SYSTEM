using Hotel_System.DTOs;
using Hotel_System.Entities;
using Hotel_System.Repositories.Interfaces;
using Hotel_System.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hotel_System.Services.Implementations
{
    /**
     * [V.2.6 GuestService Implementation]
     * Lớp xử lý nghiệp vụ quản lý hồ sơ và lịch sử khách lưu trú.
     * Phục vụ trực tiếp phân hệ Quản lý hồ sơ khách hàng (UC15) và hỗ trợ thông tin gốc cho quy trình Đặt phòng (UC13).
     */
    public class GuestService : IGuestService
    {
        private readonly IGuestRepository _repo;
        private readonly ILogger<GuestService> _logger;

        /** Inject các dịch vụ phụ thuộc về dữ liệu khách hàng và ghi vết hệ thống qua Constructor. */
        public GuestService(IGuestRepository repo, ILogger<GuestService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        /** Lấy danh sách khách hàng gọn nhẹ chưa xóa mềm, phục vụ bộ lọc tra cứu nhanh tại giao diện (UC15). */
        public async Task<List<GuestListDto>> GetAllAsync(string? search)
        {
            var list = await _repo.GetAllAsync(search);
            return list.Select(g => new GuestListDto
            {
                Id = g.Id,
                FullName = g.FullName,
                Phone = g.Phone
            }).ToList();
        }

        /** Xem thông tin hồ sơ chi tiết của khách kèm theo tổng số lượt đặt phòng lịch sử để đánh giá độ thân thiết. */
        public async Task<GuestDetailDto?> GetDetailAsync(int id)
        {
            var g = await _repo.GetByIdAsync(id);
            if (g == null) return null;

            var totalReservations = await _repo.CountReservationsAsync(id);

            return new GuestDetailDto
            {
                Id = g.Id,
                FullName = g.FullName,
                Phone = g.Phone,
                IdNumber = g.IdNumber, // 🔥 BỔ SUNG: Ánh xạ CCCD lên giao diện khi bấm vào xem chi tiết
                Email = g.Email,
                CreatedAt = g.CreatedAt,
                TotalReservations = totalReservations
            };
        }

        /** 
         * Khởi tạo hồ sơ khách hàng mới (UC15.1).
         * Ràng buộc nghiệp vụ: Kiểm tra trùng lặp Số điện thoại toàn hệ thống nhằm ngăn chặn dữ liệu rác.
         */
        public async Task<(bool Success, string Message)> CreateAsync(CreateGuestDto dto)
        {
            try
            {
                var existing = await _repo.GetByPhoneAsync(dto.Phone);
                if (existing != null)
                {
                    _logger.LogWarning("Guest creation rejected: duplicate phone {Phone}", dto.Phone);
                    return (false, $"Số điện thoại {dto.Phone} đã tồn tại (khách: {existing.FullName}).");
                }

                // Cần đảm bảo không trùng số CCCD nếu hệ thống yêu cầu duy nhất
                var guest = new Guest
                {
                    FullName = dto.FullName,
                    Phone = dto.Phone,
                    IdNumber = dto.IdNumber, // 🔥 BỔ SUNG: Ánh xạ CCCD từ DTO vào thực thể để lưu xuống Database
                    Email = dto.Email,
                    CreatedAt = DateTime.Now,
                    IsDeleted = false
                };

                await _repo.AddAsync(guest);
                await _repo.SaveChangesAsync();

                return (true, "Tạo khách hàng thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error while creating guest Phone={Phone}", dto.Phone);
                return (false, "Có lỗi hệ thống xảy ra, vui lòng thử lại.");
            }
        }

        /** Cập nhật thông tin khách hàng, thực hiện đối chiếu trùng SĐT và loại trừ mã định danh của chính khách hàng hiện tại. */
        public async Task<(bool Success, string Message)> UpdateAsync(int id, UpdateGuestDto dto)
        {
            try
            {
                var guest = await _repo.GetByIdAsync(id);
                if (guest == null)
                    return (false, "Không tìm thấy khách hàng.");

                var duplicate = await _repo.GetByPhoneAsync(dto.Phone, excludeId: id);
                if (duplicate != null)
                {
                    _logger.LogWarning("Guest update rejected: phone {Phone} used by GuestId={OtherId}", dto.Phone, duplicate.Id);
                    return (false, $"Số điện thoại {dto.Phone} đã được dùng bởi khách khác.");
                }

                guest.FullName = dto.FullName;
                guest.Phone = dto.Phone;
                guest.IdNumber = dto.IdNumber; // 🔥 BỔ SUNG: Cho phép đồng bộ cập nhật lại số CCCD/Passport mới
                guest.Email = dto.Email;

                _repo.Update(guest);
                await _repo.SaveChangesAsync();

                return (true, "Cập nhật thông tin khách hàng thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error while updating guest Id={Id}", id);
                return (false, "Có lỗi hệ thống xảy ra, vui lòng thử lại.");
            }
        }

        /** 
         * Áp dụng cơ chế Xóa mềm (Soft Delete) hồ sơ khách hàng ra khỏi danh mục hiển thị.
         * Đảm bảo không phá vỡ ràng buộc khóa ngoại (Referential Integrity) với lịch sử đặt phòng và kế toán Folio trước đó.
         */
        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            try
            {
                var guest = await _repo.GetByIdAsync(id);
                if (guest == null)
                    return (false, "Không tìm thấy khách hàng.");

                if (guest.IsDeleted)
                    return (false, "Khách hàng này đã được xóa trước đó.");

                var totalReservations = await _repo.CountReservationsAsync(id);

                _repo.SoftDelete(guest);
                await _repo.SaveChangesAsync();

                _logger.LogInformation("Guest Id={Id} soft-deleted with {Count} linked reservations", id, totalReservations);

                return (true, totalReservations > 0
                    ? $"Đã xóa khách hàng (lưu ý: khách có {totalReservations} lịch sử đặt phòng vẫn được giữ lại để báo cáo)."
                    : "Xóa khách hàng thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error while soft-deleting guest Id={Id}", id);
                return (false, "Có lỗi hệ thống xảy ra, vui lòng thử lại.");
            }
        }
    }
}