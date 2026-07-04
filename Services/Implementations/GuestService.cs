using Hotel_System.DTOs;
using Hotel_System.Entities;
using Hotel_System.Repositories.Interfaces;
using Hotel_System.Services.Interfaces;

namespace Hotel_System.Services.Implementations
{
    public class GuestService : IGuestService
    {
        private readonly IGuestRepository _repo;
        private readonly ILogger<GuestService> _logger;

        public GuestService(IGuestRepository repo, ILogger<GuestService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

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
                Email = g.Email,
                CreatedAt = g.CreatedAt,
                TotalReservations = totalReservations
            };
        }

        public async Task<(bool Success, string Message)> CreateAsync(CreateGuestDto dto)
        {
            try
            {
                // Root Cause Gate: chống trùng SĐT (UC15.1)
                var existing = await _repo.GetByPhoneAsync(dto.Phone);
                if (existing != null)
                {
                    _logger.LogWarning("Guest creation rejected: duplicate phone {Phone}", dto.Phone);
                    return (false, $"Số điện thoại {dto.Phone} đã tồn tại (khách: {existing.FullName}).");
                }

                var guest = new Guest
                {
                    FullName = dto.FullName,
                    Phone = dto.Phone,
                    Email = dto.Email
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

        public async Task<(bool Success, string Message)> UpdateAsync(int id, UpdateGuestDto dto)
        {
            try
            {
                var guest = await _repo.GetByIdAsync(id);
                if (guest == null)
                    return (false, "Không tìm thấy khách hàng.");

                // Root Cause Gate: chống trùng SĐT với khách khác
                var duplicate = await _repo.GetByPhoneAsync(dto.Phone, excludeId: id);
                if (duplicate != null)
                {
                    _logger.LogWarning("Guest update rejected: phone {Phone} used by GuestId={OtherId}", dto.Phone, duplicate.Id);
                    return (false, $"Số điện thoại {dto.Phone} đã được dùng bởi khách khác.");
                }

                guest.FullName = dto.FullName;
                guest.Phone = dto.Phone;
                guest.Email = dto.Email;

                _repo.Update(guest);
                await _repo.SaveChangesAsync();

                return (true, "Cập nhật khách hàng thành công.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error while updating guest Id={Id}", id);
                return (false, "Có lỗi hệ thống xảy ra, vui lòng thử lại.");
            }
        }

        public async Task<(bool Success, string Message)> DeleteAsync(int id)
        {
            try
            {
                var guest = await _repo.GetByIdAsync(id);
                if (guest == null)
                    return (false, "Không tìm thấy khách hàng.");

                if (guest.IsDeleted)
                    return (false, "Khách hàng này đã được xóa trước đó.");

                // Root Cause Gate: cảnh báo nếu có reservation, nhưng vẫn cho soft-delete
                // (khác hard-delete: soft-delete không phá vỡ toàn vẹn dữ liệu vì record vẫn tồn tại)
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