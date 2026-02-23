USE EventifyDb;
GO

/* =========================================================
   06_views.sql
   Minst 2 views:
   - 1 "public view" (säkra kolumner)
   - 1 "report view" för Console App
   ========================================================= */

-- ---------------------------------------------------------
-- View 1: Public view (endast public/säkra kolumner)
-- ---------------------------------------------------------
IF OBJECT_ID('dbo.vPublicEvents', 'V') IS NOT NULL
    DROP VIEW dbo.vPublicEvents;
GO

CREATE VIEW dbo.vPublicEvents
AS
SELECT
    e.EventId,
    e.Title,
    e.StartAt,
    e.EndAt,
    e.BasePrice,
    e.Status,
    v.Name AS VenueName,
    v.City,
    v.Capacity
FROM dbo.Events e
JOIN dbo.Venues v ON v.VenueId = e.VenueId;
GO


-- ---------------------------------------------------------
-- View 2: Report view (Console App kan läsa direkt)
--  - Summerar PAID försäljning per event
-- ---------------------------------------------------------
IF OBJECT_ID('dbo.vReportEventSales', 'V') IS NOT NULL
    DROP VIEW dbo.vReportEventSales;
GO

CREATE VIEW dbo.vReportEventSales
AS
SELECT
    e.EventId,
    e.Title,
    e.StartAt,
    v.Name AS VenueName,
    v.City,
    SUM(t.Quantity) AS TicketsSold,
    SUM(t.Quantity * t.UnitPrice) AS Revenue
FROM dbo.Events e
JOIN dbo.Venues v ON v.VenueId = e.VenueId
LEFT JOIN dbo.Tickets t ON t.EventId = e.EventId
LEFT JOIN dbo.Orders o ON o.OrderId = t.OrderId
WHERE o.OrderStatus = 'PAID'
GROUP BY
    e.EventId,
    e.Title,
    e.StartAt,
    v.Name,
    v.City;
GO


-- ---------------------------------------------------------
-- Snabbtest (valfritt): se att views fungerar
-- ---------------------------------------------------------
SELECT TOP 10 * FROM dbo.vPublicEvents ORDER BY StartAt;
SELECT TOP 10 * FROM dbo.vReportEventSales ORDER BY Revenue DESC;
GO
