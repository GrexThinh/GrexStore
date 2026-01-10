using API.Data;
using API.DTOs;
using API.Entities.OrderAggregate;
using API.Extensions;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly StoreContext _context;

        public OrderRepository(StoreContext context)
        {
            _context = context;
        }

        public async Task<List<OrderDto>> GetOrdersByBuyerIdAsync(string buyerId)
        {
            return await _context.Orders
                .ProjectOrderToOrderDto()
                .Where(x => x.BuyerId == buyerId)
                .ToListAsync();
        }

        public async Task<OrderDto?> GetOrderByIdAndBuyerIdAsync(int id, string buyerId)
        {
            return await _context.Orders
                .Where(x => x.BuyerId == buyerId && x.Id == id)
                .ProjectOrderToOrderDto()
                .FirstOrDefaultAsync();
        }

        public async Task<Order?> GetOrderByPaymentIntentIdAsync(string paymentIntentId)
        {
            return await _context.Orders
                .FirstOrDefaultAsync(x => x.PaymentIntentId == paymentIntentId);
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            await _context.Orders.AddAsync(order);
            return order;
        }

        public void UpdateOrder(Order order)
        {
            _context.Orders.Update(order);
        }
    }
}

