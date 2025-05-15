using System;
using System.Net.Http.Json;
using FrontendApplication.Models;
using FrontendApplication.Services.Interfaces;
using Newtonsoft.Json;

namespace FrontendApplication.Services;

public class GroupServiceApi : IGroupServiceApi
{
    private readonly HttpClient _httpClient;

    public GroupServiceApi(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("DefaultClient");
    }

    public async Task<List<GroupModel>> GetUserGroups(UserModel user)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Groups/getUserGroups?UserId={user.Id}");
            response.EnsureSuccessStatusCode();
            
            var groups = await response.Content.ReadFromJsonAsync<List<GroupModel>>();
            return groups ?? new List<GroupModel>();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Failed to fetch user groups. Please check your internet connection.", ex);
        }
        catch (JsonException ex)
        {
            throw new Exception("Failed to process the groups data. Please try again later.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while fetching user groups.", ex);
        }
    }

    public async Task<GroupModel> GetGroup(int groupId)
    {   
        try
        {
            var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Groups/getGroup?id={groupId}");
            response.EnsureSuccessStatusCode();
            
            var group = await response.Content.ReadFromJsonAsync<GroupModel>();
            return group ?? new GroupModel();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Failed to fetch group with ID {groupId}. Please check your internet connection.", ex);
        }
        catch (JsonException ex)
        {
            throw new Exception("Failed to process the group data. Please try again later.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception($"An unexpected error occurred while fetching group {groupId}.", ex);
        }
    }

    public async Task<List<DebtModel>> GetDebtsForUserAsync(int groupId, int userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Groups/getDebtsForUser?groupId={groupId}&userId={userId}");
            response.EnsureSuccessStatusCode();
            
            var debts = await response.Content.ReadFromJsonAsync<List<DebtModel>>();
            return debts ?? new List<DebtModel>();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Failed to fetch user debts. Please check your internet connection.", ex);
        }
        catch (JsonException ex)
        {
            throw new Exception("Failed to process the debt data. Please try again later.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while fetching user debts.", ex);
        }
    }

    public async Task<List<DebtModel>> GetDebtsOwedByUserAsync(int groupId, int userId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Groups/getDebtsOwedByUser?groupId={groupId}&userId={userId}");
            response.EnsureSuccessStatusCode();
            
            var debts = await response.Content.ReadFromJsonAsync<List<DebtModel>>();
            return debts ?? new List<DebtModel>();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Failed to fetch debts owed. Please check your internet connection.", ex);
        }
        catch (JsonException ex)
        {
            throw new Exception("Failed to process the debts data. Please try again later.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while fetching debts owed.", ex);
        }
    }


    public async Task addExpenseAsync(ExpenseModel expenseDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}/Groups/addExpense", expenseDto);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                throw new Exception($"Failed to add expense: {errorResponse?.Message ?? "Unknown error"}");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Failed to add expense. Please check your internet connection.", ex);
        }
        catch (JsonException ex)
        {
            throw new Exception("Failed to process the expense data. Please try again later.", ex);
        }
        catch (Exception ex) when (ex.Message.Contains("Failed to add expense:"))
        {
            throw; // Re-throw the custom error message
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while adding the expense.", ex);
        }
    }

    public async Task InviteUserToGroupByEmailAsync(InviteToGroupByEmailDto inviteDto)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}/Invites/inviteUserToGroupByEmail", inviteDto);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                throw new Exception($"Failed to send invitation: {errorResponse?.Message ?? "Unknown error"}");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Failed to send invitation. Please check your internet connection.", ex);
        }
        catch (JsonException ex)
        {
            throw new Exception("Failed to process the invitation data. Please try again later.", ex);
        }
        catch (Exception ex) when (ex.Message.Contains("Failed to send invitation:"))
        {
            throw; // Re-throw the custom error message
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while sending the invitation.", ex);
        }
    }

    //GetExpensesForGroupAsync
    public async Task<List<ExpenseModel>> GetExpensesForGroupAsync(int groupId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Groups/getExpensesForGroup?groupId={groupId}");
            response.EnsureSuccessStatusCode();
            
            var expenses = await response.Content.ReadFromJsonAsync<List<ExpenseModel>>();
            return expenses ?? new List<ExpenseModel>();
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Failed to fetch expenses for group {groupId}. Please check your internet connection.", ex);
        }
        catch (JsonException ex)
        {
            throw new Exception("Failed to process the expenses data. Please try again later.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while fetching group expenses.", ex);
        }
    }

    public async Task SnoozeMember(SnoozeToPayDto snoozeInfo)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"{_httpClient.BaseAddress}/Groups/snoozeMemberToPay", snoozeInfo);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                throw new Exception($"Failed to snooze payment: {errorResponse?.Message ?? "Unknown error"}");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Failed to snooze payment. Please check your internet connection.", ex);
        }
        catch (JsonException ex)
        {
            throw new Exception("Failed to process the snooze request. Please try again later.", ex);
        }
        catch (Exception ex) when (ex.Message.Contains("Failed to snooze payment:"))
        {
            throw; // Re-throw the custom error message
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while snoozing the payment.", ex);
        }
    }

    public async Task<string> GetGeiminiResponseForExpenses(int groupId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{_httpClient.BaseAddress}/Groups/getGeiminiResponseForExpenses?groupId={groupId}");
            response.EnsureSuccessStatusCode();
            
            var geminiAnswer = await response.Content.ReadAsStringAsync();
            return geminiAnswer;
        }
        catch (HttpRequestException ex)
        {
            throw new Exception("Failed to get expense analysis. Please check your internet connection.", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("An unexpected error occurred while analyzing expenses.", ex);
        }
    }

    public async Task DeleteGroupAsync(int groupId, int userId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/Groups/deleteGroup?groupId={groupId}&userId={userId}", null);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                throw new Exception($"Failed to delete group: {errorResponse?.Message ?? "Unknown error"}");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Failed to delete group {groupId}. Please check your internet connection.", ex);
        }
        catch (JsonException ex)
        {
            throw new Exception("Failed to process the delete request. Please try again later.", ex);
        }
        catch (Exception ex) when (ex.Message.Contains("Failed to delete group:"))
        {
            throw; // Re-throw the custom error message
        }
        catch (Exception ex)
        {
            throw new Exception($"An unexpected error occurred while deleting group {groupId}.", ex);
        }
    }

    public async Task ExitGroupAsync(int userId, int groupId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"{_httpClient.BaseAddress}/Groups/exitGroup?userId={userId}&groupId={groupId}", null);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                var errorResponse = JsonConvert.DeserializeObject<ErrorResponse>(errorContent);
                throw new Exception($"Failed to exit group: {errorResponse?.Message ?? "Unknown error"}");
            }
        }
        catch (HttpRequestException ex)
        {
            throw new Exception($"Failed to exit group {groupId}. Please check your internet connection.", ex);
        }
        catch (JsonException ex)
        {
            throw new Exception("Failed to process the exit request. Please try again later.", ex);
        }
        catch (Exception ex) when (ex.Message.Contains("Failed to exit group:"))
        {
            throw; // Re-throw the custom error message
        }
        catch (Exception ex)
        {
            throw new Exception($"An unexpected error occurred while exiting group {groupId}.", ex);
        }
    }
}
