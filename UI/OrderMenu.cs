using Eventify.Models;
using Eventify.Services.Interfaces;
using Spectre.Console;

namespace Eventify.UI
{
	/// <summary>
	/// Ordermeny - Skapa order med biljetter, uppdatera status, ta bort
	/// </summary>
	public class OrderMenu
	{
		private readonly IOrderService _orderService;
		private readonly ICustomerService _customerService;
		private readonly IEventService _eventService;
		private readonly ITicketService _ticketService;

		public OrderMenu(
			IOrderService orderService,
			ICustomerService customerService,
			IEventService eventService,
			ITicketService ticketService)
		{
			_orderService = orderService;
			_customerService = customerService;
			_eventService = eventService;
			_ticketService = ticketService;
		}

		public void Show()
		{
			while (true)
			{
				AnsiConsole.Clear();
				AnsiConsole.Write(new Rule("[blue]Ordrar & Biljetter[/]").LeftJustified());
				AnsiConsole.WriteLine();

				var choice = AnsiConsole.Prompt(
					new SelectionPrompt<string>()
						.Title("[green]Vad vill du göra?[/]")
						.AddChoices(
							"Lista alla ordrar",
							"Skapa ny order med biljetter",
							"Uppdatera orderstatus",
							"Ta bort order",
							"Tillbaka"));

				switch (choice)
				{
					case "Lista alla ordrar":
						ListAll();
						break;
					case "Skapa ny order med biljetter":
						CreateOrderWithTickets();
						break;
					case "Uppdatera orderstatus":
						UpdateStatus();
						break;
					case "Ta bort order":
						Delete();
						break;
					case "Tillbaka":
						return;
				}
			}
		}

		private void ListAll()
		{
			var orders = _orderService.GetAll();

			if (orders.Count == 0)
			{
				AnsiConsole.MarkupLine("[yellow]Inga ordrar hittades.[/]");
			}
			else
			{
				var table = new Table()
					.Border(TableBorder.Rounded)
					.AddColumn("[bold]ID[/]")
					.AddColumn("[bold]Ordernummer[/]")
					.AddColumn("[bold]Kund[/]")
					.AddColumn("[bold]Status[/]")
					.AddColumn("[bold]Biljetter[/]")
					.AddColumn("[bold]Skapad[/]");

				foreach (var o in orders)
				{
					var statusMarkup = o.OrderStatus switch
					{
						"PAID" => "[green]PAID[/]",
						"CANCELLED" => "[red]CANCELLED[/]",
						_ => "[yellow]CREATED[/]"
					};

					var ticketCount = o.Tickets?.Sum(t => t.Quantity) ?? 0;

					table.AddRow(
						o.OrderId.ToString(),
						o.OrderNumber,
						$"{o.Customer?.FirstName} {o.Customer?.LastName}",
						statusMarkup,
						ticketCount.ToString(),
						o.CreatedAt.ToString("yyyy-MM-dd"));
				}

				AnsiConsole.Write(table);
			}

			WaitForKey();
		}

		private void CreateOrderWithTickets()
		{
			// Hämta kunder och event
			var customers = _customerService.GetAll();
			var events = _eventService.GetAll();

			if (customers.Count == 0)
			{
				AnsiConsole.MarkupLine("[red]Du måste skapa en kund först.[/]");
				WaitForKey();
				return;
			}
			if (events.Count == 0)
			{
				AnsiConsole.MarkupLine("[red]Du måste skapa ett event först.[/]");
				WaitForKey();
				return;
			}

			AnsiConsole.Write(new Rule("[green]Skapa ny order med biljetter[/]").LeftJustified());

			// Välj kund
			var customerChoice = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("[green]Välj kund:[/]")
					.PageSize(10)
					.AddChoices(customers.Select(c => $"{c.CustomerId}: {c.FirstName} {c.LastName}")
						.Append("Avbryt")));

			if (customerChoice == "Avbryt") return;

			var customerId = int.Parse(customerChoice.Split(':')[0]);

			// Skapa ordernummer automatiskt
			var orderNumber = $"ORD-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}";
			AnsiConsole.MarkupLine($"[dim]Ordernummer: {orderNumber}[/]");

			// Skapa ordern
			var order = new Order
			{
				CustomerId = customerId,
				OrderNumber = orderNumber,
				OrderStatus = "CREATED"
			};

			try
			{
				_orderService.Create(order);
				AnsiConsole.MarkupLine("[green]Order skapad![/]");

				// Lägg till biljetter
				var addMore = true;
				while (addMore)
				{
					AnsiConsole.Write(new Rule("[dim]Lägg till biljett[/]").LeftJustified());

					// Välj event
					var eventChoice = AnsiConsole.Prompt(
						new SelectionPrompt<string>()
							.Title("[green]Välj event:[/]")
							.PageSize(10)
							.AddChoices(events.Select(e => $"{e.EventId}: {e.Title} ({e.BasePrice:N2} kr)")
								.Append("Avbryt")));

					if (eventChoice == "Avbryt") break;

					var eventId = int.Parse(eventChoice.Split(':')[0]);
					var selectedEvent = events.First(e => e.EventId == eventId);

					// Välj biljett-typ
					var ticketType = AnsiConsole.Prompt(
						new SelectionPrompt<string>()
							.Title("[green]Biljett-typ:[/]")
							.AddChoices("STANDARD", "VIP", "CHILD", "STUDENT"));

					var quantity = AnsiConsole.Prompt(
						new TextPrompt<int>("[green]Antal:[/]")
							.Validate(val => val > 0
								? ValidationResult.Success()
								: ValidationResult.Error("[red]Antal måste vara minst 1[/]")));

					// Beräkna pris baserat på typ
					var unitPrice = ticketType switch
					{
						"VIP" => selectedEvent.BasePrice * 1.5m,
						"CHILD" => selectedEvent.BasePrice * 0.5m,
						"STUDENT" => selectedEvent.BasePrice * 0.7m,
						_ => selectedEvent.BasePrice
					};

					var ticket = new Ticket
					{
						OrderId = order.OrderId,
						EventId = eventId,
						TicketType = ticketType,
						Quantity = quantity,
						UnitPrice = unitPrice
					};

					_ticketService.Create(ticket);
					AnsiConsole.MarkupLine($"[green]Biljett tillagd! {quantity}x {ticketType} à {unitPrice:N2} kr[/]");

					addMore = AnsiConsole.Confirm("[green]Vill du lägga till fler biljetter?[/]", false);
				}

				AnsiConsole.MarkupLine("[green]Order med biljetter klar![/]");
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Fel: {ex.InnerException?.Message ?? ex.Message}[/]");
			}

			WaitForKey();
		}

		private void UpdateStatus()
		{
			var orders = _orderService.GetAll();
			if (orders.Count == 0)
			{
				AnsiConsole.MarkupLine("[yellow]Inga ordrar att uppdatera.[/]");
				WaitForKey();
				return;
			}

			var selected = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("[green]Välj order att uppdatera:[/]")
					.PageSize(10)
					.AddChoices(orders.Select(o =>
						$"{o.OrderId}: {o.OrderNumber} - {o.Customer?.FirstName} {o.Customer?.LastName} (nuvarande: {o.OrderStatus})")
						.Append("Avbryt")));

			if (selected == "Avbryt") return;

			var orderId = int.Parse(selected.Split(':')[0]);

			var newStatus = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("[green]Välj ny status:[/]")
					.AddChoices("CREATED", "PAID", "CANCELLED"));

			try
			{
				var success = _orderService.UpdateStatus(orderId, newStatus);
				AnsiConsole.MarkupLine(success
					? "[green]Orderstatus uppdaterad![/]"
					: "[red]Kunde inte uppdatera status.[/]");
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Fel: {ex.InnerException?.Message ?? ex.Message}[/]");
			}

			WaitForKey();
		}

		private void Delete()
		{
			var orders = _orderService.GetAll();
			if (orders.Count == 0)
			{
				AnsiConsole.MarkupLine("[yellow]Inga ordrar att ta bort.[/]");
				WaitForKey();
				return;
			}

			var selected = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("[green]Välj order att ta bort:[/]")
					.PageSize(10)
					.AddChoices(orders.Select(o =>
						$"{o.OrderId}: {o.OrderNumber} - {o.Customer?.FirstName} {o.Customer?.LastName}")
						.Append("Avbryt")));

			if (selected == "Avbryt") return;

			var orderId = int.Parse(selected.Split(':')[0]);

			var confirm = AnsiConsole.Confirm("[red]Är du säker? Ordern och dess biljetter tas bort permanent.[/]", false);
			if (!confirm) return;

			try
			{
				var success = _orderService.Delete(orderId);
				AnsiConsole.MarkupLine(success
					? "[green]Order borttagen![/]"
					: "[red]Kunde inte ta bort ordern.[/]");
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Fel: {ex.InnerException?.Message ?? ex.Message}[/]");
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
