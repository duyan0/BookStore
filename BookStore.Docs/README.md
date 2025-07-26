# 📚 BookStore - Hệ thống quản lý bán sách trực tuyến

## 🎯 Giới thiệu
BookStore là một hệ thống e-commerce hoàn chỉnh dành cho việc bán sách trực tuyến, được xây dựng bằng ASP.NET Core với kiến trúc Clean Architecture.

## ✨ Tính năng chính

### 👥 Dành cho Khách hàng:
- 🏠 **Trang chủ** với slider và sách nổi bật
- 🔍 **Tìm kiếm và duyệt sách** theo danh mục
- 📖 **Xem chi tiết sách** với mô tả đầy đủ
- 🛒 **Giỏ hàng** với AJAX và notifications
- 💳 **Đặt hàng** và theo dõi trạng thái
- 👤 **Quản lý tài khoản** và đổi mật khẩu
- 📧 **Email notifications** tự động
- 📱 **Responsive design** cho mọi thiết bị

### 🔧 Dành cho Admin:
- 📊 **Dashboard** với thống kê và biểu đồ
- 📚 **Quản lý sách** với Rich Text Editor
- 👥 **Quản lý tác giả và thể loại**
- 🛒 **Xử lý đơn hàng** và cập nhật trạng thái
- 👤 **Quản lý người dùng**
- 🖼️ **Thư viện ảnh** với upload multiple files
- 🎠 **Quản lý Slider và Banner**
- 📧 **Email templates** và notifications
- 📊 **Báo cáo doanh thu** và thống kê

## 🏗️ Kiến trúc hệ thống

### Công nghệ sử dụng:
- **Backend:** ASP.NET Core 9.0 Web API
- **Frontend:** ASP.NET Core MVC
- **Database:** SQL Server với Entity Framework Core
- **Authentication:** JWT Token
- **UI Framework:** Bootstrap 5
- **Rich Text Editor:** TinyMCE
- **Charts:** Chart.js
- **Email:** SMTP với Gmail

### Cấu trúc Project:
```
BookStore/
├── BookStore.API/          # Web API Backend
├── BookStore.Web/          # MVC Frontend  
├── BookStore.Core/         # Business Logic & Entities
├── BookStore.Infrastructure/ # Data Access & Services
├── BookStore.Docs/         # Documentation
└── BookStore.Tests/        # Unit Tests
```

## 🚀 Hướng dẫn cài đặt nhanh

### Yêu cầu hệ thống:
- .NET 9.0 SDK
- SQL Server (Express/LocalDB)
- Visual Studio 2022 hoặc VS Code

### Các bước cài đặt:

#### 1. Clone repository:
```bash
git clone [YOUR_REPOSITORY_URL]
cd BookStore
```

#### 2. Cấu hình Database:
```bash
# Cập nhật connection string trong BookStore.API/appsettings.json
# Chạy migration
cd BookStore.API
dotnet ef database update
```

#### 3. Chạy ứng dụng:
```bash
# Terminal 1 - API Backend
cd BookStore.API
dotnet run

# Terminal 2 - Web Frontend  
cd BookStore.Web
dotnet run
```

#### 4. Truy cập ứng dụng:
- **Website:** http://localhost:5106
- **API:** http://localhost:5274
- **Admin:** http://localhost:5106/Admin
- **Swagger:** http://localhost:5274/swagger

#### 5. Đăng nhập Admin:
- **Email:** admin@bookstore.com
- **Password:** Admin123!

## 📖 Tài liệu hướng dẫn

### 📋 Danh sách tài liệu:
1. **[DATABASE_SETUP_GUIDE.md](DATABASE_SETUP_GUIDE.md)** - Hướng dẫn setup database
2. **[DEPLOYMENT_GUIDE.md](DEPLOYMENT_GUIDE.md)** - Hướng dẫn deploy application
3. **[USER_GUIDE.md](USER_GUIDE.md)** - Hướng dẫn sử dụng cho khách hàng
4. **[ADMIN_GUIDE.md](ADMIN_GUIDE.md)** - Hướng dẫn quản trị hệ thống

### 🎯 Chọn tài liệu phù hợp:
- **Developer/IT:** DATABASE_SETUP_GUIDE.md + DEPLOYMENT_GUIDE.md
- **End User:** USER_GUIDE.md
- **Admin/Manager:** ADMIN_GUIDE.md

## 🔧 Cấu hình

### Database Connection:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=BookStoreDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Email Settings:
```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "BookStore"
  }
}
```

### JWT Configuration:
```json
{
  "Jwt": {
    "Key": "YourSecretKeyHere_AtLeast32Characters",
    "Issuer": "BookStoreAPI",
    "Audience": "BookStoreClient"
  }
}
```

## 🌟 Tính năng nổi bật

### 🛒 Shopping Cart với AJAX:
- Thêm sách vào giỏ không reload trang
- Counter hiển thị số lượng sách trong giỏ
- Notifications thông báo thành công/lỗi
- Tự động check đăng nhập

### 📝 Rich Text Editor:
- TinyMCE integration cho mô tả sách
- Toolbar đầy đủ với formatting options
- Preview và code view
- Auto-save content

### 🖼️ Image Library:
- Upload multiple images cùng lúc
- Preview thumbnails
- Copy URL để sử dụng
- Organized file management

### 📧 Email System:
- Automated email notifications
- Professional HTML templates
- Order status updates
- Password reset emails

### 📊 Admin Dashboard:
- Real-time statistics
- Revenue charts
- Order management
- User analytics

## 🚨 Troubleshooting

### Lỗi thường gặp:

#### Database Connection:
```bash
# Kiểm tra SQL Server đang chạy
# Cập nhật connection string
# Chạy migration: dotnet ef database update
```

#### Email không gửi:
```bash
# Kiểm tra Gmail App Password
# Kiểm tra SMTP settings
# Kiểm tra firewall
```

#### TinyMCE không load:
```bash
# Clear browser cache
# Kiểm tra CDN connection
# Kiểm tra JavaScript console
```

## 📞 Hỗ trợ

### Khi gặp vấn đề:
1. Kiểm tra **console logs** cho errors
2. Xem **database connection** 
3. Kiểm tra **email configuration**
4. Tham khảo **documentation** chi tiết
5. Kiểm tra **firewall và antivirus**

### Thông tin liên hệ:
- **Documentation:** Xem các file .md trong thư mục Docs
- **Issues:** Tạo issue trong repository
- **Email:** [Your support email]

## 📄 License
This project is licensed under the MIT License.

## 🙏 Acknowledgments
- ASP.NET Core Team
- Entity Framework Team
- Bootstrap Team
- TinyMCE Team
- Chart.js Team

---
**BookStore v1.0** - Hệ thống bán sách trực tuyến hoàn chỉnh 📚
