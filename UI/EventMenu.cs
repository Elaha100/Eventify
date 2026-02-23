using Eventify.Models;
using Eventify.Services.Interfaces;
using Spectre.Console;

namespace Eventify.UI
{
	/// <summary>
	/// Evenemangsmeny - CRUD + statusuppdatering för event
	/// </summary>
	public class EventMenu
	{
		private readonly IEventService _eventService;
		private readonly IVenueService _venueService;

		public EventMenu(IEventService eventService, IVenueService venueService)
		{
			_eventService = eventService;
			_venueService = venueService;
		}

		public void Show()
		{
			while (true)
			{
				AnsiConsole.Clear();
				AnsiConsole.Write(new Rule("[blue]Evenemang[/]").LeftJustified());
				AnsiConsole.WriteLine();

				var choice = AnsiConsole.Prompt(
					new SelectionPrompt<string>()
						.Title("[green]Vad vill du göra?[/]")
						.AddChoices(
							"Lista alla event",
							"Skapa nytt event",
							"Uppdatera status",
							"Ta bort event",
							"Tillbaka"));

				switch (choice)
				{
					case "Lista alla event":
						ListAll();
						break;
					case "Skapa nytt event":
						Create();
						break;
					case "Uppdatera status":
						UpdateStatus();
						break;
					case "Ta bort event":
						Delete();
						break;
					case "Tillbaka":
						return;
				}
			}
		}

		private void ListAll()
		{
			var events = _eventService.GetAll();

			if (events.Count == 0)
			{
				AnsiConsole.MarkupLine("[yellow]Inga event hittades.[/]");
			}
			else
			{
				var table = new Table()
					.Border(TableBorder.Rounded)
					.AddColumn("[bold]ID[/]")
					.AddColumn("[bold]Titel[/]")
					.AddColumn("[bold]Lokal[/]")
					.AddColumn("[bold]Start[/]")
					.AddColumn("[bold]Slut[/]")
					.AddColumn("[bold]Pris[/]")
					.AddColumn("[bold]Status[/]");

				foreach (var e in events)
				{
					// Färga statusen beroende på värde
					var statusMarkup = e.Status switch
					{
						"PUBLISHED" => "[green]PUBLISHED[/]",
						"CANCELLED" => "[red]CANCELLED[/]",
						_ => "[yellow]DRAFT[/]"
					};

					table.AddRow(
						e.EventId.ToString(),
						e.Title,
						e.Venue?.Name ?? "-",
						e.StartAt.ToString("yyyy-MM-dd HH:mm"),
						e.EndAt.ToString("yyyy-MM-dd HH:mm"),
						e.BasePrice.ToString("N2") + " kr",
						statusMarkup);
				}

				AnsiConsole.Write(table);
			}

			WaitForKey();
		}

		private void Create()
		{
			var venues = _venueService.GetAll();
			if (venues.Count == 0)
			{
				AnsiConsole.MarkupLine("[red]Du måste skapa en lokal först.[/]");
				WaitForKey();
				return;
			}

			AnsiConsole.Write(new Rule("[green]Skapa nytt event[/]").LeftJustified());
			AnsiConsole.MarkupLine("[dim]Lämna tomt och tryck Enter för att avbryta[/]");

			var title = AnsiConsole.Prompt(
				new TextPrompt<string>("[green]Titel:[/]")
					.AllowEmpty());

			if (string.IsNullOrWhiteSpace(title))
			{
				AnsiConsole.MarkupLine("[yellow]Avbrutet.[/]");
				WaitForKey();
				return;
			}

			var description = AnsiConsole.Prompt(
				new TextPrompt<string>("[green]Beskrivning:[/]")
					.Validate(val => !string.IsNullOrWhiteSpace(val)
						? ValidationResult.Success()
						: ValidationResult.Error("[red]Beskrivning får inte vara tom[/]")));

			// Välj lokal
			var venueChoice = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("[green]Välj lokal:[/]")
					.AddChoices(venues.Select(v => $"{v.VenueId}: {v.Name} ({v.City}, {v.Capacity} platser)")));
			var venueId = int.Parse(venueChoice.Split(':')[0]);

			var startAt = AnsiConsole.Prompt(
				new TextPrompt<DateTime>("[green]Startdatum (yyyy-MM-dd HH:mm):[/]")
					.Validate(val => val > DateTime.Now
						? ValidationResult.Success()
						: ValidationResult.Error("[red]Startdatum måste vara i framtiden[/]")));

			var endAt = AnsiConsole.Prompt(
				new TextPrompt<DateTime>("[green]Slutdatum (yyyy-MM-dd HH:mm):[/]")
					.Validate(val => val > startAt
						? ValidationResult.Success()
						: ValidationResult.Error("[red]Slutdatum måste vara efter startdatum[/]")));

			var basePrice = AnsiConsole.Prompt(
				new TextPrompt<decimal>("[green]Grundpris (kr):[/]")
					.Validate(val => val >= 0
						? ValidationResult.Success()
						: ValidationResult.Error("[red]Priset kan inte vara negativt[/]")));

			// Välj status
			var status = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("[green]Status:[/]")
					.AddChoices("DRAFT", "PUBLISHED", "CANCELLED"));

			var ev = new Event
			{
				Title = title,
				Description = description,
				VenueId = venueId,
				StartAt = startAt,
				EndAt = endAt,
				BasePrice = basePrice,
				Status = status
			};

			try
			{
				_eventService.Create(ev);
				AnsiConsole.MarkupLine("[green]Event skapat![/]");
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Fel: {ex.InnerException?.Message ?? ex.Message}[/]");
			}

			WaitForKey();
		}

		private void UpdateStatus()
		{
			var events = _eventService.GetAll();
			if (events.Count == 0)
			{
				AnsiConsole.MarkupLine("[yellow]Inga event att uppdatera.[/]");
				WaitForKey();
				return;
			}

			var selected = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("[green]Välj event att uppdatera:[/]")
					.PageSize(10)
					.AddChoices(events.Select(e => $"{e.EventId}: {e.Title} (nuvarande: {e.Status})")
						.Append("Avbryt")));

			if (selected == "Avbryt") return;

			var eventId = int.Parse(selected.Split(':')[0]);

			var newStatus = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("[green]Välj ny status:[/]")
					.AddChoices("DRAFT", "PUBLISHED", "CANCELLED"));

			try
			{
				var success = _eventService.UpdateStatus(eventId, newStatus);
				AnsiConsole.MarkupLine(success
					? "[green]Status uppdaterad![/]"
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
			var events = _eventService.GetAll();
			if (events.Count == 0)
			{
				AnsiConsole.MarkupLine("[yellow]Inga event att ta bort.[/]");
				WaitForKey();
				return;
			}

			var selected = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("[green]Välj event att ta bort:[/]")
					.PageSize(10)
					.AddChoices(events.Select(e => $"{e.EventId}: {e.Title}")
						.Append("Avbryt")));

			if (selected == "Avbryt") return;

			var eventId = int.Parse(selected.Split(':')[0]);

			var confirm = AnsiConsole.Confirm("[red]Är du säker? Eventet tas bort permanent.[/]", false);
			if (!confirm) return;

			try
			{
				var success = _eventService.Delete(eventId);
				AnsiConsole.MarkupLine(success
					? "[green]Event borttaget![/]"
					: "[red]Kunde inte ta bort eventet. Det kan ha biljetter kopplade.[/]");
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
