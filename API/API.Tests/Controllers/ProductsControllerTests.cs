using API.Controllers;
using API.Entities;
using API.Repositories;
using API.RequestHelpers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace API.Tests.Controllers
{
    public class ProductsControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly ProductsController _controller;

        public ProductsControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockProductRepository = new Mock<IProductRepository>();
            _controller = new ProductsController(_mockUnitOfWork.Object);

            // Setup UnitOfWork to return mocked repository
            _mockUnitOfWork.Setup(u => u.Products).Returns(_mockProductRepository.Object);
        }

        [Fact]
        public async Task GetProduct_WithValidId_ReturnsProduct()
        {
            // Arrange
            var productId = 1;
            var expectedProduct = new Product
            {
                Id = productId,
                Name = "Test Product",
                Description = "Test Description",
                Price = 100,
                PictureUrl = "test.jpg",
                Type = "Test Type",
                Brand = "Test Brand",
                QuantityInStock = 10
            };

            _mockProductRepository
                .Setup(r => r.GetProductByIdAsync(productId))
                .ReturnsAsync(expectedProduct);

            // Act
            var result = await _controller.GetProduct(productId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Product>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var product = Assert.IsType<Product>(okResult.Value);
            Assert.Equal(productId, product.Id);
            Assert.Equal("Test Product", product.Name);
        }

        [Fact]
        public async Task GetProduct_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var productId = 999;
            _mockProductRepository
                .Setup(r => r.GetProductByIdAsync(productId))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _controller.GetProduct(productId);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Product>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task GetProducts_WithValidParams_ReturnsPagedList()
        {
            // Arrange
            var productParams = new ProductParams
            {
                PageNumber = 1,
                PageSize = 10,
                OrderBy = "name",
                SearchTerm = "",
                Brands = "",
                Types = ""
            };

            var products = new List<Product>
            {
                new Product { Id = 1, Name = "Product 1", Price = 100 },
                new Product { Id = 2, Name = "Product 2", Price = 200 }
            }.AsQueryable();

            _mockProductRepository
                .Setup(r => r.GetProductsQueryAsync(productParams))
                .ReturnsAsync(products);

            // Act
            var result = await _controller.GetProducts(productParams);

            // Assert
            var actionResult = Assert.IsType<ActionResult<PagedList<Product>>>(result);
            var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
            var pagedList = Assert.IsType<PagedList<Product>>(okResult.Value);
            Assert.NotNull(pagedList);
        }

        [Fact]
        public async Task GetFilters_ReturnsBrandsAndTypes()
        {
            // Arrange
            var expectedBrands = new List<string> { "Brand1", "Brand2" };
            var expectedTypes = new List<string> { "Type1", "Type2" };

            _mockProductRepository
                .Setup(r => r.GetBrandsAsync())
                .ReturnsAsync(expectedBrands);

            _mockProductRepository
                .Setup(r => r.GetTypesAsync())
                .ReturnsAsync(expectedTypes);

            // Act
            var result = await _controller.GetFilters();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var filters = okResult.Value as dynamic;
            Assert.NotNull(filters);
        }
    }
}

