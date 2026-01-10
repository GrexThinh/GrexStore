using API.Data;
using API.Entities;
using API.Extensions;
using API.RequestHelpers;
using Microsoft.EntityFrameworkCore;

namespace API.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly StoreContext _context;

        public ProductRepository(StoreContext context)
        {
            _context = context;
        }

        public async Task<IQueryable<Product>> GetProductsQueryAsync(ProductParams productParams)
        {
            var query = _context.Products
                .Sort(productParams.OrderBy)
                .Search(productParams.SearchTerm)
                .Filter(productParams.Brands, productParams.Types)
                .AsQueryable();

            return await Task.FromResult(query);
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id);
        }

        public async Task<List<string>> GetBrandsAsync()
        {
            return await _context.Products
                .Select(p => p.Brand)
                .Distinct()
                .ToListAsync();
        }

        public async Task<List<string>> GetTypesAsync()
        {
            return await _context.Products
                .Select(p => p.Type)
                .Distinct()
                .ToListAsync();
        }
    }
}

