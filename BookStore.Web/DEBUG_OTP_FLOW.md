# Debug OTP Flow - Hướng Dẫn Kiểm Tra Chi Tiết

## 🔍 **CÁCH KIỂM TRA LOGS ĐỂ DEBUG**

### **1. Khởi động ứng dụng với logging chi tiết**
```bash
cd BookStore.Web
dotnet run --verbosity detailed
```

### **2. Logs cần quan sát khi đăng ký**

#### **Bước 1: Submit Registration Form**
Tìm logs này trong console:
```
info: BookStore.Web.Controllers.AccountController[0]
      Email service result for user@gmail.com: True/False
```

#### **Bước 2: Email Service Execution**
```
info: BookStore.Web.Services.EmailService[0]
      Attempting to send email to user@gmail.com via SMTP smtp.gmail.com:587
```

#### **Bước 3: Redirect to VerifyOtp**
```
info: BookStore.Web.Controllers.AccountController[0]
      About to redirect to VerifyOtp with email: user@gmail.com
```

#### **Bước 4: VerifyOtp Action Called**
```
info: BookStore.Web.Controllers.AccountController[0]
      VerifyOtp GET action called with email: user@gmail.com
```

---

## 🚨 **CÁC TRƯỜNG HỢP LỖI VÀ CÁCH XỬ LÝ**

### **Trường hợp 1: Email Service trả về False**
```
info: Email service result for user@gmail.com: False
fail: Failed to send OTP email to user@gmail.com. Email service returned false.
```

**Nguyên nhân có thể:**
- Gmail App Password sai
- Gmail address sai
- Chưa bật 2FA
- Cấu hình SMTP sai

**Cách sửa:**
1. Kiểm tra appsettings.json
2. Tạo lại App Password
3. Test với Gmail khác

### **Trường hợp 2: SMTP Authentication Error**
```
fail: SMTP error sending email to user@gmail.com. StatusCode: MailboxBusy, Message: 5.7.0 Authentication Required
```

**Cách sửa:**
1. Bật 2-Factor Authentication
2. Tạo App Password mới
3. Cập nhật appsettings.json

### **Trường hợp 3: Không redirect đến VerifyOtp**
```
info: Email service result for user@gmail.com: True
info: About to redirect to VerifyOtp with email: user@gmail.com
// Nhưng không thấy log "VerifyOtp GET action called"
```

**Nguyên nhân có thể:**
- Browser cache
- JavaScript error
- Routing issue

**Cách sửa:**
1. Clear browser cache
2. Thử browser khác
3. Kiểm tra browser console (F12)

### **Trường hợp 4: OTP không valid**
```
info: VerifyOtp GET action called with email: user@gmail.com
info: OTP validity check for user@gmail.com: False
warn: OTP not valid for user@gmail.com. Redirecting to Register.
```

**Nguyên nhân:**
- OTP đã hết hạn (10 phút)
- OTP chưa được tạo
- Cache issue

**Cách sửa:**
1. Đăng ký lại
2. Kiểm tra OTP generation logs
3. Restart ứng dụng

---

## 🧪 **TESTING STEP-BY-STEP**

### **Bước 1: Chuẩn bị**
1. Cập nhật Gmail credentials trong appsettings.json
2. Restart ứng dụng
3. Mở browser và console logs (F12)

### **Bước 2: Test Registration**
1. Truy cập: http://localhost:5106/Account/Register
2. Điền form với email thật
3. Submit và quan sát console logs

### **Bước 3: Kiểm tra Email**
1. Mở Gmail
2. Tìm email từ "BookStore Support"
3. Lấy mã OTP 6 chữ số

### **Bước 4: Test OTP Verification**
1. Nhập OTP vào form
2. Quan sát logs validation
3. Kiểm tra account creation

---

## 📊 **LOGS MONG ĐỢI KHI THÀNH CÔNG**

### **Complete Success Flow:**
```
info: Serializing user data for OTP storage
info: OTP generated for email user@gmail.com, expires at 2024-01-01 10:10:00
info: Attempting to send email to user@gmail.com via SMTP smtp.gmail.com:587
info: Email sent successfully to user@gmail.com
info: Email service result for user@gmail.com: True
info: OTP sent successfully to user@gmail.com. Redirecting to VerifyOtp.
info: About to redirect to VerifyOtp with email: user@gmail.com
info: VerifyOtp GET action called with email: user@gmail.com
info: OTP validity check for user@gmail.com: True
info: Creating VerifyOtp view model for user@gmail.com with 5 attempts
info: Returning VerifyOtp view for user@gmail.com
```

---

## 🔧 **MANUAL TESTING COMMANDS**

### **Test Email Configuration:**
Tạo test endpoint để kiểm tra email:

```csharp
// Thêm vào AccountController để test
[HttpGet]
public async Task<IActionResult> TestEmail(string email = "test@gmail.com")
{
    try
    {
        var result = await _emailService.SendEmailAsync(email, "Test Email", "This is a test email from BookStore");
        return Json(new { success = result, message = result ? "Email sent successfully" : "Email failed" });
    }
    catch (Exception ex)
    {
        return Json(new { success = false, message = ex.Message });
    }
}
```

### **Test URL:**
http://localhost:5106/Account/TestEmail?email=your-email@gmail.com

---

## 📋 **DEBUGGING CHECKLIST**

### **Trước khi test:**
- [ ] Đã cập nhật Gmail credentials trong appsettings.json
- [ ] Đã bật 2FA và tạo App Password
- [ ] Đã restart ứng dụng
- [ ] Đã clear browser cache

### **Trong quá trình test:**
- [ ] Quan sát console logs
- [ ] Kiểm tra browser console (F12)
- [ ] Kiểm tra email inbox
- [ ] Note down error messages

### **Sau khi test:**
- [ ] Verify account được tạo trong database
- [ ] Test login với account mới
- [ ] Kiểm tra session management

---

## 🎯 **KẾT QUẢ MONG ĐỢI**

### **Thành công hoàn toàn khi:**
1. ✅ Registration form submit thành công
2. ✅ Email service trả về True
3. ✅ Redirect đến VerifyOtp page
4. ✅ Email OTP được nhận trong Gmail
5. ✅ OTP verification thành công
6. ✅ Account được tạo trong database
7. ✅ Redirect đến login page
8. ✅ Có thể login với account mới

### **Logs không có error:**
- Không có "fail:" messages
- Không có "error:" messages
- Tất cả "info:" logs hiển thị success

---

## 🔄 **FALLBACK PLAN**

### **Nếu Gmail không hoạt động:**

#### **Option 1: Sử dụng Outlook**
```json
{
  "Email": {
    "SmtpServer": "smtp-mail.outlook.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@outlook.com",
    "SmtpPassword": "your-password",
    "FromEmail": "your-email@outlook.com",
    "FromName": "BookStore Support",
    "EnableSsl": true
  }
}
```

#### **Option 2: Quay lại MockEmailService**
```csharp
// Trong Program.cs
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddScoped<IEmailService, MockEmailService>();
}
```

#### **Option 3: Sử dụng SendGrid/Mailgun**
Cấu hình third-party email service cho production.

---

## 📞 **SUPPORT CONTACT**

Nếu vẫn gặp vấn đề sau khi làm theo hướng dẫn:

1. **Capture logs** từ console
2. **Screenshot** error messages
3. **Note down** exact steps đã thực hiện
4. **Check** Gmail settings và App Password

**Logs quan trọng nhất:**
- Email service result (True/False)
- SMTP error messages
- Redirect logs
- OTP validity checks
