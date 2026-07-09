using Hotel_System.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hotel_System.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }

        public DbSet<HotelInfo> HotelInfos { get; set; }

        public DbSet<RoomType> RoomTypes { get; set; }

        public DbSet<Room> Rooms { get; set; }

        public DbSet<Service> Services { get; set; }
        public DbSet<Guest> Guests { get; set; } = null!;
        public DbSet<Reservation> Reservations { get; set; } = null!;

        public DbSet<HousekeepingTask> HousekeepingTasks { get; set; }

        public DbSet<MaintenanceIssue> MaintenanceIssues { get; set; }
        public DbSet<ServiceUsage> ServiceUsages { get; set; }
        public DbSet<Invoice> Invoices { get; set; }



        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Account>(e => 
            {
                e.HasKey(u => u.Id);
                e.Property(u => u.Username).IsRequired().HasMaxLength(50);
                e.Property(u => u.Email).IsRequired().HasMaxLength(100);
                e.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                e.HasIndex(u => u.Username).IsUnique();
                e.HasIndex(u => u.Email).IsUnique();
            });

            modelBuilder.Entity<HotelInfo>(e => 
            {
                e.HasKey(h => h.Id);
                e.Property(h => h.HotelName).IsRequired().HasMaxLength(200);
                e.Property(h => h.Email).HasMaxLength(100);
                e.Property(h => h.Phone).HasMaxLength(20);
            });

            modelBuilder.Entity<Service>(e => 
            {
                e.HasKey(s => s.Id);
                e.Property(s => s.ServiceName).IsRequired().HasMaxLength(200);
                e.Property(s => s.Price).HasColumnType("decimal(18,2)");
            });

            modelBuilder.Entity<Room>()
            .HasOne(r => r.RoomType)
            .WithMany(rt => rt.Rooms)
            .HasForeignKey(r => r.RoomTypeId)
             .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Room>()
                .HasIndex(r => r.RoomNumber)
                .IsUnique();

            modelBuilder.Entity<Reservation>(e =>
            {
                e.HasKey(r => r.Id);
                e.HasOne(r => r.Room).WithMany().HasForeignKey(r => r.RoomId).OnDelete(DeleteBehavior.Restrict);
                e.HasOne(r => r.Guest).WithMany(g => g.Reservations).HasForeignKey(r => r.GuestId).OnDelete(DeleteBehavior.Restrict);
            });


            modelBuilder.Entity<Guest>(e =>
            {
                e.HasKey(g => g.Id);
                e.Property(g => g.FullName).IsRequired().HasMaxLength(150);
                e.Property(g => g.Phone).IsRequired().HasMaxLength(20);
                e.Property(g => g.IsDeleted).HasDefaultValue(false); // MỚI
            });

            modelBuilder.Entity<RoomType>().Property(r => r.BasePrice).HasColumnType("decimal(18,2)");

            modelBuilder.Entity<HousekeepingTask>(e => {
                e.HasKey(t => t.Id);
                e.HasOne(t => t.Room)
                 .WithMany()
                 .HasForeignKey(t => t.RoomId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(t => t.AssignedTo)
                 .WithMany()
                 .HasForeignKey(t => t.AssignedToId)
                 .OnDelete(DeleteBehavior.Restrict);
                e.HasOne(t => t.CreatedBy)
                 .WithMany()
                 .HasForeignKey(t => t.CreatedById)
                 .OnDelete(DeleteBehavior.Restrict);
            });

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
                 .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Invoice>(e => {
                e.HasKey(i => i.Id);
                e.Property(i => i.RoomCharge).HasColumnType("decimal(18,2)");
                e.Property(i => i.ServiceCharge).HasColumnType("decimal(18,2)");
                e.Property(i => i.TotalAmount).HasColumnType("decimal(18,2)");
                e.HasOne(i => i.Reservation)
                 .WithOne()
                 .HasForeignKey<Invoice>(i => i.ReservationId)
                 .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}