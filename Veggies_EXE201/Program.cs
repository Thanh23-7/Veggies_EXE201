using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Veggies_EXE201.Models;
using Veggies_EXE201.Models.ViewModels;
using Veggies_EXE201.Repositories;
using Veggies_EXE201.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
// Đăng ký PasswordHasher cho model User của bạn
builder.Services.AddSingleton<IPasswordHasher<User>, PasswordHasher<User>>();

// Đăng ký AuthService
builder.Services.AddScoped<AuthService>();
builder.Services.AddDbContext<VeggiesDb2Context>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();


// Thêm dịch vụ Authentication (nếu bạn dùng Session/Cookie Authentication)
builder.Services.AddAuthentication("CookieAuth")
    .AddCookie("CookieAuth", options =>
    {
        options.LoginPath = "/Account/Login"; // Đường dẫn đến trang đăng nhập
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
