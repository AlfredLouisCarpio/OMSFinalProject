CREATE TABLE UserAccount (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    Username NVARCHAR(50) NULL,
    Password NVARCHAR(50) NULL,
    Role NVARCHAR(50) NULL
);

CREATE TABLE Orders (
    OrderID INT PRIMARY KEY IDENTITY(1,1),
    CustomerName NVARCHAR(100) NULL,
    ProductID INT NULL,
    Quantity INT NULL,
    OrderStatus NVARCHAR(50) NULL
);

CREATE TABLE Reports (
    ReportID INT PRIMARY KEY IDENTITY(1,1),
    OrderID INT,
    CustomerName VARCHAR(100) NULL,
    ProductName VARCHAR(100) NULL,
    Quantity INT NULL,
    
);

CREATE TABLE Products (
    ProductID INT PRIMARY KEY IDENTITY(1,1),
    ProductName NVARCHAR(100) NULL,
    QuantityInStock INT NULL
);

drop table UserAccount