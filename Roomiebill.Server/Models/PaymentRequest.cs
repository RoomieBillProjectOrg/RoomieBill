using System;

namespace Roomiebill.Server.Models;

public class PaymentRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public User PayerInfo { get; set; }
    public User PayeeInfo { get; set; }
    public string PaymentMethod { get; set; }

    public PaymentRequest(){}

    public PaymentRequest(decimal amount, string currency, User payerInfo, User payeeInfo, string paymentMethod){
        Amount = amount;
        Currency = currency;
        PayerInfo = payerInfo;
        PayeeInfo = payeeInfo;
        PaymentMethod = paymentMethod;
    }
}
