using Eventify.Data;
using Eventify.Models;
using Eventify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Eventify.Services
{
	public class OrderService : IOrderService
	{
		private readonly AppDbContext _context;

		// Giltiga statusvärden för en order
		private static readonly string[] ValidStatuses = { "CREATED", "PAID", "CANCELLED" };

		public OrderService(AppDbContext context)
		{
			_context = context;
		}

		public List<Order> GetAll()
		{
			return _context.Orders
				.AsNoTracking()
				.Include(o => o.Customer)
				.Include(o => o.Tickets)
				.OrderByDescending(o => o.CreatedAt)
				.ToList();
		}

		public Order? GetById(int orderId)
		{
			return _context.Orders
				.AsNoTracking()
				.Include(o => o.Customer)
				.Include(o => o.Tickets)
					.ThenInclude(t => t.Event)
				.FirstOrDefault(o => o.OrderId == orderId);
		}

		public Order Create(Order order)
		{
			_context.Orders.Add(order);
			_context.SaveChanges();
			return order;
		}

		public bool UpdateStatus(int orderId, string newStatus)
		{
			// Validera att statusen är giltig
			if (!ValidStatuses.Contains(newStatus))
				return false;

			var existing = _context.Orders.FirstOrDefault(o => o.OrderId == orderId);
			if (existing is null) return false;

			existing.OrderStatus = newStatus;
			_context.SaveChanges();
			return true;
		}

		public bool Delete(int orderId)
		{
			var existing = _context.Orders
				.Include(o => o.Tickets)
				.FirstOrDefault(o => o.OrderId == orderId);

			if (existing is null) return false;

			// Ta bort biljetter som hör till ordern först
			_context.Tickets.RemoveRange(existing.Tickets);
			_context.Orders.Remove(existing);
			_context.SaveChanges();
			return true;
		}
	}
}
