using System.Text;
using Hotel_System.Data;
using Hotel_System.Repositories.Implementations;
using Hotel_System.Repositories.Interfaces;
using Hotel_System.Services.Implementations;
using Hotel_System.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddControllers();

// ===== CẤU HÌNH XÁC THỰC JWT TOKEN =====
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };

    // Đọc token lưu trong cookie của trình duyệt
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["jwt_token"];
            return Task.CompletedTask;
        }
    };
});

// ===== CẤU HÌNH CƠ SỞ DỮ LIỆU =====
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===== ĐĂNG KÝ CÁC PHÂN HỆ DI (DEPENDENCY INJECTION) =====
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IHotelInfoRepository, HotelInfoRepository>();
builder.Services.AddScoped<IHotelInfoService, HotelInfoService>();
builder.Services.AddScoped<IServiceRepository, ServiceRepository>();
builder.Services.AddScoped<IServiceManager, ServiceManager>();
builder.Services.AddScoped<IRoomTypeRepository, RoomTypeRepository>();
builder.Services.AddScoped<IRoomTypeService, RoomTypeService>();

// Room & Reservation Management
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<IGuestRepository, GuestRepository>();
builder.Services.AddScoped<IGuestService, GuestService>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IReservationService, ReservationService>();

// Task & Infrastructure Services
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<ICheckOutService, CheckOutService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();

// ===== CẤU HÌNH PHÂN QUYỀN HỆ THỐNG =====
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("AdminOrManager", policy =>
        policy.RequireRole("Admin", "HotelManager"));

    options.AddPolicy("Receptionist", policy =>
        policy.RequireRole("Admin", "HotelManager", "Receptionist"));

    options.AddPolicy("RoomStaffOnly", policy =>
        policy.RequireRole("RoomStaff"));

    options.AddPolicy("AllStaff", policy =>
        policy.RequireRole("Admin", "HotelManager", "Receptionist", "RoomStaff"));
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Login";
    options.AccessDeniedPath = "/AccessDenied";
});

var app = builder.Build();

// ===== TỰ ĐỘNG KHỞI TẠO MIGRATION & SEED DATA =====
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();


    // 🔥 THÊM DUY NHẤT DÒNG NÀY VÀO ĐỂ ÉP XÓA DATABASE CŨ KHI CHẠY DỰ ÁN
    await context.Database.EnsureDeletedAsync();


    await context.Database.MigrateAsync();
    await SeedData.SeedAsync(context);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// ĐỒNG BỘ: Luôn luôn đặt Authentication đứng TRƯỚC Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();
app.MapControllers();

// Điều hướng mặc định về trang Login khi chạy dự án
app.MapGet("/", () => Results.Redirect("/Login"));

app.Run();  