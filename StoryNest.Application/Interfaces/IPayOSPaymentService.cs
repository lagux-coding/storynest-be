
using Net.payOS.Types;

namespace StoryNest.Application.Interfaces
{
    public interface IPayOSPaymentService
    {
        public Task<CreatePaymentResult> CheckoutAsync(long userId, int planId);
        public Task<bool> CancelAsync(long userId, long orderCode);
    }
}
