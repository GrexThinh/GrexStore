using API.DTOs;
using API.Entities.OrderAggregate;
using API.Extensions;
using API.Repositories;
using API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace API.Controllers
{
    public class PaymentsController: BaseApiController
    {
        private readonly PaymentService _paymentService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;

        public PaymentsController(PaymentService paymentService, IUnitOfWork unitOfWork, IConfiguration config)
        {
            _paymentService = paymentService;
            _unitOfWork = unitOfWork;
            _config = config;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<BasketDto>> CreateOrUpdatePaymentIntent()
        {
            var basket = await _unitOfWork.Baskets.GetBasketWithItemsByBuyerIdAsync(User.Identity.Name);

            if (basket == null) return NotFound();

            var intent = await _paymentService.CreateOrUpdatePaymentIntent(basket);

            if (intent == null) return BadRequest(new ProblemDetails { Title = "Problem creating payment intent" });

            basket.PaymentIntentId = basket.PaymentIntentId ?? intent.Id;
            basket.ClientSecret = basket.ClientSecret ?? intent.ClientSecret;

            _unitOfWork.Baskets.UpdateBasket(basket);

            var result = await _unitOfWork.SaveChangesAsync(true);

            if (!result) return BadRequest(new ProblemDetails { Title = "Problem updating basket with intent" });

            return basket.MapBasketToDto();
        }

        [HttpPost("webhook")]
        public async Task<ActionResult> StripeWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], _config["StripeSettings:WhSecret"]);

            var charge = (Charge)stripeEvent.Data.Object;

            var order = await _unitOfWork.Orders.GetOrderByPaymentIntentIdAsync(charge.PaymentIntentId);

            if (charge.Status == "succeeded") order.OrderStatus = OrderStatus.PaymentReceived;

            _unitOfWork.Orders.UpdateOrder(order);
            await _unitOfWork.SaveChangesAsync();

            return new EmptyResult();
        }
    }
}
