using System;
using System.Net.Http.Json;
using FrontendApplication.Models;
using FrontendApplication.Services.Interfaces;

namespace FrontendApplication.Services;


public class PaymentService : IPaymentService
{
    private readonly HttpClient _httpClient;

    public PaymentService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("DefaultClient");
    }

    private async Task RedirectToPaymentAndVerifyAsync(PaymentRequestModel request)
    {
        try
        {
            // Validate BitLink
            if (string.IsNullOrWhiteSpace(request.BitLink))
            {
                throw new ArgumentException("Payment link is missing or invalid.", nameof(request.BitLink));
            }

            // Open the payment page in the browser
            try
            {
                await Browser.Default.OpenAsync(request.BitLink, BrowserLaunchMode.SystemPreferred);
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to open payment page. Please check if you have a default browser set up.", ex);
            }

            // Verify payment status
            bool paymentVerified = await CheckPaymentStatusAsync(request);

            if (!paymentVerified)
            {
                throw new Exception("Payment verification failed. Please ensure you've completed the payment process.");
            }
        }
        catch (ArgumentException)
        {
            throw; // Re-throw argument-related exceptions as they are already well-formatted
        }
        catch (Exception ex) when (ex.Message.StartsWith("Failed to open payment page"))
        {
            throw; // Re-throw browser-related errors as they are already well-formatted
        }
        catch (Exception ex)
        {
            throw new Exception("An error occurred during the payment process. Please try again.", ex);
        }
    }
    private async Task<bool> CheckPaymentStatusAsync(PaymentRequestModel request)
    {
        try
        {
            // Simulate a delay to allow the user to complete the payment
            await Task.Delay(5000); // Wait for 5 seconds (adjust as needed)

            // TODO: Implement actual payment verification logic
            return true;
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to verify payment status. Please try again.", ex);
        }
    }

    public async Task<bool> ProcessPaymentAsync(PaymentRequestModel request)
    {
        try
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request), "Payment request details are missing.");
            }

            // Validate amount
            if (request.Amount <= 0)
            {
                throw new ArgumentException("Payment amount must be greater than zero.", nameof(request.Amount));
            }

            // Redirect to the payment page and verify the payment
            await RedirectToPaymentAndVerifyAsync(request);

            // Send a POST request to process the payment
            var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}/Payment/processPayment", request);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Payment processing failed: {errorContent}");
            }

            return true;
        }
        catch (ArgumentNullException ex)
        {
            throw new Exception("Invalid payment request: " + ex.Message);
        }
        catch (ArgumentException ex)
        {
            throw new Exception("Invalid payment details: " + ex.Message);
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Failed to process payment. Please check your internet connection.", ex);
        }
        catch (Exception ex) when (ex.Message.Contains("Payment processing failed:"))
        {
            throw; // Re-throw payment processing errors as they are already well-formatted
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while processing your payment. Please try again.", ex);
        }
    }
}
