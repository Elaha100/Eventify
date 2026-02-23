/* =========================================================
   07_security.sql
   Skapar roll + användare och ger SELECT på views.
   Rollen ska INTE ha direkt SELECT på tabellerna.
   ========================================================= */

USE EventifyDb;
GO

-- ---------------------------------------------------------
-- 1) Skapa (valfritt) server-login
--    Kör endast om ni har rättigheter på servern.
-- ---------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = 'eventify_reader_login')
BEGIN
    PRINT 'Skapar login: eventify_reader_login';
    -- Byt lösenord innan inlämning om ni vill
    CREATE LOGIN eventify_reader_login
    WITH PASSWORD = 'ChangeMe!12345',
         CHECK_POLICY = OFF,
         CHECK_EXPIRATION = OFF;
END
ELSE
BEGIN
    PRINT 'Login finns redan: eventify_reader_login';
END
GO

-- ---------------------------------------------------------
-- 2) Skapa databas-user kopplad till login
-- ---------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'eventify_reader_user')
BEGIN
    PRINT 'Skapar user: eventify_reader_user';
    CREATE USER eventify_reader_user FOR LOGIN eventify_reader_login;
END
ELSE
BEGIN
    PRINT 'User finns redan: eventify_reader_user';
END
GO

-- ---------------------------------------------------------
-- 3) Skapa roll
-- ---------------------------------------------------------
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = 'eventify_reader_role' AND type = 'R')
BEGIN
    PRINT 'Skapar roll: eventify_reader_role';
    CREATE ROLE eventify_reader_role;
END
ELSE
BEGIN
    PRINT 'Roll finns redan: eventify_reader_role';
END
GO

-- ---------------------------------------------------------
-- 4) Lägg user i rollen
-- ---------------------------------------------------------
IF NOT EXISTS
(
    SELECT 1
    FROM sys.database_role_members rm
    JOIN sys.database_principals r ON r.principal_id = rm.role_principal_id
    JOIN sys.database_principals u ON u.principal_id = rm.member_principal_id
    WHERE r.name = 'eventify_reader_role'
      AND u.name = 'eventify_reader_user'
)
BEGIN
    PRINT 'Lägger eventify_reader_user i eventify_reader_role';
    ALTER ROLE eventify_reader_role ADD MEMBER eventify_reader_user;
END
ELSE
BEGIN
    PRINT 'User är redan medlem i rollen';
END
GO

-- ---------------------------------------------------------
-- 5) Säkerställ att rollen INTE har SELECT på tabellerna
--    (om den råkat få det tidigare)
-- ---------------------------------------------------------
DENY SELECT ON dbo.Customers TO eventify_reader_role;
DENY SELECT ON dbo.Venues    TO eventify_reader_role;
DENY SELECT ON dbo.Tags      TO eventify_reader_role;
DENY SELECT ON dbo.Events    TO eventify_reader_role;
DENY SELECT ON dbo.Orders    TO eventify_reader_role;
DENY SELECT ON dbo.Tickets   TO eventify_reader_role;
DENY SELECT ON dbo.EventTags TO eventify_reader_role;
GO

-- ---------------------------------------------------------
-- 6) Ge SELECT på views (rollen får läsa dessa)
-- ---------------------------------------------------------
GRANT SELECT ON dbo.vPublicEvents      TO eventify_reader_role;
GRANT SELECT ON dbo.vReportEventSales  TO eventify_reader_role;
GO

-- ---------------------------------------------------------
-- 7) (Valfritt) Snabbtest: visa rättigheter (metadata)
-- ---------------------------------------------------------
PRINT 'Klart: eventify_reader_role har SELECT på views men DENY på tabeller.';
GO
