using AutoMapper;
using Mango.Services.CouponAPI.DbContexts;
using Mango.Services.CouponAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.CouponAPI.Repository
{
    public class CouponRepository : ICouponRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;
        public CouponRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<CouponDTO> GetCouponByCode(string couponeCode)
        {
            var couponFromDb = await _db.Coupons.FirstOrDefaultAsync(x => x.CouponCode == couponeCode);
            return _mapper.Map<CouponDTO>(couponFromDb);
        }
    }
}
