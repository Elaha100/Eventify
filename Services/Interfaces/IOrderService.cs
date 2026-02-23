using Eventify.Models;

namespace Eventify.Services.Interfaces
{
	public interface IOrderService
	{
		List<Order> GetAll();
		Order? GetById(int orderId);
		Order Create(Order order);
		bool UpdateStatus(int orderId, string newStatus);
		bool Delete(int orderId);
	}
}
