using API.Entities;
using API.RequestHelpers;

namespace API.Repositories
{
    public interface IProductRepository
    {
        Task<IQueryable<Product>> GetProductsQueryAsync(ProductParams productParams);
        Task<Product?> GetProductByIdAsync(int id);
        Task<List<string>> GetBrandsAsync();
        Task<List<string>> GetTypesAsync();
    }
}

