using BookStore.Core.Entities;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace BookStore.Infrastructure.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            await context.Database.MigrateAsync();

            // Check if there is any data in the database
            if (await context.Users.AnyAsync() || await context.Categories.AnyAsync() || 
                await context.Authors.AnyAsync() || await context.Books.AnyAsync())
            {
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
            await context.SaveChangesAsync();
        }

        private static string HashPassword(string password)
        {
            using var hmac = new HMACSHA512();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            // Combine salt and hash
            var hashBytes = new byte[salt.Length + hash.Length];
            Array.Copy(salt, 0, hashBytes, 0, salt.Length);
            Array.Copy(hash, 0, hashBytes, salt.Length, hash.Length);

            return Convert.ToBase64String(hashBytes);
        }
    }
} 