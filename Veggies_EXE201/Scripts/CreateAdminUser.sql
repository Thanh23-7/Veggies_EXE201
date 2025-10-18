-- Script tạo tài khoản Admin mẫu
-- Chạy script này trong SQL Server Management Studio hoặc Azure Data Studio

USE VeggiesDb2; -- Thay đổi tên database nếu cần

-- Kiểm tra xem tài khoản admin đã tồn tại chưa
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'admin@veggies.com')
BEGIN
    -- Tạo tài khoản admin
    INSERT INTO Users (FullName, Email, PasswordHash, Role, CreatedAt, Phone, Address)
    VALUES (
        'Administrator',
        'admin@veggies.com',
        'AQAAAAEAACcQAAAAEExampleHashHere', -- Thay đổi thành hash thật từ AuthService
        'Admin',
        GETDATE(),
        '0123456789',
        '123 Admin Street, Ho Chi Minh City'
    );
    
    PRINT 'Tài khoản Admin đã được tạo thành công!';
    PRINT 'Email: admin@veggies.com';
    PRINT 'Vui lòng cập nhật PasswordHash bằng hash thật từ ứng dụng.';
END
ELSE
BEGIN
    PRINT 'Tài khoản Admin đã tồn tại.';
END
