using FrontendApplication.Models;
using FrontendApplication.Services.Interfaces;
using System.Net.Http.Json;
using Newtonsoft.Json;

namespace FrontendApplication.Services
{
    public class PaymentReminderService : IPaymentReminderService
    {
        private readonly HttpClient _httpClient;

        public PaymentReminderService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultClient");
            _httpClient.Timeout = TimeSpan.FromSeconds(60);
        }

        public async Task<List<PaymentReminderModel>> GetUserReminders(int userId)
        {
            try
            {
                if (userId <= 0)
                {
                    throw new ArgumentException("Invalid user ID.", nameof(userId));
                }

                var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/PaymentReminders/GetUserReminders/user/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var reminders = await response.Content.ReadFromJsonAsync<List<PaymentReminderModel>>();
                    return reminders ?? new List<PaymentReminderModel>();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                throw new Exception($"Failed to get reminders: {errorResponse?.Message ?? "Unknown error"}");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Failed to fetch payment reminders. Please check your internet connection.", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Failed to process the reminders data. Please try again.", ex);
            }
            catch (Exception ex) when (ex.Message.Contains("Failed to get reminders:"))
            {
                throw; // Re-throw reminder-specific errors as they are already well-formatted
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while fetching payment reminders. Please try again.", ex);
            }
        }

        public async Task<PaymentReminderModel> CreateReminder(CreatePaymentReminderRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request), "Reminder creation details are missing.");
                }

                ValidateReminderRequest(request);

                var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}/PaymentReminders/CreateReminder", request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var reminder = JsonConvert.DeserializeObject<PaymentReminderModel>(content);
                    return reminder ?? throw new Exception("Failed to retrieve reminder data after creation.");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                throw new Exception($"Failed to create reminder: {errorResponse?.Message ?? "Unknown error"}");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Failed to create payment reminder. Please check your internet connection.", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Failed to process the reminder creation response. Please try again.", ex);
            }
            catch (Exception ex) when (ex.Message.Contains("Failed to create reminder:"))
            {
                throw; // Re-throw reminder-specific errors as they are already well-formatted
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while creating the reminder. Please try again.", ex);
            }
        }

        public async Task<PaymentReminderModel> UpdateReminder(UpdatePaymentReminderRequest request)
        {
            try
            {
                if (request == null)
                {
                    throw new ArgumentNullException(nameof(request), "Reminder update details are missing.");
                }

                if (request.Id <= 0)
                {
                    throw new ArgumentException("Invalid reminder ID.", nameof(request.Id));
                }

                ValidateReminderRequest(request);

                var response = await _httpClient.PutAsJsonAsync($"{_httpClient.BaseAddress}/PaymentReminders/UpdateReminder/{request.Id}", request);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var reminder = JsonConvert.DeserializeObject<PaymentReminderModel>(content);
                    return reminder ?? throw new Exception("Failed to retrieve reminder data after update.");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                throw new Exception($"Failed to update reminder: {errorResponse?.Message ?? "Unknown error"}");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Failed to update payment reminder. Please check your internet connection.", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Failed to process the reminder update response. Please try again.", ex);
            }
            catch (Exception ex) when (ex.Message.Contains("Failed to update reminder:"))
            {
                throw; // Re-throw reminder-specific errors as they are already well-formatted
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while updating the reminder. Please try again.", ex);
            }
        }

        public async Task DeleteReminder(int reminderId)
        {
            try
            {
                if (reminderId <= 0)
                {
                    throw new ArgumentException("Invalid reminder ID.", nameof(reminderId));
                }

                var response = await _httpClient.DeleteAsync($"{_httpClient.BaseAddress}/PaymentReminders/DeleteReminder/{reminderId}");

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                    throw new Exception($"Failed to delete reminder: {errorResponse?.Message ?? "Unknown error"}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Failed to delete payment reminder. Please check your internet connection.", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Failed to process the deletion response. Please try again.", ex);
            }
            catch (Exception ex) when (ex.Message.Contains("Failed to delete reminder:"))
            {
                throw; // Re-throw reminder-specific errors as they are already well-formatted
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while deleting the reminder. Please try again.", ex);
            }
        }

        private void ValidateReminderRequest(dynamic request)
        {
            // Common validation for both create and update requests
            if (request.UserId <= 0)
            {
                throw new ArgumentException("Invalid user ID.", nameof(request.UserId));
            }

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new ArgumentException("Reminder title is required.", nameof(request.Title));
            }

            if (request.Amount <= 0)
            {
                throw new ArgumentException("Reminder amount must be greater than zero.", nameof(request.Amount));
            }

            if (request.DueDate <= DateTime.Now)
            {
                throw new ArgumentException("Due date must be in the future.", nameof(request.DueDate));
            }
        }
    }
}
