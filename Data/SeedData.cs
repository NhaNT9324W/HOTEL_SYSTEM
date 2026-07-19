using Hotel_System.Entities;
using Hotel_System.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Hotel_System.Data
{
    /**
     * [System Initialization - SeedData]
     * Lớp tĩnh chịu trách nhiệm khởi tạo và cấu hình dữ liệu mẫu mặc định (Seed Data) cho hệ thống quản lý khách sạn.
     * Đảm bảo hệ thống luôn có sẵn cấu hình nền tảng bao gồm tài khoản hệ thống, thông tin khách sạn, danh mục dịch vụ,
     * cấu hình hạng phòng, phòng vật lý và lịch sử đơn đặt phòng mẫu khi cơ sở dữ liệu trống.
     */
    public static class SeedData
    {
        /**
         * [System Initialization / UC01, UC07, UC09, UC10, UC13, UC15 Preconditions]
         * Thực hiện nạp dữ liệu mẫu bất đồng bộ vào cơ sở dữ liệu khi hệ thống khởi chạy lần đầu.
         * 
         * @param context Đối tượng ngữ cảnh cơ sở dữ liệu (AppDbContext) dùng để thực thi các tác vụ truy vấn và lưu dữ liệu
         */
        public static async Task SeedAsync(AppDbContext context)
        {
            // 1. Khởi tạo tài khoản nhân viên mặc định (UC01)
            if (!await context.Accounts.AnyAsync())
            {
                var users = new List<Account>
                {
                    new() {
                        FullName = "System Admin",
                        Username = "admin",
                        PasswordHash = HashPassword("Admin@123"),
                        Email = "admin@hotel.com",
                        Phone = "0900000000",
                        Role = Role.Admin,
                        Status = AccountStatus.Active
                    },
                    new() {
                        FullName = "Hotel Manager",
                        Username = "manager",
                        PasswordHash = HashPassword("Manager@123"),
                        Email = "manager@hotel.com",
                        Phone = "0900000001",
                        Role = Role.HotelManager,
                        Status = AccountStatus.Active
                    },
                    new() {
                        FullName = "Receptionist",
                        Username = "receptionist",
                        PasswordHash = HashPassword("Recept@123"),
                        Email = "receptionist@hotel.com",
                        Phone = "0900000002",
                        Role = Role.Receptionist,
                        Status = AccountStatus.Active
                    },
                    new() {
                        FullName = "Room Staff",
                        Username = "roomstaff",
                        PasswordHash = HashPassword("Staff@123"),
                        Email = "roomstaff@hotel.com",
                        Phone = "0900000003",
                        Role = Role.RoomStaff,
                        Status = AccountStatus.Active
                    }
                };

                await context.Accounts.AddRangeAsync(users);
                await context.SaveChangesAsync();
            }

            // 2. Khởi tạo hồ sơ khách hàng mẫu theo Schema thực tế (UC15)
            if (!await context.Guests.AnyAsync())
            {
                var guests = new List<Guest>
                {
                    new() {
                        FullName = "Nguyễn Văn Minh",
                        Phone = "0903123456",
                        IdNumber = "079096001234",
                        Email = "minh.nguyen@gmail.com",
                        IsDeleted = false
                    },
                    new() {
                        FullName = "Trần Thị Thanh Tuyền",
                        Phone = "0918987654",
                        IdNumber = "082092005678",
                        Email = "tuyen.ttt@yahoo.com",
                        IsDeleted = false
                    },
                    new() {
                        FullName = "Lê Hoàng Long",
                        Phone = "0989111222",
                        IdNumber = "035095009999",
                        Email = null,
                        IsDeleted = false
                    },
                    new() {
                        FullName = "Khách Hàng Bị Ẩn",
                        Phone = "0944555666",
                        IdNumber = "012345678901",
                        Email = null,
                        IsDeleted = true
                    }
                };

                await context.Guests.AddRangeAsync(guests);
                await context.SaveChangesAsync();
            }

            // 3. Khởi tạo thông tin khách sạn gốc (UC06)
            if (!await context.HotelInfos.AnyAsync())
            {
                await context.HotelInfos.AddAsync(new HotelInfo
                {
                    HotelName = "FPT Uni Hotel",
                    Address = "123 Nguyen Van Cu, Ninh Kieu, Can Tho City",
                    Phone = "0352859392",
                    Email = "fpt@hotel.com",
                    Website = "https://hotel.com",
                    Description = "A luxury hotel in the heart of Can Tho City"
                });
                await context.SaveChangesAsync();
            }

            // 4. Khởi tạo danh mục tiện ích dịch vụ gốc (UC10)
            if (!await context.Services.AnyAsync())
            {
                var services = new List<Service>
                {
                    new() { ServiceName = "Airport Transfer", Description = "Pick up from airport", Price = 200000, Status = ServiceStatus.Active },
                    new() { ServiceName = "Breakfast", Description = "Daily breakfast buffet", Price = 150000, Status = ServiceStatus.Active },
                    new() { ServiceName = "Laundry", Description = "Laundry service per kg", Price = 50000, Status = ServiceStatus.Active },
                    new() { ServiceName = "Spa", Description = "Full body massage 60 minutes", Price = 500000, Status = ServiceStatus.Active }
                };
                await context.Services.AddRangeAsync(services);
                await context.SaveChangesAsync();
            }

            // 5. Khởi tạo danh mục cấu hình hạng phòng và biểu giá (UC09)
            if (!await context.RoomTypes.AnyAsync())
            {
                var roomTypes = new List<RoomType>
                {
                    new() { Name = "Standard Twin", Description = "Phòng tiêu chuẩn gồm 2 giường đơn", BasePrice = 600000, MaxOccupancy = 2, IsActive = true },
                    new() { Name = "Deluxe Double", Description = "Phòng cao cấp gồm 1 giường đôi lớn", BasePrice = 1200000, MaxOccupancy = 2, IsActive = true },
                    new() { Name = "Suite Premium", Description = "Phòng hạng sang có phòng khách biệt lập", BasePrice = 3000000, MaxOccupancy = 4, IsActive = true }
                };
                await context.RoomTypes.AddRangeAsync(roomTypes);
                await context.SaveChangesAsync();
            }

            // 6. Khởi tạo danh sách phòng vật lý cụ thể (UC07)
            if (!await context.Rooms.AnyAsync())
            {
                var stdTwin = await context.RoomTypes.FirstOrDefaultAsync(rt => rt.Name == "Standard Twin");
                var delDouble = await context.RoomTypes.FirstOrDefaultAsync(rt => rt.Name == "Deluxe Double");
                var suitePrem = await context.RoomTypes.FirstOrDefaultAsync(rt => rt.Name == "Suite Premium");

                if (stdTwin != null && delDouble != null && suitePrem != null)
                {
                    var rooms = new List<Room>
                    {
                        new() { RoomNumber = "101", Floor = 1, RoomTypeId = stdTwin.Id, BookingStatus = (RoomBookingStatus)0, HousekeepingStatus = (RoomHousekeepingStatus)2 },
                        new() { RoomNumber = "102", Floor = 1, RoomTypeId = stdTwin.Id, BookingStatus = (RoomBookingStatus)0, HousekeepingStatus = (RoomHousekeepingStatus)2 },
                        new() { RoomNumber = "201", Floor = 2, RoomTypeId = delDouble.Id, BookingStatus = (RoomBookingStatus)0, HousekeepingStatus = (RoomHousekeepingStatus)2 },
                        new() { RoomNumber = "202", Floor = 2, RoomTypeId = delDouble.Id, BookingStatus = (RoomBookingStatus)0, HousekeepingStatus = (RoomHousekeepingStatus)2 },
                        new() { RoomNumber = "305", Floor = 3, RoomTypeId = suitePrem.Id, BookingStatus = (RoomBookingStatus)0, HousekeepingStatus = (RoomHousekeepingStatus)2 }
                    };
                    await context.Rooms.AddRangeAsync(rooms);
                    await context.SaveChangesAsync();
                }
            }

            // 7. Khởi tạo danh sách đơn đặt phòng mẫu liên kết chéo (UC13 & UC15.4)
            if (!await context.Reservations.AnyAsync())
            {
                var minh = await context.Guests.FirstOrDefaultAsync(g => g.FullName == "Nguyễn Văn Minh");
                var tuyen = await context.Guests.FirstOrDefaultAsync(g => g.FullName == "Trần Thị Thanh Tuyền");

                var r101 = await context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == "101");
                var r202 = await context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == "202");
                var r305 = await context.Rooms.FirstOrDefaultAsync(r => r.RoomNumber == "305");

                if (minh != null && tuyen != null && r101 != null && r202 != null && r305 != null)
                {
                    // Đã sửa đổi thành định dạng viết hoa toàn bộ (ALL_CAPS) khớp chuẩn Enum của hệ thống
                    var reservations = new List<Reservation>
                    {
                        new() {
                            GuestId = minh.Id,
                            RoomId = r202.Id,
                            CheckInDate = new DateTime(2026, 6, 15),
                            CheckOutDate = new DateTime(2026, 6, 18),
                            Status = ReservationStatus.CHECKED_OUT,
                            CreatedAt = new DateTime(2026, 6, 1)
                        },
                        new() {
                            GuestId = minh.Id,
                            RoomId = r305.Id,
                            CheckInDate = new DateTime(2026, 7, 5),
                            CheckOutDate = new DateTime(2026, 7, 7),
                            Status = ReservationStatus.CHECKED_OUT,
                            CreatedAt = new DateTime(2026, 6, 20)
                        },
                        new() {
                            GuestId = minh.Id,
                            RoomId = r101.Id,
                            CheckInDate = new DateTime(2026, 7, 25),
                            CheckOutDate = new DateTime(2026, 7, 26),
                            Status = ReservationStatus.CONFIRMED,
                            CreatedAt = new DateTime(2026, 7, 10)
                        },
                        new() {
                            GuestId = tuyen.Id,
                            RoomId = r202.Id,
                            CheckInDate = new DateTime(2026, 7, 18),
                            CheckOutDate = new DateTime(2026, 7, 22),
                            Status = ReservationStatus.CHECKED_IN,
                            CreatedAt = new DateTime(2026, 7, 15)
                        }
                    };
                    await context.Reservations.AddRangeAsync(reservations);

                    r202.BookingStatus = (RoomBookingStatus)2;
                    r202.HousekeepingStatus = (RoomHousekeepingStatus)0;
                    context.Rooms.Update(r202);

                    await context.SaveChangesAsync();
                }
            }
        }

        /**
         * [BR-01 - Password Encoding]
         * Phương thức băm mật khẩu người dùng nhằm chuyển đổi chuỗi ký tự văn bản thô (plain text) 
         * thành chuỗi mã hóa Base64 trước khi lưu trữ vào hệ thống.
         * 
         * @param password Chuỗi mật khẩu dạng văn bản thô (plain text)
         * @return Chuỗi ký tự mật khẩu sau khi được mã hóa SHA256 an toàn dưới dạng Base64 String
         */
        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}