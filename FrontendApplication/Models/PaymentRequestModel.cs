using System;

namespace FrontendApplication.Models;


public class PaymentRequestModel
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public UserModel PayerInfo { get; set; }
    public UserModel PayeeInfo { get; set; }
    public string PaymentMethod { get; set; }

    public PaymentRequestModel(){}

    public PaymentRequestModel(decimal amount, string currency, UserModel payerInfo, UserModel payeeInfo, string paymentMethod){
        Amount = amount;
        Currency = currency;
        PayerInfo = payerInfo;
        PayeeInfo = payeeInfo;
        PaymentMethod = paymentMethod;
    }
}
