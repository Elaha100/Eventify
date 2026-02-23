using Eventify.Data;
using Eventify.Models;
using Eventify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Eventify.Services
{
	public class EventService : IEventService
	{
		private readonly AppDbContext _context;

		// Giltiga statusvärden för ett event
		private static readonly string[] ValidStatuses = { "DRAFT", "PUBLISHED", "CANCELLED" };

		public EventService(AppDbContext context)
		{
			_context = context;
		}

		public List<Event> GetAll()
		{
			return _context.Events
				.AsNoTracking()
				.Include(e => e.Venue)
				.OrderBy(e => e.StartAt)
				.ToList();
		}

		public Event? GetById(int eventId)
		{
			return _context.Events
				.AsNoTracking()
				.Include(e => e.Venue)
				.FirstOrDefault(e => e.EventId == eventId);
		}

		public Event Create(Event ev)
		{
			_context.Events.Add(ev);
			_context.SaveChanges();
			return ev;
		}

		public bool UpdateStatus(int eventId, string newStatus)
		{
			// Validera att statusen är giltig
			if (!ValidStatuses.Contains(newStatus))
				return false;

			var existing = _context.Events.FirstOrDefault(e => e.EventId == eventId);
			if (existing is null) return false;

			existing.Status = newStatus;
			_context.SaveChanges();
			return true;
		}

		public bool Delete(int eventId)
		{
			var existing = _context.Events.FirstOrDefault(e => e.EventId == eventId);
			if (existing is null) return false;

			_context.Events.Remove(existing);
			_context.SaveChanges();
			return true;
		}
	}
}
