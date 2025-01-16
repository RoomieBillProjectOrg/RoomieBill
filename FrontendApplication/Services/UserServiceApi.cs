using System.Net.Http.Json;
using System.Text;
using Android.Telephony.Euicc;
using Bumptech.Glide.Load.Model.Stream;
using FrontendApplication.Models;
using Newtonsoft.Json;

namespace FrontendApplication.Services
{
    public class UserServiceApi
    {
        private readonly HttpClient _httpClient;

        public UserServiceApi(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultClient");
        }

        public async Task<bool> RegisterUserAsync(string email, string username, string password)
        {
            RegisterUserDto user = new RegisterUserDto()
            {
                username = username,
                password = password,
                email = email
            };
            try{
                var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}/Users/register", user);
                response.EnsureSuccessStatusCode(); // Ensures that an error is thrown if the response is not successful
                return response.IsSuccessStatusCode;
            }
            // Just catch for debug pause here for now.
            catch (Exception ex){
                return false;
            } 
        }

        //LoginUserAsync
        public async Task<bool> LoginUserAsync(string username, string password)
        {
            LoginDto user = new LoginDto()
            {
                username = username,
                password = password
            };

            try{
                var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}/Users/login", user);
                response.EnsureSuccessStatusCode(); // Ensures that an error is thrown if the response is not successful
                return response.IsSuccessStatusCode;
            }
            // Just catch for debug pause here for now.
            catch (Exception ex){
                return false;
            } 
        }
    }
}
