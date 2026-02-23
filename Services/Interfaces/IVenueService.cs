using Eventify.Models;

namespace Eventify.Services.Interfaces
{
	public interface IVenueService
	{
		List<Venue> GetAll();
		Venue? GetById(int venueId);
		Venue Create(Venue venue);
	}
}
