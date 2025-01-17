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

        public async Task<UserModel> LoginUserAsync(string username, string password)
        {
            LoginDto user = new LoginDto()
            {
                username = username,
                password = password
            };

            // Connect to the server and attempt to login the user
            var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}/Users/login", user);

            // If IsSuccessStatusCode is true, then the user was successfully logged in
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var userResponse = JsonConvert.DeserializeObject<UserModel>(content);
                return userResponse;
            }

            // Else - there was an exception in the server and we want to fail the login attempt
            // and return the exception message to the user.
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
            throw new Exception(errorResponse.Message);
        }
    }
}
