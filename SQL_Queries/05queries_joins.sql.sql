USE EventifyDb;
GO

/* =========================================================
   05_queries_joins.sql
   8+ SELECT queries som matchar ERD
   ========================================================= */

-- ---------------------------------------------------------
-- Q1) Lista alla events (enkel SELECT + ORDER BY)
-- ---------------------------------------------------------
PRINT 'Q1 - Alla events (sorterade på startdatum)';

SELECT
    e.EventId,
    e.Title,
    e.StartAt,
    e.EndAt,
    e.BasePrice,
    e.Status
FROM dbo.Events e
ORDER BY e.StartAt;
GO

-- ---------------------------------------------------------
-- Q2) Events med venue-info (JOIN + WHERE + ORDER BY)
-- ---------------------------------------------------------
PRINT 'Q2 - PUBLISHED events med venue (JOIN + WHERE + ORDER BY)';

SELECT
    e.EventId,
    e.Title,
    v.Name  AS VenueName,
    v.City,
    v.Capacity,
    e.StartAt,
    e.EndAt,
    e.BasePrice
FROM dbo.Events e
JOIN dbo.Venues v ON v.VenueId = e.VenueId
WHERE e.Status = 'PUBLISHED'
ORDER BY e.StartAt;
GO

-- ---------------------------------------------------------
-- Q3) Event + taggar (M:N via EventTags) (JOIN)
-- ---------------------------------------------------------
PRINT 'Q3 - Event med taggar (M:N)';

SELECT
    e.EventId,
    e.Title,
    t.Name AS TagName
FROM dbo.Events e
JOIN dbo.EventTags et ON et.EventId = e.EventId
JOIN dbo.Tags t ON t.TagId = et.TagId
ORDER BY e.EventId, t.Name;
GO

-- ---------------------------------------------------------
-- Q4) Orders med kundinfo (JOIN)
-- ---------------------------------------------------------
PRINT 'Q4 - Orders med kund (JOIN)';

SELECT
    o.OrderId,
    o.OrderNumber,
    o.OrderStatus,
    o.CreatedAt,
    c.CustomerId,
    c.FirstName,
    c.LastName,
    c.Email
FROM dbo.Orders o
JOIN dbo.Customers c ON c.CustomerId = o.CustomerId
ORDER BY o.CreatedAt DESC;
GO

-- ---------------------------------------------------------
-- Q5) Orderdetaljer: ticket-rader + event (JOIN)
-- ---------------------------------------------------------
PRINT 'Q5 - Orderdetaljer (tickets + event) (JOIN)';

SELECT
    o.OrderNumber,
    o.OrderStatus,
    e.Title AS EventTitle,
    t.TicketType,
    t.Quantity,
    t.UnitPrice,
    (t.Quantity * t.UnitPrice) AS RowTotal
FROM dbo.Orders o
JOIN dbo.Tickets t ON t.OrderId = o.OrderId
JOIN dbo.Events e ON e.EventId = t.EventId
ORDER BY o.OrderNumber, e.Title, t.TicketType;
GO

-- ---------------------------------------------------------
-- Q6) Summering per event: sålda biljetter + intäkter (GROUP BY + aggregat)
--     OBS: Vi räknar bara PAID orders som "riktig försäljning".
-- ---------------------------------------------------------
PRINT 'Q6 - Försäljning per event (PAID) (GROUP BY + SUM)';

SELECT
    e.EventId,
    e.Title,
    SUM(t.Quantity) AS TicketsSold,
    SUM(t.Quantity * t.UnitPrice) AS Revenue
FROM dbo.Events e
JOIN dbo.Tickets t ON t.EventId = e.EventId
JOIN dbo.Orders o ON o.OrderId = t.OrderId
WHERE o.OrderStatus = 'PAID'
GROUP BY e.EventId, e.Title
ORDER BY Revenue DESC;
GO

-- ---------------------------------------------------------
-- Q7) Rapport: Top customers (flest köp / högst spend) (GROUP BY + aggregat)
-- ---------------------------------------------------------
PRINT 'Q7 - Top customers (PAID orders)';

SELECT
    c.CustomerId,
    c.FirstName,
    c.LastName,
    c.Email,
    COUNT(DISTINCT o.OrderId) AS PaidOrders,
    SUM(t.Quantity) AS TicketsBought,
    SUM(t.Quantity * t.UnitPrice) AS TotalSpend
FROM dbo.Customers c
JOIN dbo.Orders o ON o.CustomerId = c.CustomerId
JOIN dbo.Tickets t ON t.OrderId = o.OrderId
WHERE o.OrderStatus = 'PAID'
GROUP BY c.CustomerId, c.FirstName, c.LastName, c.Email
ORDER BY TotalSpend DESC, PaidOrders DESC;
GO

-- ---------------------------------------------------------
-- Q8) "At risk"-lista: orders som inte är PAID (WHERE + ORDER BY)
-- ---------------------------------------------------------
PRINT 'Q8 - At risk: Orders som inte är PAID';

SELECT
    o.OrderId,
    o.OrderNumber,
    o.OrderStatus,
    o.CreatedAt,
    c.FirstName,
    c.LastName,
    c.Email
FROM dbo.Orders o
JOIN dbo.Customers c ON c.CustomerId = o.CustomerId
WHERE o.OrderStatus IN ('CREATED', 'CANCELLED')
ORDER BY o.CreatedAt DESC;
GO

-- ---------------------------------------------------------
-- Q9) Extra: Kapacitetskontroll (GROUP BY + HAVING)
--     Beräknar sålda biljetter per event och flaggar om det överskrider venue-capacity.
--     (Bra som "integritets-/biz-check".)
-- ---------------------------------------------------------
PRINT 'Q9 - Kapacitetscheck (HAVING)';

SELECT
    e.EventId,
    e.Title,
    v.Name AS VenueName,
    v.Capacity,
    SUM(CASE WHEN o.OrderStatus = 'PAID' THEN t.Quantity ELSE 0 END) AS PaidTicketsSold
FROM dbo.Events e
JOIN dbo.Venues v ON v.VenueId = e.VenueId
LEFT JOIN dbo.Tickets t ON t.EventId = e.EventId
LEFT JOIN dbo.Orders o ON o.OrderId = t.OrderId
GROUP BY e.EventId, e.Title, v.Name, v.Capacity
HAVING SUM(CASE WHEN o.OrderStatus = 'PAID' THEN t.Quantity ELSE 0 END) > v.Capacity
ORDER BY PaidTicketsSold DESC;
GO
