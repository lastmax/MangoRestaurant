using Mango.Services.OrderAPI.DbContexts;
using Mango.Services.OrderAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace Mango.Services.OrderAPI.Repository
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DbContextOptions<ApplicationDbContext> _dbContext;
        public OrderRepository(DbContextOptions<ApplicationDbContext> dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> AddOrder(OrderHeader orderHeader)
        {
            try
            {
                await using var db = new ApplicationDbContext(_dbContext);
                db.OrderHeaders.Add(orderHeader);
                await db.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                var message = ex.Message;
                return false;
            }
        }

        public async Task UpdateOrderPaymentStatus(int orderHeader, bool paid)
        {
            await using var db = new ApplicationDbContext(_dbContext);
            var orderHeaderFromDb = await db.OrderHeaders.FirstOrDefaultAsync(oh => oh.OrderHeaderId == orderHeader);
            if (orderHeaderFromDb != null)
            {
                orderHeaderFromDb.PaymentStatus = paid;
                await db.SaveChangesAsync();
            }
        }
    }
}
