using API.Entities;

namespace API.Repositories
{
    public interface IBasketRepository
    {
        Task<Basket?> GetBasketByBuyerIdAsync(string buyerId);
        Task<Basket?> GetBasketWithItemsByBuyerIdAsync(string buyerId);
        Task<Basket> CreateBasketAsync(Basket basket);
        void UpdateBasket(Basket basket);
        void RemoveBasket(Basket basket);
    }
}

