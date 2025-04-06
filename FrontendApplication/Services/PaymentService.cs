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

    private async Task RedirectToPaymentAndVerifyAsync(PaymentRequestModel request)
    {
        // Redirect to the BitLink webpage
        if (string.IsNullOrWhiteSpace(request.BitLink))
        {
            throw new ArgumentException("BitLink cannot be null or empty.", nameof(request.BitLink));
        }

        // Open the payment page in the browser
        await Browser.Default.OpenAsync(request.BitLink, BrowserLaunchMode.SystemPreferred);

        // Simulate verifying payment status
        bool paymentVerified = await CheckPaymentStatusAsync(request);

        if (!paymentVerified)
        {
            throw new Exception("Payment verification failed.");
        }
    }
    private async Task<bool> CheckPaymentStatusAsync(PaymentRequestModel request)
    {
        // Simulate a delay to allow the user to complete the payment
        await Task.Delay(5000); // Wait for 5 seconds (adjust as needed)

        // Always return true for now
        return true;
    }

    public async Task<bool> ProcessPaymentAsync(PaymentRequestModel request)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request), "Payment request cannot be null.");
        }
        // Redirect to the payment page and verify the payment
        await RedirectToPaymentAndVerifyAsync(request);

        // Send a POST request to process the payment
        var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}/Payment/processPayment", request);
        response.EnsureSuccessStatusCode();

        return response.IsSuccessStatusCode;
    }
}