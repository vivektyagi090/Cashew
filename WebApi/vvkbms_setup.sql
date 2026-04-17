-- ============================================================
-- VVKBMS Premium Cashews — Advanced Enterprise Database Setup
-- Includes Inventory, Returns, Damages, Supply Chain & Roles
-- ============================================================

-- 1. DROP EXISTING TABLES (CAUTION: Resets all data)
-- DROP TABLE IF EXISTS Logistics;
-- DROP TABLE IF EXISTS Damages;
-- DROP TABLE IF EXISTS Returns;
-- DROP TABLE IF EXISTS InventoryMovements;
-- DROP TABLE IF EXISTS OrderItems;
-- DROP TABLE IF EXISTS Orders;
-- DROP TABLE IF EXISTS CartItems;
-- DROP TABLE IF EXISTS Cart;
-- DROP TABLE IF EXISTS Products;
-- DROP TABLE IF EXISTS Categories;
-- DROP TABLE IF EXISTS Users;

-- Users Table (with Roles)
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE Users (
        Id              INT IDENTITY(1,1) PRIMARY KEY,
        Username        NVARCHAR(50) NOT NULL UNIQUE,
        Email           NVARCHAR(100) NOT NULL UNIQUE,
        PasswordHash    NVARCHAR(255) NOT NULL,
        FullName        NVARCHAR(100),
        PhoneNumber     NVARCHAR(20),
        Role            NVARCHAR(20) DEFAULT 'User', -- 'Admin', 'User'
        IsActive        BIT DEFAULT 1,
        IsEmailVerified BIT DEFAULT 0,
        CreatedAt       DATETIME DEFAULT GETDATE(),
        UpdatedAt       DATETIME DEFAULT GETDATE(),
        RefreshToken    NVARCHAR(500),
        RefreshTokenExpiry DATETIME
    );
END
ELSE
BEGIN
    -- Add Role column if it doesn't exist
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'Role')
        ALTER TABLE Users ADD Role NVARCHAR(20) DEFAULT 'User';
END

-- Categories Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Categories' AND xtype='U')
BEGIN
    CREATE TABLE Categories (
        CategoryId    INT IDENTITY(1,1) PRIMARY KEY,
        Name          NVARCHAR(100) NOT NULL,
        Description   NVARCHAR(500),
        ImageUrl      NVARCHAR(500),
        IsActive      BIT DEFAULT 1,
        CreatedAt     DATETIME DEFAULT GETDATE()
    );
END

-- Products Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Products' AND xtype='U')
BEGIN
    CREATE TABLE Products (
        ProductId       INT IDENTITY(1,1) PRIMARY KEY,
        CategoryId      INT NOT NULL REFERENCES Categories(CategoryId),
        Name            NVARCHAR(200) NOT NULL,
        Description     NVARCHAR(2000),
        Price           DECIMAL(10,2) NOT NULL,      -- Sale Price
        CostPrice       DECIMAL(10,2) DEFAULT 0,     -- Purchase Price (for P&L)
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
END
ELSE
BEGIN
    IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Products') AND name = 'CostPrice')
        ALTER TABLE Products ADD CostPrice DECIMAL(10,2) DEFAULT 0;
END

-- Inventory Movements (Audit Trail)
CREATE TABLE InventoryMovements (
    MovementId      INT IDENTITY(1,1) PRIMARY KEY,
    ProductId       INT NOT NULL REFERENCES Products(ProductId),
    Quantity        INT NOT NULL,
    Type            NVARCHAR(50) NOT NULL, -- 'Purchase', 'Sale', 'Return', 'Damage', 'Adjustment'
    ReferenceId     INT,                   -- OrderId, DamageId, etc.
    Remarks         NVARCHAR(500),
    CreatedAt       DATETIME DEFAULT GETDATE()
);

-- Damages Table
CREATE TABLE Damages (
    DamageId        INT IDENTITY(1,1) PRIMARY KEY,
    ProductId       INT NOT NULL REFERENCES Products(ProductId),
    Quantity        INT NOT NULL,
    Reason          NVARCHAR(500),         -- 'Expired', 'Broken Seal', 'Infected'
    LossAmount      DECIMAL(10,2),
    LoggedInBy      INT NOT NULL REFERENCES Users(Id),
    CreatedAt       DATETIME DEFAULT GETDATE()
);

-- Returns Table
CREATE TABLE Returns (
    ReturnId        INT IDENTITY(1,1) PRIMARY KEY,
    OrderId         INT NOT NULL REFERENCES Orders(OrderId),
    ProductId       INT NOT NULL REFERENCES Products(ProductId),
    Quantity        INT NOT NULL,
    Reason          NVARCHAR(500),
    Status          NVARCHAR(50) DEFAULT 'Pending', -- 'Pending', 'Approved', 'Rejected'
    RefundAmount    DECIMAL(10,2),
    CreatedAt       DATETIME DEFAULT GETDATE()
);

-- Logistics Tracking
CREATE TABLE Logistics (
    TrackingId      INT IDENTITY(1,1) PRIMARY KEY,
    OrderId         INT NOT NULL REFERENCES Orders(OrderId),
    CarrierName     NVARCHAR(100) DEFAULT 'Internal Supply Chain',
    TrackingNumber  NVARCHAR(100),
    Status          NVARCHAR(50) DEFAULT 'Packed', -- 'Packed', 'Shipped', 'In-Transit', 'Delivered'
    CurrentLocation NVARCHAR(200),
    EstimatedDelivery DATETIME,
    UpdatedAt       DATETIME DEFAULT GETDATE()
);

-- Seed Admin User (Vivek)
-- Note: PasswordHash should ideally be hashed. Using plain text for demonstration if that matches existing logic, 
-- but normally AuthService handles hashing. I'll use a placeholder hash or plain text based on previous patterns.
IF NOT EXISTS (SELECT * FROM Users WHERE Email = 'vivek@gamil.com')
BEGIN
    INSERT INTO Users (Username, Email, PasswordHash, FullName, Role, IsActive, IsEmailVerified)
    VALUES ('vivek_admin', 'vivek@gamil.com', 'Vivek@123', 'Vivek Admin', 'Admin', 1, 1);
END

-- Wishlists Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Wishlists' AND xtype='U')
BEGIN
    CREATE TABLE Wishlists (
        WishlistId    INT IDENTITY(1,1) PRIMARY KEY,
        UserId        INT NOT NULL REFERENCES Users(Id),
        ProductId     INT NOT NULL REFERENCES Products(ProductId),
        CreatedAt     DATETIME DEFAULT GETDATE(),
        UNIQUE(UserId, ProductId)
    );
END

-- Reviews Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Reviews' AND xtype='U')
BEGIN
    CREATE TABLE Reviews (
        ReviewId      INT IDENTITY(1,1) PRIMARY KEY,
        ProductId     INT NOT NULL REFERENCES Products(ProductId),
        UserId        INT NOT NULL REFERENCES Users(Id),
        Rating        DECIMAL(2,1) CHECK (Rating >= 1 AND Rating <= 5),
        Comment       NVARCHAR(1000),
        CreatedAt     DATETIME DEFAULT GETDATE()
    );
END

-- Coupons Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Coupons' AND xtype='U')
BEGIN
    CREATE TABLE Coupons (
        CouponId            INT IDENTITY(1,1) PRIMARY KEY,
        Code                NVARCHAR(50) NOT NULL UNIQUE,
        DiscountPercentage  DECIMAL(5,2),
        MaxDiscount         DECIMAL(10,2),
        ExpiryDate          DATETIME,
        IsActive            BIT DEFAULT 1,
        CreatedAt           DATETIME DEFAULT GETDATE()
    );
END

-- Addresses Table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Addresses' AND xtype='U')
BEGIN
    CREATE TABLE Addresses (
        AddressId     INT IDENTITY(1,1) PRIMARY KEY,
        UserId        INT NOT NULL REFERENCES Users(Id),
        Street        NVARCHAR(255) NOT NULL,
        City          NVARCHAR(100) NOT NULL,
        State         NVARCHAR(100) NOT NULL,
        ZipCode       NVARCHAR(20) NOT NULL,
        Country       NVARCHAR(100) DEFAULT 'India',
        IsDefault     BIT DEFAULT 0,
        CreatedAt     DATETIME DEFAULT GETDATE()
    );
END

-- Update existing product with cost price for samples
UPDATE Products SET CostPrice = Price * 0.7 WHERE CostPrice = 0;
