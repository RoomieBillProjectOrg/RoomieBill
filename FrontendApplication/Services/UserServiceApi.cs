using System.Net.Http.Json;
using System.Text;
using FrontendApplication.Models;
using Newtonsoft.Json;
using Roomiebill.Server.DataAccessLayer.Dtos;

namespace FrontendApplication.Services
{
    public class UserServiceApi
    {
        private readonly HttpClient _httpClient;

        public UserServiceApi(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultClient");
        }

        public async Task<bool> RegisterUserAsync(string email, string username, string password, string firebaseToken)
        {
            RegisterUserDto user = new RegisterUserDto()
            {
                username = username,
                password = password,
                email = email,
                firebaseToken = firebaseToken
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

        public async Task<UserModel> LoginUserAsync(string username, string password, string firebaseToken)
        {
            LoginDto user = new LoginDto()
            {
                username = username,
                password = password,
                firebaseToken = firebaseToken
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

        public async Task LogoutUserAsync(string username)
        {
            var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}/Users/logout", username);

            // If there was an exception in the server and we want to fail the logout attempt
            // and return the exception message to the user.
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                throw new Exception(errorResponse.Message);
            }
        }

        public async Task UpdateUserPasswordAsync(UpdatePasswordDto updatePasswordDto)
        {
            // Connect to the server and attempt to update the user password
            var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}/Users/updatePassword", updatePasswordDto);

            // If there was an exception in the server and we want to fail the update password attempt
            // and return the exception message to the user.
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                throw new Exception(errorResponse.Message);
            }
        }

        public async Task<GroupModel> CreateNewGroupAsync(CreateNewGroupDto newGroupDto)
        {
            // Connect to the server and attempt to create a new group
            var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}/Groups/createNewGroup", newGroupDto);

            // If IsSuccessStatusCode is true, then the group was successfully created
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var groupResponse = JsonConvert.DeserializeObject<GroupModel>(content);
                return groupResponse;
            }

            // Else - there was an exception in the server and we want to fail the group creation attempt
            // and return the exception message to the user.
            var errorContent = await response.Content.ReadAsStringAsync();
            var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
            throw new Exception(errorResponse.Message);
        }

        public async Task<List<InviteModel>> ShowUserInvites(string username)
        {
            // Connect to the server and attempt to get the user invites
            var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Users/getUserInvites?username={username}");

            var invites = await response.Content.ReadFromJsonAsync<List<InviteModel>>();
            return invites ?? new List<InviteModel>(); // Return an empty list if the deserialization results in null
        }

        public async Task AnswerInviteAsync(AnswerInviteByUserDto answer)
        {
            // Connect to the server and attempt to accept the invite
            var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}/Invites/answerInvite", answer);

            // If there was an exception in the server and we want to fail the invite acceptance attempt
            // and return the exception message to the user.
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                throw new Exception(errorResponse.Message);
            }
        }
    }
}
