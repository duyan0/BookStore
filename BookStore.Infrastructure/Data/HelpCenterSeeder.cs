using BookStore.Core.Entities;
using BookStore.Core.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Data
{
    public static class HelpCenterSeeder
    {
        public static async Task SeedHelpArticlesAsync(ApplicationDbContext context)
        {
            if (await context.HelpArticles.AnyAsync())
                return; // Already seeded

            // Get admin user as author
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.IsAdmin);
            if (adminUser == null)
                return; // No admin user found

            var helpArticles = new List<HelpArticle>
            {
                // FAQ Articles
                new HelpArticle
                {
                    Title = "Làm thế nào để đặt hàng?",
                    Slug = "lam-the-nao-de-dat-hang",
                    Content = @"<h3>Hướng dẫn đặt hàng tại BookStore</h3>
                    <p>Để đặt hàng tại BookStore, bạn có thể thực hiện theo các bước sau:</p>
                    <ol>
                        <li><strong>Tìm kiếm sách:</strong> Sử dụng thanh tìm kiếm hoặc duyệt theo danh mục</li>
                        <li><strong>Chọn sách:</strong> Click vào sách bạn muốn mua để xem chi tiết</li>
                        <li><strong>Thêm vào giỏ hàng:</strong> Click nút 'Thêm vào giỏ hàng'</li>
                        <li><strong>Kiểm tra giỏ hàng:</strong> Click vào biểu tượng giỏ hàng để xem lại</li>
                        <li><strong>Thanh toán:</strong> Click 'Thanh toán' và điền thông tin giao hàng</li>
                        <li><strong>Xác nhận:</strong> Kiểm tra lại thông tin và hoàn tất đơn hàng</li>
                    </ol>
                    <p>Sau khi đặt hàng thành công, bạn sẽ nhận được email xác nhận.</p>",
                    Summary = "Hướng dẫn chi tiết cách đặt hàng tại BookStore từ A đến Z",
                    Type = HelpArticleType.FAQ,
                    Category = HelpArticleCategory.Orders,
                    DisplayOrder = 1,
                    IsPublished = true,
                    IsFeatured = true,
                    MetaTitle = "Hướng dẫn đặt hàng - BookStore",
                    MetaDescription = "Hướng dẫn chi tiết cách đặt hàng sách tại BookStore một cách dễ dàng và nhanh chóng",
                    AuthorId = adminUser.Id,
                    CreatedAt = DateTimeExtensions.GetVietnamNow()
                },

                new HelpArticle
                {
                    Title = "Các phương thức thanh toán được hỗ trợ",
                    Slug = "cac-phuong-thuc-thanh-toan",
                    Content = @"<h3>Phương thức thanh toán tại BookStore</h3>
                    <p>BookStore hỗ trợ các phương thức thanh toán sau:</p>
                    <h4>1. Thanh toán khi nhận hàng (COD)</h4>
                    <ul>
                        <li>Thanh toán bằng tiền mặt khi nhận hàng</li>
                        <li>Áp dụng cho tất cả đơn hàng trong nội thành</li>
                        <li>Phí COD: 15,000 VND cho đơn hàng dưới 500,000 VND</li>
                    </ul>
                    <h4>2. Chuyển khoản ngân hàng</h4>
                    <ul>
                        <li>Chuyển khoản qua Internet Banking</li>
                        <li>Chuyển khoản tại quầy ngân hàng</li>
                        <li>Thông tin tài khoản sẽ được gửi qua email sau khi đặt hàng</li>
                    </ul>
                    <h4>3. Ví điện tử</h4>
                    <ul>
                        <li>MoMo</li>
                        <li>ZaloPay</li>
                        <li>VNPay</li>
                    </ul>",
                    Summary = "Thông tin về các phương thức thanh toán được hỗ trợ tại BookStore",
                    Type = HelpArticleType.FAQ,
                    Category = HelpArticleCategory.Payment,
                    DisplayOrder = 2,
                    IsPublished = true,
                    IsFeatured = true,
                    MetaTitle = "Phương thức thanh toán - BookStore",
                    MetaDescription = "Tìm hiểu các phương thức thanh toán được hỗ trợ tại BookStore",
                    AuthorId = adminUser.Id,
                    CreatedAt = DateTimeExtensions.GetVietnamNow()
                },

                new HelpArticle
                {
                    Title = "Chính sách giao hàng",
                    Slug = "chinh-sach-giao-hang",
                    Content = @"<h3>Chính sách giao hàng BookStore</h3>
                    <h4>Thời gian giao hàng</h4>
                    <ul>
                        <li><strong>Nội thành Hà Nội, TP.HCM:</strong> 1-2 ngày làm việc</li>
                        <li><strong>Các tỉnh thành khác:</strong> 2-5 ngày làm việc</li>
                        <li><strong>Vùng sâu, vùng xa:</strong> 5-7 ngày làm việc</li>
                    </ul>
                    <h4>Phí giao hàng</h4>
                    <ul>
                        <li><strong>Miễn phí giao hàng:</strong> Đơn hàng từ 200,000 VND</li>
                        <li><strong>Nội thành:</strong> 25,000 VND</li>
                        <li><strong>Ngoại thành:</strong> 35,000 VND</li>
                        <li><strong>Tỉnh khác:</strong> 45,000 VND</li>
                    </ul>
                    <h4>Lưu ý</h4>
                    <p>Thời gian giao hàng có thể thay đổi tùy thuộc vào tình hình thực tế và các yếu tố bất khả kháng như thời tiết, giao thông.</p>",
                    Summary = "Thông tin chi tiết về chính sách giao hàng của BookStore",
                    Type = HelpArticleType.Policy,
                    Category = HelpArticleCategory.Shipping,
                    DisplayOrder = 1,
                    IsPublished = true,
                    IsFeatured = false,
                    MetaTitle = "Chính sách giao hàng - BookStore",
                    MetaDescription = "Tìm hiểu chính sách giao hàng, thời gian và phí giao hàng tại BookStore",
                    AuthorId = adminUser.Id,
                    CreatedAt = DateTimeExtensions.GetVietnamNow()
                },

                new HelpArticle
                {
                    Title = "Chính sách đổi trả",
                    Slug = "chinh-sach-doi-tra",
                    Content = @"<h3>Chính sách đổi trả BookStore</h3>
                    <h4>Điều kiện đổi trả</h4>
                    <ul>
                        <li>Sách còn nguyên vẹn, không bị rách, ướt, bẩn</li>
                        <li>Còn nguyên tem, nhãn của BookStore</li>
                        <li>Trong thời hạn 7 ngày kể từ ngày nhận hàng</li>
                        <li>Có hóa đơn mua hàng hoặc email xác nhận đơn hàng</li>
                    </ul>
                    <h4>Các trường hợp được đổi trả</h4>
                    <ul>
                        <li>Sách bị lỗi in ấn, thiếu trang</li>
                        <li>Giao sai sách so với đơn hàng</li>
                        <li>Sách bị hư hỏng trong quá trình vận chuyển</li>
                        <li>Khách hàng không hài lòng với sản phẩm</li>
                    </ul>
                    <h4>Quy trình đổi trả</h4>
                    <ol>
                        <li>Liên hệ hotline: 1900-xxxx hoặc email: support@bookstore.com</li>
                        <li>Cung cấp thông tin đơn hàng và lý do đổi trả</li>
                        <li>Đóng gói sách và gửi về địa chỉ BookStore cung cấp</li>
                        <li>BookStore kiểm tra và xử lý trong 3-5 ngày làm việc</li>
                    </ol>",
                    Summary = "Chính sách đổi trả sách tại BookStore - điều kiện và quy trình",
                    Type = HelpArticleType.Policy,
                    Category = HelpArticleCategory.Returns,
                    DisplayOrder = 2,
                    IsPublished = true,
                    IsFeatured = false,
                    MetaTitle = "Chính sách đổi trả - BookStore",
                    MetaDescription = "Tìm hiểu chính sách đổi trả sách tại BookStore, điều kiện và quy trình đổi trả",
                    AuthorId = adminUser.Id,
                    CreatedAt = DateTimeExtensions.GetVietnamNow()
                },

                new HelpArticle
                {
                    Title = "Cách tạo tài khoản và đăng nhập",
                    Slug = "cach-tao-tai-khoan-va-dang-nhap",
                    Content = @"<h3>Hướng dẫn tạo tài khoản BookStore</h3>
                    <h4>Tạo tài khoản mới</h4>
                    <ol>
                        <li>Click vào nút 'Đăng ký' ở góc phải màn hình</li>
                        <li>Điền đầy đủ thông tin: Họ tên, Email, Mật khẩu</li>
                        <li>Xác nhận mật khẩu</li>
                        <li>Click 'Đăng ký' để hoàn tất</li>
                        <li>Kiểm tra email để xác nhận tài khoản (nếu có)</li>
                    </ol>
                    <h4>Đăng nhập</h4>
                    <ol>
                        <li>Click vào nút 'Đăng nhập'</li>
                        <li>Nhập email và mật khẩu</li>
                        <li>Click 'Đăng nhập'</li>
                    </ol>
                    <h4>Quên mật khẩu</h4>
                    <p>Nếu quên mật khẩu, click vào 'Quên mật khẩu?' và làm theo hướng dẫn để reset mật khẩu qua email.</p>",
                    Summary = "Hướng dẫn tạo tài khoản, đăng nhập và khôi phục mật khẩu",
                    Type = HelpArticleType.Guide,
                    Category = HelpArticleCategory.Account,
                    DisplayOrder = 1,
                    IsPublished = true,
                    IsFeatured = true,
                    MetaTitle = "Tạo tài khoản và đăng nhập - BookStore",
                    MetaDescription = "Hướng dẫn tạo tài khoản, đăng nhập và quản lý tài khoản tại BookStore",
                    AuthorId = adminUser.Id,
                    CreatedAt = DateTimeExtensions.GetVietnamNow()
                }
            };

            await context.HelpArticles.AddRangeAsync(helpArticles);
            await context.SaveChangesAsync();
        }
    }
}
