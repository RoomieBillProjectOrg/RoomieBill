using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using FrontendApplication.Models;

namespace FrontendApplication.Services
{
    public class UserService
    {
        private readonly HttpClient _httpClient;

        public UserService(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("BackendClient");
        }

        public async Task<bool> RegisterUserAsync(string email, string username, string password)
        {
            var payload = new
            {
                Email = email,
                Username = username,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync("/users/register", payload);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> LoginUserAsync(string username, string password)
        {
            var payload = new
            {
                Username = username,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync("/users/login", payload);

            return response.IsSuccessStatusCode;
        }
    }
}
