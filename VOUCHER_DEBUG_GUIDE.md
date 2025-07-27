# 🔧 Hướng dẫn Debug Voucher System

## 🎯 Tóm tắt vấn đề đã sửa

### **Vấn đề chính:**
1. **Property Name Mismatch** - Frontend đọc sai tên properties từ API response
2. **Authentication Issue** - API endpoint yêu cầu authentication không cần thiết
3. **Inconsistent Casing** - Các controllers sử dụng casing khác nhau

### **Giải pháp đã áp dụng:**

#### 1. **Sửa Property Names trong Controllers**
```csharp
// BookStore.Web/Controllers/ShopController.cs - FIXED
var result = await _apiService.PostAsync<VoucherValidationResultDto>("vouchers/validate", validationDto);
return Json(new {
    success = result.IsValid,        // ✅ Đúng casing
    message = result.Message,        // ✅ Đúng casing  
    discountAmount = result.DiscountAmount,  // ✅ Đúng casing
    freeShipping = result.FreeShipping      // ✅ Đúng casing
});

// BookStore.Web/Controllers/VouchersController.cs - FIXED
return Json(new { 
    success = result.IsValid,        // ✅ Đã sửa từ result.isValid
    message = result.Message,        // ✅ Đã sửa từ result.message
    discountAmount = result.DiscountAmount,  // ✅ Đã sửa từ result.discountAmount
    freeShipping = result.FreeShipping,     // ✅ Đã sửa từ result.freeShipping
    voucherName = result.Voucher?.Name ?? "",
    voucherType = result.Voucher?.TypeName ?? ""
});
```

#### 2. **Thêm AllowAnonymous cho API Endpoint**
```csharp
// BookStore.API/Controllers/VouchersController.cs - FIXED
[HttpPost("validate")]
[AllowAnonymous] // ✅ Cho phép truy cập không cần authentication
public async Task<ActionResult<VoucherValidationResultDto>> ValidateVoucher(VoucherValidationDto validationDto)
```

#### 3. **Thêm Logging để Debug**
```csharp
// BookStore.Infrastructure/Services/VoucherService.cs - ENHANCED
_logger.LogInformation("Validating voucher: Code={Code}, OrderAmount={OrderAmount}, UserId={UserId}", 
    validationDto.Code, validationDto.OrderAmount, validationDto.UserId);

_logger.LogInformation("Found voucher: Id={Id}, Code={Code}, Type={Type}, Value={Value}, IsActive={IsActive}, IsValid={IsValid}", 
    voucher.Id, voucher.Code, voucher.Type, voucher.Value, voucher.IsActive, voucher.IsValid);
```

## 🧪 Cách test voucher system

### **1. Kiểm tra voucher trong database:**
```sql
SELECT * FROM Vouchers WHERE IsActive = 1;
```

### **2. Test API endpoint trực tiếp:**
```bash
curl -X POST "http://localhost:5274/api/vouchers/validate" \
  -H "Content-Type: application/json" \
  -d '{
    "code": "DISCOUNT10",
    "orderAmount": 500000,
    "userId": 1
  }'
```

### **3. Test từ browser console:**
```javascript
// Mở trang checkout và chạy trong console
fetch('/Shop/ValidateVoucher', {
    method: 'POST',
    headers: {
        'Content-Type': 'application/x-www-form-urlencoded',
        'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
    },
    body: 'voucherCode=DISCOUNT10&orderAmount=500000'
})
.then(response => response.json())
.then(data => console.log('Voucher result:', data))
.catch(error => console.error('Error:', error));
```

## 🔍 Debug checklist

### **Frontend (BookStore.Web):**
- [ ] Kiểm tra JavaScript console có lỗi không
- [ ] Verify AJAX request được gửi đúng format
- [ ] Kiểm tra response từ server có đúng structure không
- [ ] Verify antiforgery token được gửi kèm

### **Backend (BookStore.API):**
- [ ] Kiểm tra logs trong console khi chạy API
- [ ] Verify voucher tồn tại trong database
- [ ] Kiểm tra voucher IsActive = true
- [ ] Verify voucher chưa hết hạn (StartDate <= now <= EndDate)
- [ ] Kiểm tra orderAmount >= MinimumOrderAmount

### **Database:**
- [ ] Voucher code tồn tại và đúng format (uppercase)
- [ ] IsActive = 1
- [ ] StartDate <= current date <= EndDate
- [ ] UsageLimit chưa đạt tối đa (nếu có)
- [ ] UsedCount < UsageLimit (nếu có)

## 📝 Tạo voucher test

### **SQL Script tạo voucher mẫu:**
```sql
INSERT INTO Vouchers (Code, Name, Description, Type, Value, MinimumOrderAmount, MaximumDiscountAmount, UsageLimit, UsedCount, UsageLimitPerUser, StartDate, EndDate, IsActive, CreatedAt)
VALUES 
('DISCOUNT10', 'Giảm 10%', 'Giảm 10% cho đơn hàng từ 200k', 1, 10, 200000, 100000, 100, 0, 5, GETDATE(), DATEADD(month, 1, GETDATE()), 1, GETDATE()),
('SAVE50K', 'Giảm 50k', 'Giảm 50k cho đơn hàng từ 300k', 2, 50000, 300000, NULL, 50, 0, 3, GETDATE(), DATEADD(month, 1, GETDATE()), 1, GETDATE()),
('FREESHIP', 'Miễn phí ship', 'Miễn phí vận chuyển', 3, 1, 100000, NULL, 200, 0, 10, GETDATE(), DATEADD(month, 1, GETDATE()), 1, GETDATE());
```

## 🚨 Các lỗi thường gặp và cách khắc phục

### **1. "Mã voucher không tồn tại"**
- **Nguyên nhân:** Code không match (case sensitive)
- **Giải pháp:** Đảm bảo voucher code được lưu uppercase trong DB

### **2. "Voucher đã hết hạn"**
- **Nguyên nhân:** EndDate < current date
- **Giải pháp:** Cập nhật EndDate trong database

### **3. "Đơn hàng tối thiểu XXX VND"**
- **Nguyên nhân:** OrderAmount < MinimumOrderAmount
- **Giải pháp:** Kiểm tra giá trị orderAmount được gửi từ frontend

### **4. "Có lỗi xảy ra khi kiểm tra voucher"**
- **Nguyên nhân:** Exception trong API hoặc network error
- **Giải pháp:** Kiểm tra logs server và network connectivity

### **5. Response "null" hoặc undefined**
- **Nguyên nhân:** API endpoint không accessible hoặc authentication failed
- **Giải pháp:** Đã thêm [AllowAnonymous] attribute

## 🎯 Kết quả mong đợi

Sau khi áp dụng các fix trên, voucher system sẽ hoạt động như sau:

1. **Nhập mã voucher hợp lệ** → Hiển thị thông báo "Voucher hợp lệ"
2. **Số tiền giảm giá** → Được tính toán và hiển thị đúng
3. **Tổng tiền cuối** → Được cập nhật sau khi trừ discount
4. **Free shipping** → Được áp dụng nếu voucher type = FreeShipping

## 📞 Troubleshooting nâng cao

### **Nếu vẫn gặp lỗi:**

1. **Kiểm tra logs:**
   - BookStore.API console output
   - Browser Developer Tools → Network tab
   - Browser Developer Tools → Console tab

2. **Verify database:**
   ```sql
   SELECT Code, Name, Type, Value, IsActive, StartDate, EndDate, UsageLimit, UsedCount
   FROM Vouchers 
   WHERE Code = 'YOUR_VOUCHER_CODE';
   ```

3. **Test API trực tiếp:**
   - Sử dụng Postman hoặc curl
   - Bypass frontend để test backend logic

4. **Kiểm tra session:**
   - User đã đăng nhập chưa
   - Session có UserId không
   - Token còn valid không

---

*Tài liệu debug được tạo ngày 27/07/2025*
