using Hotel_System.Data;
using Hotel_System.Entities;
using Hotel_System.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;

namespace Hotel_System.Repositories.Implementations
{
    /**
     * [V.1.3 HotelInfoRepository Implementation]
     * Lớp triển khai các phương thức tương tác với cơ sở dữ liệu cho thực thể HotelInfo (Thông tin cấu hình khách sạn).
     * Áp dụng mẫu kiến trúc Repository Pattern nhằm cô lập logic truy xuất dữ liệu lớp nền và quản lý bản ghi cấu hình hệ thống.
     * Dữ liệu từ thực thể này phục vụ trực tiếp cho chức năng Quản lý thông tin khách sạn (UC06), đồng thời cung cấp thông tin gốc 
     * (Tên khách sạn, Địa chỉ, Mã số thuế, Hotline) để in lên tiêu đề các chứng từ tài chính khi xuất Hóa đơn (UC18).
     */
    public class HotelInfoRepository : IHotelInfoRepository
    {
        private readonly AppDbContext _context;

        /** Inject dependency AppDbContext thông qua Constructor nhằm quản lý vòng đời kết nối DB Context (Scoped Lifetime). */
        public HotelInfoRepository(AppDbContext context) => _context = context;

        /** 
         * Truy xuất bản ghi thông tin cấu hình duy nhất của khách sạn trong cơ sở dữ liệu.
         * Sử dụng `FirstOrDefaultAsync` vì theo thiết kế hệ thống và Business Rule, bảng này chỉ chứa duy nhất một dòng dữ liệu 
         * đại diện cho chính tổ chức khách sạn đang vận hành phần mềm này.
         */
        public async Task<HotelInfo?> GetAsync() =>
            await _context.HotelInfos.FirstOrDefaultAsync();

        /** 
         * Cập nhật các thông tin thay đổi về hồ sơ pháp lý, thông tin liên hệ hoặc logo của khách sạn (UC06).
         * Trước khi thực hiện đồng bộ, hệ thống tự động thiết lập thuộc tính hệ thống `UpdatedAt` theo chuẩn thời gian UTC 
         * nhằm phục vụ mục đích kiểm toán dữ liệu lịch sử (Data Auditing) trước khi lưu xuống cơ sở dữ liệu vật lý.
         */
        public async Task UpdateAsync(HotelInfo hotelInfo)
        {
            hotelInfo.UpdatedAt = DateTime.UtcNow;
            _context.HotelInfos.Update(hotelInfo);
            await _context.SaveChangesAsync();
        }
    }
}