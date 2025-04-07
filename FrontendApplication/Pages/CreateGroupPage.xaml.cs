using Firebase.Messaging;
using FrontendApplication.Models;
using FrontendApplication.Services;
using Roomiebill.Server.DataAccessLayer.Dtos;

namespace FrontendApplication.Pages;

public partial class CreateGroupPage : ContentPage
{
    private readonly UserServiceApi _userService;
    private readonly GroupServiceApi _groupService;
    private readonly PaymentService _paymentService;
    private readonly UploadServiceApi _uploadService;
    
    private UserModel _user;

    public CreateGroupPage(UserServiceApi userService, GroupServiceApi groupServiceApi, PaymentService paymentService, UploadServiceApi uploadService, UserModel user)
    {
        InitializeComponent();
        _userService = userService;
        _groupService = groupServiceApi;
        _paymentService = paymentService;
        _uploadService = uploadService;
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
            GroupMembersEmailsList = string.IsNullOrEmpty(membersInput)
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

    private async void OnHomePageButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new UserHomePage(_userService, _groupService, _paymentService, _uploadService, _user));
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}
