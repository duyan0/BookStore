# BookStore Flutter App

Ứng dụng Flutter bán sách trực tuyến tích hợp với API BookStore hiện có.

## 📱 Tính năng

### 👤 Cho User (Khách hàng)
- [x] Xem danh sách sách với layout grid 2 cột trên mobile
- [x] Xem chi tiết sách với giá gốc và giá khuyến mãi
- [x] Tìm kiếm sách theo tên, tác giả, thể loại với search history
- [x] Thêm sách vào giỏ hàng với quantity controls
- [x] Quản lý giỏ hàng (xem, sửa, xóa) với price summary
- [x] Bottom navigation với cart badge
- [x] Home page với featured books và categories
- [x] Profile page với logout functionality
- [ ] Đặt hàng và thanh toán
- [ ] Xem lịch sử đơn hàng

### 👨‍💼 Cho Admin
- [x] Dashboard với drawer navigation
- [x] Role-based routing và access control
- [ ] Quản lý sách (thêm, sửa, xóa)
- [ ] Quản lý tác giả và thể loại
- [ ] Quản lý đơn hàng
- [ ] Xem thống kê bán hàng chi tiết

## 🏗️ Kiến trúc

Ứng dụng được xây dựng theo **Clean Architecture** với cấu trúc:

```
lib/
├── core/                    # Core utilities
│   ├── constants/           # API endpoints, colors, strings
│   ├── errors/             # Error handling
│   ├── network/            # HTTP client setup
│   ├── utils/              # Helpers, formatters
│   └── theme/              # App theme
├── data/                   # Data layer
│   ├── models/             # Data models (DTOs)
│   ├── repositories/       # Repository implementations
│   └── services/           # API services
├── domain/                 # Business logic layer
│   ├── entities/           # Business entities
│   ├── repositories/       # Repository interfaces
│   └── usecases/           # Business use cases
├── presentation/           # UI layer
│   ├── pages/              # Screen widgets
│   ├── widgets/            # Reusable widgets
│   ├── providers/          # State management
│   └── routes/             # Navigation
└── main.dart
```

## 🛠️ Công nghệ sử dụng

- **Flutter SDK**: ^3.7.2
- **State Management**: Provider
- **HTTP Client**: Dio
- **Local Storage**: SharedPreferences + FlutterSecureStorage
- **JSON Serialization**: json_annotation + build_runner
- **Navigation**: Go Router
- **Image Loading**: CachedNetworkImage
- **Currency Formatting**: Intl

## 🚀 Cài đặt và chạy

### Yêu cầu hệ thống
- Flutter SDK 3.7.2 hoặc cao hơn
- Dart SDK 2.19.0 hoặc cao hơn
- Android Studio / VS Code
- API BookStore đang chạy tại `http://localhost:5274`

### Các bước cài đặt

1. **Clone repository**
```bash
git clone <repository-url>
cd bookstore_flutter
```

2. **Cài đặt dependencies**
```bash
flutter pub get
```

3. **Generate code**
```bash
dart run build_runner build
```

4. **Cấu hình API endpoint**
Mở file `lib/core/constants/api_constants.dart` và cập nhật:
```dart
// Cho Android emulator
static const String baseUrl = 'http://10.0.2.2:5274/api';

// Cho iOS simulator
static const String baseUrl = 'http://localhost:5274/api';

// Cho device thật (thay IP thực của máy)
static const String baseUrl = 'http://192.168.1.100:5274/api';
```

5. **Chạy ứng dụng**
```bash
flutter run
```

## 🔧 Cấu hình

### API Configuration
File: `lib/core/constants/api_constants.dart`
- Cập nhật `baseUrl` để trỏ đến API server
- Timeout settings có thể điều chỉnh

### Theme Configuration
File: `lib/core/theme/app_theme.dart`
- Màu sắc theo yêu cầu: trắng, đen, xám đậm
- Typography và component styles

### Currency Formatting
File: `lib/core/utils/currency_formatter.dart`
- Format VND với dấu phẩy: 100,000₫
- Hỗ trợ hiển thị giá khuyến mãi

## 🧪 Testing

### Chạy tests
```bash
# Unit tests
flutter test

# Widget tests
flutter test test/widget_test.dart

# Integration tests (nếu có)
flutter test integration_test/
```

### Test Coverage
```bash
flutter test --coverage
genhtml coverage/lcov.info -o coverage/html
```

## 🔐 Authentication

### Login Credentials (Demo)
- **Admin**: `admin` / `admin123`
- **User**: `user` / `user123`

### Token Management
- JWT tokens được lưu trong FlutterSecureStorage
- Auto-refresh khi token hết hạn
- Role-based access control

## 🎨 UI/UX Design

### Color Scheme
- **Background**: #FFFFFF (trắng)
- **Primary Text**: #000000 (đen)
- **Secondary Text**: #333333, #666666 (xám đậm)
- **Primary Color**: #2196F3 (xanh)
- **Error**: #F44336 (đỏ)

### Typography
- Consistent font sizes và weights
- High contrast cho accessibility
- Vietnamese text support

## 🚧 Roadmap

### Phase 1 (Hoàn thành)
- [x] Project setup và architecture
- [x] Authentication system với JWT
- [x] API integration layer hoàn chỉnh
- [x] Basic UI components và theme
- [x] User screens implementation
- [x] Shopping cart functionality
- [x] Navigation và routing system
- [x] Search với history
- [x] Role-based access control

### Phase 2 (Hoàn thành)
- [x] Order management và checkout hoàn chỉnh
- [x] Admin CRUD operations (UI ready)
- [x] Statistics dashboard với charts
- [x] Order history và tracking

### Phase 3 (Kế hoạch)
- [ ] Push notifications
- [ ] Offline support
- [ ] Performance optimization
- [ ] Advanced testing coverage

## 📱 Tính năng đã implement

### ✅ **User Features**
- **Home Page**: Featured books, categories, search navigation
- **Book List**: Grid 2 cột, category filtering, add to cart
- **Book Detail**: Full info, quantity selector, stock status
- **Search**: Real-time search với history, grid results
- **Cart**: Item management, price summary, quantity controls
- **Checkout**: Complete checkout flow với address form, payment methods
- **Order History**: List orders với status tracking, cancel functionality
- **Order Detail**: Full order information với item details
- **Profile**: User info, logout, navigation to order history
- **Bottom Navigation**: Home, Books, Search, Cart (với badge), Profile

### ✅ **Admin Features**
- **Dashboard**: Quick stats với real-time data, drawer navigation
- **Statistics**: Pie chart cho order status, revenue tracking
- **Book Management**: List view với edit/delete actions
- **Role-based Routing**: Auto redirect based on user role
- **Access Control**: Admin-only routes protection

### ✅ **Technical Features**
- **Authentication**: JWT token management, secure storage
- **State Management**: Provider pattern với multiple providers
- **API Integration**: Complete service layer với error handling
- **Currency Formatting**: VND với dấu phẩy, discount display
- **Image Loading**: Cached network images với placeholders
- **Local Storage**: Cart persistence, search history, user data
- **Navigation**: Go Router với deep linking support
- **Theme**: Consistent design theo yêu cầu UI/UX
- **Charts**: FL Chart integration cho admin statistics
- **Order Management**: Complete order lifecycle từ checkout đến tracking

## 📞 Support

Nếu bạn gặp vấn đề hoặc có câu hỏi, vui lòng tạo issue trên GitHub repository.
