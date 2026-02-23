/* =========================================================
   08_cleanup.sql
   Full cleanup:
   - Drop views
   - Drop tables i rätt FK-ordning
   - Drop user/role/login (om finns)
   ========================================================= */

USE EventifyDb;
GO

-- ---------------------------------------------------------
-- 1) Drop views först (för att undvika beroenden)
-- ---------------------------------------------------------
IF OBJECT_ID('dbo.vReportEventSales', 'V') IS NOT NULL
    DROP VIEW dbo.vReportEventSales;
GO

IF OBJECT_ID('dbo.vPublicEvents', 'V') IS NOT NULL
    DROP VIEW dbo.vPublicEvents;
GO

-- ---------------------------------------------------------
-- 2) Drop tabeller i FK-ordning
-- ---------------------------------------------------------
IF OBJECT_ID('dbo.EventTags', 'U') IS NOT NULL DROP TABLE dbo.EventTags;
IF OBJECT_ID('dbo.Tickets',   'U') IS NOT NULL DROP TABLE dbo.Tickets;
IF OBJECT_ID('dbo.Orders',    'U') IS NOT NULL DROP TABLE dbo.Orders;
IF OBJECT_ID('dbo.Events',    'U') IS NOT NULL DROP TABLE dbo.Events;
IF OBJECT_ID('dbo.Tags',      'U') IS NOT NULL DROP TABLE dbo.Tags;
IF OBJECT_ID('dbo.Venues',    'U') IS NOT NULL DROP TABLE dbo.Venues;
IF OBJECT_ID('dbo.Customers', 'U') IS NOT NULL DROP TABLE dbo.Customers;
GO

-- ---------------------------------------------------------
-- 3) Drop security-objekt (valfritt men "full cleanup")
-- ---------------------------------------------------------
-- Ta bort user från databasen
IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'eventify_reader_user')
BEGIN
    DROP USER eventify_reader_user;
END
GO

-- Ta bort roll från databasen
IF EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'eventify_reader_role' AND type = 'R')
BEGIN
    DROP ROLE eventify_reader_role;
END
GO

-- Ta bort login från servern (kräver rättigheter)
IF EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'eventify_reader_login')
BEGIN
    DROP LOGIN eventify_reader_login;
END
GO

PRINT 'Cleanup klart.';
GO
