using Eventify.Data;
using Eventify.Services;
using Eventify.Services.Interfaces;
using Eventify.UI;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Spectre.Console;

namespace Eventify
{
	internal class Program
	{
		static void Main(string[] args)
		{
			// Bygg konfiguration från appsettings-filer
			var config = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json", optional: false)
				.AddJsonFile("appsettings.Development.json", optional: true)
				.Build();

			// Hämta connection string
			var connectionString = config.GetConnectionString("EventifyDb");

			if (string.IsNullOrWhiteSpace(connectionString))
			{
				AnsiConsole.MarkupLine("[red]FEL: Connection string 'EventifyDb' saknas i appsettings.json[/]");
				return;
			}

			// Bygg DbContext
			var options = new DbContextOptionsBuilder<AppDbContext>()
				.UseSqlServer(connectionString)
				.Options;

			using var context = new AppDbContext(options);

			// Skapa alla services
			ICustomerService customerService = new CustomerService(context);
			IEventService eventService = new EventService(context);
			IVenueService venueService = new VenueService(context);
			IOrderService orderService = new OrderService(context);
			ITicketService ticketService = new TicketService(context);
			ITagService tagService = new TagService(context);

			// Skapa alla menyer
			var customerMenu = new CustomerMenu(customerService);
			var eventMenu = new EventMenu(eventService, venueService);
			var venueMenu = new VenueMenu(venueService);
			var orderMenu = new OrderMenu(orderService, customerService, eventService, ticketService);
			var tagMenu = new TagMenu(tagService, eventService);
			var reportMenu = new ReportMenu(context);

			// Starta huvudmenyn
			var mainMenu = new MainMenu(
				customerMenu,
				eventMenu,
				venueMenu,
				orderMenu,
				tagMenu,
				reportMenu);

			mainMenu.Run();
		}
	}
}
