# Eventify

Konsolapplikation for event- och biljetthantering byggd med C#, Entity Framework Core och SQL Server.
Projektet anvander Database First-metoden och Spectre.Console for ett interaktivt terminalgranssnitt.

## Team

**Team3 — Database First Squad**

| Namn | Roll |
|---|---|
| Klas Olsson | Utvecklare |
| Elaha Sultani | Utvecklare |
| Mohammed Yusur | Utvecklare |
| Georgia Chalkia | Utvecklare |

## Funktioner

- **Kunder** — Skapa, lista, uppdatera och ta bort kunder
- **Evenemang** — Hantera event med titel, beskrivning, lokal, datum och status (DRAFT / PUBLISHED / CANCELLED)
- **Lokaler** — Skapa och lista lokaler med adress, stad och kapacitet
- **Ordrar & Biljetter** — Skapa ordrar kopplade till kunder, lagg till biljetter med typ (STANDARD, VIP, CHILD, STUDENT) och automatisk prisberakning
- **Taggar** — Skapa taggar och koppla/ta bort dem fran event (many-to-many)
- **Rapporter** — Fordefinierade rapporter via SQL-vyer

## Teknikstack

| Komponent | Version |
|---|---|
| .NET | 10.0 |
| Entity Framework Core | 10.0.2 |
| SQL Server (LocalDB / Express) | — |
| Spectre.Console | 0.54.0 |

## Projektstruktur

```
Eventify/
├── Program.cs                  # Startpunkt — bygger DI och startar huvudmenyn
├── appsettings.json            # Connection string
│
├── Models/                     # EF Core-entiteter (Database First)
│   ├── Customer.cs
│   ├── Event.cs
│   ├── EventTag.cs             # Kopplingstabell Event ↔ Tag
│   ├── Order.cs
│   ├── Tag.cs
│   ├── Ticket.cs
│   └── Venue.cs
│
├── Data/
│   └── AppDbContext.cs          # DbContext med Fluent API-konfiguration
│
├── Services/                    # Affarslogik (CRUD)
│   ├── CustomerService.cs
│   ├── EventService.cs
│   ├── OrderService.cs
│   ├── TagService.cs
│   ├── TicketService.cs
│   ├── VenueService.cs
│   └── Interfaces/              # Abstraktioner for varje service
│       ├── ICustomerService.cs
│       ├── IEventService.cs
│       ├── IOrderService.cs
│       ├── ITagService.cs
│       ├── ITicketService.cs
│       └── IVenueService.cs
│
├── UI/                          # Spectre.Console-menyer
│   ├── MainMenu.cs
│   ├── CustomerMenu.cs
│   ├── EventMenu.cs
│   ├── VenueMenu.cs
│   ├── OrderMenu.cs
│   ├── TagMenu.cs
│   └── ReportMenu.cs
│
├── SQL_Queries/                 # SQL-skript for databashantering
│   ├── 01Creata_Database-sql.sql
│   ├── 02_create_tables.sql
│   ├── 03seed_data.sql.sql
│   ├── 04crud_examples.sql.sql
│   ├── 05queries_joins.sql.sql
│   ├── 06views.sql.sql
│   ├── 07security.sql.sql
│   └── 08cleanup.sql.sql
│
└── ER-diagram.png               # ER-diagram over databasen
```

## Kom igang

### Forutsattningar

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (Express eller LocalDB)

### 1. Klona repot

```bash
git clone <repo-url>
cd Eventify
```

### 2. Skapa databasen

Kor SQL-skripten i `SQL_Queries/` i ordning mot din SQL Server-instans:

```
01Creata_Database-sql.sql   → Skapar databasen
02_create_tables.sql        → Skapar tabeller
03seed_data.sql.sql         → Exempeldata (valfritt)
```

### 3. Konfigurera connection string

Uppdatera `appsettings.json` med din egen SQL Server-instans:

```json
{
  "ConnectionStrings": {
    "EventifyDb": "Server=DIN_SERVER;Database=EventifyDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### 4. Bygg och kor

```bash
dotnet build
dotnet run
```

## Databasdesign

Applikationen anvander foljande tabeller:

| Tabell | Beskrivning |
|---|---|
| `Customers` | Kunder med namn, e-post och telefon |
| `Venues` | Lokaler med adress, stad och kapacitet |
| `Events` | Evenemang kopplade till en lokal |
| `Orders` | Ordrar kopplade till en kund |
| `Tickets` | Biljetter kopplade till en order och ett event |
| `Tags` | Taggar for kategorisering |
| `EventTags` | Kopplingstabell for many-to-many mellan Event och Tag |

Se `ER-diagram.png` for en visuell oversikt.

## Anvandning

Applikationen startar med en interaktiv huvudmeny:

```
╭──────────────────────────────────────────────────────────────────╮
│ Eventify                                                         │
│ Event & Biljetthantering                                         │
│                                                                  │
│ Created by Team3 — Database First Squad                          │
│ Klas Olsson · Elaha Sultani · Mohammed Yusur · Georgia Chalkia   │
╰──────────────────────────────────────────────────────────────────╯

> Kunder
  Evenemang
  Lokaler
  Ordrar & Biljetter
  Taggar
  Rapporter
  Avsluta
```

Navigera med piltangenterna och tryck Enter for att valja. I alla formulär och entity-val kan du avbryta och ga tillbaka.
