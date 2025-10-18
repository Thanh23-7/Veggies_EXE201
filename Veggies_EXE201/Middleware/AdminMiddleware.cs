using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Veggies_EXE201.Middleware
{
    public class AdminMiddleware
    {
        private readonly RequestDelegate _next;

        public AdminMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Kiểm tra nếu đang truy cập vào route admin
            if (context.Request.Path.StartsWithSegments("/admin"))
            {
                // Kiểm tra xem user có đăng nhập không
                if (!context.User.Identity.IsAuthenticated)
                {
                    context.Response.Redirect("/Account/Login?returnUrl=" + Uri.EscapeDataString(context.Request.Path));
                    return;
                }

                // Kiểm tra role Admin
                var role = context.User.FindFirstValue(ClaimTypes.Role);
                if (role != "Admin")
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("Bạn không có quyền truy cập trang này.");
                    return;
                }
            }

            await _next(context);
        }
    }
}
