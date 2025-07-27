# 📚 BookStore API Documentation

## 🎯 Tổng quan
Tài liệu này mô tả chi tiết tất cả các API endpoints của hệ thống BookStore, bao gồm request/response format, authentication requirements và ví dụ sử dụng.

## 🔗 Base URL
```
http://localhost:5274/api
```

## 🔐 Authentication
Hệ thống sử dụng JWT Bearer Token authentication.

### Headers cần thiết:
```http
Authorization: Bearer {your-jwt-token}
Content-Type: application/json
```

### Roles:
- **Admin**: Toàn quyền truy cập
- **User**: Quyền hạn chế (chỉ xem và tương tác với dữ liệu của mình)

---

## 🔑 Authentication APIs

### 1. Đăng ký tài khoản
```http
POST /api/Auth/register
```

**Request Body:**
```json
{
  "username": "john_doe",
  "email": "john@example.com",
  "password": "Password123!",
  "firstName": "John",
  "lastName": "Doe",
  "phone": "0123456789",
  "address": "123 Main St, City"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Đăng ký thành công",
  "token": "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "username": "john_doe",
    "email": "john@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "fullName": "John Doe",
    "phone": "0123456789",
    "address": "123 Main St, City",
    "avatarUrl": null,
    "isAdmin": false,
    "createdAt": "2025-01-27T10:00:00Z"
  }
}
```

### 2. Đăng nhập
```http
POST /api/Auth/login
```

**Request Body:**
```json
{
  "email": "admin@bookstore.com",
  "password": "Admin123!"
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Đăng nhập thành công",
  "token": "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9...",
  "user": {
    "id": 1,
    "username": "admin",
    "email": "admin@bookstore.com",
    "firstName": "Admin",
    "lastName": "User",
    "fullName": "Admin User",
    "isAdmin": true,
    "createdAt": "2025-01-27T10:00:00Z"
  }
}
```

### 3. Lấy danh sách người dùng (Admin only)
```http
GET /api/Auth/users
```

**Headers:** `Authorization: Bearer {admin-token}`

**Response (200 OK):**
```json
{
  "users": [
    {
      "id": 1,
      "username": "admin",
      "email": "admin@bookstore.com",
      "firstName": "Admin",
      "lastName": "User",
      "fullName": "Admin User",
      "phone": "",
      "address": "",
      "avatarUrl": null,
      "isAdmin": true,
      "createdAt": "2025-01-27T10:00:00Z"
    }
  ],
  "authInfo": {
    "isAuthenticated": true,
    "name": "admin@bookstore.com",
    "roles": ["Admin"]
  }
}
```

---

## 📖 Books APIs

### 1. Lấy danh sách sách
```http
GET /api/Books
```

**Query Parameters:**
- `categoryId` (optional): Lọc theo danh mục
- `authorId` (optional): Lọc theo tác giả
- `page` (optional): Số trang (default: 1)
- `pageSize` (optional): Số items per page (default: 10)

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "title": "Harry Potter and the Philosopher's Stone",
    "description": "The first novel in the Harry Potter series",
    "price": 250000,
    "discountPercentage": 20,
    "discountAmount": 0,
    "isOnSale": true,
    "saleStartDate": "2025-01-01T00:00:00Z",
    "saleEndDate": "2025-02-01T00:00:00Z",
    "discountedPrice": 200000,
    "isDiscountActive": true,
    "totalDiscountAmount": 50000,
    "quantity": 100,
    "isbn": "978-0747532743",
    "publisher": "Bloomsbury",
    "publicationYear": 1997,
    "imageUrl": "https://example.com/harry-potter.jpg",
    "categoryId": 1,
    "categoryName": "Fiction",
    "authorId": 1,
    "authorName": "J.K. Rowling",
    "createdAt": "2025-01-27T10:00:00Z",
    "updatedAt": null
  }
]
```

### 2. Lấy thông tin sách theo ID
```http
GET /api/Books/{id}
```

**Response (200 OK):** Giống như item trong danh sách sách

**Response (404 Not Found):**
```json
{
  "message": "Book not found"
}
```

### 3. Tìm kiếm sách
```http
GET /api/Books/search?term={searchTerm}
```

**Query Parameters:**
- `term` (required): Từ khóa tìm kiếm

**Response (200 OK):** Array of BookDto (giống GET /api/Books)

### 4. Tạo sách mới (Admin only)
```http
POST /api/Books
```

**Headers:** `Authorization: Bearer {admin-token}`

**Request Body:**
```json
{
  "title": "New Book Title",
  "description": "Book description",
  "price": 300000,
  "discountPercentage": 15,
  "discountAmount": 0,
  "isOnSale": true,
  "saleStartDate": "2025-01-27T00:00:00Z",
  "saleEndDate": "2025-03-01T00:00:00Z",
  "quantity": 50,
  "isbn": "978-1234567890",
  "publisher": "Publisher Name",
  "publicationYear": 2025,
  "imageUrl": "https://example.com/book-cover.jpg",
  "categoryId": 1,
  "authorId": 1
}
```

**Response (201 Created):** BookDto object

### 5. Cập nhật sách (Admin only)
```http
PUT /api/Books/{id}
```

**Headers:** `Authorization: Bearer {admin-token}`

**Request Body:** Giống như POST /api/Books

**Response (200 OK):** BookDto object

### 6. Xóa sách (Admin only)
```http
DELETE /api/Books/{id}
```

**Headers:** `Authorization: Bearer {admin-token}`

**Response (204 No Content)**

---

## 👥 Authors APIs

### 1. Lấy danh sách tác giả
```http
GET /api/Authors
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "firstName": "J.K.",
    "lastName": "Rowling",
    "fullName": "J.K. Rowling",
    "biography": "British author, best known for the Harry Potter series",
    "bookCount": 7,
    "createdAt": "2025-01-27T10:00:00Z",
    "updatedAt": null
  }
]
```

### 2. Lấy thông tin tác giả theo ID
```http
GET /api/Authors/{id}
```

**Response (200 OK):** AuthorDto object

### 3. Tìm kiếm tác giả
```http
GET /api/Authors/search?term={searchTerm}
```

**Response (200 OK):** Array of AuthorDto

### 4. Tạo tác giả mới (Admin only)
```http
POST /api/Authors
```

**Headers:** `Authorization: Bearer {admin-token}`

**Request Body:**
```json
{
  "firstName": "New",
  "lastName": "Author",
  "biography": "Author biography"
}
```

**Response (201 Created):** AuthorDto object

### 5. Cập nhật tác giả (Admin only)
```http
PUT /api/Authors/{id}
```

**Headers:** `Authorization: Bearer {admin-token}`

**Request Body:** Giống như POST /api/Authors

**Response (200 OK):** AuthorDto object

### 6. Xóa tác giả (Admin only)
```http
DELETE /api/Authors/{id}
```

**Headers:** `Authorization: Bearer {admin-token}`

**Response (204 No Content)**

---

## 📂 Categories APIs

### 1. Lấy danh sách danh mục
```http
GET /api/Categories
```

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "name": "Fiction",
    "description": "Fictional books and novels",
    "bookCount": 25,
    "createdAt": "2025-01-27T10:00:00Z",
    "updatedAt": null
  }
]
```

### 2. Lấy thông tin danh mục theo ID
```http
GET /api/Categories/{id}
```

**Response (200 OK):** CategoryDto object

### 3. Tạo danh mục mới (Admin only)
```http
POST /api/Categories
```

**Headers:** `Authorization: Bearer {admin-token}`

**Request Body:**
```json
{
  "name": "Science Fiction",
  "description": "Science fiction books"
}
```

**Response (201 Created):** CategoryDto object

### 4. Cập nhật danh mục (Admin only)
```http
PUT /api/Categories/{id}
```

**Headers:** `Authorization: Bearer {admin-token}`

**Request Body:** Giống như POST /api/Categories

**Response (200 OK):** CategoryDto object

### 5. Xóa danh mục (Admin only)
```http
DELETE /api/Categories/{id}
```

**Headers:** `Authorization: Bearer {admin-token}`

**Response (204 No Content)**

---

## 🛒 Orders APIs

### 1. Lấy danh sách đơn hàng (Admin only)
```http
GET /api/Orders
```

**Headers:** `Authorization: Bearer {admin-token}`

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "userId": 2,
    "userName": "john_doe",
    "userFullName": "John Doe",
    "orderDate": "2025-01-27T10:00:00Z",
    "totalAmount": 500000,
    "status": "Pending",
    "shippingAddress": "123 Main St, City",
    "paymentMethod": "Credit Card",
    "orderDetails": [
      {
        "id": 1,
        "bookId": 1,
        "bookTitle": "Harry Potter and the Philosopher's Stone",
        "bookImageUrl": "https://example.com/harry-potter.jpg",
        "quantity": 2,
        "unitPrice": 200000,
        "totalPrice": 400000
      }
    ],
    "createdAt": "2025-01-27T10:00:00Z",
    "updatedAt": null
  }
]
```

### 2. Lấy đơn hàng của user hiện tại
```http
GET /api/Orders/user
```

**Headers:** `Authorization: Bearer {user-token}`

**Response (200 OK):** Array of OrderDto (chỉ đơn hàng của user hiện tại)

### 3. Lấy thông tin đơn hàng theo ID
```http
GET /api/Orders/{id}
```

**Headers:** `Authorization: Bearer {token}`

**Response (200 OK):** OrderDto object

**Note:** User chỉ có thể xem đơn hàng của mình, Admin có thể xem tất cả

### 4. Lấy đơn hàng theo trạng thái (Admin only)
```http
GET /api/Orders/status/{status}
```

**Headers:** `Authorization: Bearer {admin-token}`

**Path Parameters:**
- `status`: pending, processing, shipped, delivered, cancelled

**Response (200 OK):** Array of OrderDto

### 5. Tạo đơn hàng mới
```http
POST /api/Orders
```

**Headers:** `Authorization: Bearer {user-token}`

**Request Body:**
```json
{
  "userId": 2,
  "shippingAddress": "123 Main St, City",
  "paymentMethod": "Credit Card",
  "voucherCode": "DISCOUNT10",
  "voucherDiscount": 50000,
  "freeShipping": false,
  "shippingFee": 30000,
  "subTotal": 400000,
  "orderDetails": [
    {
      "bookId": 1,
      "quantity": 2,
      "unitPrice": 200000
    }
  ]
}
```

**Response (201 Created):** OrderDto object

### 6. Cập nhật trạng thái đơn hàng (Admin only)
```http
PUT /api/Orders/{id}
```

**Headers:** `Authorization: Bearer {admin-token}`

**Request Body:**
```json
{
  "status": "Processing",
  "shippingAddress": "Updated address",
  "paymentMethod": "Bank Transfer"
}
```

**Response (200 OK):** OrderDto object

### 7. Mua lại đơn hàng
```http
POST /api/Orders/{id}/reorder
```

**Headers:** `Authorization: Bearer {user-token}`

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Đã thêm 2 sản phẩm vào giỏ hàng",
  "addedItems": [
    {
      "bookId": 1,
      "bookTitle": "Harry Potter",
      "quantity": 2,
      "unitPrice": 200000
    }
  ],
  "unavailableItems": [],
  "totalItemsAdded": 2,
  "totalItemsUnavailable": 0
}
```

### 8. Thống kê đơn hàng (Admin only)
```http
GET /api/Orders/statistics
```

**Headers:** `Authorization: Bearer {admin-token}`

**Response (200 OK):**
```json
{
  "totalOrders": 150,
  "pendingOrders": 25,
  "processingOrders": 30,
  "shippedOrders": 20,
  "deliveredOrders": 70,
  "cancelledOrders": 5,
  "totalRevenue": 50000000,
  "averageOrderValue": 333333,
  "ordersThisMonth": 45,
  "ordersThisWeek": 12,
  "lastUpdated": "2025-01-27T10:00:00Z"
}
```

---

## ⭐ Reviews APIs

### 1. Lấy danh sách đánh giá (Admin only)
```http
GET /api/Reviews
```

**Headers:** `Authorization: Bearer {admin-token}`

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "bookId": 1,
    "bookTitle": "Harry Potter and the Philosopher's Stone",
    "userId": 2,
    "userName": "john_doe",
    "userFullName": "John Doe",
    "rating": 5,
    "comment": "Excellent book! Highly recommended.",
    "status": "Approved",
    "adminNote": null,
    "reviewedByAdminId": 1,
    "reviewedByAdminName": "Admin User",
    "reviewedAt": "2025-01-27T10:00:00Z",
    "isVerifiedPurchase": true,
    "orderId": 1,
    "helpfulCount": 15,
    "notHelpfulCount": 2,
    "createdAt": "2025-01-27T09:00:00Z",
    "updatedAt": null,
    "statusText": "Đã duyệt",
    "ratingStars": "★★★★★",
    "totalVotes": 17,
    "helpfulPercentage": 88.24
  }
]
```

### 2. Lấy đánh giá theo sách
```http
GET /api/Reviews/book/{bookId}
```

**Query Parameters:**
- `status` (optional): pending, approved, rejected, hidden

**Response (200 OK):** Array of ReviewDto

### 3. Lấy đánh giá của user hiện tại
```http
GET /api/Reviews/user
```

**Headers:** `Authorization: Bearer {user-token}`

**Response (200 OK):** Array of ReviewDto (chỉ đánh giá của user hiện tại)

### 4. Tạo đánh giá mới
```http
POST /api/Reviews
```

**Headers:** `Authorization: Bearer {user-token}`

**Request Body:**
```json
{
  "bookId": 1,
  "rating": 5,
  "comment": "Great book! Really enjoyed reading it.",
  "orderId": 1
}
```

**Response (201 Created):** ReviewDto object

### 5. Cập nhật trạng thái đánh giá (Admin only)
```http
PUT /api/Reviews/{id}/status
```

**Headers:** `Authorization: Bearer {admin-token}`

**Request Body:**
```json
{
  "status": "Approved",
  "adminNote": "Review approved after verification"
}
```

**Response (200 OK):** ReviewDto object

### 6. Đánh giá hữu ích
```http
POST /api/Reviews/{id}/helpful
```

**Headers:** `Authorization: Bearer {user-token}`

**Request Body:**
```json
{
  "isHelpful": true
}
```

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Cảm ơn bạn đã đánh giá!",
  "helpfulCount": 16,
  "notHelpfulCount": 2
}
```

### 7. Thống kê đánh giá (Admin only)
```http
GET /api/Reviews/statistics
```

**Headers:** `Authorization: Bearer {admin-token}`

**Response (200 OK):**
```json
{
  "totalReviews": 250,
  "pendingReviews": 15,
  "approvedReviews": 200,
  "rejectedReviews": 25,
  "hiddenReviews": 10,
  "verifiedPurchaseReviews": 180,
  "averageRating": 4.2,
  "reviewsThisMonth": 45,
  "reviewsThisWeek": 12,
  "lastUpdated": "2025-01-27T10:00:00Z",
  "topReviewedBooks": [
    {
      "bookId": 1,
      "bookTitle": "Harry Potter",
      "reviewCount": 25,
      "averageRating": 4.8
    }
  ]
}
```

---

## 🖼️ Banners APIs

### 1. Lấy danh sách banner (Admin only)
```http
GET /api/Banners
```

**Headers:** `Authorization: Bearer {admin-token}`

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "title": "Giảm giá 20%",
    "description": "Cho tất cả sách văn học",
    "imageUrl": "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c",
    "linkUrl": "/Shop?categoryId=1",
    "displayOrder": 1,
    "isActive": true,
    "position": "home",
    "size": "medium",
    "buttonText": "Mua ngay",
    "buttonStyle": "danger",
    "createdAt": "2025-01-27T10:00:00Z",
    "updatedAt": null
  }
]
```

### 2. Lấy banner công khai
```http
GET /api/Banners/public
```

**Query Parameters:**
- `position` (optional): home, shop, category

**Response (200 OK):** Array of BannerDto (chỉ banner active)

### 3. Tạo banner mới (Admin only)
```http
POST /api/Banners
```

**Headers:** `Authorization: Bearer {admin-token}`

**Request Body:**
```json
{
  "title": "New Banner",
  "description": "Banner description",
  "imageUrl": "https://example.com/banner.jpg",
  "linkUrl": "/shop",
  "displayOrder": 1,
  "isActive": true,
  "position": "home",
  "size": "large",
  "buttonText": "Shop Now",
  "buttonStyle": "primary"
}
```

**Response (201 Created):** BannerDto object

### 4. Cập nhật banner (Admin only)
```http
PUT /api/Banners/{id}
```

**Headers:** `Authorization: Bearer {admin-token}`

**Request Body:** Giống như POST /api/Banners

**Response (200 OK):** BannerDto object

### 5. Xóa banner (Admin only)
```http
DELETE /api/Banners/{id}
```

**Headers:** `Authorization: Bearer {admin-token}`

**Response (204 No Content)**

---

## 🎠 Sliders APIs

### 1. Lấy danh sách slider (Admin only)
```http
GET /api/Sliders
```

**Headers:** `Authorization: Bearer {admin-token}`

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "title": "Welcome to BookStore",
    "description": "Discover amazing books",
    "imageUrl": "https://example.com/slider1.jpg",
    "linkUrl": "/shop",
    "displayOrder": 1,
    "isActive": true,
    "createdAt": "2025-01-27T10:00:00Z",
    "updatedAt": null
  }
]
```

### 2. Lấy slider công khai
```http
GET /api/Sliders/public
```

**Response (200 OK):** Array of SliderDto (chỉ slider active)

### 3. Tạo slider mới (Admin only)
```http
POST /api/Sliders
```

**Headers:** `Authorization: Bearer {admin-token}`

**Request Body:**
```json
{
  "title": "New Slider",
  "description": "Slider description",
  "imageUrl": "https://example.com/slider.jpg",
  "linkUrl": "/shop",
  "displayOrder": 1,
  "isActive": true
}
```

**Response (201 Created):** SliderDto object

### 4. Cập nhật slider (Admin only)
```http
PUT /api/Sliders/{id}
```

**Headers:** `Authorization: Bearer {admin-token}`

**Request Body:** Giống như POST /api/Sliders

**Response (200 OK):** SliderDto object

### 5. Xóa slider (Admin only)
```http
DELETE /api/Sliders/{id}
```

**Headers:** `Authorization: Bearer {admin-token}`

**Response (204 No Content)**

---

## 🎫 Vouchers APIs

### 1. Lấy danh sách voucher (Admin only)
```http
GET /api/Vouchers
```

**Headers:** `Authorization: Bearer {admin-token}`

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "code": "DISCOUNT10",
    "name": "Giảm giá 10%",
    "description": "Giảm 10% cho đơn hàng từ 500k",
    "discountType": "Percentage",
    "discountValue": 10,
    "minimumOrderAmount": 500000,
    "maximumDiscountAmount": 100000,
    "usageLimit": 100,
    "usedCount": 25,
    "isActive": true,
    "validFrom": "2025-01-01T00:00:00Z",
    "validTo": "2025-02-01T00:00:00Z",
    "createdAt": "2025-01-27T10:00:00Z",
    "updatedAt": null
  }
]
```

### 2. Kiểm tra voucher
```http
GET /api/Vouchers/validate/{code}
```

**Headers:** `Authorization: Bearer {user-token}`

**Query Parameters:**
- `orderAmount` (required): Số tiền đơn hàng

**Response (200 OK):**
```json
{
  "isValid": true,
  "message": "Voucher hợp lệ",
  "voucher": {
    "id": 1,
    "code": "DISCOUNT10",
    "name": "Giảm giá 10%",
    "discountType": "Percentage",
    "discountValue": 10,
    "maximumDiscountAmount": 100000
  },
  "discountAmount": 50000,
  "finalAmount": 450000
}
```

### 3. Tạo voucher mới (Admin only)
```http
POST /api/Vouchers
```

**Headers:** `Authorization: Bearer {admin-token}`

**Request Body:**
```json
{
  "code": "NEWDISCOUNT",
  "name": "New Discount",
  "description": "Special discount",
  "discountType": "Percentage",
  "discountValue": 15,
  "minimumOrderAmount": 300000,
  "maximumDiscountAmount": 150000,
  "usageLimit": 50,
  "isActive": true,
  "validFrom": "2025-01-27T00:00:00Z",
  "validTo": "2025-03-01T00:00:00Z"
}
```

**Response (201 Created):** VoucherDto object

### 4. Cập nhật voucher (Admin only)
```http
PUT /api/Vouchers/{id}
```

**Headers:** `Authorization: Bearer {admin-token}`

**Request Body:** Giống như POST /api/Vouchers

**Response (200 OK):** VoucherDto object

### 5. Xóa voucher (Admin only)
```http
DELETE /api/Vouchers/{id}
```

**Headers:** `Authorization: Bearer {admin-token}`

**Response (204 No Content)**

---

## 📚 Help Articles APIs

### 1. Lấy danh sách bài viết hỗ trợ
```http
GET /api/HelpArticles
```

**Query Parameters:**
- `category` (optional): Lọc theo danh mục
- `search` (optional): Tìm kiếm theo tiêu đề/nội dung

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "title": "Cách đặt hàng",
    "content": "Hướng dẫn chi tiết cách đặt hàng...",
    "category": "ordering",
    "tags": ["đặt hàng", "hướng dẫn"],
    "isPublished": true,
    "viewCount": 150,
    "createdAt": "2025-01-27T10:00:00Z",
    "updatedAt": null
  }
]
```

### 2. Lấy bài viết theo ID
```http
GET /api/HelpArticles/{id}
```

**Response (200 OK):** HelpArticleDto object

### 3. Tìm kiếm bài viết
```http
GET /api/HelpArticles/search?query={searchTerm}
```

**Response (200 OK):** Array of HelpArticleDto

### 4. Tạo bài viết mới (Admin only)
```http
POST /api/HelpArticles
```

**Headers:** `Authorization: Bearer {admin-token}`

**Request Body:**
```json
{
  "title": "New Help Article",
  "content": "Article content...",
  "category": "general",
  "tags": ["help", "guide"],
  "isPublished": true
}
```

**Response (201 Created):** HelpArticleDto object

### 5. Cập nhật bài viết (Admin only)
```http
PUT /api/HelpArticles/{id}
```

**Headers:** `Authorization: Bearer {admin-token}`

**Request Body:** Giống như POST /api/HelpArticles

**Response (200 OK):** HelpArticleDto object

### 6. Xóa bài viết (Admin only)
```http
DELETE /api/HelpArticles/{id}
```

**Headers:** `Authorization: Bearer {admin-token}`

**Response (204 No Content)**

---

## 📷 Book Image APIs

### 1. Upload ảnh sách (Admin only)
```http
POST /api/BookImage/upload
```

**Headers:**
- `Authorization: Bearer {admin-token}`
- `Content-Type: multipart/form-data`

**Request Body (Form Data):**
- `file`: Image file (jpg, jpeg, png, gif, webp, max 5MB)

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Upload thành công",
  "imageUrl": "/uploads/books/20250127_book_12345.jpg",
  "fileName": "20250127_book_12345.jpg",
  "fileSize": 1024000
}
```

### 2. Lấy danh sách ảnh (Admin only)
```http
GET /api/BookImage/images
```

**Headers:** `Authorization: Bearer {admin-token}`

**Response (200 OK):**
```json
{
  "images": [
    {
      "fileName": "20250127_book_12345.jpg",
      "url": "/uploads/books/20250127_book_12345.jpg",
      "size": 1024000,
      "uploadDate": "2025-01-27T10:00:00Z"
    }
  ],
  "totalCount": 1
}
```

### 3. Xóa ảnh (Admin only)
```http
DELETE /api/BookImage/{fileName}
```

**Headers:** `Authorization: Bearer {admin-token}`

**Response (200 OK):**
```json
{
  "success": true,
  "message": "Xóa ảnh thành công"
}
```

---

## 🚫 Error Responses

### Common Error Formats:

**400 Bad Request:**
```json
{
  "message": "Validation failed",
  "errors": {
    "Title": ["The Title field is required."],
    "Price": ["The field Price must be between 0 and 10000000."]
  }
}
```

**401 Unauthorized:**
```json
{
  "message": "Unauthorized access"
}
```

**403 Forbidden:**
```json
{
  "message": "Access denied. Admin role required."
}
```

**404 Not Found:**
```json
{
  "message": "Resource not found"
}
```

**500 Internal Server Error:**
```json
{
  "message": "An error occurred while processing your request"
}
```

---

## 📝 Notes

### Authentication Flow:
1. Đăng ký/Đăng nhập để nhận JWT token
2. Gửi token trong header `Authorization: Bearer {token}`
3. Token có thời hạn, cần refresh khi hết hạn

### Pagination:
- Các API danh sách hỗ trợ pagination với query parameters `page` và `pageSize`
- Default: page=1, pageSize=10

### File Upload:
- Chỉ hỗ trợ các định dạng: jpg, jpeg, png, gif, webp
- Kích thước tối đa: 5MB
- Files được lưu trong thư mục `/wwwroot/uploads/`

### Status Codes:
- **200**: Success
- **201**: Created
- **204**: No Content (Delete success)
- **400**: Bad Request (Validation error)
- **401**: Unauthorized
- **403**: Forbidden
- **404**: Not Found
- **500**: Internal Server Error

---

*Tài liệu API được cập nhật cho BookStore v1.0 - Ngày cập nhật: 27/07/2025*
