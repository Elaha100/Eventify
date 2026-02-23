using Eventify.Models;

namespace Eventify.Services.Interfaces
{
	public interface ITicketService
	{
		List<Ticket> GetByOrderId(int orderId);
		Ticket Create(Ticket ticket);
	}
}
