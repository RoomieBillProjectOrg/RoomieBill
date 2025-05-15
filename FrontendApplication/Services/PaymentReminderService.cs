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
            var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/PaymentReminders/GetUserReminders/user/{userId}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<List<PaymentReminderModel>>() ?? new List<PaymentReminderModel>();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
            throw new Exception(errorResponse.Message);
        }

        public async Task<PaymentReminderModel> CreateReminder(CreatePaymentReminderRequest request)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}/PaymentReminders/CreateReminder", request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<PaymentReminderModel>(content);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
            throw new Exception(errorResponse.Message);
        }

        public async Task<PaymentReminderModel> UpdateReminder(UpdatePaymentReminderRequest request)
        {
            var response = await _httpClient.PutAsJsonAsync($"{_httpClient.BaseAddress}/PaymentReminders/UpdateReminder/{request.Id}", request);
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<PaymentReminderModel>(content);
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
            throw new Exception(errorResponse.Message);
        }

        public async Task DeleteReminder(int reminderId)
        {
            var response = await _httpClient.DeleteAsync($"{_httpClient.BaseAddress}/PaymentReminders/DeleteReminder/{reminderId}");
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                throw new Exception(errorResponse.Message);
            }
        }
    }
}
