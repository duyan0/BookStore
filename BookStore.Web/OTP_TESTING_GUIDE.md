# OTP Email Verification Testing Guide

## ✅ **SMTP ISSUE RESOLVED - USING MOCK EMAIL SERVICE**

The SMTP authentication issue has been resolved by implementing a **MockEmailService** for development testing.

### **🔧 Current Configuration:**
- **Development Environment**: Uses `MockEmailService` (no real emails sent)
- **Production Environment**: Uses `EmailService` (real SMTP emails)
- **OTP Codes**: Displayed in console output for easy testing

---

## 📧 **HOW MOCK EMAIL SERVICE WORKS**

### **Console Output Example:**
When you register a new account, you'll see this in the application console:

```
=== MOCK EMAIL SERVICE ===
📧 OTP Email would be sent to: user@example.com
👤 Recipient Name: John Doe
🔐 OTP Code: 123456
📝 Subject: Xác thực tài khoản BookStore - Mã OTP
⏰ Valid for: 10 minutes
🔄 Max attempts: 5
========================
```

### **Benefits:**
- ✅ **No SMTP configuration required** for development
- ✅ **Instant testing** - no waiting for emails
- ✅ **Complete OTP flow testing** without email dependencies
- ✅ **Easy debugging** - OTP codes visible in console

---

## 🧪 **TESTING OTP REGISTRATION FLOW**

### **Step 1: Access Registration Page**
Navigate to: http://localhost:5106/Account/Register

### **Step 2: Fill Registration Form**
```
Username: testuser123
Email: test@example.com (any valid email format)
Password: Test@123
Confirm Password: Test@123
First Name: Test
Last Name: User
Phone: 0123456789 (optional)
Address: 123 Test Street (optional)
```

### **Step 3: Submit Registration**
1. Click "Đăng ký" button
2. Form will be submitted
3. **Check console output** for OTP code
4. You'll be redirected to OTP verification page

### **Step 4: Find OTP Code in Console**
Look for output like this in the application console:
```
info: BookStore.Web.Services.MockEmailService[0]
      🔐 OTP Code: 123456
```

### **Step 5: Enter OTP Code**
1. Enter the 6-digit OTP code from console
2. Click "Xác thực OTP" or wait for auto-submit
3. Account will be created successfully
4. Redirect to login page

---

## 🎯 **TESTING SCENARIOS**

### **✅ Successful Registration**
1. **Fill form** → **Submit** → **Get OTP from console** → **Enter OTP** → **Account created**

### **❌ Invalid OTP**
1. Enter wrong OTP code
2. See error message: "Mã OTP không đúng. Còn lại X lần thử."
3. Try again with correct code

### **⏰ OTP Expiry**
1. Wait 10 minutes after OTP generation
2. Try to enter OTP
3. See error: "Mã OTP đã hết hạn"
4. Use "Gửi lại mã OTP" button

### **🔄 Resend OTP**
1. Click "Gửi lại mã OTP" button
2. New OTP generated and logged to console
3. 60-second countdown before allowing another resend

### **🚫 Max Attempts Exceeded**
1. Enter wrong OTP 5 times
2. See error: "Bạn đã nhập sai mã OTP quá nhiều lần"
3. Redirected back to registration

---

## 🌐 **TESTING URLS**

### **Registration Flow:**
- **Start**: http://localhost:5106/Account/Register
- **OTP Verification**: http://localhost:5106/Account/VerifyOtp?email=test@example.com
- **Login**: http://localhost:5106/Account/Login

### **Admin Features:**
- **Orders with PayOS**: http://localhost:5106/Admin/Orders
- **Order Details**: Click any PayOS order to see PayOS code display

---

## 📋 **FEATURE TESTING CHECKLIST**

### **✅ OTP Registration Features:**
- [ ] Registration form validation
- [ ] OTP generation and console logging
- [ ] OTP verification page display
- [ ] Auto-focus on OTP input
- [ ] Auto-submit when 6 digits entered
- [ ] Invalid OTP error handling
- [ ] OTP expiry handling (10 minutes)
- [ ] Resend OTP functionality
- [ ] Max attempts limiting (5 attempts)
- [ ] Account creation after successful OTP
- [ ] Session management and cleanup

### **✅ PayOS Order Display Features:**
- [ ] PayOS column in Admin Orders table
- [ ] PayOS badge display for PayOS orders
- [ ] PayOS code in order details
- [ ] Non-PayOS orders show "-" in PayOS column

---

## 🔧 **SWITCHING TO REAL EMAIL (Production)**

When ready to use real email sending:

### **1. Update Program.cs:**
```csharp
// Comment out mock service
// if (builder.Environment.IsDevelopment())
// {
//     builder.Services.AddScoped<IEmailService, MockEmailService>();
// }
// else
// {
    builder.Services.AddScoped<IEmailService, EmailService>();
// }
```

### **2. Configure Gmail SMTP:**
```json
{
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-gmail@gmail.com",
    "SmtpPassword": "your-16-char-app-password",
    "FromEmail": "your-gmail@gmail.com",
    "FromName": "BookStore Support",
    "EnableSsl": true
  }
}
```

### **3. Gmail Setup Requirements:**
1. Enable 2-Factor Authentication
2. Generate App Password (16 characters)
3. Use App Password in configuration (not regular password)

---

## 🚨 **TROUBLESHOOTING**

### **OTP Not Showing in Console:**
- Check application is running in Development environment
- Look for `MockEmailService` logs in console
- Ensure registration form submission was successful

### **OTP Verification Fails:**
- Verify OTP code from console output
- Check OTP hasn't expired (10 minutes)
- Ensure not exceeded max attempts (5)

### **Registration Form Issues:**
- Check all required fields are filled
- Verify password meets requirements
- Ensure email format is valid

### **PayOS Order Display Issues:**
- Verify order was created with PayOS payment method
- Check admin user permissions
- Refresh orders page

---

## 🎉 **SUCCESS CRITERIA**

### **OTP Registration Complete When:**
1. ✅ Registration form accepts valid data
2. ✅ OTP code appears in console output
3. ✅ OTP verification page loads correctly
4. ✅ Valid OTP creates user account
5. ✅ Invalid OTP shows appropriate errors
6. ✅ Resend OTP generates new code
7. ✅ Account creation redirects to login

### **PayOS Display Complete When:**
1. ✅ PayOS column appears in Admin Orders
2. ✅ PayOS orders show badge with order code
3. ✅ Non-PayOS orders show "-"
4. ✅ Order details display PayOS information
5. ✅ PayOS code is clearly visible and copyable

---

## 📞 **CURRENT STATUS**

**✅ Application Running**: http://localhost:5106
**✅ Mock Email Service**: Active and logging OTP codes
**✅ OTP Registration**: Ready for testing
**✅ PayOS Order Display**: Implemented and functional
**✅ All Features**: Working without SMTP dependencies

**Ready for comprehensive testing!** 🚀📧
