# 📊 BookStore Database Setup Guide

## 🎯 Mục đích
Hướng dẫn này giúp bạn thiết lập database cho BookStore application trên máy mới.

## 📋 Yêu cầu hệ thống

### Phần mềm cần thiết:
- **SQL Server** (SQL Server Express 2019 trở lên hoặc SQL Server LocalDB)
- **SQL Server Management Studio (SSMS)** - Khuyến nghị
- **.NET 9.0 SDK** - Để chạy Entity Framework migrations

## 🔧 Các phương pháp setup Database

### 📌 **Phương pháp 1: Sử dụng SQL Server (Khuyến nghị)**

#### Bước 1: Cài đặt SQL Server
1. Tải và cài đặt **SQL Server Express** từ Microsoft
2. Hoặc cài đặt **SQL Server LocalDB** (nhẹ hơn)
3. Cài đặt **SQL Server Management Studio (SSMS)**

#### Bước 2: Tạo Database
1. Mở **SQL Server Management Studio**
2. Kết nối đến SQL Server instance của bạn
3. Tạo database mới:
```sql
CREATE DATABASE BookStoreDB;
```

#### Bước 3: Cấu hình Connection String
Mở file `BookStore.API/appsettings.json` và cập nhật:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER_NAME;Database=BookStoreDB;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

**Ví dụ các connection string phổ biến:**

- **SQL Server Express:**
```json
"DefaultConnection": "Server=.\\SQLEXPRESS;Database=BookStoreDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

- **SQL Server LocalDB:**
```json
"DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=BookStoreDB;Trusted_Connection=True;TrustServerCertificate=True;"
```

- **SQL Server với username/password:**
```json
"DefaultConnection": "Server=YOUR_SERVER;Database=BookStoreDB;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
```

### 📌 **Phương pháp 2: Sử dụng Entity Framework Migrations**

#### Bước 1: Mở Command Prompt/PowerShell
Điều hướng đến thư mục `BookStore.API`:
```bash
cd path/to/BookStore.API
```

#### Bước 2: Chạy Database Migration
```bash
dotnet ef database update
```

Nếu gặp lỗi, thử:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 📌 **Phương pháp 3: Automatic Database Creation**

BookStore application có tính năng **tự động tạo database** khi khởi động:

1. Đảm bảo connection string đúng trong `appsettings.json`
2. Chạy BookStore.API application
3. Database sẽ được tạo tự động với sample data

## 🗃️ Cấu trúc Database

### Các bảng chính:
- **Users** - Thông tin người dùng
- **Categories** - Danh mục sách
- **Authors** - Tác giả
- **Books** - Thông tin sách
- **Orders** - Đơn hàng
- **OrderDetails** - Chi tiết đơn hàng
- **Reviews** - Đánh giá sách
- **Sliders** - Banner slider trang chủ
- **Banners** - Banner quảng cáo

### Sample Data
Application sẽ tự động tạo:
- **Admin user:** admin@bookstore.com / Admin123!
- **Sample categories:** Văn học, Khoa học, Lịch sử, etc.
- **Sample authors và books**
- **Sample sliders và banners**

## 🔍 Kiểm tra Database

### Sử dụng SSMS:
1. Mở SQL Server Management Studio
2. Kết nối đến server
3. Expand database `BookStoreDB`
4. Kiểm tra các bảng đã được tạo

### Sử dụng Command Line:
```bash
sqlcmd -S YOUR_SERVER -d BookStoreDB -Q "SELECT COUNT(*) FROM Users"
```

## 🚨 Troubleshooting

### Lỗi thường gặp:

#### 1. "Cannot connect to server"
- Kiểm tra SQL Server service đang chạy
- Kiểm tra server name trong connection string
- Đảm bảo SQL Server Browser service đang chạy

#### 2. "Login failed"
- Kiểm tra username/password
- Sử dụng Windows Authentication nếu có thể
- Kiểm tra user có quyền truy cập database

#### 3. "Database does not exist"
- Tạo database thủ công bằng SSMS
- Hoặc để application tự tạo khi chạy lần đầu

#### 4. "Invalid column name 'AvatarUrl'"
- Chạy migration update:
```bash
dotnet ef database update
```

### Script khắc phục nhanh:
Nếu gặp vấn đề với migrations, chạy script sau trong SSMS:

```sql
USE BookStoreDB;
GO

-- Add missing AvatarUrl column if not exists
IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
               WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'AvatarUrl')
BEGIN
    ALTER TABLE Users ADD AvatarUrl NVARCHAR(500) NULL;
    PRINT 'AvatarUrl column added successfully.';
END
```

## 📞 Hỗ trợ

Nếu gặp vấn đề, kiểm tra:
1. **Connection string** đúng format
2. **SQL Server service** đang chạy
3. **Firewall** không block kết nối
4. **User permissions** đủ quyền tạo database

---
*Tài liệu này được cập nhật cho BookStore v1.0*
