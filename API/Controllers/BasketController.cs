using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class BasketController : BaseApiController
    {
        private readonly IUnitOfWork _unitOfWork;

        public BasketController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet(Name = "GetBasket")]
        public async Task<ActionResult<BasketDto>> GetBasket()
        {
            var basket = await RetrieveBasket(GetBuyerId());

            if (basket == null) return NotFound();

            return basket.MapBasketToDto();
        }

        [HttpPost]
        public async Task<ActionResult<BasketDto>> AddItemToBasket(int productId, int quantity)
        {
            var basket = await RetrieveBasket(GetBuyerId());
            if (basket == null) basket = CreateBasket();

            var product = await _unitOfWork.Products.GetProductByIdAsync(productId);

            if (product == null) return BadRequest(new ProblemDetails{Title = "Product Not Found"});

            basket.AddItem(product, quantity);

            var result = await _unitOfWork.SaveChangesAsync(true);

            if (result) return CreatedAtRoute("GetBasket", basket.MapBasketToDto());

            return BadRequest(new ProblemDetails { Title = "Problem saving item to basket" });
        }

        [HttpDelete]
        public async Task<ActionResult> RemoveBasketItem(int productId, int quantity = 1)
        {
            var basket = await RetrieveBasket(GetBuyerId());

            if (basket == null) return NotFound();

            basket.RemoveItem(productId, quantity);

            var result = await _unitOfWork.SaveChangesAsync(true);

            if (result) return Ok();

            return BadRequest(new ProblemDetails { Title = "Problem removing item from the basket" });
        }

        private async Task<Basket?> RetrieveBasket(string buyerId)
        {
            if (string.IsNullOrEmpty(buyerId)) {
                Response.Cookies.Delete("buyerId");
                return null;
            }
            return await _unitOfWork.Baskets.GetBasketByBuyerIdAsync(buyerId);
        }

        private string GetBuyerId() {
            return User.Identity?.Name ?? Request.Cookies["buyerId"];
        }

        private Basket CreateBasket()
        {
            var buyerId = User.Identity?.Name;
            if (string.IsNullOrEmpty(buyerId)) {
                buyerId = Guid.NewGuid().ToString();
                var cookieOptions = new CookieOptions { IsEssential = true, Expires = DateTime.Now.AddDays(30) };
                Response.Cookies.Append("buyerId", buyerId, cookieOptions);
            }
           
            var basket = new Basket { BuyerId = buyerId };
            _ = _unitOfWork.Baskets.CreateBasketAsync(basket);
            return basket;
        }
    }
}