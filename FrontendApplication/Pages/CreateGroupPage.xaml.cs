using FrontendApplication.Models;
using FrontendApplication.Services;
using Roomiebill.Server.DataAccessLayer.Dtos;

namespace FrontendApplication.Pages;

public partial class CreateGroupPage : ContentPage
{
    private readonly UserServiceApi _userService;
    private readonly UserModel _user;

    public CreateGroupPage(UserServiceApi userService, UserModel user)
    {
        InitializeComponent();
        _userService = userService;
        _user = user;
    }

    private async void OnCreateGroupClicked(object sender, EventArgs e)
    {
        // Get user input
        string groupName = GroupNameEntry.Text?.Trim();
        string membersInput = MembersEntry.Text?.Trim();

        // Validate input
        if (string.IsNullOrWhiteSpace(groupName))
        {
            ShowError("Group Name is required.");
            return;
        }

        // Prepare the DTO
        var newGroupDto = new CreateNewGroupDto
        {
            GroupName = groupName,
            AdminGroupUsername = _user.Username,
            GroupMembersUsernamesList = string.IsNullOrEmpty(membersInput)
                ? new List<string>()
                : membersInput.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList()
        };

        try
        {
            // Call backend service to create the group
            var newGroup = await _userService.CreateNewGroupAsync(newGroupDto);

            // Success feedback
            await DisplayAlert("Success", $"Group '{newGroup.GroupName}' created successfully!", "OK");

            // Navigate to the Group Details Page (you can replace this with another page if needed)
            await Navigation.PushAsync(new GroupViewPage(_userService, null, newGroup, _user));
        }
        catch (Exception ex)
        {
            // Show error message
            ShowError($"Failed to create group: {ex.Message}");
        }
    }

    private void ShowError(string message)
    {
        ErrorLabel.Text = message;
        ErrorLabel.IsVisible = true;
    }
}
