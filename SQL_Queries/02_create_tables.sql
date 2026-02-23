USE EventifyDb;
GO

/* =========================================================
   02_create_tables.sql
   Matchar ERD: Customers, Venues, Tags, Events, Orders,
   Tickets, EventTags
   Innehåller PK, FK, DEFAULT (CreatedAt) och CHECK constraints
   ========================================================= */

-- För omkörning under utveckling (valfritt men praktiskt)
-- OBS: I er 08_cleanup.sql ska ni ha drop i rätt ordning också.
IF OBJECT_ID('dbo.EventTags', 'U') IS NOT NULL DROP TABLE dbo.EventTags;
IF OBJECT_ID('dbo.Tickets',   'U') IS NOT NULL DROP TABLE dbo.Tickets;
IF OBJECT_ID('dbo.Orders',    'U') IS NOT NULL DROP TABLE dbo.Orders;
IF OBJECT_ID('dbo.Events',    'U') IS NOT NULL DROP TABLE dbo.Events;
IF OBJECT_ID('dbo.Tags',      'U') IS NOT NULL DROP TABLE dbo.Tags;
IF OBJECT_ID('dbo.Venues',    'U') IS NOT NULL DROP TABLE dbo.Venues;
IF OBJECT_ID('dbo.Customers', 'U') IS NOT NULL DROP TABLE dbo.Customers;
GO

/* =======================
   Customers
   ======================= */
CREATE TABLE dbo.Customers
(
    CustomerId  INT IDENTITY(1,1) NOT NULL,
    FirstName   NVARCHAR(50)  NOT NULL,
    LastName    NVARCHAR(50)  NOT NULL,
    Email       NVARCHAR(120) NOT NULL,
    Phone       NVARCHAR(30)  NULL,
    CreatedAt   DATETIME2     NOT NULL CONSTRAINT DF_Customers_CreatedAt DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT PK_Customers PRIMARY KEY (CustomerId),
    CONSTRAINT UQ_Customers_Email UNIQUE (Email),
    CONSTRAINT CK_Customers_Email_NotEmpty CHECK (LEN(LTRIM(RTRIM(Email))) > 3)
);
GO

/* =======================
   Venues
   ======================= */
CREATE TABLE dbo.Venues
(
    VenueId   INT IDENTITY(1,1) NOT NULL,
    Name      NVARCHAR(120) NOT NULL,
    Address   NVARCHAR(200) NOT NULL,
    City      NVARCHAR(80)  NOT NULL,
    Capacity  INT           NOT NULL,
    CreatedAt DATETIME2     NOT NULL CONSTRAINT DF_Venues_CreatedAt DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT PK_Venues PRIMARY KEY (VenueId),
    CONSTRAINT CK_Venues_Capacity CHECK (Capacity > 0)
);
GO

/* =======================
   Tags
   ======================= */
CREATE TABLE dbo.Tags
(
    TagId     INT IDENTITY(1,1) NOT NULL,
    Name      NVARCHAR(50) NOT NULL,
    CreatedAt DATETIME2    NOT NULL CONSTRAINT DF_Tags_CreatedAt DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT PK_Tags PRIMARY KEY (TagId),
    CONSTRAINT UQ_Tags_Name UNIQUE (Name),
    CONSTRAINT CK_Tags_Name_NotEmpty CHECK (LEN(LTRIM(RTRIM(Name))) > 0)
);
GO

/* =======================
   Events
   ======================= */
CREATE TABLE dbo.Events
(
    EventId      INT IDENTITY(1,1) NOT NULL,
    VenueId      INT           NOT NULL,
    Title        NVARCHAR(150) NOT NULL,
    Description  NVARCHAR(500) NOT NULL,
    StartAt      DATETIME2     NOT NULL,
    EndAt        DATETIME2     NOT NULL,
    BasePrice    DECIMAL(10,2) NOT NULL,
    Status       NVARCHAR(20)  NOT NULL,
    CreatedAt    DATETIME2     NOT NULL CONSTRAINT DF_Events_CreatedAt DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT PK_Events PRIMARY KEY (EventId),
    CONSTRAINT FK_Events_Venues FOREIGN KEY (VenueId)
        REFERENCES dbo.Venues (VenueId),

    CONSTRAINT CK_Events_Time CHECK (EndAt > StartAt),
    CONSTRAINT CK_Events_BasePrice CHECK (BasePrice >= 0),
    CONSTRAINT CK_Events_Status CHECK (Status IN ('DRAFT','PUBLISHED','CANCELLED'))
);
GO

/* =======================
   Orders
   ======================= */
CREATE TABLE dbo.Orders
(
    OrderId       INT IDENTITY(1,1) NOT NULL,
    CustomerId    INT          NOT NULL,
    OrderNumber   NVARCHAR(30) NOT NULL,
    OrderStatus   NVARCHAR(20) NOT NULL,
    CreatedAt     DATETIME2    NOT NULL CONSTRAINT DF_Orders_CreatedAt DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT PK_Orders PRIMARY KEY (OrderId),
    CONSTRAINT UQ_Orders_OrderNumber UNIQUE (OrderNumber),

    CONSTRAINT FK_Orders_Customers FOREIGN KEY (CustomerId)
        REFERENCES dbo.Customers (CustomerId),

    CONSTRAINT CK_Orders_Status CHECK (OrderStatus IN ('CREATED','PAID','CANCELLED'))
);
GO

/* =======================
   Tickets
   ======================= */
CREATE TABLE dbo.Tickets
(
    TicketId    INT IDENTITY(1,1) NOT NULL,
    OrderId     INT          NOT NULL,
    EventId     INT          NOT NULL,
    TicketType  NVARCHAR(20) NOT NULL,
    Quantity    INT          NOT NULL,
    UnitPrice   DECIMAL(10,2) NOT NULL,
    CreatedAt   DATETIME2    NOT NULL CONSTRAINT DF_Tickets_CreatedAt DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT PK_Tickets PRIMARY KEY (TicketId),

    CONSTRAINT FK_Tickets_Orders FOREIGN KEY (OrderId)
        REFERENCES dbo.Orders (OrderId),

    CONSTRAINT FK_Tickets_Events FOREIGN KEY (EventId)
        REFERENCES dbo.Events (EventId),

    CONSTRAINT CK_Tickets_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_Tickets_UnitPrice CHECK (UnitPrice >= 0),
    CONSTRAINT CK_Tickets_TicketType CHECK (TicketType IN ('STANDARD','VIP','CHILD','STUDENT'))
);
GO

/* =======================
   EventTags (M:N)
   ======================= */
CREATE TABLE dbo.EventTags
(
    EventId    INT       NOT NULL,
    TagId      INT       NOT NULL,
    CreatedAt  DATETIME2 NOT NULL CONSTRAINT DF_EventTags_CreatedAt DEFAULT (SYSUTCDATETIME()),

    CONSTRAINT PK_EventTags PRIMARY KEY (EventId, TagId),

    CONSTRAINT FK_EventTags_Events FOREIGN KEY (EventId)
        REFERENCES dbo.Events (EventId),

    CONSTRAINT FK_EventTags_Tags FOREIGN KEY (TagId)
        REFERENCES dbo.Tags (TagId)
);
GO
