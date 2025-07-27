using BookStore.Web.Services;
using BookStore.Web.Middleware;
using BookStore.Web.Models;
using Microsoft.AspNetCore.Http;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Cấu hình Authentication
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/AccessDenied";
    });

// Cấu hình Session
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Đăng ký HttpClient và ApiService
builder.Services.AddHttpClient();
builder.Services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
builder.Services.AddScoped<ApiService>();
builder.Services.AddScoped<BookStore.Core.Services.ITimezoneService, BookStore.Infrastructure.Services.TimezoneService>();

// Cấu hình PayOS
builder.Services.Configure<PayOSConfiguration>(builder.Configuration.GetSection("PayOS"));
builder.Services.AddScoped<IPayOSService, PayOSService>();

// Cấu hình Email Service
builder.Services.Configure<EmailConfiguration>(builder.Configuration.GetSection("Email"));

// Use Real Email Service for testing (temporarily)
// Comment out mock service to test real email functionality
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Logging.AddConsole(); // Ensure console logging

// Uncomment below to use Mock Email Service in Development
// if (builder.Environment.IsDevelopment())
// {
//     builder.Services.AddScoped<IEmailService, MockEmailService>();
//     builder.Logging.AddConsole(); // Ensure console logging for mock email output
// }
// else
// {
//     builder.Services.AddScoped<IEmailService, EmailService>();
// }

// Cấu hình OTP Service
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IOtpService, OtpService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Sử dụng Session
app.UseSession();

// Sử dụng Authentication
app.UseAuthentication();

// Sử dụng Authentication Middleware
app.UseAuthenticationMiddleware();

app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
