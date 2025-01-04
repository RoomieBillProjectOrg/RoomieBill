using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using FrontendApplication.Models;
using Newtonsoft.Json;

namespace FrontendApplication.Services
{
    public class UserServiceApi
    {
        private readonly HttpClient _httpClient;

        public UserServiceApi()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(AppConfig.ApiBaseUrl)
            };
        }

        public async Task<bool> RegisterUserAsync(string email, string username, string password)
        {
            var user = new
            {
                Email = email,
                Username = username,
                Password = password
            };

            var json = JsonConvert.SerializeObject(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/UsersController/register", content);

            return response.IsSuccessStatusCode;
        }
    }
}
