using Mango.MessageBus;
using Mango.Services.ShoppingCartAPI.Messages;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Repository;
using Microsoft.AspNetCore.Mvc;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [ApiController]
    [Route("api/cart")]
    public class CartAPIController : Controller
    {
        private readonly ICouponRepository _couponRepository;
        private readonly ICartRepository _cartRepository;
        private readonly IMessageBus _messageBus;
        protected ResponseDTO _response;

        public CartAPIController(ICartRepository cartRepository, IMessageBus messageBus, ICouponRepository couponRepository)
        {
            _couponRepository = couponRepository;
            _cartRepository = cartRepository;
            _messageBus = messageBus;
            _response = new ResponseDTO();
        }

        [HttpGet("GetCart/{userId}")]
        public async Task<object> GetCart(string userId)
        {
            try
            {
                CartDTO cartDTO = await _cartRepository.GetCartByUserId(userId);
                _response.Result = cartDTO;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }

            return _response;
        }

        [HttpPost("AddCart")]
        public async Task<object> AddCart([FromBody] CartDTO cartDTO)
        {
            try
            {
                CartDTO cartDt = await _cartRepository.CreateUpdateCart(cartDTO);
                _response.Result = cartDt;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }

            return _response;
        }

        [HttpPost("UpdateCart")]
        public async Task<object> UpdateCart([FromBody] CartDTO cartDTO)
        {
            try
            {
                CartDTO cartDt = await _cartRepository.CreateUpdateCart(cartDTO);
                _response.Result = cartDt;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }

            return _response;
        }

        [HttpPost("RemoveFromCart")]
        public async Task<object> RemoveFromCart([FromBody] int cartDetailsId)
        {
            try
            {
                bool isSuccess = await _cartRepository.RemoveFromCart(cartDetailsId);
                _response.Result = isSuccess;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }

            return _response;
        }

        [HttpPost("ClearCart")]
        public async Task<object> ClearCart([FromBody] string userId)
        {
            try
            {
                bool isSuccess = await _cartRepository.ClearCart(userId);
                _response.Result = isSuccess;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }

            return _response;
        }

        [HttpPost("ApplyCoupon")]
        public async Task<object> ApplyCoupon([FromBody] CartDTO cartDTO)
        {
            try
            {
                bool isSuccess = await _cartRepository.ApplyCoupon(cartDTO.CartHeader.UserId,
                    cartDTO.CartHeader.CouponCode);
                _response.Result = isSuccess;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }

            return _response;
        }

        [HttpPost("RemoveCoupon")]
        public async Task<object> RemoveCoupon([FromBody] string userId)
        {
            try
            {
                bool isSuccess = await _cartRepository.RemoveCoupon(userId);
                _response.Result = isSuccess;
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }

            return _response;
        }

        [HttpPost("Checkout")]
        public async Task<object> Checkout([FromBody] CheckoutHeaderDTO checkoutHeader)
        {
            try
            {
                var cartDTO = await _cartRepository.GetCartByUserId(checkoutHeader.UserId);
                if (cartDTO == null)
                    return BadRequest();

                if (!string.IsNullOrEmpty(checkoutHeader.CouponCode))
                {
                    CouponDTO coupon = await _couponRepository.GetCoupon(checkoutHeader.CouponCode);
                    if(checkoutHeader.DiscountTotal != coupon.DiscountAmount)
                    {
                        _response.IsSuccess = false;
                        _response.ErrorMessages = new List<string>() { "Coupon Price has changed, please confirm" };
                        _response.DisplayMessage = "Coupon Price has changed, please confirm";
                        return _response;
                    }
                }

                checkoutHeader.CartDetails = cartDTO.CartDetails;
                await _messageBus.PublishMessage(checkoutHeader, SD.CheckoutQueue);
                await _cartRepository.ClearCart(checkoutHeader.UserId);
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }

            return _response;
        }

        private void HandleError(Exception ex)
        {
            _response.IsSuccess = false;
            _response.ErrorMessages = new List<string>() { ex.ToString() };
        }
    }
}
