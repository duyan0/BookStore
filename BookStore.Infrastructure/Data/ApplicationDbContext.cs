using BookStore.Core.Entities;
using BookStore.Core.Extensions;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Book> Books { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Slider> Sliders { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<Voucher> Vouchers { get; set; }
        public DbSet<VoucherUsage> VoucherUsages { get; set; }
        public DbSet<ReviewHelpfulness> ReviewHelpfulness { get; set; }
        public DbSet<HelpArticle> HelpArticles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Book
            modelBuilder.Entity<Book>()
                .HasOne(b => b.Category)
                .WithMany(c => c.Books)
                .HasForeignKey(b => b.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Book>()
                .HasOne(b => b.Author)
                .WithMany(a => a.Books)
                .HasForeignKey(b => b.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Order
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // OrderDetail
            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Book)
                .WithMany(b => b.OrderDetails)
                .HasForeignKey(od => od.BookId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Book)
                .WithMany(b => b.Reviews)
                .HasForeignKey(r => r.BookId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Voucher
            modelBuilder.Entity<Voucher>()
                .HasIndex(v => v.Code)
                .IsUnique();

            // Order - Voucher relationship
            modelBuilder.Entity<Order>()
                .HasOne(o => o.Voucher)
                .WithMany(v => v.Orders)
                .HasForeignKey(o => o.VoucherId)
                .OnDelete(DeleteBehavior.SetNull);

            // VoucherUsage relationships
            modelBuilder.Entity<VoucherUsage>()
                .HasOne(vu => vu.Voucher)
                .WithMany(v => v.VoucherUsages)
                .HasForeignKey(vu => vu.VoucherId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VoucherUsage>()
                .HasOne(vu => vu.User)
                .WithMany(u => u.VoucherUsages)
                .HasForeignKey(vu => vu.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<VoucherUsage>()
                .HasOne(vu => vu.Order)
                .WithMany(o => o.VoucherUsages)
                .HasForeignKey(vu => vu.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Review - Admin relationship
            modelBuilder.Entity<Review>()
                .HasOne(r => r.ReviewedByAdmin)
                .WithMany()
                .HasForeignKey(r => r.ReviewedByAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review - Order relationship (for purchase verification)
            modelBuilder.Entity<Review>()
                .HasOne(r => r.Order)
                .WithMany()
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.SetNull);

            // ReviewHelpfulness relationships
            modelBuilder.Entity<ReviewHelpfulness>()
                .HasOne(rh => rh.Review)
                .WithMany()
                .HasForeignKey(rh => rh.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ReviewHelpfulness>()
                .HasOne(rh => rh.User)
                .WithMany()
                .HasForeignKey(rh => rh.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Ensure one vote per user per review
            modelBuilder.Entity<ReviewHelpfulness>()
                .HasIndex(rh => new { rh.ReviewId, rh.UserId })
                .IsUnique();

            // HelpArticle relationships
            modelBuilder.Entity<HelpArticle>()
                .HasOne(ha => ha.Author)
                .WithMany()
                .HasForeignKey(ha => ha.AuthorId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<HelpArticle>()
                .HasOne(ha => ha.LastModifiedBy)
                .WithMany()
                .HasForeignKey(ha => ha.LastModifiedById)
                .OnDelete(DeleteBehavior.Restrict);

            // HelpArticle indexes
            modelBuilder.Entity<HelpArticle>()
                .HasIndex(ha => ha.Slug)
                .IsUnique();

            modelBuilder.Entity<HelpArticle>()
                .HasIndex(ha => new { ha.Type, ha.Category, ha.IsPublished });

            modelBuilder.Entity<HelpArticle>()
                .HasIndex(ha => ha.DisplayOrder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var entries = ChangeTracker
                .Entries()
                .Where(e => e.Entity is BaseEntity && (e.State == EntityState.Added || e.State == EntityState.Modified));

            foreach (var entityEntry in entries)
            {
                var entity = (BaseEntity)entityEntry.Entity;

                if (entityEntry.State == EntityState.Added)
                {
                    entity.CreatedAt = BookStore.Core.Extensions.DateTimeExtensions.GetVietnamNow();
                }
                else if (entityEntry.State == EntityState.Modified)
                {
                    entity.UpdatedAt = BookStore.Core.Extensions.DateTimeExtensions.GetVietnamNow();
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
} 