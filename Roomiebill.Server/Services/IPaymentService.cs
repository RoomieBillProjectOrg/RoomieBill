using System;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.Services;

public interface IPaymentService
{
    Task<bool> ProcessPaymentAsync(PaymentRequest request);
}


