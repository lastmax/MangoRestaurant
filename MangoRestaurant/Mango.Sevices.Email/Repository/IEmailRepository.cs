using Mango.Services.Email.Models;

namespace Mango.Services.Email.Repository
{
    public interface IEmailRepository
    {
        Task<bool> AddOrder(OrderHeader orderHeader);
        Task UpdateOrderPaymentStatus(int orderHeader, bool paid);
    }
}
