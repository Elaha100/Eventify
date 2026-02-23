using Eventify.Models;
using Eventify.Services.Interfaces;
using Spectre.Console;

namespace Eventify.UI
{
	/// <summary>
	/// Kundmeny - CRUD för kunder
	/// </summary>
	public class CustomerMenu
	{
		private readonly ICustomerService _customerService;

		public CustomerMenu(ICustomerService customerService)
		{
			_customerService = customerService;
		}

		public void Show()
		{
			while (true)
			{
				AnsiConsole.Clear();
				AnsiConsole.Write(new Rule("[blue]Kunder[/]").LeftJustified());
				AnsiConsole.WriteLine();

				var choice = AnsiConsole.Prompt(
					new SelectionPrompt<string>()
						.Title("[green]Vad vill du göra?[/]")
						.AddChoices(
							"Lista alla kunder",
							"Skapa ny kund",
							"Uppdatera kund",
							"Ta bort kund",
							"Tillbaka"));

				switch (choice)
				{
					case "Lista alla kunder":
						ListAll();
						break;
					case "Skapa ny kund":
						Create();
						break;
					case "Uppdatera kund":
						Update();
						break;
					case "Ta bort kund":
						Delete();
						break;
					case "Tillbaka":
						return;
				}
			}
		}

		private void ListAll()
		{
			var customers = _customerService.GetAll();

			if (customers.Count == 0)
			{
				AnsiConsole.MarkupLine("[yellow]Inga kunder hittades.[/]");
			}
			else
			{
				var table = new Table()
					.Border(TableBorder.Rounded)
					.AddColumn("[bold]ID[/]")
					.AddColumn("[bold]Förnamn[/]")
					.AddColumn("[bold]Efternamn[/]")
					.AddColumn("[bold]E-post[/]")
					.AddColumn("[bold]Telefon[/]")
					.AddColumn("[bold]Skapad[/]");

				foreach (var c in customers)
				{
					table.AddRow(
						c.CustomerId.ToString(),
						c.FirstName,
						c.LastName,
						c.Email,
						c.Phone ?? "-",
						c.CreatedAt.ToString("yyyy-MM-dd"));
				}

				AnsiConsole.Write(table);
			}

			WaitForKey();
		}

		private void Create()
		{
			AnsiConsole.Write(new Rule("[green]Skapa ny kund[/]").LeftJustified());
			AnsiConsole.MarkupLine("[dim]Lämna tomt och tryck Enter för att avbryta[/]");

			var firstName = AnsiConsole.Prompt(
				new TextPrompt<string>("[green]Förnamn:[/]")
					.AllowEmpty());

			if (string.IsNullOrWhiteSpace(firstName))
			{
				AnsiConsole.MarkupLine("[yellow]Avbrutet.[/]");
				WaitForKey();
				return;
			}

			var lastName = AnsiConsole.Prompt(
				new TextPrompt<string>("[green]Efternamn:[/]")
					.Validate(val => !string.IsNullOrWhiteSpace(val)
						? ValidationResult.Success()
						: ValidationResult.Error("[red]Efternamn får inte vara tomt[/]")));

			var email = AnsiConsole.Prompt(
				new TextPrompt<string>("[green]E-post:[/]")
					.Validate(val => val.Contains('@')
						? ValidationResult.Success()
						: ValidationResult.Error("[red]Ange en giltig e-postadress[/]")));

			var phone = AnsiConsole.Prompt(
				new TextPrompt<string>("[green]Telefon (valfritt):[/]")
					.AllowEmpty());

			var customer = new Customer
			{
				FirstName = firstName,
				LastName = lastName,
				Email = email,
				Phone = string.IsNullOrWhiteSpace(phone) ? null : phone
			};

			try
			{
				_customerService.Create(customer);
				AnsiConsole.MarkupLine("[green]Kund skapad![/]");
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Fel: {ex.InnerException?.Message ?? ex.Message}[/]");
			}

			WaitForKey();
		}

		private void Update()
		{
			var customers = _customerService.GetAll();
			if (customers.Count == 0)
			{
				AnsiConsole.MarkupLine("[yellow]Inga kunder att uppdatera.[/]");
				WaitForKey();
				return;
			}

			// Välj kund från lista
			var selected = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("[green]Välj kund att uppdatera:[/]")
					.PageSize(10)
					.AddChoices(customers.Select(c => $"{c.CustomerId}: {c.FirstName} {c.LastName} ({c.Email})")
						.Append("Avbryt")));

			if (selected == "Avbryt") return;

			var customerId = int.Parse(selected.Split(':')[0]);
			var customer = _customerService.GetById(customerId);
			if (customer is null) return;

			AnsiConsole.Write(new Rule("[green]Uppdatera kund[/]").LeftJustified());
			AnsiConsole.MarkupLine("[dim]Tryck Enter för att behålla nuvarande värde[/]");

			var firstName = AnsiConsole.Prompt(
				new TextPrompt<string>($"[green]Förnamn ({customer.FirstName}):[/]")
					.AllowEmpty());
			if (!string.IsNullOrWhiteSpace(firstName)) customer.FirstName = firstName;

			var lastName = AnsiConsole.Prompt(
				new TextPrompt<string>($"[green]Efternamn ({customer.LastName}):[/]")
					.AllowEmpty());
			if (!string.IsNullOrWhiteSpace(lastName)) customer.LastName = lastName;

			var email = AnsiConsole.Prompt(
				new TextPrompt<string>($"[green]E-post ({customer.Email}):[/]")
					.AllowEmpty());
			if (!string.IsNullOrWhiteSpace(email)) customer.Email = email;

			var phone = AnsiConsole.Prompt(
				new TextPrompt<string>($"[green]Telefon ({customer.Phone ?? "-"}):[/]")
					.AllowEmpty());
			if (!string.IsNullOrWhiteSpace(phone)) customer.Phone = phone;

			try
			{
				var success = _customerService.Update(customer);
				AnsiConsole.MarkupLine(success
					? "[green]Kund uppdaterad![/]"
					: "[red]Kunde inte uppdatera kund.[/]");
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Fel: {ex.InnerException?.Message ?? ex.Message}[/]");
			}

			WaitForKey();
		}

		private void Delete()
		{
			var customers = _customerService.GetAll();
			if (customers.Count == 0)
			{
				AnsiConsole.MarkupLine("[yellow]Inga kunder att ta bort.[/]");
				WaitForKey();
				return;
			}

			var selected = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("[green]Välj kund att ta bort:[/]")
					.PageSize(10)
					.AddChoices(customers.Select(c => $"{c.CustomerId}: {c.FirstName} {c.LastName} ({c.Email})")
						.Append("Avbryt")));

			if (selected == "Avbryt") return;

			var customerId = int.Parse(selected.Split(':')[0]);

			// Bekräfta borttagning
			var confirm = AnsiConsole.Confirm("[red]Är du säker? Kunden tas bort permanent.[/]", false);
			if (!confirm) return;

			try
			{
				var success = _customerService.Delete(customerId);
				AnsiConsole.MarkupLine(success
					? "[green]Kund borttagen![/]"
					: "[red]Kunde inte ta bort kunden. Kunden kan ha ordrar kopplade.[/]");
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Fel: {ex.InnerException?.Message ?? ex.Message}[/]");
			}

			WaitForKey();
		}

		/// <summary>
		/// Vänta på att användaren trycker Enter
		/// </summary>
		private static void WaitForKey()
		{
			AnsiConsole.WriteLine();
			AnsiConsole.MarkupLine("[dim]Tryck Enter för att fortsätta...[/]");
			Console.ReadLine();
		}
	}
}
