using Eventify.Models;
using Eventify.Services.Interfaces;
using Spectre.Console;

namespace Eventify.UI
{
	/// <summary>
	/// Taggmeny - Lista taggar, koppla/ta bort tag från event
	/// </summary>
	public class TagMenu
	{
		private readonly ITagService _tagService;
		private readonly IEventService _eventService;

		public TagMenu(ITagService tagService, IEventService eventService)
		{
			_tagService = tagService;
			_eventService = eventService;
		}

		public void Show()
		{
			while (true)
			{
				AnsiConsole.Clear();
				AnsiConsole.Write(new Rule("[blue]Taggar[/]").LeftJustified());
				AnsiConsole.WriteLine();

				var choice = AnsiConsole.Prompt(
					new SelectionPrompt<string>()
						.Title("[green]Vad vill du göra?[/]")
						.AddChoices(
							"Lista alla taggar",
							"Skapa ny tag",
							"Koppla tag till event",
							"Ta bort tag från event",
							"Tillbaka"));

				switch (choice)
				{
					case "Lista alla taggar":
						ListAll();
						break;
					case "Skapa ny tag":
						Create();
						break;
					case "Koppla tag till event":
						AddTagToEvent();
						break;
					case "Ta bort tag från event":
						RemoveTagFromEvent();
						break;
					case "Tillbaka":
						return;
				}
			}
		}

		private void ListAll()
		{
			var tags = _tagService.GetAll();

			if (tags.Count == 0)
			{
				AnsiConsole.MarkupLine("[yellow]Inga taggar hittades.[/]");
			}
			else
			{
				var table = new Table()
					.Border(TableBorder.Rounded)
					.AddColumn("[bold]ID[/]")
					.AddColumn("[bold]Namn[/]")
					.AddColumn("[bold]Skapad[/]");

				foreach (var t in tags)
				{
					table.AddRow(
						t.TagId.ToString(),
						t.Name,
						t.CreatedAt.ToString("yyyy-MM-dd"));
				}

				AnsiConsole.Write(table);
			}

			WaitForKey();
		}

		private void Create()
		{
			AnsiConsole.Write(new Rule("[green]Skapa ny tag[/]").LeftJustified());
			AnsiConsole.MarkupLine("[dim]Lämna tomt och tryck Enter för att avbryta[/]");

			var name = AnsiConsole.Prompt(
				new TextPrompt<string>("[green]Taggnamn:[/]")
					.AllowEmpty());

			if (string.IsNullOrWhiteSpace(name))
			{
				AnsiConsole.MarkupLine("[yellow]Avbrutet.[/]");
				WaitForKey();
				return;
			}

			try
			{
				_tagService.Create(new Tag { Name = name });
				AnsiConsole.MarkupLine("[green]Tag skapad![/]");
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Fel: {ex.InnerException?.Message ?? ex.Message}[/]");
			}

			WaitForKey();
		}

		private void AddTagToEvent()
		{
			var events = _eventService.GetAll();
			var tags = _tagService.GetAll();

			if (events.Count == 0 || tags.Count == 0)
			{
				AnsiConsole.MarkupLine("[red]Du behöver minst ett event och en tag.[/]");
				WaitForKey();
				return;
			}

			// Välj event
			var eventChoice = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("[green]Välj event:[/]")
					.PageSize(10)
					.AddChoices(events.Select(e => $"{e.EventId}: {e.Title}")
						.Append("Avbryt")));

			if (eventChoice == "Avbryt") return;

			var eventId = int.Parse(eventChoice.Split(':')[0]);

			// Visa befintliga taggar för eventet
			var existingTags = _tagService.GetTagsForEvent(eventId);
			if (existingTags.Count > 0)
			{
				AnsiConsole.MarkupLine("[dim]Befintliga taggar:[/] " +
					string.Join(", ", existingTags.Select(et => et.Tag.Name)));
			}

			// Välj tag att koppla
			var tagChoice = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("[green]Välj tag att koppla:[/]")
					.AddChoices(tags.Select(t => $"{t.TagId}: {t.Name}")
						.Append("Avbryt")));

			if (tagChoice == "Avbryt") return;

			var tagId = int.Parse(tagChoice.Split(':')[0]);

			try
			{
				var success = _tagService.AddTagToEvent(eventId, tagId);
				AnsiConsole.MarkupLine(success
					? "[green]Tag kopplad till event![/]"
					: "[yellow]Taggen är redan kopplad till detta event.[/]");
			}
			catch (Exception ex)
			{
				AnsiConsole.MarkupLine($"[red]Fel: {ex.InnerException?.Message ?? ex.Message}[/]");
			}

			WaitForKey();
		}

		private void RemoveTagFromEvent()
		{
			var events = _eventService.GetAll();
			if (events.Count == 0)
			{
				AnsiConsole.MarkupLine("[yellow]Inga event hittades.[/]");
				WaitForKey();
				return;
			}

			// Välj event
			var eventChoice = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("[green]Välj event:[/]")
					.PageSize(10)
					.AddChoices(events.Select(e => $"{e.EventId}: {e.Title}")
						.Append("Avbryt")));

			if (eventChoice == "Avbryt") return;

			var eventId = int.Parse(eventChoice.Split(':')[0]);

			// Hämta taggar för eventet
			var eventTags = _tagService.GetTagsForEvent(eventId);
			if (eventTags.Count == 0)
			{
				AnsiConsole.MarkupLine("[yellow]Eventet har inga taggar.[/]");
				WaitForKey();
				return;
			}

			var tagChoice = AnsiConsole.Prompt(
				new SelectionPrompt<string>()
					.Title("[green]Välj tag att ta bort:[/]")
					.AddChoices(eventTags.Select(et => $"{et.TagId}: {et.Tag.Name}")
						.Append("Avbryt")));

			if (tagChoice == "Avbryt") return;

			var tagId = int.Parse(tagChoice.Split(':')[0]);

			try
			{
				var success = _tagService.RemoveTagFromEvent(eventId, tagId);
				AnsiConsole.MarkupLine(success
					? "[green]Tag borttagen från event![/]"
					: "[red]Kunde inte ta bort taggen.[/]");
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
