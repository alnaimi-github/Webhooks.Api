using Webhooks.Api.Models;

namespace Webhooks.Api.Repositories;

public class InMemoryOrderRepository
{
    private readonly List<Order> _orders = [];
    public InMemoryOrderRepository()
    {
        _orders.Add(new Order(Guid.NewGuid(), "John Doe", 100, DateTime.UtcNow));
        _orders.Add(new Order(Guid.NewGuid(), "Jane Doe", 200, DateTime.UtcNow));
    }
    public IEnumerable<Order> GetAll()
    {
        return _orders;
    }

    public void AddOrder(Order order)
    {
        _orders.Add(order);
    }
}