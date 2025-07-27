# Gmail SMTP Setup Guide for BookStore OTP Email Service

## 🚨 **CURRENT STATUS: USING MOCK EMAIL SERVICE**

The application is currently configured to use **MockEmailService** in development mode, which means:
- ✅ **OTP codes are logged to console** instead of being sent via email
- ✅ **Registration flow works perfectly** for testing
- ✅ **No email configuration required** for development

## 📧 **MOCK EMAIL SERVICE OUTPUT**

When you register a new account, you'll see output like this in the console:

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

**To test OTP registration:**
1. Go to registration page
2. Fill out the form and submit
3. Check the console output for the OTP code
4. Use that OTP code in the verification page

---

## 📧 **GMAIL SMTP SETUP (For Production)**

When you're ready to use real email sending in production, follow these steps:

### **Step 1: Enable 2-Factor Authentication**
1. Go to [Google Account Settings](https://myaccount.google.com/)
2. Click **Security** in the left sidebar
3. Under **Signing in to Google**, click **2-Step Verification**
4. Follow the setup process to enable 2FA

### **Step 2: Generate App Password**
1. After enabling 2FA, go back to **Security** settings
2. Under **Signing in to Google**, click **App passwords**
3. Select **Mail** as the app
4. Select **Other (Custom name)** as the device
5. Enter "BookStore Application" as the name
6. Click **Generate**
7. **Copy the 16-character password** (e.g., `abcd efgh ijkl mnop`)

### **Step 3: Update appsettings.json**
Replace the email configuration in `appsettings.json`:

```json
{
  "Email": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-gmail-address@gmail.com",
    "SmtpPassword": "your-16-character-app-password",
    "FromEmail": "your-gmail-address@gmail.com",
    "FromName": "BookStore Support",
    "EnableSsl": true
  }
}
```

**Important:**
- Use your **Gmail address** for `SmtpUsername` and `FromEmail`
- Use the **16-character App Password** (not your regular Gmail password)
- Remove spaces from the App Password: `abcdefghijklmnop`

### **Step 4: Switch to Real Email Service**
In `Program.cs`, change the email service registration:

```csharp
// For Production - Use Real Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

// Comment out or remove the development mock service
// if (builder.Environment.IsDevelopment())
// {
//     builder.Services.AddScoped<IEmailService, MockEmailService>();
// }
```

---

## 🔧 **ALTERNATIVE EMAIL PROVIDERS**

### **Microsoft Outlook/Hotmail**
```json
{
  "Email": {
    "SmtpServer": "smtp-mail.outlook.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@outlook.com",
    "SmtpPassword": "your-password-or-app-password",
    "FromEmail": "your-email@outlook.com",
    "FromName": "BookStore Support",
    "EnableSsl": true
  }
}
```

### **Yahoo Mail**
```json
{
  "Email": {
    "SmtpServer": "smtp.mail.yahoo.com",
    "SmtpPort": 587,
    "SmtpUsername": "your-email@yahoo.com",
    "SmtpPassword": "your-app-password",
    "FromEmail": "your-email@yahoo.com",
    "FromName": "BookStore Support",
    "EnableSsl": true
  }
}
```

### **Custom SMTP Server**
```json
{
  "Email": {
    "SmtpServer": "mail.yourdomain.com",
    "SmtpPort": 587,
    "SmtpUsername": "noreply@yourdomain.com",
    "SmtpPassword": "your-email-password",
    "FromEmail": "noreply@yourdomain.com",
    "FromName": "BookStore Support",
    "EnableSsl": true
  }
}
```

---

## 🧪 **TESTING EMAIL CONFIGURATION**

### **Test with Mock Service (Current Setup)**
1. Register a new account
2. Check console output for OTP code
3. Use the OTP code to complete registration

### **Test with Real Email Service**
1. Update `appsettings.json` with real email credentials
2. Switch to `EmailService` in `Program.cs`
3. Register with a real email address
4. Check your email inbox for OTP code
5. Complete the registration process

---

## 🚨 **TROUBLESHOOTING**

### **Common Gmail Issues:**

#### **"Authentication Required" Error**
- ✅ **Solution**: Enable 2FA and use App Password
- ❌ **Don't use**: Regular Gmail password

#### **"Less Secure App" Error**
- ✅ **Solution**: Use App Password instead of enabling less secure apps
- ❌ **Avoid**: Enabling "Less secure app access" (deprecated)

#### **"Invalid Credentials" Error**
- ✅ **Check**: App Password is correct (16 characters, no spaces)
- ✅ **Check**: Gmail address is correct
- ✅ **Check**: 2FA is enabled

### **Testing Steps:**
1. **Start with Mock Service**: Test registration flow first
2. **Configure Real Email**: Set up Gmail App Password
3. **Test Real Email**: Send test OTP to your own email
4. **Deploy to Production**: Use real email service

---

## 📝 **CURRENT DEVELOPMENT WORKFLOW**

Since we're using MockEmailService in development:

1. **Register New Account**: http://localhost:5106/Account/Register
2. **Fill Registration Form**: Enter any valid email address
3. **Check Console Output**: Look for OTP code in application logs
4. **Enter OTP**: Use the logged OTP code in verification page
5. **Complete Registration**: Account will be created successfully

**Example Console Output:**
```
info: BookStore.Web.Services.MockEmailService[0]
      === MOCK EMAIL SERVICE ===
info: BookStore.Web.Services.MockEmailService[0]
      📧 OTP Email would be sent to: test@example.com
info: BookStore.Web.Services.MockEmailService[0]
      🔐 OTP Code: 123456
```

This allows you to test the complete OTP registration flow without needing to configure real email services during development! 🎉
