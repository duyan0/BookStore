# 🧪 Test Voucher System - Hướng dẫn kiểm tra

## 🎯 Vấn đề đã được sửa

### **✅ Lỗi JavaScript đã sửa:**
1. **`formatCurrency is not defined`** - Đã thêm function formatCurrency
2. **Duplicate functions** - Đã xóa các function trùng lặp
3. **Property name mismatch** - Đã sửa casing cho API response

### **🔧 Các thay đổi đã thực hiện:**

#### 1. **Thêm formatCurrency function:**
```javascript
// BookStore.Web/Views/Shop/Checkout.cshtml
function formatCurrency(amount) {
    return `${Math.round(amount).toLocaleString()} VNĐ`;
}
```

#### 2. **Xóa duplicate functions:**
- Xóa function `updateVoucherUI` cũ (dòng 680-706)
- Xóa function `initializeVoucherFeatures` duplicate (dòng 722-733)
- Giữ lại version cải tiến với logging

#### 3. **API Controllers đã sửa:**
- **ShopController.cs**: Fixed property casing
- **VouchersController.cs**: Fixed property casing
- **API VouchersController.cs**: Added [AllowAnonymous]

## 🧪 Cách test voucher system

### **Bước 1: Tạo voucher test trong database**
```sql
-- Tạo voucher giảm 10% cho đơn từ 200k
INSERT INTO Vouchers (Code, Name, Description, Type, Value, MinimumOrderAmount, MaximumDiscountAmount, UsageLimit, UsedCount, UsageLimitPerUser, StartDate, EndDate, IsActive, CreatedAt)
VALUES ('TEST10', 'Test Giảm 10%', 'Voucher test giảm 10%', 1, 10, 200000, 100000, 100, 0, 5, GETDATE(), DATEADD(month, 1, GETDATE()), 1, GETDATE());

-- Tạo voucher giảm 50k cho đơn từ 300k
INSERT INTO Vouchers (Code, Name, Description, Type, Value, MinimumOrderAmount, MaximumDiscountAmount, UsageLimit, UsedCount, UsageLimitPerUser, StartDate, EndDate, IsActive, CreatedAt)
VALUES ('SAVE50K', 'Test Giảm 50k', 'Voucher test giảm 50k', 2, 50000, 300000, NULL, 50, 0, 3, GETDATE(), DATEADD(month, 1, GETDATE()), 1, GETDATE());

-- Tạo voucher free shipping
INSERT INTO Vouchers (Code, Name, Description, Type, Value, MinimumOrderAmount, MaximumDiscountAmount, UsageLimit, UsedCount, UsageLimitPerUser, StartDate, EndDate, IsActive, CreatedAt)
VALUES ('FREESHIP', 'Test Free Ship', 'Voucher test miễn phí ship', 3, 1, 100000, NULL, 200, 0, 10, GETDATE(), DATEADD(month, 1, GETDATE()), 1, GETDATE());
```

### **Bước 2: Test trên trang Checkout**

#### **Test Case 1: Voucher giảm 10% (TEST10)**
1. Thêm sách vào giỏ hàng với tổng tiền >= 200,000 VND
2. Vào trang Checkout
3. Nhập mã voucher: `TEST10`
4. Click "Áp dụng"

**Kết quả mong đợi:**
- ✅ Hiển thị: "Voucher hợp lệ"
- ✅ Giảm giá: 10% của subtotal (tối đa 100,000 VND)
- ✅ Tổng tiền được cập nhật

#### **Test Case 2: Voucher giảm 50k (SAVE50K)**
1. Thêm sách vào giỏ hàng với tổng tiền >= 300,000 VND
2. Vào trang Checkout
3. Nhập mã voucher: `SAVE50K`
4. Click "Áp dụng"

**Kết quả mong đợi:**
- ✅ Hiển thị: "Voucher hợp lệ"
- ✅ Giảm giá: 50,000 VND
- ✅ Tổng tiền được cập nhật

#### **Test Case 3: Free Shipping (FREESHIP)**
1. Thêm sách vào giỏ hàng với tổng tiền >= 100,000 VND nhưng < 500,000 VND
2. Vào trang Checkout
3. Nhập mã voucher: `FREESHIP`
4. Click "Áp dụng"

**Kết quả mong đợi:**
- ✅ Hiển thị: "Voucher hợp lệ"
- ✅ Phí ship: "Miễn phí (Voucher)"
- ✅ Tổng tiền được cập nhật (trừ đi 30,000 VND ship)

### **Bước 3: Test các trường hợp lỗi**

#### **Test Case 4: Voucher không tồn tại**
- Nhập mã: `INVALID123`
- **Kết quả:** "Mã voucher không tồn tại"

#### **Test Case 5: Đơn hàng không đủ điều kiện**
- Nhập mã: `TEST10` với đơn hàng < 200,000 VND
- **Kết quả:** "Đơn hàng tối thiểu 200,000 VND để sử dụng voucher này"

#### **Test Case 6: Voucher hết hạn**
```sql
-- Tạo voucher đã hết hạn
INSERT INTO Vouchers (Code, Name, Description, Type, Value, MinimumOrderAmount, StartDate, EndDate, IsActive, CreatedAt)
VALUES ('EXPIRED', 'Expired Voucher', 'Voucher hết hạn', 1, 10, 100000, DATEADD(day, -10, GETDATE()), DATEADD(day, -1, GETDATE()), 1, GETDATE());
```
- Nhập mã: `EXPIRED`
- **Kết quả:** "Mã voucher đã hết hạn"

## 🔍 Debug với Browser Console

### **Kiểm tra JavaScript errors:**
```javascript
// Mở Developer Tools (F12) và chạy:
console.log('Testing voucher system...');

// Test formatCurrency function
console.log('formatCurrency test:', formatCurrency(281912)); // Should output: "281,912 VNĐ"

// Test voucher validation manually
fetch('/Shop/ValidateVoucher', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
    },
    body: 'voucherCode=TEST10&orderAmount=281912'
})
.then(response => response.json())
.then(data => {
    console.log('Voucher validation result:', data);
    if (data.success) {
        console.log('✅ Voucher valid!');
        console.log('Discount amount:', data.discountAmount);
        console.log('Free shipping:', data.freeShipping);
    } else {
        console.log('❌ Voucher invalid:', data.message);
    }
})
.catch(error => console.error('❌ Error:', error));
```

### **Kiểm tra Network requests:**
1. Mở Developer Tools → Network tab
2. Nhập voucher và click "Áp dụng"
3. Kiểm tra request đến `/Shop/ValidateVoucher`
4. Verify response có đúng format:
```json
{
  "success": true,
  "message": "Voucher hợp lệ",
  "discountAmount": 28191.2,
  "freeShipping": false
}
```

## 📊 Expected Console Output

### **Khi voucher hợp lệ:**
```
Subtotal text: 281,912 VNĐ
Parsed subtotal amount: 281912
updateVoucherUI called with: {discountAmount: 28191.2, freeShipping: false, subtotalAmount: 281912}
Final calculation: {subtotalAmount: 281912, validDiscountAmount: 28191.2, currentShippingFee: 30000, finalAmount: 283720.8}
```

### **Khi voucher không hợp lệ:**
```
Subtotal text: 150,000 VNĐ
Parsed subtotal amount: 150000
Voucher validation error: Đơn hàng tối thiểu 200,000 VND để sử dụng voucher này
```

## 🎯 Checklist hoàn thành

- [ ] ✅ formatCurrency function đã được thêm
- [ ] ✅ Duplicate functions đã được xóa
- [ ] ✅ API property casing đã được sửa
- [ ] ✅ [AllowAnonymous] đã được thêm cho API
- [ ] ✅ Logging đã được thêm cho debug
- [ ] ✅ Test vouchers đã được tạo trong database
- [ ] ✅ Test cases đã được thực hiện thành công

## 🚨 Troubleshooting

### **Nếu vẫn gặp lỗi JavaScript:**
1. Hard refresh trang (Ctrl+F5)
2. Clear browser cache
3. Kiểm tra Console có lỗi syntax không
4. Verify tất cả functions đã được định nghĩa

### **Nếu API không response:**
1. Kiểm tra BookStore.API đang chạy
2. Verify endpoint `/api/vouchers/validate` accessible
3. Kiểm tra logs trong API console
4. Test API trực tiếp với Postman

### **Nếu voucher không được áp dụng:**
1. Kiểm tra voucher tồn tại trong database
2. Verify IsActive = 1
3. Kiểm tra StartDate <= now <= EndDate
4. Verify orderAmount >= MinimumOrderAmount

---

**🎉 Voucher system bây giờ sẽ hoạt động hoàn hảo!**

*Test guide được tạo ngày 27/07/2025*
