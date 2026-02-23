USE EventifyDb;
GO

/* =========================================================
   03_seed_data.sql
   Seedar testdata som matchar ERD:
   Customers, Venues, Tags, Events, Orders, Tickets, EventTags
   ========================================================= */

-- =========================
-- Venues (minst några)
-- =========================
INSERT INTO dbo.Venues (Name, Address, City, Capacity)
VALUES
('Göteborg Arena',        'Avenyn 1',          'Göteborg', 12000),
('Stockholm Live',        'Centralvägen 10',   'Stockholm', 18000),
('Malmö Kulturhus',       'Storgatan 5',       'Malmö',     2500),
('Uppsala Konferens',     'Campusvägen 3',     'Uppsala',   1500),
('Linköping Eventhall',   'Industrigatan 9',   'Linköping', 6000);
GO

-- =========================
-- Tags
-- =========================
INSERT INTO dbo.Tags (Name)
VALUES
('Music'),
('Comedy'),
('Tech'),
('Sports'),
('Family'),
('Theatre'),
('Food'),
('Workshop');
GO

-- =========================
-- Customers (minst 10)
-- =========================
INSERT INTO dbo.Customers (FirstName, LastName, Email, Phone)
VALUES
('Anna',   'Svensson',  'anna.svensson@example.com',   '070-1111111'),
('Erik',   'Johansson', 'erik.johansson@example.com',  '070-2222222'),
('Sara',   'Lind',      'sara.lind@example.com',       '070-3333333'),
('Johan',  'Berg',      'johan.berg@example.com',      '070-4444444'),
('Emma',   'Nilsson',   'emma.nilsson@example.com',    '070-5555555'),
('Oskar',  'Karlsson',  'oskar.karlsson@example.com',  '070-6666666'),
('Maja',   'Andersson', 'maja.andersson@example.com',  '070-7777777'),
('Filip',  'Hansen',    'filip.hansen@example.com',    '070-8888888'),
('Lina',   'Persson',   'lina.persson@example.com',    '070-9999999'),
('David',  'Larsson',   'david.larsson@example.com',   '070-1010101');
GO

-- =========================
-- Events (minst 6)
-- Status: 'DRAFT','PUBLISHED','CANCELLED'
-- =========================
INSERT INTO dbo.Events (VenueId, Title, Description, StartAt, EndAt, BasePrice, Status)
VALUES
(1, 'Rock Night Göteborg', 'En helkväll med lokala rockband och gästartister.', '2026-03-12T19:00:00', '2026-03-12T23:00:00', 399.00, 'PUBLISHED'),
(2, 'Standup Special Stockholm', 'Komikväll med 4 etablerade komiker.',         '2026-03-20T20:00:00', '2026-03-20T22:00:00', 299.00, 'PUBLISHED'),
(3, 'Familjeföreställning Malmö', 'Teater för hela familjen.',                   '2026-04-05T14:00:00', '2026-04-05T15:30:00', 199.00, 'PUBLISHED'),
(4, '.NET Workshop Uppsala', 'Hands-on workshop med fokus på databaser & EF.',   '2026-04-18T09:00:00', '2026-04-18T16:00:00', 499.00, 'PUBLISHED'),
(5, 'Food Festival Linköping', 'Mat, dryck och lokala producenter.',             '2026-05-02T11:00:00', '2026-05-02T18:00:00', 149.00, 'PUBLISHED'),
(2, 'E-sport Final Stockholm', 'Finalmatch + fan zone.',                         '2026-05-15T18:00:00', '2026-05-15T22:30:00', 349.00, 'PUBLISHED'),
(1, 'Comedy Open Mic Göteborg', 'Öppen scen för nya talanger.',                  '2026-03-28T19:30:00', '2026-03-28T21:30:00', 149.00, 'DRAFT'),
(3, 'Teaterpremiär Malmö', 'Premiärkväll med eftersnack.',                       '2026-04-25T19:00:00', '2026-04-25T21:15:00', 279.00, 'PUBLISHED');
GO

-- =========================
-- Orders
-- OrderStatus: 'CREATED','PAID','CANCELLED'
-- =========================
INSERT INTO dbo.Orders (CustomerId, OrderNumber, OrderStatus)
VALUES
(1, 'EV-2026-0001', 'PAID'),
(2, 'EV-2026-0002', 'PAID'),
(3, 'EV-2026-0003', 'PAID'),
(4, 'EV-2026-0004', 'CREATED'),
(5, 'EV-2026-0005', 'PAID'),
(6, 'EV-2026-0006', 'CANCELLED'),
(7, 'EV-2026-0007', 'PAID'),
(8, 'EV-2026-0008', 'PAID'),
(9, 'EV-2026-0009', 'CREATED'),
(10,'EV-2026-0010', 'PAID'),
(1, 'EV-2026-0011', 'PAID'),
(2, 'EV-2026-0012', 'PAID'),
(3, 'EV-2026-0013', 'CREATED'),
(4, 'EV-2026-0014', 'PAID'),
(5, 'EV-2026-0015', 'PAID');
GO

-- =========================
-- Tickets (minst 25 rader)
-- TicketType: 'STANDARD','VIP','CHILD','STUDENT'
-- =========================
INSERT INTO dbo.Tickets (OrderId, EventId, TicketType, Quantity, UnitPrice)
VALUES
-- Order 1 (Anna)
(1, 1, 'STANDARD', 2, 399.00),
(1, 1, 'VIP',      1, 599.00),

-- Order 2 (Erik)
(2, 2, 'STANDARD', 2, 299.00),

-- Order 3 (Sara)
(3, 3, 'CHILD',    2, 129.00),
(3, 3, 'STANDARD', 1, 199.00),

-- Order 4 (Johan) - CREATED
(4, 4, 'STUDENT',  1, 399.00),
(4, 4, 'STANDARD', 1, 499.00),

-- Order 5 (Emma)
(5, 5, 'STANDARD', 3, 149.00),

-- Order 6 (Oskar) - CANCELLED (ok att ha tickets, bra test)
(6, 6, 'STANDARD', 2, 349.00),

-- Order 7 (Maja)
(7, 2, 'VIP',      1, 499.00),
(7, 2, 'STANDARD', 1, 299.00),

-- Order 8 (Filip)
(8, 1, 'STANDARD', 1, 399.00),
(8, 8, 'STANDARD', 2, 279.00),

-- Order 9 (Lina) - CREATED
(9, 5, 'STANDARD', 2, 149.00),
(9, 5, 'CHILD',    1, 99.00),

-- Order 10 (David)
(10, 6, 'VIP',     1, 549.00),
(10, 6, 'STANDARD',1, 349.00),

-- Order 11 (Anna)
(11, 4, 'STUDENT', 2, 399.00),
(11, 4, 'STANDARD',1, 499.00),

-- Order 12 (Erik)
(12, 3, 'STANDARD',2, 199.00),
(12, 3, 'CHILD',   2, 129.00),

-- Order 13 (Sara) - CREATED
(13, 8, 'STANDARD',1, 279.00),
(13, 8, 'VIP',     1, 429.00),

-- Order 14 (Johan)
(14, 2, 'STANDARD',3, 299.00),

-- Order 15 (Emma)
(15, 1, 'STANDARD',2, 399.00),
(15, 6, 'STANDARD',1, 349.00),
(15, 5, 'STANDARD',1, 149.00),
(15, 3, 'CHILD',   1, 129.00),
(15, 4, 'STANDARD',1, 499.00);
GO

-- =========================
-- EventTags (M:N) - valfritt men bra
-- (EventId, TagId)
-- =========================
INSERT INTO dbo.EventTags (EventId, TagId)
VALUES
(1, 1), -- Rock Night -> Music
(1, 7), -- Rock Night -> Food
(2, 2), -- Standup -> Comedy
(3, 5), -- Familj -> Family
(3, 6), -- Familj -> Theatre
(4, 3), -- Workshop -> Tech
(4, 8), -- Workshop -> Workshop
(5, 7), -- Food Festival -> Food
(5, 5), -- Food Festival -> Family
(6, 4), -- E-sport -> Sports
(6, 3), -- E-sport -> Tech
(7, 2), -- Open Mic -> Comedy
(8, 6); -- Teaterpremiär -> Theatre
GO
