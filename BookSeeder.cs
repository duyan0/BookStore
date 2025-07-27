using BookStore.Infrastructure.Data;
using BookStore.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Seeders
{
    public static class BookSeeder
    {
        public static async Task SeedBooksAsync(ApplicationDbContext context)
        {
            // Kiểm tra xem đã có sách nào chưa
            if (await context.Books.CountAsync() >= 50)
            {
                Console.WriteLine("📚 Database đã có đủ sách, bỏ qua seeding.");
                return;
            }

            Console.WriteLine("🚀 Bắt đầu thêm 50 cuốn sách...");

            var books = new List<Book>
            {
                // Sách văn học
                new Book { Title = "Tôi Thấy Hoa Vàng Trên Cỏ Xanh", Description = "Tiểu thuyết nổi tiếng của Nguyễn Nhật Ánh", OriginalPrice = 120000, DiscountPercentage = 15, IsOnSale = true, SaleStartDate = DateTime.UtcNow, SaleEndDate = DateTime.UtcNow.AddMonths(1), Quantity = 50, ISBN = "978-604-2-12345-1", Publisher = "NXB Trẻ", PublicationYear = 2010, ImageUrl = "https://images.unsplash.com/photo-1481627834876-b7833e8f5570?w=400", CategoryId = 1, AuthorId = 1 },
                new Book { Title = "Mắt Biếc", Description = "Câu chuyện tình yêu đẹp và buồn", OriginalPrice = 95000, DiscountPercentage = 20, IsOnSale = true, SaleStartDate = DateTime.UtcNow, SaleEndDate = DateTime.UtcNow.AddMonths(1), Quantity = 30, ISBN = "978-604-2-12345-2", Publisher = "NXB Trẻ", PublicationYear = 2008, ImageUrl = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400", CategoryId = 1, AuthorId = 1 },
                new Book { Title = "Cho Tôi Xin Một Vé Đi Tuổi Thơ", Description = "Hồi ký tuổi thơ đầy cảm xúc", OriginalPrice = 110000, DiscountPercentage = 10, IsOnSale = true, SaleStartDate = DateTime.UtcNow, SaleEndDate = DateTime.UtcNow.AddMonths(1), Quantity = 40, ISBN = "978-604-2-12345-3", Publisher = "NXB Trẻ", PublicationYear = 2012, ImageUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400", CategoryId = 1, AuthorId = 1 },
                new Book { Title = "Cô Gái Đến Từ Hôm Qua", Description = "Tiểu thuyết tình cảm hiện đại", OriginalPrice = 130000, DiscountPercentage = 25, IsOnSale = true, SaleStartDate = DateTime.UtcNow, SaleEndDate = DateTime.UtcNow.AddMonths(1), Quantity = 35, ISBN = "978-604-2-12345-4", Publisher = "NXB Trẻ", PublicationYear = 2015, ImageUrl = "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=400", CategoryId = 1, AuthorId = 1 },
                new Book { Title = "Totto-chan Bên Cửa Sổ", Description = "Hồi ký tuổi thơ của Tetsuko Kuroyanagi", OriginalPrice = 140000, DiscountAmount = 20000, IsOnSale = true, SaleStartDate = DateTime.UtcNow, SaleEndDate = DateTime.UtcNow.AddMonths(1), Quantity = 25, ISBN = "978-604-2-12345-5", Publisher = "NXB Hội Nhà Văn", PublicationYear = 2018, ImageUrl = "https://images.unsplash.com/photo-1481627834876-b7833e8f5570?w=400", CategoryId = 1, AuthorId = 2 },

                // Sách khoa học
                new Book { Title = "Sapiens: Lược Sử Loài Người", Description = "Cuốn sách về lịch sử nhân loại", OriginalPrice = 280000, DiscountPercentage = 30, IsOnSale = true, SaleStartDate = DateTime.UtcNow, SaleEndDate = DateTime.UtcNow.AddMonths(1), Quantity = 20, ISBN = "978-604-2-12346-1", Publisher = "NXB Thế Giới", PublicationYear = 2020, ImageUrl = "https://images.unsplash.com/photo-1532012197267-da84d127e765?w=400", CategoryId = 2, AuthorId = 2 },
                new Book { Title = "Homo Deus: Lược Sử Tương Lai", Description = "Tương lai của loài người", OriginalPrice = 320000, DiscountPercentage = 25, IsOnSale = true, SaleStartDate = DateTime.UtcNow, SaleEndDate = DateTime.UtcNow.AddMonths(1), Quantity = 15, ISBN = "978-604-2-12346-2", Publisher = "NXB Thế Giới", PublicationYear = 2021, ImageUrl = "https://images.unsplash.com/photo-1589998059171-988d887df646?w=400", CategoryId = 2, AuthorId = 2 },
                new Book { Title = "21 Bài Học Cho Thế Kỷ 21", Description = "Những thách thức của thời đại mới", OriginalPrice = 290000, DiscountPercentage = 20, IsOnSale = true, SaleStartDate = DateTime.UtcNow, SaleEndDate = DateTime.UtcNow.AddMonths(1), Quantity = 18, ISBN = "978-604-2-12346-3", Publisher = "NXB Thế Giới", PublicationYear = 2022, ImageUrl = "https://images.unsplash.com/photo-1481627834876-b7833e8f5570?w=400", CategoryId = 2, AuthorId = 2 },
                new Book { Title = "Vũ Trụ Trong Vỏ Hạt Dẻ", Description = "Stephen Hawking và bí ẩn vũ trụ", OriginalPrice = 250000, DiscountPercentage = 15, IsOnSale = true, SaleStartDate = DateTime.UtcNow, SaleEndDate = DateTime.UtcNow.AddMonths(1), Quantity = 22, ISBN = "978-604-2-12346-4", Publisher = "NXB Khoa Học", PublicationYear = 2019, ImageUrl = "https://images.unsplash.com/photo-1446776653964-20c1d3a81b06?w=400", CategoryId = 2, AuthorId = 2 },
                new Book { Title = "Lược Sử Thời Gian", Description = "Cuốn sách kinh điển về vật lý", OriginalPrice = 230000, DiscountAmount = 30000, IsOnSale = true, SaleStartDate = DateTime.UtcNow, SaleEndDate = DateTime.UtcNow.AddMonths(1), Quantity = 28, ISBN = "978-604-2-12346-5", Publisher = "NXB Khoa Học", PublicationYear = 2017, ImageUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400", CategoryId = 2, AuthorId = 2 },

                // Sách kinh doanh
                new Book { Title = "Nghĩ Giàu Làm Giàu", Description = "Bí quyết thành công trong kinh doanh", OriginalPrice = 180000, DiscountPercentage = 20, IsOnSale = true, SaleStartDate = DateTime.UtcNow, SaleEndDate = DateTime.UtcNow.AddMonths(1), Quantity = 45, ISBN = "978-604-2-12347-1", Publisher = "NXB Lao Động", PublicationYear = 2018, ImageUrl = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400", CategoryId = 3, AuthorId = 1 },
                new Book { Title = "Đắc Nhân Tâm", Description = "Nghệ thuật giao tiếp và ứng xử", OriginalPrice = 160000, DiscountPercentage = 25, IsOnSale = true, SaleStartDate = DateTime.UtcNow, SaleEndDate = DateTime.UtcNow.AddMonths(1), Quantity = 60, ISBN = "978-604-2-12347-2", Publisher = "NXB Tổng Hợp", PublicationYear = 2015, ImageUrl = "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=400", CategoryId = 3, AuthorId = 1 },
                new Book { Title = "7 Thói Quen Hiệu Quả", Description = "Phát triển bản thân và lãnh đạo", OriginalPrice = 200000, DiscountPercentage = 15, IsOnSale = true, SaleStartDate = DateTime.UtcNow, SaleEndDate = DateTime.UtcNow.AddMonths(1), Quantity = 35, ISBN = "978-604-2-12347-3", Publisher = "NXB Lao Động", PublicationYear = 2020, ImageUrl = "https://images.unsplash.com/photo-1589998059171-988d887df646?w=400", CategoryId = 3, AuthorId = 1 },
                new Book { Title = "Khởi Nghiệp Tinh Gọn", Description = "Phương pháp khởi nghiệp hiệu quả", OriginalPrice = 220000, DiscountPercentage = 30, IsOnSale = true, SaleStartDate = DateTime.UtcNow, SaleEndDate = DateTime.UtcNow.AddMonths(1), Quantity = 25, ISBN = "978-604-2-12347-4", Publisher = "NXB Thế Giới", PublicationYear = 2021, ImageUrl = "https://images.unsplash.com/photo-1446776653964-20c1d3a81b06?w=400", CategoryId = 3, AuthorId = 2 },
                new Book { Title = "Từ Tốt Đến Vĩ Đại", Description = "Bí quyết xây dựng công ty bền vững", OriginalPrice = 240000, DiscountAmount = 40000, IsOnSale = true, SaleStartDate = DateTime.UtcNow, SaleEndDate = DateTime.UtcNow.AddMonths(1), Quantity = 30, ISBN = "978-604-2-12347-5", Publisher = "NXB Lao Động", PublicationYear = 2019, ImageUrl = "https://images.unsplash.com/photo-1481627834876-b7833e8f5570?w=400", CategoryId = 3, AuthorId = 2 },

                // Sách thiếu nhi
                new Book { Title = "Doraemon Tập 1", Description = "Truyện tranh thiếu nhi nổi tiếng", OriginalPrice = 25000, DiscountPercentage = 10, IsOnSale = true, SaleStartDate = DateTime.UtcNow, SaleEndDate = DateTime.UtcNow.AddMonths(1), Quantity = 100, ISBN = "978-604-2-12348-1", Publisher = "NXB Kim Đồng", PublicationYear = 2020, ImageUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400", CategoryId = 4, AuthorId = 1 },
                new Book { Title = "Conan Thám Tử Lừng Danh Tập 1", Description = "Truyện trinh thám hấp dẫn", OriginalPrice = 30000, DiscountPercentage = 15, IsOnSale = true, SaleStartDate = DateTime.UtcNow, SaleEndDate = DateTime.UtcNow.AddMonths(1), Quantity = 80, ISBN = "978-604-2-12348-2", Publisher = "NXB Kim Đồng", PublicationYear = 2021, ImageUrl = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400", CategoryId = 4, AuthorId = 1 },
                new Book { Title = "Thần Đồng Đất Việt", Description = "Truyện cổ tích Việt Nam", OriginalPrice = 45000, DiscountPercentage = 20, IsOnSale = true, SaleStartDate = DateTime.UtcNow, SaleEndDate = DateTime.UtcNow.AddMonths(1), Quantity = 70, ISBN = "978-604-2-12348-3", Publisher = "NXB Kim Đồng", PublicationYear = 2019, ImageUrl = "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=400", CategoryId = 4, AuthorId = 1 },
                new Book { Title = "Truyện Kể Cho Bé", Description = "Tuyển tập truyện ngắn cho trẻ em", OriginalPrice = 35000, DiscountAmount = 5000, IsOnSale = true, SaleStartDate = DateTime.UtcNow, SaleEndDate = DateTime.UtcNow.AddMonths(1), Quantity = 90, ISBN = "978-604-2-12348-4", Publisher = "NXB Kim Đồng", PublicationYear = 2022, ImageUrl = "https://images.unsplash.com/photo-1589998059171-988d887df646?w=400", CategoryId = 4, AuthorId = 1 },
                new Book { Title = "Cậu Bé Rồng", Description = "Phiêu lưu kỳ thú của cậu bé", OriginalPrice = 40000, DiscountPercentage = 25, IsOnSale = true, SaleStartDate = DateTime.UtcNow, SaleEndDate = DateTime.UtcNow.AddMonths(1), Quantity = 65, ISBN = "978-604-2-12348-5", Publisher = "NXB Kim Đồng", PublicationYear = 2020, ImageUrl = "https://images.unsplash.com/photo-1446776653964-20c1d3a81b06?w=400", CategoryId = 4, AuthorId = 2 }

                // ... Thêm 30 cuốn nữa tương tự
            };

            // Thêm các sách còn lại (rút gọn để tiết kiệm dung lượng)
            var additionalBooks = GenerateAdditionalBooks();
            books.AddRange(additionalBooks);

            await context.Books.AddRangeAsync(books);
            await context.SaveChangesAsync();

            Console.WriteLine($"✅ Đã thêm thành công {books.Count} cuốn sách!");
        }

        private static List<Book> GenerateAdditionalBooks()
        {
            var books = new List<Book>();
            var random = new Random();
            var categories = new[] { 1, 2, 3, 4 }; // CategoryIds
            var authors = new[] { 1, 2 }; // AuthorIds
            var publishers = new[] { "NXB Trẻ", "NXB Thế Giới", "NXB Kim Đồng", "NXB Giáo Dục", "NXB Khoa Học" };
            var imageUrls = new[]
            {
                "https://images.unsplash.com/photo-1481627834876-b7833e8f5570?w=400",
                "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400",
                "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=400",
                "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=400",
                "https://images.unsplash.com/photo-1589998059171-988d887df646?w=400",
                "https://images.unsplash.com/photo-1446776653964-20c1d3a81b06?w=400"
            };

            var bookTitles = new[]
            {
                "Lịch Sử Việt Nam", "Chiến Tranh Việt Nam", "Hồ Chí Minh - Cuộc Đời Và Sự Nghiệp",
                "Tâm Lý Học Đại Cương", "Nghệ Thuật Sống Hạnh Phúc", "Tâm Lý Học Tích Cực",
                "Món Ăn Việt Nam", "Bánh Ngọt Homemade", "Ẩm Thực Thế Giới",
                "Lập Trình C# Cơ Bản", "JavaScript Nâng Cao", "AI và Machine Learning",
                "Triết Học Phương Đông", "Đạo Đức Kinh", "Nghệ Thuật Sống",
                "Y Học Cổ Truyền Việt Nam", "Dinh Dưỡng Và Sức Khỏe", "Yoga Và Thiền Định",
                "Du Lịch Việt Nam", "Châu Âu Trong Tầm Tay", "Nhật Bản - Xứ Sở Hoa Anh Đào",
                "Blockchain và Cryptocurrency", "Cybersecurity Cơ Bản", "Stoicism - Triết Học Khắc Kỷ",
                "Chăm Sóc Sức Khỏe Gia Đình", "Phòng Chống Bệnh Tật", "Backpacker Đông Nam Á",
                "Ẩm Thực Đường Phố Thế Giới", "Nấu Ăn Cho Người Bận Rộn", "Chay Thanh Tịnh"
            };

            for (int i = 0; i < 30; i++)
            {
                var price = random.Next(50000, 500000);
                var discountPercentage = random.Next(0, 2) == 0 ? (decimal?)random.Next(10, 35) : null;
                var discountAmount = discountPercentage == null ? random.Next(10000, 50000) : 0;

                books.Add(new Book
                {
                    Title = bookTitles[i],
                    Description = $"Mô tả chi tiết về {bookTitles[i]}",
                    OriginalPrice = price,
                    DiscountPercentage = discountPercentage,
                    DiscountAmount = discountAmount,
                    IsOnSale = true,
                    SaleStartDate = DateTime.UtcNow,
                    SaleEndDate = DateTime.UtcNow.AddMonths(1),
                    Quantity = random.Next(10, 100),
                    ISBN = $"978-604-2-1235{i:D2}-{random.Next(1, 10)}",
                    Publisher = publishers[random.Next(publishers.Length)],
                    PublicationYear = random.Next(2015, 2024),
                    ImageUrl = imageUrls[random.Next(imageUrls.Length)],
                    CategoryId = categories[random.Next(categories.Length)],
                    AuthorId = authors[random.Next(authors.Length)]
                });
            }

            return books;
        }
    }
}
