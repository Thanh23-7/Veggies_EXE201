using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Veggies_EXE201.Models;
using Veggies_EXE201.Repositories;
using Veggies_EXE201.Services;

var builder = WebApplication.CreateBuilder(args);

// --- 1. Cấu hình các dịch vụ cơ bản ---
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddDbContext<VeggiesDb2Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- 2. Cấu hình Authentication & Authorization tùy chỉnh ---
// (Dùng cho hệ thống đăng nhập không sử dụng ASP.NET Core Identity mặc định)
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Account/Login"; // Đường dẫn đến trang đăng nhập
        options.AccessDeniedPath = "/Home/AccessDenied"; // Trang thông báo không có quyền
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();

// --- 3. Cấu hình Session ---
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// --- 4. Đăng ký các Service và Repository (Dependency Injection) ---
builder.Services.AddScoped<AuthService>(); // AuthService không cần interface nếu chỉ dùng ở 1 nơi
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IPayOSService, PayOSService>();
builder.Services.AddScoped<ReviewService>(); // Đăng ký ReviewService
builder.Services.AddScoped<Veggies_EXE201.Services.ProductService>();

// =========================================================================

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// --- Cấu hình thứ tự Middleware (Rất quan trọng) ---
app.UseRouting(); // 1. Tìm ra endpoint nào sẽ xử lý request

app.UseSession(); // 2. Kích hoạt Session để các controller có thể sử dụng

app.UseAuthentication(); // 3. Xác định người dùng là ai (đọc cookie)
app.UseAuthorization(); // 4. Kiểm tra xem người dùng có quyền truy cập endpoint đó không

// 5. Admin Middleware (kiểm tra quyền Admin)
app.UseMiddleware<Veggies_EXE201.Middleware.AdminMiddleware>();

// 5. Ánh xạ request tới Controller Action tương ứng
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();