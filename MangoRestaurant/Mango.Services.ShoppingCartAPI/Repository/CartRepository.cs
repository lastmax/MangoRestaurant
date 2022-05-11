using AutoMapper;
using Mango.Services.ShoppingCartAPI.DbContexts;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.ShoppingCartAPI.Repository
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _db;
        private IMapper _mapper;

        public CartRepository(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<bool> ClearCart(string userId)
        {
            var cartHeaderFromDb = await _db.CartHeaders.FirstOrDefaultAsync(ch => ch.UserId == userId);
            if (cartHeaderFromDb != null)
            {
                _db.CartDetails.RemoveRange(
                    _db.CartDetails.Where(cd => cd.CartHeaderId == cartHeaderFromDb.CartHeaderId));
                _db.CartHeaders.Remove(cartHeaderFromDb);
                await _db.SaveChangesAsync();
                return true;
            }

            return false;
        }

        public async Task<CartDTO> CreateUpdateCart(CartDTO cartDTO)
        {
            var cart = _mapper.Map<Cart>(cartDTO);

            var prodInDb = await _db.Products
                .FirstOrDefaultAsync(p => p.ProductId == cart.CartDetails.FirstOrDefault().ProductId);

            if (prodInDb == null)
            {
                _db.Products.Add(cart.CartDetails.FirstOrDefault().Product);
                await _db.SaveChangesAsync();
            }

            var cartHeaderFromDb = await _db.CartHeaders.AsNoTracking()
                .FirstOrDefaultAsync(ch => ch.UserId == cart.CartHeader.UserId);

            if (cartHeaderFromDb == null)
            {
                _db.CartHeaders.Add(cart.CartHeader);
                await _db.SaveChangesAsync();
                cart.CartDetails.FirstOrDefault().CartHeaderId = cart.CartHeader.CartHeaderId;
                cart.CartDetails.FirstOrDefault().Product = null;
                _db.CartDetails.Add(cart.CartDetails.FirstOrDefault());
                await _db.SaveChangesAsync();
            }
            else
            {
                var cartDetailsFromDb = await _db.CartDetails.AsNoTracking().FirstOrDefaultAsync(
                    d => d.ProductId == cart.CartDetails.FirstOrDefault().ProductId &&
                    d.CartHeaderId == cartHeaderFromDb.CartHeaderId);

                if (cartDetailsFromDb == null)
                {
                    cart.CartDetails.FirstOrDefault().CartHeaderId = cartHeaderFromDb.CartHeaderId;
                    cart.CartDetails.FirstOrDefault().Product = null;
                    _db.CartDetails.Add(cart.CartDetails.FirstOrDefault());
                    await _db.SaveChangesAsync();
                }
                else
                {
                    cart.CartDetails.FirstOrDefault().Product = null;
                    cart.CartDetails.FirstOrDefault().Count += cartDetailsFromDb.Count;
                    _db.CartDetails.Update(cart.CartDetails.FirstOrDefault());
                    await _db.SaveChangesAsync();
                }
            }

            return _mapper.Map<CartDTO>(cart);
        }

        public async Task<CartDTO> GetCartByUserId(string userId)
        {
            Cart cart = new()
            {
                CartHeader = await _db.CartHeaders.FirstOrDefaultAsync(ch => ch.UserId == userId)
            };

            cart.CartDetails = _db.CartDetails
                .Where(cd => cd.CartHeaderId == cart.CartHeader.CartHeaderId).Include(p => p.Product);

            return _mapper.Map<CartDTO>(cart);
        }

        public async Task<bool> RemoveFromCart(int cartDetailsId)
        {
            try
            {
                var cartDetails = await _db.CartDetails.FirstOrDefaultAsync(cd => cd.CartDetailsId == cartDetailsId);
                var totalDetailsInCart = _db.CartDetails
                    .Where(cd => cd.CartHeaderId == cartDetails.CartHeaderId).Count();

                _db.CartDetails.Remove(cartDetails);

                if (totalDetailsInCart == 1)
                {
                    var cartHeaderToRemove = await _db.CartHeaders
                        .FirstOrDefaultAsync(ch => ch.CartHeaderId == cartDetails.ProductId);

                    _db.CartHeaders.Remove(cartHeaderToRemove);
                }

                await _db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
