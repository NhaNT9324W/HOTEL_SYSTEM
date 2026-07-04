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

            modelBuilder.Entity<RoomType>().Property(r => r.BasePrice).HasColumnType("decimal(18,2)");

        }
    }
}