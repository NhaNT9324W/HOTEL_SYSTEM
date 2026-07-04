using Hotel_System.Entities;
using Hotel_System.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Hotel_System.Data
{
    public static class SeedData
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Accounts.AnyAsync()) return;

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

        }

        public static string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}