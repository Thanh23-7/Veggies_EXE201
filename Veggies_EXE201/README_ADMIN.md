# Hướng dẫn sử dụng hệ thống Admin

## Tài khoản Admin

### Tạo tài khoản Admin
1. Chạy script SQL trong file `Scripts/CreateAdminUser.sql`
2. Hoặc tạo tài khoản trực tiếp trong database với Role = 'Admin'

### Đăng nhập Admin
- Email: admin@veggies.com (hoặc email admin bạn đã tạo)
- Password: [mật khẩu bạn đã đặt]

## Chức năng Admin Dashboard

### 1. Bảng điều khiển (Dashboard)
- Thống kê tổng quan: tổng người dùng, sản phẩm, đơn hàng, đánh giá
- Biểu đồ thống kê đơn hàng theo tháng
- Biểu đồ người dùng mới
- Thao tác nhanh đến các trang quản lý

### 2. Quản lý Tài khoản
- Xem danh sách tất cả người dùng
- Xem chi tiết thông tin người dùng
- Thay đổi trạng thái tài khoản (chức năng đang phát triển)

### 3. Quản lý Sản phẩm
- Xem danh sách tất cả sản phẩm
- Thêm sản phẩm mới
- Chỉnh sửa thông tin sản phẩm
- Xóa sản phẩm

### 4. Quản lý Đánh giá
- Xem tất cả đánh giá từ khách hàng
- Xem chi tiết đánh giá
- Xóa đánh giá không phù hợp

### 5. Quản lý Log Hệ thống
- Chức năng này đã được loại bỏ khỏi phiên bản hiện tại

## Bảo mật

### Phân quyền
- Chỉ người dùng có Role = 'Admin' mới có thể truy cập các trang admin
- Middleware AdminMiddleware tự động kiểm tra quyền truy cập

### Middleware
- AdminMiddleware được đăng ký trong Program.cs
- Tự động chuyển hướng đến trang đăng nhập nếu chưa đăng nhập
- Trả về lỗi 403 nếu không có quyền Admin

## Cấu trúc File

```
Controllers/
├── AdminController.cs          # Controller chính cho admin
├── AccountController.cs        # Đã cập nhật hỗ trợ role Admin

Models/
├── User.cs                     # Đã cập nhật hỗ trợ role Admin

Services/
├── AuthService.cs              # Đã thêm phương thức hỗ trợ Admin

Views/
├── Admin/
│   ├── Index.cshtml            # Dashboard chính
│   ├── ManageAccounts.cshtml   # Quản lý tài khoản
│   ├── Products/
│   │   ├── Index.cshtml        # Danh sách sản phẩm
│   │   ├── Create.cshtml       # Thêm sản phẩm
│   │   └── Edit.cshtml         # Chỉnh sửa sản phẩm
│   └── Reviews/
│       └── Index.cshtml        # Quản lý đánh giá

Shared/
├── _AdminLayout.cshtml         # Layout cho admin với sidebar

Middleware/
├── AdminMiddleware.cs          # Middleware kiểm tra quyền Admin
```

## Lưu ý quan trọng

1. **Tạo tài khoản Admin**: Sử dụng script SQL hoặc tạo trực tiếp trong database
2. **Bảo mật**: Role 'Admin' không hiển thị công khai trong form đăng ký
3. **Middleware**: Đảm bảo AdminMiddleware được đăng ký đúng thứ tự trong Program.cs
4. **Database**: Đảm bảo bảng Users có cột Role và có thể chứa giá trị 'Admin'

## Troubleshooting

### Không thể truy cập trang admin
- Kiểm tra tài khoản có Role = 'Admin'
- Kiểm tra middleware đã được đăng ký
- Kiểm tra cookie authentication

### Lỗi 403 Forbidden
- Đảm bảo tài khoản đã đăng nhập
- Kiểm tra Role trong database
- Xóa cookie và đăng nhập lại
