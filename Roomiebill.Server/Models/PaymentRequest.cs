using System;

namespace Roomiebill.Server.Models;

public class PaymentRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public User PayeeInfo { get; set; }
    public User PayerInfo { get; set; }
    public string PaymentMethod { get; set; }
    public int GroupId { get; set; }

    public PaymentRequest(){}

    //TODO: maybe transfer user ids instead
    public PaymentRequest(decimal amount, string currency, User payeeInfo, User payerInfo, string paymentMethod, int groupId){
        Amount = amount;
        Currency = currency;
        PayerInfo = payerInfo;
        PayeeInfo = payeeInfo;
        PaymentMethod = paymentMethod;
        GroupId = groupId;
    }
}
