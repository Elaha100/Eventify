using Eventify.Data;
using Eventify.Models;
using Eventify.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Eventify.Services
{
	public class CustomerService : ICustomerService
	{
		private readonly AppDbContext _context;

		public CustomerService(AppDbContext context)
		{
			_context = context;
		}

		public List<Customer> GetAll()
		{
			return _context.Customers
				.AsNoTracking()
				.ToList();
		}

		public Customer? GetById(int customerId)
		{
			return _context.Customers
				.AsNoTracking()
				.FirstOrDefault(c => c.CustomerId == customerId);
		}

		public Customer Create(Customer customer)
		{
			_context.Customers.Add(customer);
			_context.SaveChanges();
			return customer;
		}

		public bool Update(Customer customer)
		{
			var existing = _context.Customers.FirstOrDefault(c => c.CustomerId == customer.CustomerId);
			if (existing is null) return false;

			// ÄNDRA dessa så de matchar Customer.cs
			existing.FirstName = customer.FirstName;
			existing.LastName = customer.LastName;
			existing.Email = customer.Email;
			existing.Phone = customer.Phone;


			_context.SaveChanges();
			return true;
		}

		public bool Delete(int customerId)
		{
			var existing = _context.Customers
				.FirstOrDefault(c => c.CustomerId == customerId);

			if (existing is null) return false;

			_context.Customers.Remove(existing);
			_context.SaveChanges();
			return true;
		}


	}
}