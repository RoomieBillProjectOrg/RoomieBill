using System.Net.Http.Json;
using FrontendApplication.Models;
using FrontendApplication.Services.Interfaces;
using Newtonsoft.Json;
using Roomiebill.Server.DataAccessLayer.Dtos;

namespace FrontendApplication.Services
{
    public class UserServiceApi : IUserServiceApi
    {
        private readonly HttpClient _httpClient;

        public UserServiceApi(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("DefaultClient");
        }

        public async Task<bool> RegisterUserAsync(RegisterUserDto user)
        {
            try
            {
                if (user == null)
                {
                    throw new ArgumentNullException(nameof(user), "Registration details are missing.");
                }

                if (string.IsNullOrWhiteSpace(user.Username))
                {
                    throw new ArgumentException("Username is required.", nameof(user.Username));
                }

                if (string.IsNullOrWhiteSpace(user.Password))
                {
                    throw new ArgumentException("Password is required.", nameof(user.Password));
                }

                var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}/Users/register", user);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var userResponse = JsonConvert.DeserializeObject<UserModel>(content);
                    return true;
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                throw new Exception($"Registration failed: {errorResponse?.Message ?? "Unknown error"}");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Failed to connect to the registration service. Please check your internet connection.", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Failed to process the registration response. Please try again.", ex);
            }
            catch (ArgumentException ex)
            {
                throw new Exception($"Invalid registration details: {ex.Message}");
            }
            catch (Exception ex) when (ex.Message.Contains("Registration failed:"))
            {
                throw; // Re-throw registration-specific errors as they are already well-formatted
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred during registration. Please try again.", ex);
            }
        }

        public async Task<UserModel> LoginUserAsync(string username, string password, string firebaseToken)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    throw new ArgumentException("Username is required.", nameof(username));
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    throw new ArgumentException("Password is required.", nameof(password));
                }

                LoginDto user = new LoginDto()
                {
                    username = username,
                    password = password,
                    firebaseToken = firebaseToken
                };

                var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}/Users/login", user);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var userResponse = JsonConvert.DeserializeObject<UserModel>(content);
                    return userResponse ?? throw new Exception("Failed to retrieve user data after login.");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                throw new Exception($"Login failed: {errorResponse?.Message ?? "Invalid username or password"}");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Failed to connect to the login service. Please check your internet connection.", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Failed to process the login response. Please try again.", ex);
            }
            catch (ArgumentException ex)
            {
                throw new Exception($"Invalid login details: {ex.Message}");
            }
            catch (Exception ex) when (ex.Message.Contains("Login failed:"))
            {
                throw; // Re-throw login-specific errors as they are already well-formatted
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred during login. Please try again.", ex);
            }
        }

        public async Task LogoutUserAsync(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    throw new ArgumentException("Username is required for logout.", nameof(username));
                }

                var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}/Users/logout", username);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                    throw new Exception($"Logout failed: {errorResponse?.Message ?? "Unknown error"}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Failed to connect to the logout service. Your session might still be active.", ex);
            }
            catch (ArgumentException ex)
            {
                throw new Exception($"Invalid logout request: {ex.Message}");
            }
            catch (Exception ex) when (ex.Message.Contains("Logout failed:"))
            {
                throw; // Re-throw logout-specific errors as they are already well-formatted
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred during logout. Your session might still be active.", ex);
            }
        }

        public async Task<UserModel> UpdateUserPasswordAsync(UpdatePasswordDto updatePasswordDto)
        {
            try
            {
                if (updatePasswordDto == null)
                {
                    throw new ArgumentNullException(nameof(updatePasswordDto), "Password update details are missing.");
                }

                if (string.IsNullOrWhiteSpace(updatePasswordDto.NewPassword))
                {
                    throw new ArgumentException("New password cannot be empty.", nameof(updatePasswordDto.NewPassword));
                }

                var response = await _httpClient.PutAsJsonAsync($"{_httpClient.BaseAddress}/Users/updatePassword", updatePasswordDto);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var userResponse = JsonConvert.DeserializeObject<UserModel>(content);
                    return userResponse ?? throw new Exception("Failed to retrieve user data after password update.");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                throw new Exception($"Password update failed: {errorResponse?.Message ?? "Unknown error"}");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Failed to connect to the password update service. Please check your internet connection.", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Failed to process the password update response. Please try again.", ex);
            }
            catch (ArgumentException ex)
            {
                throw new Exception($"Invalid password update details: {ex.Message}");
            }
            catch (Exception ex) when (ex.Message.Contains("Password update failed:"))
            {
                throw; // Re-throw password update-specific errors as they are already well-formatted
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while updating the password. Please try again.", ex);
            }
        }

        public async Task<GroupModel> CreateNewGroupAsync(CreateNewGroupDto newGroupDto)
        {
            try
            {
                if (newGroupDto == null)
                {
                    throw new ArgumentNullException(nameof(newGroupDto), "Group creation details are missing.");
                }

                if (string.IsNullOrWhiteSpace(newGroupDto.GroupName))
                {
                    throw new ArgumentException("Group name is required.", nameof(newGroupDto.GroupName));
                }

                if (string.IsNullOrWhiteSpace(newGroupDto.AdminGroupUsername))
                {
                    throw new ArgumentException("Group admin username is required.", nameof(newGroupDto.AdminGroupUsername));
                }

                var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}/Groups/createNewGroup", newGroupDto);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var groupResponse = JsonConvert.DeserializeObject<GroupModel>(content);
                    return groupResponse ?? throw new Exception("Failed to retrieve group data after creation.");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                throw new Exception($"Failed to create group: {errorResponse?.Message ?? "Unknown error"}");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Failed to connect to the group creation service. Please check your internet connection.", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Failed to process the group creation response. Please try again.", ex);
            }
            catch (ArgumentException ex)
            {
                throw new Exception($"Invalid group creation details: {ex.Message}");
            }
            catch (Exception ex) when (ex.Message.Contains("Failed to create group:"))
            {
                throw; // Re-throw group creation-specific errors as they are already well-formatted
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while creating the group. Please try again.", ex);
            }
        }

        public async Task<List<InviteModel>> ShowUserInvites(string username)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(username))
                {
                    throw new ArgumentException("Username is required to fetch invites.", nameof(username));
                }

                var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Users/getUserInvites?username={username}");

                if (response.IsSuccessStatusCode)
                {
                    var invites = await response.Content.ReadFromJsonAsync<List<InviteModel>>();
                    return invites ?? new List<InviteModel>();
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                throw new Exception($"Failed to fetch invites: {errorResponse?.Message ?? "Unknown error"}");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Failed to fetch invites. Please check your internet connection.", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Failed to process the invites data. Please try again.", ex);
            }
            catch (ArgumentException ex)
            {
                throw new Exception($"Invalid request: {ex.Message}");
            }
            catch (Exception ex) when (ex.Message.Contains("Failed to fetch invites:"))
            {
                throw; // Re-throw invite-specific errors as they are already well-formatted
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while fetching invites. Please try again.", ex);
            }
        }

        public async Task AnswerInviteAsync(AnswerInviteByUserDto answer)
        {
            try
            {
                if (answer == null)
                {
                    throw new ArgumentNullException(nameof(answer), "Invite response details are missing.");
                }

                var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}/Invites/answerInvite", answer);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                    throw new Exception($"Failed to respond to invite: {errorResponse?.Message ?? "Unknown error"}");
                }
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Failed to send invite response. Please check your internet connection.", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Failed to process the invite response. Please try again.", ex);
            }
            catch (ArgumentException ex)
            {
                throw new Exception($"Invalid invite response: {ex.Message}");
            }
            catch (Exception ex) when (ex.Message.Contains("Failed to respond to invite:"))
            {
                throw; // Re-throw invite-specific errors as they are already well-formatted
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred while responding to the invite. Please try again.", ex);
            }
        }

        public async Task<VerifiyCodeModel> VerifyUserRegisterDetails(RegisterUserDto user)
        {
            try
            {
                if (user == null)
                {
                    throw new ArgumentNullException(nameof(user), "Registration details are missing for verification.");
                }

                if (string.IsNullOrWhiteSpace(user.Username))
                {
                    throw new ArgumentException("Username is required for verification.", nameof(user.Username));
                }

                var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}/Users/verifyUserRegisterDetails", user);

                if (response.IsSuccessStatusCode)
                {
                    var verificationResponse = await response.Content.ReadFromJsonAsync<VerifiyCodeModel>();
                    return verificationResponse ?? throw new Exception("Failed to retrieve verification code.");
                }

                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                throw new Exception($"Verification failed: {errorResponse?.Message ?? "Unknown error"}");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception("Failed to connect to the verification service. Please check your internet connection.", ex);
            }
            catch (JsonException ex)
            {
                throw new Exception("Failed to process the verification response. Please try again.", ex);
            }
            catch (ArgumentException ex)
            {
                throw new Exception($"Invalid verification details: {ex.Message}");
            }
            catch (Exception ex) when (ex.Message.Contains("Verification failed:"))
            {
                throw; // Re-throw verification-specific errors as they are already well-formatted
            }
            catch (Exception ex)
            {
                throw new Exception("An unexpected error occurred during verification. Please try again.", ex);
            }
        }
    }
}
