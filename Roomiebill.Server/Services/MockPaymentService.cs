using System;
using Roomiebill.Server.Models;

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

