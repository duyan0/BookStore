# 🚀 Hướng dẫn thiết lập dự án BookStore

## 📋 Tổng quan
Tài liệu này hướng dẫn chi tiết cách thiết lập dự án BookStore trên máy mới sau khi clone từ GitHub. Dự án BookStore là hệ thống quản lý bán sách online được xây dựng bằng .NET 9.0 với kiến trúc Clean Architecture.

## 🏗️ Cấu trúc dự án
```
BookStore/
├── BookStore.API/          # Web API Backend (Port: 5274)
├── BookStore.Web/          # MVC Frontend (Port: 5106)
├── BookStore.Core/         # Business Logic & Entities
├── BookStore.Infrastructure/ # Data Access & Services
├── BookStore.Docs/         # Documentation
└── BookStore.Tests/        # Unit Tests
```

## 📋 Yêu cầu hệ thống

### Phần mềm bắt buộc:
- **.NET 9.0 SDK** - [Tải tại đây](https://dotnet.microsoft.com/download/dotnet/9.0)
- **SQL Server** (một trong các tùy chọn sau):
  - SQL Server Express 2019+ (Khuyến nghị)
  - SQL Server LocalDB
  - SQL Server Developer Edition
- **SQL Server Management Studio (SSMS)** - [Tải tại đây](https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms)
- **Visual Studio 2022** hoặc **VS Code** với C# extension

### Phần mềm tùy chọn:
- **Git** - Để clone repository
- **Postman** - Để test API endpoints

## 🔧 Bước 1: Cài đặt SQL Server

### Tùy chọn 1: SQL Server Express (Khuyến nghị)
1. Truy cập [SQL Server Downloads](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
2. Tải **SQL Server Express**
3. Chạy file cài đặt và chọn **Basic** installation
4. Ghi nhớ **Server name** (thường là `DESKTOP-XXX\SQLEXPRESS` hoặc `.\SQLEXPRESS`)

### Tùy chọn 2: SQL Server LocalDB (Nhẹ hơn)
1. Mở **Command Prompt** với quyền Administrator
2. Chạy lệnh:
```cmd
sqllocaldb create MSSQLLocalDB
sqllocaldb start MSSQLLocalDB
```

### Kiểm tra cài đặt
Mở **Command Prompt** và chạy:
```cmd
sqlcmd -S .\SQLEXPRESS -E
```
Hoặc với LocalDB:
```cmd
sqlcmd -S (localdb)\MSSQLLocalDB -E
```

Nếu kết nối thành công, bạn sẽ thấy prompt `1>`. Gõ `exit` để thoát.

## 🔧 Bước 2: Clone và cấu hình dự án

### 2.1. Clone repository
```bash
git clone [URL_REPOSITORY_CỦA_BẠN]
cd BookStore
```

### 2.2. Restore packages
```bash
dotnet restore
```

### 2.3. Cấu hình Connection String

#### Mở file `BookStore.API/appsettings.json`
Tìm và cập nhật connection string phù hợp với SQL Server của bạn:

**Với SQL Server Express:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=BookStoreDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Với SQL Server LocalDB:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=BookStoreDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Với SQL Server có username/password:**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=BookStoreDB;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
  }
}
```

## 🗄️ Bước 3: Thiết lập Database

### Phương pháp 1: Sử dụng Entity Framework Migrations (Khuyến nghị)

1. **Mở Terminal/Command Prompt** và điều hướng đến thư mục `BookStore.API`:
```bash
cd BookStore.API
```

2. **Cài đặt EF Core Tools** (nếu chưa có):
```bash
dotnet tool install --global dotnet-ef
```

3. **Chạy migrations để tạo database:**
```bash
dotnet ef database update
```

Lệnh này sẽ:
- Tự động tạo database `BookStoreDB`
- Tạo tất cả các bảng cần thiết
- Áp dụng tất cả migrations hiện có

### Phương pháp 2: Tự động tạo khi chạy ứng dụng

Dự án BookStore có tính năng tự động tạo database và seed dữ liệu mẫu khi khởi động:

1. Đảm bảo connection string đã được cấu hình đúng
2. Chạy BookStore.API (xem bước 4)
3. Database sẽ được tạo tự động với dữ liệu mẫu

## 🚀 Bước 4: Chạy ứng dụng

### 4.1. Chạy Backend API
Mở Terminal tại thư mục `BookStore.API`:
```bash
cd BookStore.API
dotnet run
```

API sẽ chạy tại: `http://localhost:5274`
Swagger UI: `http://localhost:5274/swagger`

### 4.2. Chạy Frontend Web
Mở Terminal mới tại thư mục `BookStore.Web`:
```bash
cd BookStore.Web
dotnet run
```

Web sẽ chạy tại: `http://localhost:5106`

## 👤 Bước 5: Đăng nhập hệ thống

### Tài khoản Admin mặc định:
- **Email:** `admin@bookstore.com`
- **Password:** `Admin123!`

### Tài khoản User mẫu:
- **Email:** `user@bookstore.com`
- **Password:** `User123!`

## 📊 Cấu trúc Database được tạo

Sau khi setup thành công, database sẽ có các bảng sau:
- **Users** - Quản lý người dùng
- **Categories** - Danh mục sách
- **Authors** - Tác giả
- **Books** - Thông tin sách
- **Orders** - Đơn hàng
- **OrderDetails** - Chi tiết đơn hàng
- **Reviews** - Đánh giá sách
- **Sliders** - Banner slider trang chủ
- **Banners** - Banner quảng cáo
- **Vouchers** - Mã giảm giá
- **HelpArticles** - Bài viết hỗ trợ

## 🔧 Cấu hình bổ sung (Tùy chọn)

### Email Configuration
Để sử dụng tính năng gửi email, cập nhật trong `BookStore.API/appsettings.json`:
```json
{
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "Username": "your-email@gmail.com",
    "Password": "your-app-password",
    "FromEmail": "your-email@gmail.com",
    "FromName": "BookStore - Hệ thống quản lý sách"
  }
}
```

### PayOS Configuration (Thanh toán)
Cập nhật trong `BookStore.Web/appsettings.json`:
```json
{
  "PayOS": {
    "ClientId": "your-client-id",
    "ApiKey": "your-api-key",
    "ChecksumKey": "your-checksum-key",
    "ReturnUrl": "http://localhost:5106/Shop/PaymentReturn",
    "CancelUrl": "http://localhost:5106/Shop/PaymentCancel"
  }
}
```

## ❗ Troubleshooting - Khắc phục sự cố

### 🔴 Lỗi kết nối Database

#### Lỗi: "Cannot connect to SQL Server"
**Nguyên nhân:** SQL Server service chưa chạy hoặc connection string sai.

**Giải pháp:**
1. **Kiểm tra SQL Server service:**
   - Mở **Services** (Win + R → `services.msc`)
   - Tìm **SQL Server (SQLEXPRESS)** hoặc **SQL Server (MSSQLSERVER)**
   - Đảm bảo service đang **Running**

2. **Kiểm tra connection string:**
   - Mở **SQL Server Management Studio**
   - Kết nối thành công với server name
   - Sử dụng chính xác server name đó trong connection string

#### Lỗi: "Login failed for user"
**Nguyên nhân:** Quyền truy cập không đủ.

**Giải pháp:**
1. **Sử dụng Windows Authentication:**
```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=BookStoreDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

2. **Hoặc tạo SQL Server user:**
```sql
CREATE LOGIN bookstore_user WITH PASSWORD = 'YourPassword123!';
CREATE USER bookstore_user FOR LOGIN bookstore_user;
ALTER ROLE db_owner ADD MEMBER bookstore_user;
```

### 🔴 Lỗi Entity Framework

#### Lỗi: "No migrations were applied"
**Giải pháp:**
```bash
cd BookStore.API
dotnet ef migrations add InitialCreate
dotnet ef database update
```

#### Lỗi: "Column 'AvatarUrl' doesn't exist"
**Giải pháp:** Chạy script SQL sau trong SSMS:
```sql
USE BookStoreDB;
GO

IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS
               WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'AvatarUrl')
BEGIN
    ALTER TABLE Users ADD AvatarUrl NVARCHAR(500) NULL;
    PRINT 'AvatarUrl column added successfully.';
END
```

### 🔴 Lỗi khi chạy ứng dụng

#### Lỗi: "Port already in use"
**Giải pháp:**
1. **Thay đổi port trong `launchSettings.json`:**
   - `BookStore.API/Properties/launchSettings.json`
   - `BookStore.Web/Properties/launchSettings.json`

2. **Hoặc kill process đang sử dụng port:**
```cmd
netstat -ano | findstr :5274
taskkill /PID [PID_NUMBER] /F
```

#### Lỗi: "API connection failed"
**Nguyên nhân:** BookStore.Web không kết nối được với BookStore.API.

**Giải pháp:**
1. **Đảm bảo BookStore.API đang chạy** trước khi chạy BookStore.Web
2. **Kiểm tra URL trong `BookStore.Web/appsettings.json`:**
```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:5274/api/"
  }
}
```

### 🔴 Lỗi .NET SDK

#### Lỗi: "The specified framework 'Microsoft.NETCore.App', version '9.0.0' was not found"
**Giải pháp:**
1. Tải và cài đặt **.NET 9.0 SDK** từ [Microsoft](https://dotnet.microsoft.com/download/dotnet/9.0)
2. Kiểm tra version:
```bash
dotnet --version
```

### 🔴 Lỗi packages

#### Lỗi: "Package restore failed"
**Giải pháp:**
```bash
dotnet clean
dotnet restore
dotnet build
```

## 🧪 Kiểm tra cài đặt thành công

### 1. Kiểm tra Database
Mở **SQL Server Management Studio** và kiểm tra:
- Database `BookStoreDB` đã được tạo
- Có 11 bảng trong database
- Bảng `Users` có dữ liệu admin

### 2. Kiểm tra API
Truy cập: `http://localhost:5274/swagger`
- Swagger UI hiển thị đầy đủ endpoints
- Test endpoint `GET /api/Books` trả về danh sách sách

### 3. Kiểm tra Web
Truy cập: `http://localhost:5106`
- Trang chủ hiển thị đúng
- Đăng nhập với tài khoản admin thành công
- Truy cập được trang admin: `http://localhost:5106/Admin`

## 📞 Hỗ trợ

### Nếu vẫn gặp vấn đề:

1. **Kiểm tra logs:**
   - Console output khi chạy `dotnet run`
   - Windows Event Viewer

2. **Kiểm tra cấu hình:**
   - Connection strings đúng format
   - Ports không bị conflict
   - SQL Server services đang chạy

3. **Reset database:**
```bash
cd BookStore.API
dotnet ef database drop
dotnet ef database update
```

4. **Liên hệ team development** với thông tin:
   - OS version
   - .NET version (`dotnet --version`)
   - SQL Server version
   - Error messages đầy đủ

---

## 📝 Ghi chú quan trọng

- **Luôn chạy BookStore.API trước** khi chạy BookStore.Web
- **Backup database** trước khi thực hiện migrations
- **Không commit** file `appsettings.json` có chứa thông tin nhạy cảm
- **Sử dụng HTTPS** trong production environment

---

*Tài liệu này được cập nhật cho BookStore v1.0 - Ngày cập nhật: 27/07/2025*
