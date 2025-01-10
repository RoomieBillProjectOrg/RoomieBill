using System.Text;
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
            RegisterUserDto user = new RegisterUserDto()
            {
                Username = username,
                Password = password,
                Email = email
            };

            var json = JsonConvert.SerializeObject(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/UsersController/register", content);

            return response.IsSuccessStatusCode;
        }

        //LoginUserAsync
        public async Task<bool> LoginUserAsync(string username, string password)
        {
            LoginDto user = new LoginDto()
            {
                Username = username,
                Password = password
            };

            var json = JsonConvert.SerializeObject(user);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/UsersController/login", content);

            return response.IsSuccessStatusCode;
        }
    }
}
