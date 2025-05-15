using FrontendApplication.Models;

namespace FrontendApplication.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<bool> ProcessPaymentAsync(PaymentRequestModel request);
    }
}
