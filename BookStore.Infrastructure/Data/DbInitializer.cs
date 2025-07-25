using BookStore.Core.Entities;
using BookStore.Core.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            await context.Database.MigrateAsync();

            // Check and add AvatarUrl column if it doesn't exist
            await EnsureAvatarUrlColumnExists(context);

            // Check if there is any data in the database
            if (await context.Users.AnyAsync() || await context.Categories.AnyAsync() ||
                await context.Authors.AnyAsync() || await context.Books.AnyAsync())
            {
                // Main data exists, but check if Sliders and Banners need seeding
                await SeedSlidersIfNeeded(context);
                await SeedBannersIfNeeded(context);
                return; // DB has been seeded
            }

            // Seed Admin User
            var adminUser = new User
            {
                Username = "admin",
                Email = "admin@bookstore.com",
                PasswordHash = HashPassword("Admin@123"),
                FirstName = "Admin",
                LastName = "User",
                Phone = "1234567890",
                Address = "Admin Address",
                IsAdmin = true,
                CreatedAt = DateTime.UtcNow
            };

            await context.Users.AddAsync(adminUser);

            // Seed Categories
            var categories = new List<Category>
            {
                new Category { Name = "Fiction", Description = "Fictional literature", CreatedAt = DateTime.UtcNow },
                new Category { Name = "Non-Fiction", Description = "Non-fictional literature", CreatedAt = DateTime.UtcNow },
                new Category { Name = "Science Fiction", Description = "Science fiction literature", CreatedAt = DateTime.UtcNow },
                new Category { Name = "Mystery", Description = "Mystery literature", CreatedAt = DateTime.UtcNow },
                new Category { Name = "Romance", Description = "Romance literature", CreatedAt = DateTime.UtcNow }
            };

            await context.Categories.AddRangeAsync(categories);

            // Seed Authors
            var authors = new List<Author>
            {
                new Author { FirstName = "J.K.", LastName = "Rowling", Biography = "British author best known for the Harry Potter series", CreatedAt = DateTime.UtcNow },
                new Author { FirstName = "George R.R.", LastName = "Martin", Biography = "American novelist best known for A Song of Ice and Fire", CreatedAt = DateTime.UtcNow },
                new Author { FirstName = "Stephen", LastName = "King", Biography = "American author of horror, supernatural fiction, suspense, and fantasy novels", CreatedAt = DateTime.UtcNow },
                new Author { FirstName = "Agatha", LastName = "Christie", Biography = "English writer known for her detective novels", CreatedAt = DateTime.UtcNow },
                new Author { FirstName = "J.R.R.", LastName = "Tolkien", Biography = "English writer, poet, philologist, and academic", CreatedAt = DateTime.UtcNow }
            };

            await context.Authors.AddRangeAsync(authors);
            await context.SaveChangesAsync();

            // Get saved categories and authors
            var savedCategories = await context.Categories.ToListAsync();
            var savedAuthors = await context.Authors.ToListAsync();

            // Seed Books
            var books = new List<Book>
            {
                new Book
                {
                    Title = "Harry Potter and the Philosopher's Stone",
                    Description = "The first novel in the Harry Potter series",
                    Price = 19.99m,
                    Quantity = 100,
                    ISBN = "978-0747532743",
                    Publisher = "Bloomsbury",
                    PublicationYear = 1997,
                    ImageUrl = "https://example.com/harry-potter.jpg",
                    CategoryId = savedCategories.First(c => c.Name == "Fiction").Id,
                    AuthorId = savedAuthors.First(a => a.LastName == "Rowling").Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Book
                {
                    Title = "A Game of Thrones",
                    Description = "The first novel in A Song of Ice and Fire series",
                    Price = 24.99m,
                    Quantity = 80,
                    ISBN = "978-0553103540",
                    Publisher = "Bantam Spectra",
                    PublicationYear = 1996,
                    ImageUrl = "https://example.com/game-of-thrones.jpg",
                    CategoryId = savedCategories.First(c => c.Name == "Fiction").Id,
                    AuthorId = savedAuthors.First(a => a.LastName == "Martin").Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Book
                {
                    Title = "The Shining",
                    Description = "A horror novel by Stephen King",
                    Price = 18.99m,
                    Quantity = 70,
                    ISBN = "978-0385121675",
                    Publisher = "Doubleday",
                    PublicationYear = 1977,
                    ImageUrl = "https://example.com/the-shining.jpg",
                    CategoryId = savedCategories.First(c => c.Name == "Fiction").Id,
                    AuthorId = savedAuthors.First(a => a.LastName == "King").Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Book
                {
                    Title = "Murder on the Orient Express",
                    Description = "A detective novel by Agatha Christie",
                    Price = 15.99m,
                    Quantity = 60,
                    ISBN = "978-0007119318",
                    Publisher = "Collins Crime Club",
                    PublicationYear = 1934,
                    ImageUrl = "https://example.com/murder-orient-express.jpg",
                    CategoryId = savedCategories.First(c => c.Name == "Mystery").Id,
                    AuthorId = savedAuthors.First(a => a.LastName == "Christie").Id,
                    CreatedAt = DateTime.UtcNow
                },
                new Book
                {
                    Title = "The Lord of the Rings",
                    Description = "An epic high-fantasy novel",
                    Price = 29.99m,
                    Quantity = 90,
                    ISBN = "978-0618640157",
                    Publisher = "Allen & Unwin",
                    PublicationYear = 1954,
                    ImageUrl = "https://example.com/lord-of-the-rings.jpg",
                    CategoryId = savedCategories.First(c => c.Name == "Fiction").Id,
                    AuthorId = savedAuthors.First(a => a.LastName == "Tolkien").Id,
                    CreatedAt = DateTime.UtcNow
                }
            };

            await context.Books.AddRangeAsync(books);

            // Seed Sliders
            var sliders = new List<Slider>
            {
                new Slider
                {
                    Title = "Khám phá thế giới sách",
                    Description = "Hàng ngàn đầu sách chất lượng cao đang chờ bạn khám phá",
                    ImageUrl = "https://images.unsplash.com/photo-1481627834876-b7833e8f5570?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=1200&q=80",
                    LinkUrl = "/Shop",
                    DisplayOrder = 1,
                    IsActive = true,
                    ButtonText = "Khám phá ngay",
                    ButtonStyle = "primary",
                    CreatedAt = BookStore.Core.Extensions.DateTimeExtensions.GetVietnamNow()
                },
                new Slider
                {
                    Title = "Sách mới nhất 2025",
                    Description = "Cập nhật những cuốn sách hot nhất năm 2025",
                    ImageUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=1200&q=80",
                    LinkUrl = "/Shop",
                    DisplayOrder = 2,
                    IsActive = true,
                    ButtonText = "Xem ngay",
                    ButtonStyle = "success",
                    CreatedAt = BookStore.Core.Extensions.DateTimeExtensions.GetVietnamNow()
                }
            };

            await context.Sliders.AddRangeAsync(sliders);

            // Seed Banners
            var banners = new List<Banner>
            {
                new Banner
                {
                    Title = "Giảm giá 20%",
                    Description = "Cho tất cả sách văn học",
                    ImageUrl = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=400&q=80",
                    LinkUrl = "/Shop?categoryId=1",
                    DisplayOrder = 1,
                    IsActive = true,
                    Position = "home",
                    Size = "medium",
                    ButtonText = "Mua ngay",
                    ButtonStyle = "danger",
                    CreatedAt = BookStore.Core.Extensions.DateTimeExtensions.GetVietnamNow()
                },
                new Banner
                {
                    Title = "Sách kỹ năng",
                    Description = "Phát triển bản thân mỗi ngày",
                    ImageUrl = "https://images.unsplash.com/photo-1434030216411-0b793f4b4173?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=400&q=80",
                    LinkUrl = "/Shop?categoryId=2",
                    DisplayOrder = 2,
                    IsActive = true,
                    Position = "home",
                    Size = "medium",
                    ButtonText = "Khám phá",
                    ButtonStyle = "info",
                    CreatedAt = BookStore.Core.Extensions.DateTimeExtensions.GetVietnamNow()
                },
                new Banner
                {
                    Title = "Sách thiếu nhi",
                    Description = "Nuôi dưỡng tâm hồn trẻ thơ",
                    ImageUrl = "https://images.unsplash.com/photo-1503676260728-1c00da094a0b?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=400&q=80",
                    LinkUrl = "/Shop?categoryId=3",
                    DisplayOrder = 3,
                    IsActive = true,
                    Position = "home",
                    Size = "medium",
                    ButtonText = "Xem thêm",
                    ButtonStyle = "warning",
                    CreatedAt = BookStore.Core.Extensions.DateTimeExtensions.GetVietnamNow()
                },
                new Banner
                {
                    Title = "Miễn phí ship",
                    Description = "Cho đơn hàng từ 200k",
                    ImageUrl = "https://images.unsplash.com/photo-1586953208448-b95a79798f07?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=400&q=80",
                    LinkUrl = "/Shop",
                    DisplayOrder = 4,
                    IsActive = true,
                    Position = "home",
                    Size = "medium",
                    ButtonText = "Mua sắm",
                    ButtonStyle = "success",
                    CreatedAt = BookStore.Core.Extensions.DateTimeExtensions.GetVietnamNow()
                }
            };

            await context.Banners.AddRangeAsync(banners);

            await context.SaveChangesAsync();
        }

        private static async Task SeedSlidersIfNeeded(ApplicationDbContext context)
        {
            if (await context.Sliders.AnyAsync())
                return; // Sliders already exist

            var sliders = new List<Slider>
            {
                new Slider
                {
                    Title = "Khám phá thế giới sách",
                    Description = "Hàng ngàn đầu sách hay đang chờ bạn",
                    ImageUrl = "https://images.unsplash.com/photo-1481627834876-b7833e8f5570?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=1000&q=80",
                    LinkUrl = "/Shop",
                    DisplayOrder = 1,
                    IsActive = true,
                    ButtonText = "Khám phá ngay",
                    ButtonStyle = "primary",
                    CreatedAt = DateTimeExtensions.GetVietnamNow()
                },
                new Slider
                {
                    Title = "Ưu đãi đặc biệt",
                    Description = "Giảm giá lên đến 50% cho sách bestseller",
                    ImageUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=1000&q=80",
                    LinkUrl = "/Shop",
                    DisplayOrder = 2,
                    IsActive = true,
                    ButtonText = "Xem ngay",
                    ButtonStyle = "success",
                    CreatedAt = DateTimeExtensions.GetVietnamNow()
                }
            };

            await context.Sliders.AddRangeAsync(sliders);
            await context.SaveChangesAsync();
        }

        private static async Task SeedBannersIfNeeded(ApplicationDbContext context)
        {
            // Check if home banners already exist
            if (await context.Banners.AnyAsync(b => b.Position == "home"))
                return; // Home banners already exist

            var banners = new List<Banner>
            {
                new Banner
                {
                    Title = "Giảm giá 20%",
                    Description = "Cho tất cả sách văn học",
                    ImageUrl = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=400&q=80",
                    LinkUrl = "/Shop?categoryId=1",
                    DisplayOrder = 1,
                    IsActive = true,
                    Position = "home",
                    Size = "medium",
                    ButtonText = "Mua ngay",
                    ButtonStyle = "danger",
                    CreatedAt = DateTimeExtensions.GetVietnamNow()
                },
                new Banner
                {
                    Title = "Sách kỹ năng",
                    Description = "Phát triển bản thân mỗi ngày",
                    ImageUrl = "https://images.unsplash.com/photo-1434030216411-0b793f4b4173?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=400&q=80",
                    LinkUrl = "/Shop?categoryId=2",
                    DisplayOrder = 2,
                    IsActive = true,
                    Position = "home",
                    Size = "medium",
                    ButtonText = "Khám phá",
                    ButtonStyle = "info",
                    CreatedAt = DateTimeExtensions.GetVietnamNow()
                },
                new Banner
                {
                    Title = "Sách thiếu nhi",
                    Description = "Nuôi dưỡng tâm hồn trẻ thơ",
                    ImageUrl = "https://images.unsplash.com/photo-1503676260728-1c00da094a0b?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=400&q=80",
                    LinkUrl = "/Shop?categoryId=3",
                    DisplayOrder = 3,
                    IsActive = true,
                    Position = "home",
                    Size = "medium",
                    ButtonText = "Xem thêm",
                    ButtonStyle = "warning",
                    CreatedAt = DateTimeExtensions.GetVietnamNow()
                },
                new Banner
                {
                    Title = "Miễn phí ship",
                    Description = "Cho đơn hàng từ 200k",
                    ImageUrl = "https://images.unsplash.com/photo-1586953208448-b95a79798f07?ixlib=rb-4.0.3&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D&auto=format&fit=crop&w=400&q=80",
                    LinkUrl = "/Shop",
                    DisplayOrder = 4,
                    IsActive = true,
                    Position = "home",
                    Size = "medium",
                    ButtonText = "Mua sắm",
                    ButtonStyle = "success",
                    CreatedAt = DateTimeExtensions.GetVietnamNow()
                }
            };

            await context.Banners.AddRangeAsync(banners);
            await context.SaveChangesAsync();
        }

        private static string HashPassword(string password)
        {
            // Store password as plain text (as requested)
            return password;
        }

        private static async Task EnsureAvatarUrlColumnExists(ApplicationDbContext context)
        {
            try
            {
                // Try to execute a simple query that uses the AvatarUrl column
                // If it fails, the column doesn't exist and we need to add it
                await context.Database.ExecuteSqlRawAsync(@"
                    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND name = 'AvatarUrl')
                    BEGIN
                        ALTER TABLE [dbo].[Users] ADD [AvatarUrl] NVARCHAR(255) NULL;
                        PRINT 'AvatarUrl column added successfully to Users table.';
                    END
                ");

                Console.WriteLine("AvatarUrl column check completed successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error ensuring AvatarUrl column exists: {ex.Message}");
                // Don't throw - let the application continue
            }
        }
    }
}