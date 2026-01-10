using API.DTOs;
using API.Entities.OrderAggregate;

namespace API.Repositories
{
    public interface IOrderRepository
    {
        Task<List<OrderDto>> GetOrdersByBuyerIdAsync(string buyerId);
        Task<OrderDto?> GetOrderByIdAndBuyerIdAsync(int id, string buyerId);
        Task<Order?> GetOrderByPaymentIntentIdAsync(string paymentIntentId);
        Task<Order> CreateOrderAsync(Order order);
        void UpdateOrder(Order order);
    }
}

