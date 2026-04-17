-- ============================================================
-- VVKBMS E-Commerce Database Setup Script
-- ============================================================

-- Categories Table
CREATE TABLE Categories (
    CategoryId    INT IDENTITY(1,1) PRIMARY KEY,
    Name          NVARCHAR(100) NOT NULL,
    Description   NVARCHAR(500),
    ImageUrl      NVARCHAR(500),
    IsActive      BIT DEFAULT 1,
    CreatedAt     DATETIME DEFAULT GETDATE()
);

-- Products Table
CREATE TABLE Products (
    ProductId       INT IDENTITY(1,1) PRIMARY KEY,
    CategoryId      INT NOT NULL REFERENCES Categories(CategoryId),
    Name            NVARCHAR(200) NOT NULL,
    Description     NVARCHAR(2000),
    Price           DECIMAL(10,2) NOT NULL,
    OriginalPrice   DECIMAL(10,2),
    StockQty        INT DEFAULT 0,
    ImageUrl        NVARCHAR(500),
    Rating          DECIMAL(3,2) DEFAULT 0,
    ReviewCount     INT DEFAULT 0,
    Brand           NVARCHAR(100),
    IsActive        BIT DEFAULT 1,
    IsFeatured      BIT DEFAULT 0,
    CreatedAt       DATETIME DEFAULT GETDATE()
);

-- Cart Table
CREATE TABLE Cart (
    CartId      INT IDENTITY(1,1) PRIMARY KEY,
    UserId      INT NOT NULL,
    CreatedAt   DATETIME DEFAULT GETDATE(),
    UpdatedAt   DATETIME DEFAULT GETDATE()
);

-- CartItems Table
CREATE TABLE CartItems (
    CartItemId  INT IDENTITY(1,1) PRIMARY KEY,
    CartId      INT NOT NULL REFERENCES Cart(CartId) ON DELETE CASCADE,
    ProductId   INT NOT NULL REFERENCES Products(ProductId),
    Quantity    INT NOT NULL DEFAULT 1,
    Price       DECIMAL(10,2) NOT NULL,
    AddedAt     DATETIME DEFAULT GETDATE()
);

-- Orders Table
CREATE TABLE Orders (
    OrderId         INT IDENTITY(1,1) PRIMARY KEY,
    UserId          INT NOT NULL,
    OrderDate       DATETIME DEFAULT GETDATE(),
    Status          NVARCHAR(50) DEFAULT 'Pending',
    TotalAmount     DECIMAL(10,2) NOT NULL,
    ShippingAddress NVARCHAR(500),
    City            NVARCHAR(100),
    State           NVARCHAR(100),
    ZipCode         NVARCHAR(20),
    PaymentMethod   NVARCHAR(50) DEFAULT 'COD',
    Notes           NVARCHAR(500)
);

-- OrderItems Table
CREATE TABLE OrderItems (
    OrderItemId INT IDENTITY(1,1) PRIMARY KEY,
    OrderId     INT NOT NULL REFERENCES Orders(OrderId) ON DELETE CASCADE,
    ProductId   INT NOT NULL REFERENCES Products(ProductId),
    ProductName NVARCHAR(200) NOT NULL,
    Quantity    INT NOT NULL,
    UnitPrice   DECIMAL(10,2) NOT NULL,
    TotalPrice  AS (Quantity * UnitPrice) PERSISTED
);

-- ============================================================
-- Seed Data
-- ============================================================

INSERT INTO Categories (Name, Description, ImageUrl) VALUES
('Electronics',     'Phones, Laptops, Gadgets',   'https://images.unsplash.com/photo-1498049794561-7780e7231661?w=400'),
('Fashion',         'Clothing and Accessories',   'https://images.unsplash.com/photo-1445205170230-053b83016050?w=400'),
('Home & Kitchen',  'Furniture and Appliances',   'https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=400'),
('Books',           'Books and Stationery',        'https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400'),
('Sports',          'Sports and Outdoors',         'https://images.unsplash.com/photo-1571019613454-1cb2f99b2d8b?w=400'),
('Beauty',          'Skincare and Cosmetics',      'https://images.unsplash.com/photo-1596462502278-27bfdc403348?w=400');

INSERT INTO Products (CategoryId, Name, Description, Price, OriginalPrice, StockQty, ImageUrl, Rating, ReviewCount, Brand, IsFeatured) VALUES
(1, 'Wireless Noise-Cancelling Headphones', 'Premium sound quality with active noise cancellation, 30h battery life', 4999, 7999, 150, 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400', 4.5, 2847, 'SoundMax', 1),
(1, 'Smart Watch Pro',                      '1.4" AMOLED display, heart rate monitor, GPS, 7-day battery',           8999, 12999, 80,  'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400', 4.3, 1203, 'TechWear', 1),
(1, '4K Action Camera',                     'Waterproof to 30m, 4K/60fps video, wide-angle 170° lens',               6499, 9999, 60,  'https://images.unsplash.com/photo-1526170375885-4d8ecf77b99f?w=400', 4.4, 987,  'ActionPro',0),
(2, 'Premium Cotton Kurta Set',             'Handcrafted premium cotton, perfect for festivals and daily wear',        1299, 1999, 300, 'https://images.unsplash.com/photo-1583391733956-3750e0ff4e8b?w=400', 4.2, 543,  'EthnicWear',1),
(2, 'Men''s Running Shoes',                  'Lightweight mesh, cushioned sole, ideal for long runs',                  3499, 5499, 200, 'https://images.unsplash.com/photo-1542291026-7eec264c27ff?w=400', 4.6, 3241, 'SpeedFit', 1),
(3, 'Air Purifier 360',                     'HEPA filter, covers 500 sq ft, silent operation, PM2.5 display',         9999, 14999, 40,  'https://images.unsplash.com/photo-1585771724684-38269d6639fd?w=400', 4.1, 421,  'CleanAir', 0),
(3, 'Stainless Steel Cookware Set (5-Piece)','Tri-ply stainless steel, induction compatible, dishwasher safe',         5499, 7999, 90,  'https://images.unsplash.com/photo-1556909114-f6e7ad7d3136?w=400', 4.7, 1872, 'KitchenPro',1),
(4, 'The Complete Web Developer Bootcamp',   'Learn HTML, CSS, JavaScript, React, Node.js from scratch',               499,  999,  999, 'https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=400', 4.8, 5632, 'EduPress', 1);
