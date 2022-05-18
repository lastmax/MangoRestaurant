using Mango.Web.Models.Dto;

namespace Mango.Web.Services.IServices
{
    public interface ICartService
    {
        Task<T> GetCartByUserIdAsync<T>(string userId, string token = null);
        Task<T> AddToCartAsync<T>(CartDTO cartDTO, string token = null);
        Task<T> UpdateCartAsync<T>(CartDTO cartDTO, string token = null);
        Task<T> RemoveFromCartAsync<T>(int cartDetailsId, string token = null);
        Task<T> ClearCartAsync<T>(int cartId, string token = null);
        Task<T> ApplyCoupon<T>(CartDTO cartDTO, string token = null);
        Task<T> RemoveCoupon<T>(string userId, string token = null);
        Task<T> Checkout<T>(CartHeaderDTO cartHeader, string token = null);
    }
}
