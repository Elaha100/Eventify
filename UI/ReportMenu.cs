using Eventify.Data;
using Microsoft.EntityFrameworkCore;
using Spectre.Console;

namespace Eventify.UI
{
	/// <summary>
	/// Rapportmeny - 4 rapporter (krav: minst 2)
	/// Använder AppDbContext direkt för komplexa LINQ-frågor
	/// </summary>
	public class ReportMenu
	{
		private readonly AppDbContext _context;

		public ReportMenu(AppDbContext context)
		{
			_context = context;
		}

		public void Show()
		{
			while (true)
			{
				AnsiConsole.Clear();
				AnsiConsole.Write(new Rule("[blue]Rapporter[/]").LeftJustified());
				AnsiConsole.WriteLine();

				var choice = AnsiConsole.Prompt(
					new SelectionPrompt<string>()
						.Title("[green]Välj rapport:[/]")
						.AddChoices(
							"Top kunder (flest köp, mest spenderat)",
							"Försäljning per event",
							"At risk-ordrar (ej betalda)",
							"Senaste 20 händelser",
							"Tillbaka"));

				switch (choice)
				{
					case "Top kunder (flest köp, mest spenderat)":
						TopCustomers();
						break;
					case "Försäljning per event":
						SalesPerEvent();
						break;
					case "At risk-ordrar (ej betalda)":
						AtRiskOrders();
						break;
					case "Senaste 20 händelser":
						LatestActivity();
						break;
					case "Tillbaka":
						return;
				}
			}
		}

		/// <summary>
		/// Rapport 1: Top kunder - flest köp och mest spenderat
		/// Motsvarar SQL-fråga Q7
		/// </summary>
		private void TopCustomers()
		{
			AnsiConsole.Write(new Rule("[yellow]Top kunder[/]").LeftJustified());

			var report = _context.Customers
				.AsNoTracking()
				.Include(c => c.Orders)
					.ThenInclude(o => o.Tickets)
				.Select(c => new
				{
					c.FirstName,
					c.LastName,
					c.Email,
					OrderCount = c.Orders.Count,
					// Summera totalt antal biljetter (bara betalda ordrar)
					TicketCount = c.Orders
						.Where(o => o.OrderStatus == "PAID")
						.SelectMany(o => o.Tickets)
						.Sum(t => t.Quantity),
					// Summera total intäkt (bara betalda ordrar)
					TotalSpent = c.Orders
						.Where(o => o.OrderStatus == "PAID")
						.SelectMany(o => o.Tickets)
						.Sum(t => t.Quantity * t.UnitPrice)
				})
				.OrderByDescending(x => x.TotalSpent)
				.ToList();

			if (report.Count == 0)
			{
				AnsiConsole.MarkupLine("[yellow]Inga kunder hittades.[/]");
			}
			else
			{
				var table = new Table()
					.Border(TableBorder.Rounded)
					.AddColumn("[bold]Kund[/]")
					.AddColumn("[bold]E-post[/]")
					.AddColumn("[bold]Ordrar[/]")
					.AddColumn("[bold]Biljetter (betalda)[/]")
					.AddColumn("[bold]Totalt spenderat[/]");

				foreach (var r in report)
				{
					table.AddRow(
						$"{r.FirstName} {r.LastName}",
						r.Email,
						r.OrderCount.ToString(),
						r.TicketCount.ToString(),
						$"{r.TotalSpent:N2} kr");
				}

				AnsiConsole.Write(table);
			}

			WaitForKey();
		}

		/// <summary>
		/// Rapport 2: Försäljning per event - antal biljetter och intäkter
		/// Motsvarar SQL-fråga Q6
		/// </summary>
		private void SalesPerEvent()
		{
			AnsiConsole.Write(new Rule("[yellow]Försäljning per event[/]").LeftJustified());

			var report = _context.Events
				.AsNoTracking()
				.Include(e => e.Venue)
				.Include(e => e.Tickets)
					.ThenInclude(t => t.Order)
				.Select(e => new
				{
					e.Title,
					VenueName = e.Venue.Name,
					e.StartAt,
					e.Status,
					// Räkna bara biljetter från betalda ordrar
					TicketsSold = e.Tickets
						.Where(t => t.Order.OrderStatus == "PAID")
						.Sum(t => t.Quantity),
					Revenue = e.Tickets
						.Where(t => t.Order.OrderStatus == "PAID")
						.Sum(t => t.Quantity * t.UnitPrice)
				})
				.OrderByDescending(x => x.Revenue)
				.ToList();

			if (report.Count == 0)
			{
				AnsiConsole.MarkupLine("[yellow]Inga event hittades.[/]");
			}
			else
			{
				var table = new Table()
					.Border(TableBorder.Rounded)
					.AddColumn("[bold]Event[/]")
					.AddColumn("[bold]Lokal[/]")
					.AddColumn("[bold]Datum[/]")
					.AddColumn("[bold]Status[/]")
					.AddColumn("[bold]Biljetter sålda[/]")
					.AddColumn("[bold]Intäkt[/]");

				foreach (var r in report)
				{
					var statusMarkup = r.Status switch
					{
						"PUBLISHED" => "[green]PUBLISHED[/]",
						"CANCELLED" => "[red]CANCELLED[/]",
						_ => "[yellow]DRAFT[/]"
					};

					table.AddRow(
						r.Title,
						r.VenueName,
						r.StartAt.ToString("yyyy-MM-dd"),
						statusMarkup,
						r.TicketsSold.ToString(),
						$"{r.Revenue:N2} kr");
				}

				AnsiConsole.Write(table);
			}

			WaitForKey();
		}

		/// <summary>
		/// Rapport 3: At risk-ordrar - ordrar som inte är betalda
		/// Motsvarar SQL-fråga Q8
		/// </summary>
		private void AtRiskOrders()
		{
			AnsiConsole.Write(new Rule("[yellow]At risk-ordrar (ej betalda)[/]").LeftJustified());

			var report = _context.Orders
				.AsNoTracking()
				.Include(o => o.Customer)
				.Include(o => o.Tickets)
				.Where(o => o.OrderStatus != "PAID")
				.OrderByDescending(o => o.CreatedAt)
				.Select(o => new
				{
					o.OrderNumber,
					CustomerName = o.Customer.FirstName + " " + o.Customer.LastName,
					o.OrderStatus,
					o.CreatedAt,
					TicketCount = o.Tickets.Sum(t => t.Quantity),
					Total = o.Tickets.Sum(t => t.Quantity * t.UnitPrice)
				})
				.ToList();

			if (report.Count == 0)
			{
				AnsiConsole.MarkupLine("[green]Inga at risk-ordrar! Alla ordrar är betalda.[/]");
			}
			else
			{
				var table = new Table()
					.Border(TableBorder.Rounded)
					.AddColumn("[bold]Ordernummer[/]")
					.AddColumn("[bold]Kund[/]")
					.AddColumn("[bold]Status[/]")
					.AddColumn("[bold]Biljetter[/]")
					.AddColumn("[bold]Belopp[/]")
					.AddColumn("[bold]Skapad[/]");

				foreach (var r in report)
				{
					var statusMarkup = r.OrderStatus == "CANCELLED"
						? "[red]CANCELLED[/]"
						: "[yellow]CREATED[/]";

					table.AddRow(
						r.OrderNumber,
						r.CustomerName,
						statusMarkup,
						r.TicketCount.ToString(),
						$"{r.Total:N2} kr",
						r.CreatedAt.ToString("yyyy-MM-dd"));
				}

				AnsiConsole.Write(table);
				AnsiConsole.MarkupLine($"\n[dim]Totalt {report.Count} ordrar som ej är betalda.[/]");
			}

			WaitForKey();
		}

		/// <summary>
		/// Rapport 4: Senaste 20 händelser - senaste ordrar med detaljer
		/// </summary>
		private void LatestActivity()
		{
			AnsiConsole.Write(new Rule("[yellow]Senaste 20 händelser[/]").LeftJustified());

			var report = _context.Orders
				.AsNoTracking()
				.Include(o => o.Customer)
				.Include(o => o.Tickets)
					.ThenInclude(t => t.Event)
				.OrderByDescending(o => o.CreatedAt)
				.Take(20)
				.Select(o => new
				{
					o.OrderNumber,
					CustomerName = o.Customer.FirstName + " " + o.Customer.LastName,
					o.OrderStatus,
					o.CreatedAt,
					TicketCount = o.Tickets.Sum(t => t.Quantity),
					// Hämta alla event-titlar för ordern
					Events = string.Join(", ", o.Tickets.Select(t => t.Event.Title).Distinct()),
					Total = o.Tickets.Sum(t => t.Quantity * t.UnitPrice)
				})
				.ToList();

			if (report.Count == 0)
			{
				AnsiConsole.MarkupLine("[yellow]Inga händelser hittades.[/]");
			}
			else
			{
				var table = new Table()
					.Border(TableBorder.Rounded)
					.AddColumn("[bold]Ordernummer[/]")
					.AddColumn("[bold]Kund[/]")
					.AddColumn("[bold]Event[/]")
					.AddColumn("[bold]Status[/]")
					.AddColumn("[bold]Biljetter[/]")
					.AddColumn("[bold]Belopp[/]")
					.AddColumn("[bold]Datum[/]");

				foreach (var r in report)
				{
					var statusMarkup = r.OrderStatus switch
					{
						"PAID" => "[green]PAID[/]",
						"CANCELLED" => "[red]CANCELLED[/]",
						_ => "[yellow]CREATED[/]"
					};

					table.AddRow(
						r.OrderNumber,
						r.CustomerName,
						r.Events,
						statusMarkup,
						r.TicketCount.ToString(),
						$"{r.Total:N2} kr",
						r.CreatedAt.ToString("yyyy-MM-dd HH:mm"));
				}

				AnsiConsole.Write(table);
			}

			WaitForKey();
		}

		private static void WaitForKey()
		{
			AnsiConsole.WriteLine();
			AnsiConsole.MarkupLine("[dim]Tryck Enter för att fortsätta...[/]");
			Console.ReadLine();
		}
	}
}
