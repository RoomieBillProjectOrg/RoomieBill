using System;
using System.Net.Http.Json;
using FrontendApplication.Models;

namespace FrontendApplication.Services;


public class PaymentService
{
    private readonly HttpClient _httpClient;

    public PaymentService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("DefaultClient");
    }

    public async Task<bool> ProcessPaymentAsync(PaymentRequestModel request)
    {
        var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}/Payment/processPayment", request);
        response.EnsureSuccessStatusCode();
        return response.IsSuccessStatusCode;
    }
}