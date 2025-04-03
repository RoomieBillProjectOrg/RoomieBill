using Firebase.Messaging;
using FrontendApplication.Models;
using FrontendApplication.Services;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;

namespace FrontendApplication.Pages;

public partial class UserHomePage : ContentPage
{
    private readonly UserServiceApi _userService;
    private readonly GroupServiceApi _groupService;
    private readonly PaymentService _paymentService;

    public UserModel User { get; set; }
    public ObservableCollection<GroupModel> Groups { get; set; }

    public UserHomePage(UserServiceApi userService, GroupServiceApi groupService, PaymentService paymentService, UserModel user)
    {
        InitializeComponent();

        _userService = userService;
        _groupService = groupService;
        _paymentService = paymentService;
        User = user;
        Groups = new ObservableCollection<GroupModel>();

        BindingContext = this;

        // Dynamically set the title
        Title = $"Welcome, {user.Username}!";
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            // Fetch the group list
            var groups = await _groupService.GetUserGroups(User);
            Groups.Clear();
            foreach (var group in groups)
            {
                Groups.Add(group);
            }

            // Update UI based on whether groups exist
            UpdateUI();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"An error occurred while fetching user groups: {ex.Message}", "OK");
            Groups.Clear();
            UpdateUI(); // Ensure UI is updated even if there's an error
        }
    }

    private void UpdateUI()
    {
        // If there are groups, show the group list; otherwise, show the "No groups" message
        if (Groups.Count > 0)
        {
            // Display the CollectionView for groups
            GroupListView.IsVisible = true;
            NoGroupsMessage.IsVisible = false;
        }
        else
        {
            // Display the "No groups found" message
            GroupListView.IsVisible = false;
            NoGroupsMessage.IsVisible = true;
        }
    }

    // Show the menu when the toolbar item is clicked
    public Command ShowMenuCommand => new Command(OnShowMenu);

    private async void OnShowMenu()
    {
        string action = await DisplayActionSheet("Select an option", "Cancel", null, "Add Group", "Update User Password", "Invites", "Log Out");

        switch (action)
        {
            case "Add Group":
                await OnAddGroup();
                break;
            case "Update User Password":
                await OnUpdateUserDetails();
                break;
            case "Invites":
                await OnInvites();
                break;
            case "Log Out":
                await OnLogOut();
                break;
        }
    }

    private async void OnGroupButtonClicked(object sender, EventArgs e)
    {
        if (sender is Button button && button.CommandParameter is GroupModel group)
        {
            var reminderService = App.Current.Handler.MauiContext.Services.GetRequiredService<PaymentReminderService>();
            await Navigation.PushAsync(new GroupViewPage(_userService, _groupService, _paymentService, reminderService, group, User));
        }
    }

    private async Task OnLogOut()
    {
        try
        {
            await _userService.LogoutUserAsync(User.Username);
            await DisplayAlert("Success", "User logged out successfully!", "OK");
            
            // Navigate to the main page
            await Navigation.PushAsync(new MainPage(_userService, _groupService, _paymentService));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async Task OnUpdateUserDetails()
    {
        await Navigation.PushAsync(new UpdateUserDetailsPage(_userService, _groupService, _paymentService, User));
    }

    private async Task OnAddGroup()
    {
        await Navigation.PushAsync(new CreateGroupPage(_userService, _groupService, _paymentService, User));
    }

    private async Task OnInvites()
    {
        await Navigation.PushAsync(new InvitesPage(_userService, _groupService, _paymentService, User));
    }
}
