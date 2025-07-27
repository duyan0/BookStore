# Tính năng Autocomplete Search và GOONG Maps

## Tổng quan
BookStore hiện đã được tích hợp 2 tính năng autocomplete tiên tiến:

### 1. **Autocomplete Search trong Cửa hàng** 
- **Vị trí**: Trang Shop/Index (http://localhost:5106/Shop)
- **Chức năng**: Tìm kiếm sách thông minh với gợi ý real-time

### 2. **GOONG Maps cho Địa chỉ giao hàng**
- **Vị trí**: Trang Checkout (http://localhost:5106/Shop/Checkout)  
- **Chức năng**: Chọn địa chỉ giao hàng bằng bản đồ và autocomplete

## Chi tiết tính năng

### 🔍 **Autocomplete Search trong Cửa hàng**

#### **Cách sử dụng:**
1. Vào trang cửa hàng (Shop/Index)
2. Nhập ít nhất 2 ký tự vào ô "Tìm kiếm sách"
3. Xem gợi ý xuất hiện real-time
4. Sử dụng phím mũi tên để điều hướng
5. Nhấn Enter hoặc click để chọn sách

#### **Tính năng nổi bật:**
- **Tìm kiếm thông minh**: Theo tên sách, tác giả, danh mục
- **Hiển thị đầy đủ**: Ảnh, giá, giảm giá, tồn kho
- **Điều hướng bàn phím**: Arrow keys, Enter, Escape
- **Debounced search**: Tối ưu hiệu suất (300ms delay)
- **Loading states**: Hiển thị trạng thái tải
- **Error handling**: Xử lý lỗi graceful

#### **Keyboard shortcuts:**
- `↓` / `↑`: Điều hướng trong danh sách gợi ý
- `Enter`: Chọn sách hiện tại
- `Escape`: Đóng danh sách gợi ý
- `Clear button`: Xóa nhanh nội dung tìm kiếm

### 🗺️ **GOONG Maps cho Địa chỉ giao hàng**

#### **Cách sử dụng:**
1. Vào trang thanh toán (Checkout)
2. Nhấn "Chọn trên bản đồ" để mở bản đồ
3. Click vào vị trí trên bản đồ để chọn địa chỉ
4. Hoặc nhập địa chỉ để xem gợi ý autocomplete
5. Sử dụng "Vị trí hiện tại" để định vị GPS

#### **Tính năng nổi bật:**
- **Interactive map**: Click để chọn địa chỉ
- **GPS location**: Tự động định vị hiện tại
- **Address autocomplete**: Gợi ý địa chỉ Việt Nam
- **Reverse geocoding**: Chuyển tọa độ thành địa chỉ
- **Mobile responsive**: Tối ưu cho mobile
- **API status indicator**: Hiển thị trạng thái API

#### **Map controls:**
- `🎯`: Lấy vị trí hiện tại
- `✕`: Đóng bản đồ
- `Zoom controls`: Phóng to/thu nhỏ
- `Drag & drop`: Kéo thả pin trên bản đồ

## Cấu hình API

### **GOONG API Keys**
```javascript
const GOONG_API_KEY = 'cpubDH6sLrBMA06ID8oxJtc9kdgBVKWwwvlqVmji1';
const GOONG_MAPTILES_KEY = 'cpubDH6sLrBMA06ID8oxJtc9kdgBVKWwwvlqVmji1';
```

### **API Status Indicator**
- 🟢 **API sẵn sàng**: API hoạt động bình thường
- 🟡 **API không khả dụng**: Có vấn đề kết nối
- 🔴 **Chưa cấu hình API**: Cần cấu hình API key

### **Services sử dụng:**
- **Place AutoComplete**: Gợi ý địa chỉ
- **Place Detail**: Chi tiết địa điểm
- **Geocoding**: Chuyển đổi tọa độ/địa chỉ
- **Map Tiles**: Hiển thị bản đồ

## Xử lý lỗi

### **Fallback mechanisms:**
1. **Không có internet**: Hiển thị thông báo lỗi
2. **API key sai**: Thông báo cấu hình
3. **API limit**: Graceful degradation
4. **Browser không hỗ trợ**: Fallback manual input

### **Error messages:**
- "Không thể tải bản đồ. Vui lòng kiểm tra kết nối internet."
- "Chưa cấu hình API key cho dịch vụ địa chỉ."
- "Có lỗi xảy ra khi tìm kiếm"
- "Không tìm thấy sản phẩm nào"

## Performance

### **Optimizations:**
- **Debounced requests**: 300ms cho search, 500ms cho address
- **Limited results**: Tối đa 10 kết quả search
- **Lazy loading**: Map chỉ load khi cần
- **Caching**: Browser cache cho static resources

### **Mobile optimizations:**
- **Responsive design**: Tự động điều chỉnh layout
- **Touch-friendly**: Buttons và controls lớn hơn
- **Reduced map height**: 300px trên mobile
- **Optimized API calls**: Giảm số lượng request

## Troubleshooting

### **Search không hoạt động:**
1. Kiểm tra console browser có lỗi không
2. Verify API endpoint `/Shop/SearchProducts`
3. Kiểm tra kết nối internet

### **Map không hiển thị:**
1. Kiểm tra GOONG API keys
2. Verify domain restrictions
3. Check browser console errors
4. Kiểm tra HTTPS (required cho geolocation)

### **Address autocomplete không hoạt động:**
1. Kiểm tra GOONG_API_KEY
2. Verify API quota limits
3. Check network connectivity

## Security Notes

⚠️ **Quan trọng cho Production:**
- Không expose API keys trong client-side code
- Sử dụng environment variables
- Implement server-side proxy cho API calls
- Monitor API usage thường xuyên
- Restrict API keys theo domain

## Browser Support

### **Supported browsers:**
- ✅ Chrome 60+
- ✅ Firefox 55+
- ✅ Safari 12+
- ✅ Edge 79+

### **Required features:**
- ES6 (Arrow functions, const/let)
- Fetch API
- Geolocation API (optional)
- CSS Grid/Flexbox
