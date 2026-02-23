using Spectre.Console;

namespace Eventify.UI
{
	/// <summary>
	/// Huvudmeny - navigerar till alla delmenyerna
	/// </summary>
	public class MainMenu
	{
		private readonly CustomerMenu _customerMenu;
		private readonly EventMenu _eventMenu;
		private readonly VenueMenu _venueMenu;
		private readonly OrderMenu _orderMenu;
		private readonly TagMenu _tagMenu;
		private readonly ReportMenu _reportMenu;

		public MainMenu(
			CustomerMenu customerMenu,
			EventMenu eventMenu,
			VenueMenu venueMenu,
			OrderMenu orderMenu,
			TagMenu tagMenu,
			ReportMenu reportMenu)
		{
			_customerMenu = customerMenu;
			_eventMenu = eventMenu;
			_venueMenu = venueMenu;
			_orderMenu = orderMenu;
			_tagMenu = tagMenu;
			_reportMenu = reportMenu;
		}

		public void Run()
		{
			while (true)
			{
				AnsiConsole.Clear();

				// Visa en snygg rubrik
				var panel = new Panel(
					"[bold blue]Eventify[/]\n" +
					"[dim]Event & Biljetthantering[/]\n\n" +
					"[dim]Created by[/] [bold]Team3 — Database First Squad[/]\n" +
					"[dim]Klas Olsson · Elaha Sultani · Mohammed Yusur · Georgia Chalkia[/]")
					.Border(BoxBorder.Rounded)
					.BorderColor(Color.Blue);
				AnsiConsole.Write(panel);
				AnsiConsole.WriteLine();

				var choice = AnsiConsole.Prompt(
					new SelectionPrompt<string>()
						.Title("[green]Välj menyalternativ:[/]")
						.PageSize(10)
						.AddChoices(
							"Kunder",
							"Evenemang",
							"Lokaler",
							"Ordrar & Biljetter",
							"Taggar",
							"Rapporter",
							"Avsluta"));

				switch (choice)
				{
					case "Kunder":
						_customerMenu.Show();
						break;
					case "Evenemang":
						_eventMenu.Show();
						break;
					case "Lokaler":
						_venueMenu.Show();
						break;
					case "Ordrar & Biljetter":
						_orderMenu.Show();
						break;
					case "Taggar":
						_tagMenu.Show();
						break;
					case "Rapporter":
						_reportMenu.Show();
						break;
					case "Avsluta":
						AnsiConsole.MarkupLine("[yellow]Hejdå![/]");
						return;
				}
			}
		}
	}
}
