using Firebase.Messaging;
using FrontendApplication.Models;
using FrontendApplication.Services;
using Roomiebill.Server.DataAccessLayer.Dtos;

namespace FrontendApplication.Pages;

public partial class CreateGroupPage : ContentPage
{
    private readonly UserServiceApi _userService;
    private UserModel _user;

    public CreateGroupPage(UserServiceApi userService, UserModel user)
    {
        InitializeComponent();
        _userService = userService;
        _user = user;
    }

    private async void OnCreateGroupClicked(object sender, EventArgs e)
    {
        // Get user input
        string groupName = GroupNameEntry.Text;
        string membersInput = MembersEntry.Text;

        // Validate input
        if (string.IsNullOrWhiteSpace(groupName))
        {
            ErrorLabel.Text = "Group Name is required.";
            ErrorLabel.IsVisible = true;
            return;
        }

        // Create the DTO
        var newGroupDto = new CreateNewGroupDto
        {
            GroupName = groupName,
            AdminGroupUsername = _user.Username, // Assuming _user has a Username property
            GroupMembersUsernamesList = string.IsNullOrEmpty(membersInput)
                ? new List<string>()
                : membersInput.Split(',').Select(s => s.Trim()).ToList()
        };

        try
        {
            // Call backend service to create the group
            var newGroup = await _userService.CreateNewGroupAsync(newGroupDto);
            FirebaseMessaging.Instance.SubscribeToTopic($"Group_{newGroup.Id}");
            // If successful, navigate to another page or show a success message
            await DisplayAlert("Success", $"Group '{newGroup.GroupName}' created successfully!", "OK");

            // Nevigate back to home page
            await Navigation.PopAsync();

        }
        catch (Exception ex)
        {
            // If there was an error, display it
            ErrorLabel.Text = ex.Message;
            ErrorLabel.IsVisible = true;
        }
    }
}
