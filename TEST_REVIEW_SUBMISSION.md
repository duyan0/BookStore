# 🧪 Test Review Submission - Hướng dẫn kiểm tra

## 🎯 Các lỗi đã được sửa

### **✅ Cải thiện Error Handling:**
1. **Authentication Check** - Kiểm tra user đã đăng nhập trước khi submit
2. **Detailed Error Messages** - Phân biệt các loại lỗi khác nhau
3. **Better Logging** - Log chi tiết request/response để debug
4. **JavaScript Debug** - Console logs để theo dõi quá trình

### **✅ Các thay đổi chính:**

#### **1. ReviewsController.cs:**
- Thêm check `IsUserLoggedIn()` trước khi xử lý
- Phân biệt lỗi 401 (Unauthorized) và 400 (Bad Request)
- Xử lý lỗi "Bạn đã đánh giá sách này rồi"
- Redirect đúng đến `Details` thay vì `Book`
- Thêm action `DebugSession` để kiểm tra authentication

#### **2. ApiService.cs:**
- Log chi tiết request và response
- Parse error message từ API response
- Better exception handling với specific error types

#### **3. Create.cshtml:**
- Thêm JavaScript debug information
- Check session trước khi submit
- Loading state khi submit form
- Validation phía client tốt hơn

## 🧪 Cách test hệ thống

### **Bước 1: Kiểm tra Authentication**

#### **A. Test session debug:**
```javascript
// Mở browser console trên trang Create Review và chạy:
fetch('/Reviews/DebugSession')
    .then(response => response.json())
    .then(data => {
        console.log('Session Debug:', data);
        if (!data.HasToken) {
            console.error('❌ No authentication token found!');
        } else {
            console.log('✅ Token found');
            console.log('User ID:', data.UserId);
            console.log('Username:', data.Username);
        }
    });
```

#### **B. Kết quả mong đợi:**
```json
{
  "HasToken": true,
  "TokenLength": 200,
  "UserId": 1,
  "Username": "admin",
  "IsLoggedIn": true,
  "SessionId": "abc123...",
  "TokenPreview": "eyJhbGciOiJIUzUxMiIs..."
}
```

### **Bước 2: Test Review Submission**

#### **A. Chuẩn bị dữ liệu:**
1. **Đăng nhập** với tài khoản user (không phải admin)
2. **Chọn một cuốn sách** chưa được đánh giá
3. **Vào trang Create Review:** `/Reviews/Create?bookId=1`

#### **B. Test cases:**

##### **Test Case 1: Submit thành công**
```
Input:
- Rating: 5 sao
- Comment: "Cuốn sách rất hay, tôi rất thích!"

Expected Result:
✅ TempData["Success"] = "Đánh giá của bạn đã được gửi và đang chờ duyệt!"
✅ Redirect to /Shop/Details/1
✅ Review được lưu vào database với Status = Pending
```

##### **Test Case 2: User chưa đăng nhập**
```
Input:
- Clear session hoặc logout
- Truy cập /Reviews/Create?bookId=1

Expected Result:
✅ TempData["Warning"] = "Vui lòng đăng nhập để gửi đánh giá."
✅ Redirect to /Account/Login
```

##### **Test Case 3: User đã đánh giá sách này**
```
Input:
- User đã có review cho sách này trong database
- Submit review mới

Expected Result:
✅ TempData["Warning"] = "Bạn đã đánh giá sách này rồi."
✅ Redirect to /Shop/Details/1
```

##### **Test Case 4: Validation errors**
```
Input:
- Rating: không chọn
- Comment: rỗng

Expected Result:
✅ ModelState.IsValid = false
✅ Return View(model) với validation errors
✅ Không gọi API
```

##### **Test Case 5: Token hết hạn**
```
Input:
- Token expired trong session
- Submit review

Expected Result:
✅ TempData["Warning"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại."
✅ Redirect to /Account/Login
✅ Session được clear
```

### **Bước 3: Kiểm tra Database**

#### **A. Trước khi submit:**
```sql
-- Kiểm tra user chưa có review cho sách này
SELECT * FROM Reviews WHERE UserId = 1 AND BookId = 1;
-- Kết quả: 0 rows
```

#### **B. Sau khi submit thành công:**
```sql
-- Kiểm tra review đã được tạo
SELECT TOP 1 * FROM Reviews 
WHERE UserId = 1 AND BookId = 1 
ORDER BY CreatedAt DESC;

-- Kết quả mong đợi:
-- Id: [auto-generated]
-- BookId: 1
-- UserId: 1
-- Rating: 5
-- Comment: "Cuốn sách rất hay, tôi rất thích!"
-- Status: 0 (Pending)
-- IsVerifiedPurchase: true/false (tùy user đã mua chưa)
-- CreatedAt: [current timestamp]
```

### **Bước 4: Kiểm tra Logs**

#### **A. BookStore.API Console:**
```
info: BookStore.API.Controllers.ReviewsController[0]
      Creating review for BookId: 1, UserId: 1
info: BookStore.Infrastructure.Services.ReviewService[0]
      Review created successfully with Id: 123
```

#### **B. BookStore.Web Console:**
```
info: BookStore.Web.Services.ApiService[0]
      Making POST request to: http://localhost:5274/api/reviews
info: BookStore.Web.Services.ApiService[0]
      Request body: {"BookId":1,"Rating":5,"Comment":"Cuốn sách rất hay!"}
info: BookStore.Web.Services.ApiService[0]
      Response status: Created
info: BookStore.Web.Controllers.ReviewsController[0]
      Creating review for book 1 by user 1
```

#### **C. Browser Console:**
```
Create Review Page Loaded
Book ID: 1
User logged in: true
Session Debug: {HasToken: true, UserId: 1, ...}
✅ Token found, length: 200
Rating selected: 5
Form submitting with data: {BookId: "1", Rating: "5", Comment: "Cuốn sách rất hay!"}
```

## 🚨 Troubleshooting

### **Vấn đề 1: "No authentication token found"**
**Nguyên nhân:** User chưa đăng nhập hoặc session expired
**Giải pháp:**
1. Đăng nhập lại
2. Kiểm tra session timeout settings
3. Verify JWT token generation

### **Vấn đề 2: "API POST call failed with status 401"**
**Nguyên nhân:** Token không hợp lệ hoặc hết hạn
**Giải pháp:**
1. Check token format và expiration
2. Verify JWT secret key consistency
3. Check API authentication middleware

### **Vấn đề 3: "Bạn đã đánh giá sách này rồi"**
**Nguyên nhân:** User đã có review trong database
**Giải pháp:**
1. Check database: `SELECT * FROM Reviews WHERE UserId = X AND BookId = Y`
2. Redirect user to edit existing review
3. Or allow multiple reviews per user (business decision)

### **Vấn đề 4: Form submit không có phản hồi**
**Nguyên nhân:** JavaScript error hoặc network issue
**Giải pháp:**
1. Check browser console for errors
2. Verify network connectivity
3. Check antiforgery token

### **Vấn đề 5: Review không xuất hiện trong MyReviews**
**Nguyên nhân:** Review có Status = Pending, chưa được approve
**Giải pháp:**
1. Check review status in database
2. Admin approve review trong admin panel
3. Or modify query to show pending reviews

## 🎯 Checklist hoàn thành

### **Frontend:**
- [ ] ✅ User có thể truy cập trang Create Review
- [ ] ✅ Form validation hoạt động đúng
- [ ] ✅ JavaScript debug logs hiển thị
- [ ] ✅ Loading state khi submit
- [ ] ✅ Error messages hiển thị rõ ràng

### **Backend:**
- [ ] ✅ Authentication check hoạt động
- [ ] ✅ API call được log chi tiết
- [ ] ✅ Error handling phân biệt các loại lỗi
- [ ] ✅ Review được lưu vào database
- [ ] ✅ Redirect đúng sau khi submit

### **Database:**
- [ ] ✅ Review record được tạo với đúng data
- [ ] ✅ Status = Pending
- [ ] ✅ IsVerifiedPurchase được set đúng
- [ ] ✅ Timestamps được set

### **User Experience:**
- [ ] ✅ Success message hiển thị
- [ ] ✅ User được redirect đến trang sách
- [ ] ✅ Review xuất hiện trong MyReviews
- [ ] ✅ Admin có thể duyệt review

## 🎊 Kết luận

Sau khi áp dụng tất cả các fix trên, hệ thống review sẽ:

1. **Kiểm tra authentication** trước khi cho phép submit
2. **Hiển thị lỗi chi tiết** thay vì generic error
3. **Log đầy đủ** để debug khi có vấn đề
4. **Xử lý các edge cases** như duplicate review, expired token
5. **Cung cấp feedback tốt** cho user experience

**🚀 Review system bây giờ sẽ hoạt động ổn định và user-friendly!**

---

*Test guide được tạo ngày 27/07/2025*
