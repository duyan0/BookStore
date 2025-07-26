# 🔧 BookStore - Hướng dẫn quản trị hệ thống

## 🎯 Giới thiệu
Hướng dẫn này dành cho Admin để quản lý hệ thống BookStore, bao gồm quản lý sách, đơn hàng, người dùng và nội dung.

## 🔐 Đăng nhập Admin

### Thông tin đăng nhập mặc định:
- **Email:** admin@bookstore.com
- **Mật khẩu:** Admin123!

### Truy cập Admin Panel:
1. Đăng nhập với tài khoản admin
2. Tự động chuyển hướng đến `/Admin`
3. Hoặc truy cập trực tiếp: `http://localhost:5106/Admin`

## 📊 Dashboard

### Thông tin tổng quan:
- **Tổng số sách** trong hệ thống
- **Tổng số đơn hàng** theo trạng thái
- **Doanh thu** theo tháng/năm
- **Người dùng mới** đăng ký

### Biểu đồ:
- **Biểu đồ doanh thu** theo thời gian
- **Biểu đồ đơn hàng** theo trạng thái
- **Top sách bán chạy**

## 📚 Quản lý Sách

### Danh sách sách:
- **Xem tất cả sách** với thông tin cơ bản
- **Tìm kiếm và lọc** theo tên, tác giả, thể loại
- **Sắp xếp** theo ngày tạo, giá, tên
- **Phân trang** để hiển thị nhiều sách

### Thêm sách mới:
1. Click **"Thêm sách mới"**
2. Điền thông tin:
   - **Tên sách** (bắt buộc)
   - **Tác giả** (chọn từ dropdown)
   - **Thể loại** (chọn từ dropdown)
   - **Mô tả** (sử dụng Rich Text Editor)
   - **Giá** (định dạng VND)
   - **Số lượng** trong kho
   - **ISBN, Nhà xuất bản, Năm xuất bản**
3. **Chọn ảnh bìa:**
   - Click **"Chọn từ thư viện"** để dùng ảnh có sẵn
   - Hoặc upload ảnh mới
4. Click **"Lưu"**

### Chỉnh sửa sách:
1. Từ danh sách sách, click **"Sửa"**
2. Cập nhật thông tin cần thiết
3. **Rich Text Editor** cho mô tả với formatting
4. Click **"Cập nhật"**

### Xóa sách:
1. Click **"Xóa"** tại sách cần xóa
2. Xác nhận xóa trong popup
3. **Lưu ý:** Không thể xóa sách đã có trong đơn hàng

## 👥 Quản lý Tác giả

### Thêm tác giả:
1. Vào **"Tác giả"** → **"Thêm mới"**
2. Điền thông tin:
   - **Họ và Tên** (bắt buộc)
   - **Tiểu sử** (Rich Text Editor)
3. Click **"Lưu"**

### Chỉnh sửa tác giả:
- **Cập nhật thông tin** cá nhân
- **Chỉnh sửa tiểu sử** với Rich Text Editor
- **Xem số lượng sách** của tác giả

## 🏷️ Quản lý Thể loại

### Thêm thể loại:
1. Vào **"Thể loại"** → **"Thêm mới"**
2. Điền:
   - **Tên thể loại** (bắt buộc)
   - **Mô tả** thể loại
3. Click **"Lưu"**

### Quản lý thể loại:
- **Xem số lượng sách** trong mỗi thể loại
- **Chỉnh sửa** tên và mô tả
- **Xóa** thể loại (nếu không có sách)

## 🛒 Quản lý Đơn hàng

### Danh sách đơn hàng:
- **Xem tất cả đơn hàng** với trạng thái
- **Lọc theo trạng thái:** Chờ xác nhận, Đã xác nhận, Đang giao, Hoàn thành, Hủy
- **Tìm kiếm** theo mã đơn hàng hoặc email khách hàng

### Xử lý đơn hàng:
1. **Xem chi tiết** đơn hàng
2. **Cập nhật trạng thái:**
   - Chờ xác nhận → Đã xác nhận
   - Đã xác nhận → Đang giao hàng
   - Đang giao hàng → Hoàn thành
3. **Gửi email thông báo** tự động cho khách hàng

### Hủy đơn hàng:
1. Chọn đơn hàng cần hủy
2. Chọn trạng thái **"Hủy"**
3. Nhập lý do hủy
4. Email thông báo hủy được gửi tự động

## 👤 Quản lý Người dùng

### Danh sách người dùng:
- **Xem tất cả users** đã đăng ký
- **Thông tin:** Email, tên, ngày đăng ký, trạng thái
- **Tìm kiếm** theo email hoặc tên

### Quản lý user:
- **Xem chi tiết** thông tin user
- **Xem lịch sử** đơn hàng của user
- **Khóa/Mở khóa** tài khoản user

## 🖼️ Quản lý Hình ảnh

### Image Gallery:
1. Vào **"Thư viện ảnh"**
2. **Xem tất cả ảnh** đã upload
3. **Upload ảnh mới:**
   - Chọn file (JPG, PNG, GIF, WebP)
   - Tối đa 5MB mỗi file
   - Có thể upload nhiều file cùng lúc

### Sử dụng ảnh:
- **Copy URL** để sử dụng ở nơi khác
- **Preview** ảnh trong modal
- **Xóa ảnh** không sử dụng

## 🎠 Quản lý Slider & Banner

### Slider (Trang chủ):
1. **Thêm slider mới:**
   - Tiêu đề và mô tả
   - Chọn ảnh từ thư viện
   - Link đích (tùy chọn)
   - Thứ tự hiển thị
2. **Kích hoạt/Tắt** slider
3. **Sắp xếp thứ tự** hiển thị

### Banner (Quảng cáo):
1. **Thêm banner:**
   - Tiêu đề và nội dung
   - Chọn vị trí hiển thị
   - Thời gian hiển thị (từ - đến)
   - Ảnh banner
2. **Quản lý vị trí:** Header, Sidebar, Footer
3. **Lên lịch** hiển thị banner

## 📧 Quản lý Email

### Cấu hình Email:
Trong `appsettings.json`:
```json
"Email": {
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": "587",
  "Username": "your-email@gmail.com",
  "Password": "your-app-password",
  "FromEmail": "your-email@gmail.com",
  "FromName": "BookStore"
}
```

### Email Templates:
- **Đăng ký tài khoản:** Chào mừng user mới
- **Đặt hàng:** Xác nhận đơn hàng
- **Cập nhật trạng thái:** Thông báo tiến độ
- **Hủy đơn hàng:** Thông báo hủy
- **Đổi mật khẩu:** Xác nhận thay đổi

## 🔧 Cấu hình hệ thống

### Cài đặt chung:
- **Tên website:** BookStore
- **Logo và Favicon**
- **Thông tin liên hệ**
- **Phí giao hàng**

### Cài đặt thanh toán:
- **Phương thức thanh toán** khả dụng
- **Cấu hình payment gateway**
- **Thuế và phí**

## 📊 Báo cáo và Thống kê

### Báo cáo doanh thu:
- **Theo ngày/tháng/năm**
- **Xuất Excel/PDF**
- **Biểu đồ trực quan**

### Báo cáo sản phẩm:
- **Top sách bán chạy**
- **Sách ít bán**
- **Tồn kho thấp**

### Báo cáo khách hàng:
- **Khách hàng mới**
- **Khách hàng VIP** (mua nhiều)
- **Phân tích hành vi mua**

## 🚨 Troubleshooting

### Vấn đề thường gặp:

#### 1. Rich Text Editor không hiển thị:
- Kiểm tra TinyMCE CDN
- Clear browser cache
- Kiểm tra JavaScript console

#### 2. Upload ảnh lỗi:
- Kiểm tra dung lượng file (<5MB)
- Kiểm tra định dạng file
- Kiểm tra quyền ghi thư mục uploads

#### 3. Email không gửi được:
- Kiểm tra Gmail App Password
- Kiểm tra SMTP settings
- Kiểm tra firewall

#### 4. Database lỗi:
- Kiểm tra connection string
- Chạy database migration
- Kiểm tra SQL Server service

## 🔒 Bảo mật Admin

### Best practices:
1. **Đổi mật khẩu admin** định kỳ
2. **Không chia sẻ** thông tin đăng nhập
3. **Logout** sau khi sử dụng
4. **Kiểm tra logs** thường xuyên
5. **Backup database** định kỳ

### Phân quyền:
- **Super Admin:** Toàn quyền
- **Admin:** Quản lý sách, đơn hàng
- **Editor:** Chỉ chỉnh sửa nội dung

## 📋 Checklist cho Admin mới

### Lần đầu setup:
- [ ] Đăng nhập với tài khoản admin mặc định
- [ ] Đổi mật khẩu admin
- [ ] Cấu hình email settings
- [ ] Thêm ít nhất 3 thể loại sách
- [ ] Thêm ít nhất 5 tác giả
- [ ] Upload 10-20 ảnh vào Image Gallery
- [ ] Tạo 5-10 sách mẫu
- [ ] Tạo 2-3 slider cho trang chủ
- [ ] Tạo 1-2 banner quảng cáo
- [ ] Test email notifications
- [ ] Kiểm tra responsive design

### Hàng ngày:
- [ ] Kiểm tra đơn hàng mới
- [ ] Cập nhật trạng thái đơn hàng
- [ ] Trả lời email khách hàng
- [ ] Kiểm tra tồn kho sách
- [ ] Backup database

### Hàng tuần:
- [ ] Thêm sách mới
- [ ] Cập nhật slider/banner
- [ ] Xem báo cáo doanh thu
- [ ] Kiểm tra user feedback
- [ ] Cập nhật giá sách

---
*Hướng dẫn này được cập nhật cho BookStore Admin v1.0*
