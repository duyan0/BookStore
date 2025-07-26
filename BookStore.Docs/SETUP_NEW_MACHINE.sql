-- =============================================
-- BookStore Database Setup Script for New Machine
-- =============================================
-- This script creates the BookStore database and initial data
-- Run this script on a new machine to setup the complete database

-- =============================================
-- 1. CREATE DATABASE
-- =============================================
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'BookStoreDB_VD')
BEGIN
    CREATE DATABASE BookStoreDB_VD;
    PRINT 'Database BookStoreDB_VD created successfully.';
END
ELSE
BEGIN
    PRINT 'Database BookStoreDB_VD already exists.';
END
GO

USE BookStoreDB_VD;
GO

-- =============================================
-- 2. CREATE TABLES
-- =============================================

-- Categories Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Categories' AND xtype='U')
BEGIN
    CREATE TABLE Categories (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Name NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NULL
    );
    PRINT 'Categories table created.';
END

-- Authors Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Authors' AND xtype='U')
BEGIN
    CREATE TABLE Authors (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FirstName NVARCHAR(50) NOT NULL,
        LastName NVARCHAR(50) NOT NULL,
        Biography NVARCHAR(MAX),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NULL
    );
    PRINT 'Authors table created.';
END

-- Users Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Email NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash NVARCHAR(255) NOT NULL,
        FirstName NVARCHAR(50) NOT NULL,
        LastName NVARCHAR(50) NOT NULL,
        PhoneNumber NVARCHAR(20),
        Address NVARCHAR(500),
        Role NVARCHAR(20) NOT NULL DEFAULT 'Customer',
        AvatarUrl NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NULL
    );
    PRINT 'Users table created.';
END

-- Books Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Books' AND xtype='U')
BEGIN
    CREATE TABLE Books (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(MAX),
        Price DECIMAL(18,2) NOT NULL,
        Quantity INT NOT NULL DEFAULT 0,
        ImageUrl NVARCHAR(500),
        ISBN NVARCHAR(20),
        Publisher NVARCHAR(100),
        PublishedYear INT,
        CategoryId INT NOT NULL,
        AuthorId INT NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NULL,
        FOREIGN KEY (CategoryId) REFERENCES Categories(Id),
        FOREIGN KEY (AuthorId) REFERENCES Authors(Id)
    );
    PRINT 'Books table created.';
END

-- Orders Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Orders' AND xtype='U')
BEGIN
    CREATE TABLE Orders (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserId INT NOT NULL,
        TotalAmount DECIMAL(18,2) NOT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
        ShippingAddress NVARCHAR(500) NOT NULL,
        PhoneNumber NVARCHAR(20) NOT NULL,
        Notes NVARCHAR(500),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NULL,
        FOREIGN KEY (UserId) REFERENCES Users(Id)
    );
    PRINT 'Orders table created.';
END

-- OrderDetails Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='OrderDetails' AND xtype='U')
BEGIN
    CREATE TABLE OrderDetails (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        OrderId INT NOT NULL,
        BookId INT NOT NULL,
        Quantity INT NOT NULL,
        UnitPrice DECIMAL(18,2) NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NULL,
        FOREIGN KEY (OrderId) REFERENCES Orders(Id) ON DELETE CASCADE,
        FOREIGN KEY (BookId) REFERENCES Books(Id)
    );
    PRINT 'OrderDetails table created.';
END

-- Reviews Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Reviews' AND xtype='U')
BEGIN
    CREATE TABLE Reviews (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        BookId INT NOT NULL,
        UserId INT NOT NULL,
        Rating INT NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
        Comment NVARCHAR(1000),
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NULL,
        FOREIGN KEY (BookId) REFERENCES Books(Id),
        FOREIGN KEY (UserId) REFERENCES Users(Id)
    );
    PRINT 'Reviews table created.';
END

-- Sliders Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Sliders' AND xtype='U')
BEGIN
    CREATE TABLE Sliders (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(500),
        ImageUrl NVARCHAR(500) NOT NULL,
        LinkUrl NVARCHAR(500),
        DisplayOrder INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NULL
    );
    PRINT 'Sliders table created.';
END

-- Banners Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Banners' AND xtype='U')
BEGIN
    CREATE TABLE Banners (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Title NVARCHAR(200) NOT NULL,
        Content NVARCHAR(1000),
        ImageUrl NVARCHAR(500) NOT NULL,
        LinkUrl NVARCHAR(500),
        Position NVARCHAR(50) NOT NULL DEFAULT 'Home',
        DisplayOrder INT NOT NULL DEFAULT 0,
        IsActive BIT NOT NULL DEFAULT 1,
        StartDate DATETIME2,
        EndDate DATETIME2,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
        UpdatedAt DATETIME2 NULL
    );
    PRINT 'Banners table created.';
END

-- Migration History Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='__EFMigrationsHistory' AND xtype='U')
BEGIN
    CREATE TABLE __EFMigrationsHistory (
        MigrationId NVARCHAR(150) NOT NULL PRIMARY KEY,
        ProductVersion NVARCHAR(32) NOT NULL
    );
    PRINT '__EFMigrationsHistory table created.';
END

PRINT 'All tables created successfully!';
GO

-- =============================================
-- 3. INSERT SAMPLE DATA
-- =============================================

-- Insert Categories
IF NOT EXISTS (SELECT * FROM Categories)
BEGIN
    INSERT INTO Categories (Name, Description) VALUES
    ('Văn học', 'Sách văn học trong và ngoài nước'),
    ('Khoa học', 'Sách khoa học và công nghệ'),
    ('Lịch sử', 'Sách lịch sử và văn hóa'),
    ('Kinh tế', 'Sách kinh tế và quản lý'),
    ('Giáo dục', 'Sách giáo khoa và tham khảo'),
    ('Thiếu nhi', 'Sách dành cho trẻ em'),
    ('Tâm lý', 'Sách tâm lý và phát triển bản thân'),
    ('Kỹ năng sống', 'Sách kỹ năng và phát triển cá nhân');
    PRINT 'Sample categories inserted.';
END

-- Insert Authors
IF NOT EXISTS (SELECT * FROM Authors)
BEGIN
    INSERT INTO Authors (FirstName, LastName, Biography) VALUES
    ('Nguyễn Du', '', 'Đại thi hào của dân tộc Việt Nam, tác giả của Truyện Kiều'),
    ('Nam Cao', '', 'Nhà văn hiện thực Việt Nam, tác giả của Chí Phèo, Lão Hạc'),
    ('Tô Hoài', '', 'Nhà văn Việt Nam, tác giả của Dế Mèn phiêu lưu ký'),
    ('Dale Carnegie', '', 'Tác giả người Mỹ nổi tiếng với cuốn Đắc nhân tâm'),
    ('Robert Kiyosaki', '', 'Tác giả cuốn Cha giàu cha nghèo'),
    ('Paulo Coelho', '', 'Nhà văn Brazil, tác giả của Nhà giả kim'),
    ('Haruki Murakami', '', 'Nhà văn Nhật Bản nổi tiếng thế giới'),
    ('Yuval Noah Harari', '', 'Sử gia và tác giả cuốn Sapiens');
    PRINT 'Sample authors inserted.';
END


-- Insert Sample Books
IF NOT EXISTS (SELECT * FROM Books)
BEGIN
    INSERT INTO Books (Title, Description, Price, Quantity, CategoryId, AuthorId, ISBN, Publisher, PublishedYear) VALUES
    ('Truyện Kiều', 'Tác phẩm bất hủ của Nguyễn Du', 150000, 50, 1, 1, '978-604-2-12345-1', 'NXB Văn học', 2020),
    ('Chí Phèo', 'Truyện ngắn nổi tiếng của Nam Cao', 120000, 30, 1, 2, '978-604-2-12345-2', 'NXB Văn học', 2019),
    ('Dế Mèn phiêu lưu ký', 'Tác phẩm thiếu nhi kinh điển', 180000, 40, 6, 3, '978-604-2-12345-3', 'NXB Kim Đồng', 2021),
    ('Đắc nhân tâm', 'Cuốn sách kỹ năng sống nổi tiếng', 200000, 100, 7, 4, '978-604-2-12345-4', 'NXB Tổng hợp TP.HCM', 2020),
    ('Cha giàu cha nghèo', 'Sách về tư duy tài chính', 250000, 80, 4, 5, '978-604-2-12345-5', 'NXB Lao động', 2019),
    ('Nhà giả kim', 'Tiểu thuyết triết lý nổi tiếng', 180000, 60, 1, 6, '978-604-2-12345-6', 'NXB Hội Nhà văn', 2020),
    ('Norwegian Wood', 'Tiểu thuyết của Haruki Murakami', 220000, 45, 1, 7, '978-604-2-12345-7', 'NXB Văn học', 2021),
    ('Sapiens', 'Lược sử loài người', 300000, 70, 3, 8, '978-604-2-12345-8', 'NXB Thế giới', 2020);
    PRINT 'Sample books inserted.';
END

-- Insert Sample Sliders
IF NOT EXISTS (SELECT * FROM Sliders)
BEGIN
    INSERT INTO Sliders (Title, Description, ImageUrl, LinkUrl, DisplayOrder, IsActive) VALUES
    ('Chào mừng đến BookStore', 'Khám phá thế giới sách cùng chúng tôi', '/images/slider1.jpg', '/Shop', 1, 1),
    ('Sách mới 2024', 'Cập nhật những cuốn sách mới nhất', '/images/slider2.jpg', '/Shop', 2, 1),
    ('Khuyến mãi đặc biệt', 'Giảm giá lên đến 50% cho sách văn học', '/images/slider3.jpg', '/Shop', 3, 1);
    PRINT 'Sample sliders inserted.';
END

-- Insert Sample Banners
IF NOT EXISTS (SELECT * FROM Banners)
BEGIN
    INSERT INTO Banners (Title, Content, ImageUrl, Position, DisplayOrder, IsActive) VALUES
    ('Sách hay tháng này', 'Khám phá những cuốn sách được yêu thích nhất', '/images/banner1.jpg', 'Home', 1, 1),
    ('Miễn phí vận chuyển', 'Đơn hàng từ 500.000đ được miễn phí ship', '/images/banner2.jpg', 'Home', 2, 1);
    PRINT 'Sample banners inserted.';
END

-- Insert Migration History
IF NOT EXISTS (SELECT * FROM __EFMigrationsHistory WHERE MigrationId = '20250721071656_InitialCreate')
BEGIN
    INSERT INTO __EFMigrationsHistory (MigrationId, ProductVersion) VALUES
    ('20250721071656_InitialCreate', '9.0.7'),
    ('20250721075300_NewDatabaseName', '9.0.7'),
    ('20250125120000_AddAvatarUrlToUser', '9.0.7');
    PRINT 'Migration history updated.';
END

PRINT '==============================================';
PRINT 'BookStore Database Setup Complete!';
PRINT '==============================================';
PRINT 'Admin Login: admin@bookstore.com / Admin123!';
PRINT 'Database: BookStoreDB';
PRINT 'Tables: 9 tables created with sample data';
PRINT '==============================================';
GO