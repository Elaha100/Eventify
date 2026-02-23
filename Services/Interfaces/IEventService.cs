using Eventify.Models;

namespace Eventify.Services.Interfaces
{
	public interface IEventService
	{
		List<Event> GetAll();
		Event? GetById(int eventId);
		Event Create(Event ev);
		bool UpdateStatus(int eventId, string newStatus);
		bool Delete(int eventId);
	}
}
