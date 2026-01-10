namespace API.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IProductRepository Products { get; }
        IBasketRepository Baskets { get; }
        IOrderRepository Orders { get; }
        Task<int> SaveChangesAsync();
        Task<bool> SaveChangesAsync(bool returnResult);
    }
}

