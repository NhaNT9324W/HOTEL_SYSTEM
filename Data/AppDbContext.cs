using Hotel_System.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hotel_System.Data
{
    /**
     * [IV.5 Database Design]
     * Lớp ngữ cảnh cơ sở dữ liệu (Database Context) trung tâm của Hệ thống Quản lý Khách sạn[cite: 1].
     * Chịu trách nhiệm thiết lập kết nối, quản lý các thực thể (Entities) và ánh xạ chúng thành 11 bảng dữ liệu quan hệ trong SQL Server thông qua Entity Framework Core[cite: 1].
     */
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        /** [IV.5.2.1 Account] Ánh xạ danh sách tài khoản người dùng và phân quyền nhân viên khách sạn[cite: 1]. */
        public DbSet<Account> Accounts { get; set; }

        /** [IV.5.2.3 HotelInfo] Ánh xạ bảng cấu hình thông tin singleton chung của khách sạn[cite: 1]. */
        public DbSet<HotelInfo> HotelInfos { get; set; }

        /** [IV.5.2.9 RoomType] Ánh xạ danh mục cấu hình phân loại hạng phòng, định mức biểu phí và sức chứa[cite: 1]. */
        public DbSet<RoomType> RoomTypes { get; set; }

        /** [IV.5.2.8 Room] Ánh xạ danh sách các phòng vật lý và trạng thái vận hành thời gian thực[cite: 1]. */
        public DbSet<Room> Rooms { get; set; }

        /** [IV.5.2.10 Service] Ánh xạ danh mục các gói dịch vụ tiện ích bổ sung của khách sạn[cite: 1]. */
        public DbSet<Service> Services { get; set; }

        /** [IV.5.2.2 Guest] Ánh xạ hồ sơ thông tin cá nhân và giấy tờ định danh của khách hàng[cite: 1]. */
        public DbSet<Guest> Guests { get; set; } = null!;

        /** [IV.5.2.7 Reservation] Ánh xạ thông tin chi tiết các đơn đặt phòng khách sạn[cite: 1]. */
        public DbSet<Reservation> Reservations { get; set; } = null!;

        /** [IV.5.2.4 HousekeepingTask] Ánh xạ danh sách tác vụ phân công dọn dẹp, kiểm tra buồng phòng[cite: 1]. */
        public DbSet<HousekeepingTask> HousekeepingTasks { get; set; }

        /** [IV.5.2.6 MaintenanceIssue] Ánh xạ các phiếu báo cáo sự cố hỏng hóc thiết bị vật tư trong phòng[cite: 1]. */
        public DbSet<MaintenanceIssue> MaintenanceIssues { get; set; }

        /** [IV.5.2.11 ServiceUsage] Ánh xạ hồ sơ ghi nhận lịch sử tiêu dùng dịch vụ bổ sung của khách[cite: 1]. */
        public DbSet<ServiceUsage> ServiceUsages { get; set; }

        /** [IV.5.2.5 Invoice] Ánh xạ hóa đơn tài chính chính thức lập khi hoàn tất thủ tục trả phòng[cite: 1]. */
        public DbSet<Invoice> Invoices { get; set; }

        /**
         * [IV.5.1 Database Overview - Fluent API Configuration]
         * Cấu hình chi tiết lược đồ cơ sở dữ liệu, thiết lập ràng buộc dữ liệu (Constraints),
         * khởi tạo các chỉ mục duy nhất (Unique Indexes) và định nghĩa mối quan hệ giữa các bảng (Relationships)[cite: 1].
         * Toàn bộ hành vi xóa (OnDelete) được cấu hình là 'Restrict' để bảo vệ toàn vẹn tham chiếu dữ liệu nghiêm ngặt[cite: 1].
         * 
         * @param modelBuilder Đối tượng tạo dựng mô hình thực thể (ModelBuilder) từ EF Core
         */
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình ràng buộc dữ liệu cho bảng Account (IV.5.2.1)
            modelBuilder.Entity<Account>(e =>
            {
                e.HasKey(u => u.Id);
                e.Property(u => u.Username).IsRequired().HasMaxLength(50);
                e.Property(u => u.Email).IsRequired().HasMaxLength(100);
                e.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                e.HasIndex(u => u.Username).IsUnique(); // BR-02: Username là duy nhất
                e.HasIndex(u => u.Email).IsUnique();    // BR-02: Email là duy nhất trong hệ thống[cite: 1]
            });

            // Cấu hình ràng buộc dữ liệu cho bảng HotelInfo (IV.5.2.3)
            modelBuilder.Entity<HotelInfo>(e =>
            {
                e.HasKey(h => h.Id);
                e.Property(h => h.HotelName).IsRequired().HasMaxLength(200); // BR-18: Không được để trống[cite: 1]
                e.Property(h => h.Email).HasMaxLength(100);
                e.Property(h => h.Phone).HasMaxLength(20);
            });

            // Cấu hình ràng buộc dữ liệu cho bảng Service (IV.5.2.10)
            modelBuilder.Entity<Service>(e =>
            {
                e.HasKey(s => s.Id);
                e.Property(s => s.ServiceName).IsRequired().HasMaxLength(200); // BR-34: Tên dịch vụ là duy nhất[cite: 1]
                e.Property(s => s.Price).HasColumnType("decimal(18,2)");      // BR-35: Kiểu số thực cho tiền tệ[cite: 1]
            });

            // Cấu hình mối quan hệ giữa Room và RoomType (Many-to-One) (IV.5.2.8)
            modelBuilder.Entity<Room>()
            .HasOne(r => r.RoomType)
            .WithMany(rt => rt.Rooms)
            .HasForeignKey(r => r.RoomTypeId)
             .OnDelete(DeleteBehavior.Restrict); // Chặn xóa loại phòng nếu đang có phòng thuộc loại đó[cite: 1]

            // Thiết lập chỉ mục duy nhất cho số phòng vật lý (IV.5.2.8)
            modelBuilder.Entity<Room>()
                .HasIndex(r => r.RoomNumber)
                .IsUnique(); // BR-20: Mỗi phòng phải có một số phòng duy nhất[cite: 1]

            // Cấu hình mối quan hệ cho bảng Reservation (IV.5.2.7)
            modelBuilder.Entity<Reservation>(e =>
            {
                e.HasKey(r => r.Id);
                e.HasOne(r => r.Room).WithMany().HasForeignKey(r => r.RoomId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(r => r.Guest).WithMany(g => g.Reservations).HasForeignKey(r => r.GuestId).OnDelete(DeleteBehavior.Restrict);
            });

            // Cấu hình ràng buộc dữ liệu cho bảng Guest (IV.5.2.2)
            modelBuilder.Entity<Guest>(e =>
            {
                e.HasKey(g => g.Id);
                e.Property(g => g.FullName).IsRequired().HasMaxLength(150);
                e.Property(g => g.Phone).IsRequired().HasMaxLength(20);
                e.Property(g => g.IsDeleted).HasDefaultValue(false); // Áp dụng cơ chế soft delete hồ sơ khách hàng[cite: 1]
            });

            // Cấu hình kiểu dữ liệu tiền tệ cho biểu phí cơ bản của hạng phòng (IV.5.2.9)
            modelBuilder.Entity<RoomType>().Property(r => r.BasePrice).HasColumnType("decimal(18,2)");

            // Cấu hình các mối quan hệ ràng buộc ngoại khóa cho bảng HousekeepingTask (IV.5.2.4)
            modelBuilder.Entity<HousekeepingTask>(e => {
                e.HasKey(t => t.Id);
                e.HasOne(t => t.Room)
                 .WithMany()
                 .HasForeignKey(t => t.RoomId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(t => t.AssignedTo)
                 .WithMany()
                 .HasForeignKey(t => t.AssignedToId)
                 .OnDelete(DeleteBehavior.Restrict); // Kiểm soát Task Ownership Constraint[cite: 1]
                e.HasOne(t => t.CreatedBy)
                 .WithMany()
                 .HasForeignKey(t => t.CreatedById)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // Cấu hình các mối quan hệ ràng buộc ngoại khóa cho bảng MaintenanceIssue (IV.5.2.6)
            modelBuilder.Entity<MaintenanceIssue>(e => {
                e.HasKey(m => m.Id);
                e.HasOne(m => m.Room)
                 .WithMany()
                 .HasForeignKey(m => m.RoomId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(m => m.ReportedBy)
                 .WithMany()
                 .HasForeignKey(m => m.ReportedById)
                 .OnDelete(DeleteBehavior.Restrict);
            });

            // Cấu hình ràng buộc dữ liệu và mối quan hệ ngoại khóa cho bảng ServiceUsage (IV.5.2.11)
            modelBuilder.Entity<ServiceUsage>(e => {
                e.HasKey(s => s.Id);
                e.Property(s => s.UnitPrice).HasColumnType("decimal(18,2)");
                e.Property(s => s.TotalPrice).HasColumnType("decimal(18,2)");
                e.HasOne(s => s.Reservation)
                 .WithMany()
                 .HasForeignKey(s => s.ReservationId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(s => s.Service)
                 .WithMany()
                 .HasForeignKey(s => s.ServiceId)
                 .OnDelete(DeleteBehavior.Restrict); // Ràng buộc Active Service Protection[cite: 1]
            });

            // Cấu hình kiểu tiền tệ và mối quan hệ One-to-One đặc biệt giữa Invoice và Reservation (IV.5.2.5)
            modelBuilder.Entity<Invoice>(e => {
                e.HasKey(i => i.Id);
                e.Property(i => i.RoomCharge).HasColumnType("decimal(18,2)");
                e.Property(i => i.ServiceCharge).HasColumnType("decimal(18,2)");
                e.Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");
                e.HasOne(i => i.Reservation)
                 .WithOne()
                 .HasForeignKey<Invoice>(i => i.ReservationId)
                 .OnDelete(DeleteBehavior.Restrict); // Một đơn đặt phòng chỉ xuất duy nhất một hóa đơn chính thức[cite: 1]
            });
        }
    }
}