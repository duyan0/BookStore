# Hướng dẫn thiết lập Google App Password cho BookStore

## Bước 1: Bật xác thực 2 bước (2FA)
1. Truy cập [Google Account Security](https://myaccount.google.com/security)
2. Trong phần "Signing in to Google", click "2-Step Verification"
3. Làm theo hướng dẫn để bật 2FA nếu chưa có

## Bước 2: Tạo App Password
1. Sau khi bật 2FA, quay lại [Google Account Security](https://myaccount.google.com/security)
2. Trong phần "Signing in to Google", click "App passwords"
3. Chọn "Mail" và "Other (Custom name)"
4. Nhập tên: "BookStore Email Service"
5. Click "Generate"
6. Copy mật khẩu 16 ký tự được tạo ra

## Bước 3: Cập nhật cấu hình BookStore
1. Mở file `BookStore.API/appsettings.json`
2. Thay thế giá trị trong `Email.Password` bằng App Password vừa tạo
3. Restart API để áp dụng thay đổi

## Bước 4: Test email functionality
```bash
# Test qua API endpoint
curl -X POST "http://localhost:5274/api/auth/reset-password" \
  -H "Content-Type: application/json" \
  -d '{"Email":"your-email@gmail.com"}'
```

## Lưu ý bảo mật
- ⚠️ **Không commit App Password vào Git**
- ✅ Sử dụng Environment Variables trong production
- ✅ App Password chỉ hoạt động với Gmail accounts có 2FA
- ✅ Có thể revoke App Password bất cứ lúc nào từ Google Account

## Cấu hình Environment Variables (Production)
```bash
# Thêm vào environment variables
EMAIL_USERNAME=your-email@gmail.com
EMAIL_PASSWORD=your-16-char-app-password
EMAIL_FROM_NAME="BookStore - Hệ thống quản lý sách"
```

## Troubleshooting
- **535 Authentication failed**: App Password không đúng hoặc 2FA chưa bật
- **Connection timeout**: Kiểm tra firewall và network
- **Invalid credentials**: Đảm bảo username là email đầy đủ
