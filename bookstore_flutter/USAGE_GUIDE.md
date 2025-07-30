# 📖 Hướng dẫn sử dụng BookStore Flutter App

## 🚀 Khởi chạy ứng dụng

### 1. Chuẩn bị môi trường
```bash
# Kiểm tra Flutter version
flutter --version

# Cài đặt dependencies
flutter pub get

# Generate code
dart run build_runner build
```

### 2. Cấu hình API
Mở file `lib/core/constants/api_constants.dart` và cập nhật baseUrl:

```dart
// Cho Android emulator
static const String baseUrl = 'http://10.0.2.2:5274/api';

// Cho iOS simulator  
static const String baseUrl = 'http://localhost:5274/api';

// Cho device thật (thay IP của máy)
static const String baseUrl = 'http://192.168.1.100:5274/api';
```

### 3. Chạy ứng dụng
```bash
# Debug mode
flutter run

# Release mode
flutter run --release

# Chọn device cụ thể
flutter run -d <device-id>
```

## 👤 Hướng dẫn cho User (Khách hàng)

### Đăng nhập
1. Mở ứng dụng, màn hình login sẽ hiển thị
2. Nhập thông tin đăng nhập demo:
   - **Username**: `user`
   - **Password**: `user123`
3. Nhấn "Đăng nhập"

### Trang chủ (Home)
- **Featured Books**: Xem sách nổi bật, vuốt ngang để xem thêm
- **Categories**: Nhấn vào thể loại để lọc sách
- **Search Bar**: Nhấn để chuyển đến trang tìm kiếm
- **Notifications**: Icon thông báo (chưa implement)

### Danh sách sách (Books)
- **Grid Layout**: Hiển thị 2 cột trên mobile
- **Category Filter**: Chọn "Tất cả" hoặc thể loại cụ thể
- **Add to Cart**: Nhấn nút "Thêm vào giỏ" trên mỗi sách
- **Book Detail**: Nhấn vào sách để xem chi tiết
- **Pull to Refresh**: Kéo xuống để làm mới danh sách

### Chi tiết sách (Book Detail)
- **Image**: Ảnh sách full screen với discount badge
- **Info**: Tên sách, tác giả, thể loại, ISBN, nhà xuất bản
- **Price**: Giá hiện tại và giá gốc (nếu có khuyến mãi)
- **Stock**: Trạng thái còn hàng/hết hàng
- **Quantity**: Chọn số lượng với +/- buttons
- **Add to Cart**: Thêm vào giỏ với số lượng đã chọn

### Tìm kiếm (Search)
- **Search Bar**: Nhập từ khóa và nhấn Enter hoặc icon search
- **Search History**: Lịch sử tìm kiếm được lưu tự động
- **Results**: Hiển thị kết quả dạng grid 2 cột
- **Clear**: Xóa từ khóa để quay lại lịch sử tìm kiếm

### Giỏ hàng (Cart)
- **Items List**: Danh sách sách đã thêm
- **Quantity Controls**: +/- để thay đổi số lượng
- **Remove**: Icon thùng rác để xóa sách
- **Price Summary**: Tạm tính, phí ship, tổng cộng
- **Clear All**: Xóa tất cả sách trong giỏ
- **Checkout**: Nhấn để tiến hành thanh toán

### Thanh toán (Checkout)
- **Order Summary**: Tóm tắt đơn hàng với số lượng và giá
- **Shipping Address**: Form nhập địa chỉ giao hàng (required)
- **Payment Methods**: Chọn phương thức thanh toán (COD, Bank Transfer, MoMo, ZaloPay)
- **Order Note**: Ghi chú cho đơn hàng (optional)
- **Place Order**: Xác nhận đặt hàng và chuyển đến order detail

### Lịch sử đơn hàng (Order History)
- **Orders List**: Danh sách tất cả đơn hàng với status chips
- **Order Info**: Mã đơn hàng, ngày đặt, số sản phẩm, tổng tiền
- **Status Tracking**: Theo dõi trạng thái đơn hàng (Pending, Processing, Shipped, Delivered, Cancelled)
- **Cancel Order**: Hủy đơn hàng (chỉ với status Pending/Processing)
- **Order Detail**: Nhấn "Chi tiết" để xem thông tin đầy đủ

### Chi tiết đơn hàng (Order Detail)
- **Order Status**: Trạng thái hiện tại với timestamp
- **Order Info**: Mã đơn hàng, khách hàng, địa chỉ giao hàng, phương thức thanh toán
- **Items List**: Danh sách sản phẩm với hình ảnh, tên, số lượng, giá
- **Payment Summary**: Tạm tính, phí vận chuyển, tổng cộng
- **Cancel Button**: Nút hủy đơn hàng (nếu có thể)

### Hồ sơ (Profile)
- **User Info**: Avatar, tên, email
- **Order History**: Xem lịch sử đơn hàng
- **Settings**: Cài đặt ứng dụng (chưa implement)
- **Logout**: Đăng xuất khỏi ứng dụng

## 👨‍💼 Hướng dẫn cho Admin

### Đăng nhập Admin
1. Nhập thông tin đăng nhập admin:
   - **Username**: `admin`
   - **Password**: `admin123`
2. Sau khi đăng nhập, sẽ tự động chuyển đến Admin Dashboard

### Admin Dashboard
- **Drawer Navigation**: Menu bên trái với các chức năng quản lý
- **Quick Stats**: Thẻ thống kê real-time (Tổng sách, Đơn hàng, Doanh thu, Sách bán chạy)
- **Order Statistics Chart**: Pie chart hiển thị phân bố trạng thái đơn hàng
- **Navigation**: Nhấn vào thẻ để chuyển đến trang quản lý tương ứng

### Menu Admin (Drawer)
- **Dashboard**: Trang chủ admin
- **Quản lý sách**: List view với edit/delete actions, add new book button
- **Quản lý tác giả**: CRUD operations cho tác giả (UI placeholder)
- **Quản lý thể loại**: CRUD operations cho thể loại (UI placeholder)
- **Quản lý đơn hàng**: Xem và cập nhật đơn hàng (UI placeholder)
- **Logout**: Đăng xuất

## 🔧 Tính năng kỹ thuật

### State Management
- **AuthProvider**: Quản lý trạng thái đăng nhập
- **BookProvider**: Quản lý danh sách sách, tìm kiếm, filter
- **CategoryProvider**: Quản lý danh sách thể loại
- **CartProvider**: Quản lý giỏ hàng, persistence
- **OrderProvider**: Quản lý orders, statistics, status tracking

### Local Storage
- **Secure Storage**: JWT token
- **SharedPreferences**: User data, cart data, search history
- **Auto-persistence**: Cart và search history tự động lưu

### Navigation
- **Role-based Routing**: Tự động redirect theo role user
- **Deep Linking**: Support navigation với parameters
- **Go Router**: Modern navigation với type-safe routes

### API Integration
- **Dio HTTP Client**: Với interceptors cho auth và logging
- **Error Handling**: Comprehensive error handling
- **Loading States**: Loading indicators cho tất cả API calls

### UI/UX Features
- **Responsive Design**: Tối ưu cho mobile và tablet
- **Pull-to-Refresh**: Làm mới dữ liệu
- **Loading States**: Shimmer effects và progress indicators
- **Error States**: User-friendly error messages
- **Empty States**: Thông báo khi không có dữ liệu
- **Confirmation Dialogs**: Xác nhận cho các action quan trọng

## 🐛 Troubleshooting

### Lỗi kết nối API
1. Kiểm tra API server đang chạy tại port 5274
2. Cập nhật đúng IP address trong `api_constants.dart`
3. Kiểm tra network permissions trong `android/app/src/main/AndroidManifest.xml`

### Lỗi build
```bash
# Clean và rebuild
flutter clean
flutter pub get
dart run build_runner build --delete-conflicting-outputs
flutter run
```

### Lỗi hot reload
```bash
# Restart app
flutter run --hot-restart
```

### Debug network issues
- Sử dụng Android emulator với IP `10.0.2.2`
- Hoặc iOS simulator với IP `localhost`
- Device thật cần IP thực của máy host

## 📝 Development Notes

### Thêm tính năng mới
1. Tạo model trong `data/models/`
2. Tạo service trong `data/services/`
3. Tạo provider trong `presentation/providers/`
4. Tạo UI trong `presentation/pages/`
5. Cập nhật routing trong `presentation/routes/`

### Code Generation
```bash
# Sau khi thay đổi models
dart run build_runner build --delete-conflicting-outputs
```

### Testing
```bash
# Unit tests
flutter test

# Widget tests
flutter test test/widget_test.dart
```

## 🔮 Tính năng sắp tới

### Phase 2
- [ ] Checkout và payment integration
- [ ] Order management hoàn chỉnh
- [ ] Admin CRUD operations
- [ ] Statistics dashboard

### Phase 3
- [ ] Push notifications
- [ ] Offline support
- [ ] Advanced search filters
- [ ] User reviews và ratings
