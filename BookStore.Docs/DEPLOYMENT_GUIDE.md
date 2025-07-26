# 🚀 BookStore Application Deployment Guide

## 🎯 Mục đích
Hướng dẫn deploy BookStore application trên máy mới từ source code.

## 📋 Yêu cầu hệ thống

### Phần mềm cần thiết:
- **.NET 9.0 SDK** - [Download tại đây](https://dotnet.microsoft.com/download/dotnet/9.0)
- **SQL Server** (Express hoặc LocalDB)
- **Visual Studio 2022** hoặc **Visual Studio Code** (tùy chọn)
- **Git** - Để clone source code

### Phần cứng tối thiểu:
- **RAM:** 4GB (khuyến nghị 8GB)
- **Ổ cứng:** 2GB trống
- **CPU:** Dual-core 2.0GHz trở lên

## 📁 Cấu trúc Project

```
BookStore/
├── BookStore.API/          # Web API Backend
├── BookStore.Web/          # MVC Frontend
├── BookStore.Core/         # Business Logic
├── BookStore.Infrastructure/ # Data Access
├── BookStore.Docs/         # Documentation
└── BookStore.Tests/        # Unit Tests
```

## 🔧 Hướng dẫn Deploy từng bước

### 📌 **Bước 1: Chuẩn bị môi trường**

#### 1.1. Cài đặt .NET 9.0 SDK
```bash
# Kiểm tra version đã cài
dotnet --version

# Nếu chưa có, download và cài đặt .NET 9.0 SDK
```

#### 1.2. Clone source code
```bash
git clone [YOUR_REPOSITORY_URL]
cd BookStore
```

#### 1.3. Restore packages
```bash
dotnet restore
```

### 📌 **Bước 2: Cấu hình Database**

#### 2.1. Setup SQL Server
Tham khảo chi tiết trong [DATABASE_SETUP_GUIDE.md](DATABASE_SETUP_GUIDE.md)

#### 2.2. Cấu hình Connection String
Mở `BookStore.API/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=BookStoreDB;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "ThisIsASecretKeyForJwtAuthenticationInBookStoreAPI_VeryVeryLongKeyToSolveIssueWithHmacSha512",
    "Issuer": "BookStoreAPI",
    "Audience": "BookStoreClient"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": "587",
    "Username": "YOUR_EMAIL@gmail.com",
    "Password": "YOUR_APP_PASSWORD",
    "FromEmail": "YOUR_EMAIL@gmail.com",
    "FromName": "BookStore - Hệ thống quản lý sách"
  }
}
```

#### 2.3. Cấu hình API URL cho Web
Mở `BookStore.Web/appsettings.json`:

```json
{
  "ApiSettings": {
    "BaseUrl": "http://localhost:5274/api/"
  }
}
```

### 📌 **Bước 3: Build và chạy Application**

#### 3.1. Build toàn bộ solution
```bash
dotnet build
```

#### 3.2. Chạy API Backend (Terminal 1)
```bash
cd BookStore.API
dotnet run
```
API sẽ chạy tại: `http://localhost:5274`

#### 3.3. Chạy Web Frontend (Terminal 2)
```bash
cd BookStore.Web
dotnet run
```
Web sẽ chạy tại: `http://localhost:5106`

### 📌 **Bước 4: Kiểm tra hoạt động**

#### 4.1. Kiểm tra API
Truy cập: `http://localhost:5274/swagger`
- Xem API documentation
- Test các endpoints

#### 4.2. Kiểm tra Web Application
Truy cập: `http://localhost:5106`
- Trang chủ hiển thị sách
- Đăng nhập admin: `admin@bookstore.com` / `Admin123!`

#### 4.3. Kiểm tra Database
- Mở SSMS
- Kết nối đến database
- Kiểm tra data đã được seed

## 🔄 Chạy trong Production Mode

### Publish API:
```bash
cd BookStore.API
dotnet publish -c Release -o ./publish
```

### Publish Web:
```bash
cd BookStore.Web
dotnet publish -c Release -o ./publish
```

### Chạy published files:
```bash
# API
cd BookStore.API/publish
dotnet BookStore.API.dll

# Web
cd BookStore.Web/publish
dotnet BookStore.Web.dll
```

## 🌐 Deploy lên IIS (Windows Server)

### Bước 1: Cài đặt IIS và ASP.NET Core Module
1. Enable IIS trong Windows Features
2. Cài đặt ASP.NET Core Hosting Bundle

### Bước 2: Publish applications
```bash
dotnet publish -c Release -o C:\inetpub\wwwroot\BookStoreAPI
dotnet publish -c Release -o C:\inetpub\wwwroot\BookStoreWeb
```

### Bước 3: Tạo IIS Sites
1. Tạo site cho API (port 5274)
2. Tạo site cho Web (port 5106)
3. Cấu hình Application Pool (.NET Core)

## 🐳 Deploy với Docker

### Dockerfile cho API:
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["BookStore.API/BookStore.API.csproj", "BookStore.API/"]
RUN dotnet restore "BookStore.API/BookStore.API.csproj"
COPY . .
WORKDIR "/src/BookStore.API"
RUN dotnet build "BookStore.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BookStore.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookStore.API.dll"]
```

### Docker Compose:
```yaml
version: '3.8'
services:
  bookstore-api:
    build: 
      context: .
      dockerfile: BookStore.API/Dockerfile
    ports:
      - "5274:80"
    environment:
      - ConnectionStrings__DefaultConnection=Server=sql-server;Database=BookStoreDB;User Id=sa;Password=YourPassword123!
    depends_on:
      - sql-server

  bookstore-web:
    build:
      context: .
      dockerfile: BookStore.Web/Dockerfile
    ports:
      - "5106:80"
    environment:
      - ApiSettings__BaseUrl=http://bookstore-api/api/
    depends_on:
      - bookstore-api

  sql-server:
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourPassword123!
    ports:
      - "1433:1433"
```

## 🚨 Troubleshooting

### Lỗi thường gặp:

#### 1. "Unable to connect to database"
- Kiểm tra SQL Server đang chạy
- Kiểm tra connection string
- Kiểm tra firewall settings

#### 2. "API not responding"
- Kiểm tra port 5274 không bị block
- Kiểm tra CORS settings
- Xem logs trong console

#### 3. "Email not sending"
- Kiểm tra Gmail App Password
- Kiểm tra SMTP settings
- Kiểm tra internet connection

#### 4. "JWT Token errors"
- Kiểm tra JWT Key trong appsettings.json
- Đảm bảo key đủ dài (>32 characters)

## 📊 Monitoring và Logs

### Xem logs:
```bash
# API logs
cd BookStore.API
dotnet run --verbosity detailed

# Web logs
cd BookStore.Web
dotnet run --verbosity detailed
```

### Log files location:
- **Development:** Console output
- **Production:** Windows Event Log hoặc file logs

---
*Tài liệu này được cập nhật cho BookStore v1.0*
