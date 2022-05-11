using Mango.Services.CouponAPI.Models.Dto;

namespace Mango.Services.CouponAPI.Repository
{
    public interface ICouponRepository
    {
        Task<CouponDTO> GetCouponByCode(string couponeCode);
    }
}
