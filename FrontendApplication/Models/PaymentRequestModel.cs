using System;

namespace FrontendApplication.Models;


public class PaymentRequestModel
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
     public UserModel PayeeInfo { get; set; }
    public UserModel PayerInfo { get; set; }
    public string PaymentMethod { get; set; }
    public int GroupId { get; set; }

    public PaymentRequestModel(){}

    public PaymentRequestModel(decimal amount, string currency, UserModel payeeInfo, UserModel payerInfo, string paymentMethod, int groupId){
        Amount = amount;
        Currency = currency;
        PayerInfo = payerInfo;
        PayeeInfo = payeeInfo;
        PaymentMethod = paymentMethod;
        GroupId = groupId;
    }
}
