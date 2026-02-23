using Eventify.Models;

namespace Eventify.Services.Interfaces
{
	public interface ICustomerService
	{
		List<Customer> GetAll();
		Customer? GetById(int customerId);
		Customer Create(Customer customer);
		bool Update(Customer customer);
		bool Delete(int customerId);
	}
}
