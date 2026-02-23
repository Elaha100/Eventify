USE EventifyDb;
GO

/* =========================================================
   04_crud_examples.sql
   CRUD-exempel som matchar ERD:
   Customers, Events, Orders, Tickets (+ lite EventTags)
   ========================================================= */

-- =========================================================
-- 1) CUSTOMERS - CRUD
-- =========================================================
PRINT '--- CUSTOMERS: INSERT ---';

INSERT INTO dbo.Customers (FirstName, LastName, Email, Phone)
VALUES ('Test', 'Customer', 'test.customer+crud@example.com', '070-1234567');

PRINT '--- CUSTOMERS: SELECT ---';
SELECT TOP 5 *
FROM dbo.Customers
ORDER BY CustomerId DESC;

PRINT '--- CUSTOMERS: UPDATE ---';
UPDATE dbo.Customers
SET Phone = '070-7654321'
WHERE Email = 'test.customer+crud@example.com';

SELECT *
FROM dbo.Customers
WHERE Email = 'test.customer+crud@example.com';

PRINT '--- CUSTOMERS: DELETE ---';
DELETE FROM dbo.Customers
WHERE Email = 'test.customer+crud@example.com';

-- Visa att den är borta
SELECT *
FROM dbo.Customers
WHERE Email = 'test.customer+crud@example.com';
GO


-- =========================================================
-- 2) EVENTS - CRUD (kräver VenueId)
-- =========================================================
PRINT '--- EVENTS: INSERT ---';

DECLARE @VenueId INT = (SELECT TOP 1 VenueId FROM dbo.Venues ORDER BY VenueId);

IF @VenueId IS NULL
BEGIN
    THROW 50001, 'Det finns inga Venues. Kör 03_seed_data.sql först.', 1;
END

INSERT INTO dbo.Events (VenueId, Title, Description, StartAt, EndAt, BasePrice, Status)
VALUES
(
    @VenueId,
    'CRUD Test Event',
    'Event skapat för CRUD-exempel.',
    DATEADD(DAY, 60, SYSUTCDATETIME()),
    DATEADD(HOUR, 3, DATEADD(DAY, 60, SYSUTCDATETIME())),
    250.00,
    'DRAFT'
);

DECLARE @EventId INT = (SELECT TOP 1 EventId FROM dbo.Events WHERE Title = 'CRUD Test Event' ORDER BY EventId DESC);

PRINT '--- EVENTS: SELECT ---';
SELECT *
FROM dbo.Events
WHERE EventId = @EventId;

PRINT '--- EVENTS: UPDATE (Status + pris) ---';
UPDATE dbo.Events
SET Status = 'PUBLISHED',
    BasePrice = 299.00
WHERE EventId = @EventId;

SELECT *
FROM dbo.Events
WHERE EventId = @EventId;

PRINT '--- EVENTS: DELETE ---';
-- OBS: kan inte radera om det finns Tickets/EventTags kopplade,
-- därför kör vi detta event utan kopplingar här.
DELETE FROM dbo.Events
WHERE EventId = @EventId;

-- Visa att den är borta
SELECT *
FROM dbo.Events
WHERE EventId = @EventId;
GO


-- =========================================================
-- 3) ORDERS + TICKETS - CRUD (transaktion/exempel)
--    Skapar: Customer, Order, Ticket-rader
-- =========================================================
PRINT '--- ORDERS + TICKETS: CREATE (INSERT) ---';

BEGIN TRAN;

BEGIN TRY
    -- Skapa en kund för order-exemplet
    INSERT INTO dbo.Customers (FirstName, LastName, Email, Phone)
    VALUES ('Order', 'Owner', 'order.owner+crud@example.com', '070-0000000');

    DECLARE @CustomerId INT = (SELECT CustomerId FROM dbo.Customers WHERE Email = 'order.owner+crud@example.com');

    -- Välj ett befintligt event (måste finnas)
    DECLARE @AnyEventId INT = (SELECT TOP 1 EventId FROM dbo.Events WHERE Status = 'PUBLISHED' ORDER BY EventId);

    IF @AnyEventId IS NULL
    BEGIN
        THROW 50002, 'Det finns inga PUBLISHED Events. Kör 03_seed_data.sql först.', 1;
    END

    -- Skapa order
    DECLARE @OrderNumber NVARCHAR(30) = CONCAT('CRUD-', FORMAT(SYSUTCDATETIME(), 'yyyyMMddHHmmss'));
    INSERT INTO dbo.Orders (CustomerId, OrderNumber, OrderStatus)
    VALUES (@CustomerId, @OrderNumber, 'CREATED');

    DECLARE @OrderId INT = (SELECT OrderId FROM dbo.Orders WHERE OrderNumber = @OrderNumber);

    -- Skapa tickets (TicketType + Quantity + UnitPrice)
    INSERT INTO dbo.Tickets (OrderId, EventId, TicketType, Quantity, UnitPrice)
    VALUES
    (@OrderId, @AnyEventId, 'STANDARD', 2, 299.00),
    (@OrderId, @AnyEventId, 'STUDENT',  1, 249.00);

    COMMIT;
END TRY
BEGIN CATCH
    ROLLBACK;
    THROW;
END CATCH
GO


-- =========================================================
-- 4) ORDERS + TICKETS - READ (SELECT)
-- =========================================================
PRINT '--- ORDERS + TICKETS: READ (SELECT) ---';

SELECT TOP 1
    o.OrderId,
    o.OrderNumber,
    o.OrderStatus,
    o.CreatedAt,
    c.FirstName,
    c.LastName,
    c.Email
FROM dbo.Orders o
JOIN dbo.Customers c ON c.CustomerId = o.CustomerId
WHERE o.OrderNumber LIKE 'CRUD-%'
ORDER BY o.OrderId DESC;

-- Visa ticket-rader för senaste CRUD-order
DECLARE @LatestCrudOrderId INT =
(
    SELECT TOP 1 OrderId
    FROM dbo.Orders
    WHERE OrderNumber LIKE 'CRUD-%'
    ORDER BY OrderId DESC
);

SELECT
    t.TicketId,
    t.OrderId,
    t.EventId,
    e.Title AS EventTitle,
    t.TicketType,
    t.Quantity,
    t.UnitPrice,
    t.CreatedAt
FROM dbo.Tickets t
JOIN dbo.Events e ON e.EventId = t.EventId
WHERE t.OrderId = @LatestCrudOrderId
ORDER BY t.TicketId;
GO


-- =========================================================
-- 5) ORDERS + TICKETS - UPDATE
--    - uppdatera OrderStatus
--    - uppdatera Quantity på en ticket-rad
-- =========================================================
PRINT '--- ORDERS + TICKETS: UPDATE ---';

DECLARE @OrderIdToUpdate INT =
(
    SELECT TOP 1 OrderId
    FROM dbo.Orders
    WHERE OrderNumber LIKE 'CRUD-%'
    ORDER BY OrderId DESC
);

-- Uppdatera orderstatus
UPDATE dbo.Orders
SET OrderStatus = 'PAID'
WHERE OrderId = @OrderIdToUpdate;

-- Uppdatera quantity på en ticket-rad (första ticket på ordern)
DECLARE @TicketIdToUpdate INT =
(
    SELECT TOP 1 TicketId
    FROM dbo.Tickets
    WHERE OrderId = @OrderIdToUpdate
    ORDER BY TicketId
);

UPDATE dbo.Tickets
SET Quantity = Quantity + 1
WHERE TicketId = @TicketIdToUpdate;

-- Visa resultat
SELECT o.OrderId, o.OrderNumber, o.OrderStatus
FROM dbo.Orders o
WHERE o.OrderId = @OrderIdToUpdate;

SELECT t.TicketId, t.TicketType, t.Quantity, t.UnitPrice
FROM dbo.Tickets t
WHERE t.TicketId = @TicketIdToUpdate;
GO


-- =========================================================
-- 6) ORDERS + TICKETS - DELETE
--    OBS: pga FK måste vi radera Tickets först, sedan Orders,
--    och till sist vår "order.owner" kund.
-- =========================================================
PRINT '--- ORDERS + TICKETS: DELETE ---';

BEGIN TRAN;

BEGIN TRY
    DECLARE @OrderIdToDelete INT =
    (
        SELECT TOP 1 OrderId
        FROM dbo.Orders
        WHERE OrderNumber LIKE 'CRUD-%'
        ORDER BY OrderId DESC
    );

    DECLARE @CustomerIdToDelete INT =
    (
        SELECT CustomerId
        FROM dbo.Orders
        WHERE OrderId = @OrderIdToDelete
    );

    -- 1) Radera Tickets först
    DELETE FROM dbo.Tickets
    WHERE OrderId = @OrderIdToDelete;

    -- 2) Radera Order
    DELETE FROM dbo.Orders
    WHERE OrderId = @OrderIdToDelete;

    -- 3) Radera Customer
    DELETE FROM dbo.Customers
    WHERE CustomerId = @CustomerIdToDelete;

    COMMIT;
END TRY
BEGIN CATCH
    ROLLBACK;
    THROW;
END CATCH
GO


-- =========================================================
-- 7) (Valfritt) EventTags - enkelt exempel
-- =========================================================
PRINT '--- EVENTTAGS: INSERT + DELETE (valfritt) ---';

DECLARE @SomeEventId INT = (SELECT TOP 1 EventId FROM dbo.Events ORDER BY EventId);
DECLARE @SomeTagId   INT = (SELECT TOP 1 TagId   FROM dbo.Tags   ORDER BY TagId);

IF @SomeEventId IS NOT NULL AND @SomeTagId IS NOT NULL
BEGIN
    -- Insert endast om den inte redan finns
    IF NOT EXISTS (SELECT 1 FROM dbo.EventTags WHERE EventId = @SomeEventId AND TagId = @SomeTagId)
    BEGIN
        INSERT INTO dbo.EventTags (EventId, TagId) VALUES (@SomeEventId, @SomeTagId);
    END

    SELECT * FROM dbo.EventTags WHERE EventId = @SomeEventId AND TagId = @SomeTagId;

    -- Delete igen
    DELETE FROM dbo.EventTags WHERE EventId = @SomeEventId AND TagId = @SomeTagId;
END
GO
