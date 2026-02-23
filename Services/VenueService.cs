using Eventify.Data;
using Eventify.Models;
using Eventify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Eventify.Services
{
	public class VenueService : IVenueService
	{
		private readonly AppDbContext _context;

		public VenueService(AppDbContext context)
		{
			_context = context;
		}

		public List<Venue> GetAll()
		{
			return _context.Venues
				.AsNoTracking()
				.OrderBy(v => v.Name)
				.ToList();
		}

		public Venue? GetById(int venueId)
		{
			return _context.Venues
				.AsNoTracking()
				.FirstOrDefault(v => v.VenueId == venueId);
		}

		public Venue Create(Venue venue)
		{
			_context.Venues.Add(venue);
			_context.SaveChanges();
			return venue;
		}
	}
}
