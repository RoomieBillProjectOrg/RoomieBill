using System;
using Roomiebill.Server.Models;
using Roomiebill.Server.Services.Interfaces;

namespace Roomiebill.Server.Services;

public class MockPaymentService : IPaymentService
{
    public async Task<bool> ProcessPaymentAsync(PaymentRequest request)
    {
        // Simulate processing time
        await Task.Delay(500);

        // Mock successful payment
        return true;
    }
}
