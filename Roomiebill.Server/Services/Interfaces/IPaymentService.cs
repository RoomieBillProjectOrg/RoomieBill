using System;
using Roomiebill.Server.Models;

namespace Roomiebill.Server.Services.Interfaces;

public interface IPaymentService
{
    Task<bool> ProcessPaymentAsync(PaymentRequest request);
}
