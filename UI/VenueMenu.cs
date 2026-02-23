using Eventify.Models;
using Eventify.Services.Interfaces;
using Spectre.Console;

namespace Eventify.UI
{
	/// <summary>
	/// Lokalmeny - Lista och skapa lokaler
	/// </summary>
	public class VenueMenu
	{
		private readonly IVenueService _venueService;

		public VenueMenu(IVenueService venueService)
		{
			_venueService = venueService;
		}

		public void Show()
		{
			while (true)
			{
				AnsiConsole.Clear();
				AnsiConsole.Write(new Rule("[blue]Lokaler[/]").LeftJustified());
				AnsiConsole.WriteLine();

				var choice = AnsiConsole.Prompt(
					new SelectionPrompt<string>()
						.Title("[green]Vad vill du göra?[/]")
						.AddChoices(
							"Lista alla lokaler",
							"Skapa ny lokal",
							"Tillbaka"));

				switch (choice)
				{
					case "Lista alla lokaler":
						ListAll();
						break;
					case "Skapa ny lokal":
						Create();
						break;
					case "Tillbaka":
						return;
				}
			}
		}

		private void ListAll()
		{
			var venues = _venueService.GetAll();

			if (venues.Count == 0)
			{
				AnsiConsole.MarkupLine("[yellow]Inga lokaler hittades.[/]");
			}
			else
			{
				var table = new Table()
					.Border(TableBorder.Rounded)
					.AddColumn("[bold]ID[/]")
					.AddColumn("[bold]Namn[/]")
					.AddColumn("[bold]Adress[/]")
					.AddColumn("[bold]Stad[/]")
					.AddColumn("[bold]Kapacitet[/]");

				foreach (var v in venues)
				{
					table.AddRow(
						v.VenueId.ToString(),
						v.Name,
						v.Address,
						v.City,
						v.Capacity.ToString());
				}

				AnsiConsole.Write(table);
			}

			WaitForKey();
		}

		private void Create()
		{
			AnsiConsole.Write(new Rule("[green]Skapa ny lokal[/]").LeftJustified());
			AnsiConsole.MarkupLine("[dim]Lämna tomt och tryck Enter för att avbryta[/]");

			var name = AnsiConsole.Prompt(
				new TextPrompt<string>("[green]Namn:[/]")
					.AllowEmpty());

			if (string.IsNullOrWhiteSpace(name))
			{
				AnsiConsole.MarkupLine("[yellow]Avbrutet.[/]");
				WaitForKey();
				return;
			}

			var address = AnsiConsole.Prompt(
				new TextPrompt<string>("[green]Adress:[/]")
					.Validate(val => !string.IsNullOrWhiteSpace(val)
						? ValidationResult.Success()
						: ValidationResult.Error("[red]Adress får inte vara tom[/]")));

			var city = AnsiConsole.Prompt(
				new TextPrompt<string>("[green]Stad:[/]")
					.Validate(val => !string.IsNullOrWhiteSpace(val)
						? ValidationResult.Success()
						: ValidationResult.Error("[red]Stad får inte vara tom[/]")));

			var capacity = AnsiConsole.Prompt(
				new TextPrompt<int>("[green]Kapacitet:[/]")
					.Validate(val => val > 0
						? ValidationResult.Success()
						: ValidationResult.Error("[red]Kapacitet måste vara större än 0[/]")));

			var venue = new Venue
			{
				Name = name,
				Address = address,
				City = city,
				Capacity = capacity
			};

			try
			{
				_venueService.Create(venue);
				AnsiConsole.MarkupLine("[green]Lokal skapad![/]");
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
