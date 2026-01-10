using API.Data;
using API.Entities;
using API.Extensions;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class BasketRepository : IBasketRepository
    {
        private readonly StoreContext _context;

        public BasketRepository(StoreContext context)
        {
            _context = context;
        }

        public async Task<Basket?> GetBasketByBuyerIdAsync(string buyerId)
        {
            return await _context.Baskets
                .Include(i => i.Items)
                .ThenInclude(p => p.Product)
                .FirstOrDefaultAsync(basket => basket.BuyerId == buyerId);
        }

        public async Task<Basket?> GetBasketWithItemsByBuyerIdAsync(string buyerId)
        {
            return await _context.Baskets
                .RetrieveBasketWithItems(buyerId)
                .FirstOrDefaultAsync();
        }

        public async Task<Basket> CreateBasketAsync(Basket basket)
        {
            await _context.Baskets.AddAsync(basket);
            return basket;
        }

        public void UpdateBasket(Basket basket)
        {
            _context.Baskets.Update(basket);
        }

        public void RemoveBasket(Basket basket)
        {
            _context.Baskets.Remove(basket);
        }
    }
}

