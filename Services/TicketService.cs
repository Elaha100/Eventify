using Eventify.Data;
using Eventify.Models;
using Eventify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Eventify.Services
{
	public class TicketService : ITicketService
	{
		private readonly AppDbContext _context;

		public TicketService(AppDbContext context)
		{
			_context = context;
		}

		public List<Ticket> GetByOrderId(int orderId)
		{
			return _context.Tickets
				.AsNoTracking()
				.Include(t => t.Event)
				.Where(t => t.OrderId == orderId)
				.ToList();
		}

		public Ticket Create(Ticket ticket)
		{
			_context.Tickets.Add(ticket);
			_context.SaveChanges();
			return ticket;
		}
	}
}
