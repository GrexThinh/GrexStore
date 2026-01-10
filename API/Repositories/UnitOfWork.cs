using API.Data;

namespace API.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly StoreContext _context;
        private IProductRepository? _products;
        private IBasketRepository? _baskets;
        private IOrderRepository? _orders;

        public UnitOfWork(StoreContext context)
        {
            _context = context;
        }

        public IProductRepository Products
        {
            get
            {
                _products ??= new ProductRepository(_context);
                return _products;
            }
        }

        public IBasketRepository Baskets
        {
            get
            {
                _baskets ??= new BasketRepository(_context);
                return _baskets;
            }
        }

        public IOrderRepository Orders
        {
            get
            {
                _orders ??= new OrderRepository(_context);
                return _orders;
            }
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<bool> SaveChangesAsync(bool returnResult)
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

