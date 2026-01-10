using API.Controllers;
using API.DTOs;
using API.Entities;
using API.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using Xunit;

namespace API.Tests.Controllers
{
    public class BasketControllerTests
    {
        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
        private readonly Mock<IBasketRepository> _mockBasketRepository;
        private readonly Mock<IProductRepository> _mockProductRepository;
        private readonly BasketController _controller;

        public BasketControllerTests()
        {
            _mockUnitOfWork = new Mock<IUnitOfWork>();
            _mockBasketRepository = new Mock<IBasketRepository>();
            _mockProductRepository = new Mock<IProductRepository>();
            _controller = new BasketController(_mockUnitOfWork.Object);

            // Setup UnitOfWork to return mocked repositories
            _mockUnitOfWork.Setup(u => u.Baskets).Returns(_mockBasketRepository.Object);
            _mockUnitOfWork.Setup(u => u.Products).Returns(_mockProductRepository.Object);

            // Setup controller context for authenticated user
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, "testuser")
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = claimsPrincipal
                }
            };
        }

        [Fact]
        public async Task GetBasket_WithValidBuyerId_ReturnsBasket()
        {
            // Arrange
            var buyerId = "testuser";
            var basket = new Basket
            {
                Id = 1,
                BuyerId = buyerId,
                Items = new List<BasketItem>
                {
                    new BasketItem
                    {
                        ProductId = 1,
                        Quantity = 2,
                        Product = new Product { Id = 1, Name = "Test Product", Price = 100 }
                    }
                }
            };

            _mockBasketRepository
                .Setup(r => r.GetBasketByBuyerIdAsync(buyerId))
                .ReturnsAsync(basket);

            // Act
            var result = await _controller.GetBasket();

            // Assert
            var actionResult = Assert.IsType<ActionResult<BasketDto>>(result);
            
            // When returning a value directly from ActionResult<T>, the value is in Value property
            Assert.NotNull(actionResult.Value);
            var basketDto = Assert.IsType<BasketDto>(actionResult.Value);
            Assert.Equal(buyerId, basketDto.BuyerId);
            Assert.Single(basketDto.Items);
        }

        [Fact]
        public async Task GetBasket_WithInvalidBuyerId_ReturnsNotFound()
        {
            // Arrange
            var buyerId = "nonexistent";
            _mockBasketRepository
                .Setup(r => r.GetBasketByBuyerIdAsync(buyerId))
                .ReturnsAsync((Basket?)null);

            // Act
            var result = await _controller.GetBasket();

            // Assert
            var actionResult = Assert.IsType<ActionResult<BasketDto>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }

        [Fact]
        public async Task AddItemToBasket_WithValidProduct_ReturnsCreatedBasket()
        {
            // Arrange
            var productId = 1;
            var quantity = 2;
            var buyerId = "testuser";
            var product = new Product
            {
                Id = productId,
                Name = "Test Product",
                Price = 100,
                QuantityInStock = 10
            };

            var basket = new Basket
            {
                Id = 1,
                BuyerId = buyerId,
                Items = new List<BasketItem>()
            };

            _mockBasketRepository
                .Setup(r => r.GetBasketByBuyerIdAsync(buyerId))
                .ReturnsAsync((Basket?)null);

            _mockProductRepository
                .Setup(r => r.GetProductByIdAsync(productId))
                .ReturnsAsync(product);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(true))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.AddItemToBasket(productId, quantity);

            // Assert
            var actionResult = Assert.IsType<ActionResult<BasketDto>>(result);
            Assert.NotNull(actionResult);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(true), Times.Once);
        }

        [Fact]
        public async Task AddItemToBasket_WithInvalidProduct_ReturnsBadRequest()
        {
            // Arrange
            var productId = 999;
            var quantity = 1;
            var buyerId = "testuser";

            _mockBasketRepository
                .Setup(r => r.GetBasketByBuyerIdAsync(buyerId))
                .ReturnsAsync((Basket?)null);

            _mockProductRepository
                .Setup(r => r.GetProductByIdAsync(productId))
                .ReturnsAsync((Product?)null);

            // Act
            var result = await _controller.AddItemToBasket(productId, quantity);

            // Assert
            var actionResult = Assert.IsType<ActionResult<BasketDto>>(result);
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(actionResult.Result);
            Assert.NotNull(badRequestResult);
        }

        [Fact]
        public async Task RemoveBasketItem_WithValidBasket_ReturnsOk()
        {
            // Arrange
            var productId = 1;
            var quantity = 1;
            var buyerId = "testuser";
            var basket = new Basket
            {
                Id = 1,
                BuyerId = buyerId,
                Items = new List<BasketItem>
                {
                    new BasketItem { ProductId = productId, Quantity = 2 }
                }
            };

            _mockBasketRepository
                .Setup(r => r.GetBasketByBuyerIdAsync(buyerId))
                .ReturnsAsync(basket);

            _mockUnitOfWork
                .Setup(u => u.SaveChangesAsync(true))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.RemoveBasketItem(productId, quantity);

            // Assert
            Assert.IsType<OkResult>(result);
            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(true), Times.Once);
        }
    }
}

