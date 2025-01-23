using Firebase.Messaging;
using FrontendApplication.Models;
using FrontendApplication.Services;
using System.Collections.ObjectModel;

namespace FrontendApplication.Pages;

public partial class UserHomePage : ContentPage
{
    private readonly UserServiceApi _userService;
    private readonly GroupServiceApi _groupService;

    public UserModel User { get; set; }
    public ObservableCollection<GroupModel> Groups { get; set; }

    public UserHomePage(UserServiceApi userService, GroupServiceApi groupService, UserModel user)
    {
        InitializeComponent();

        _userService = userService;
        _groupService = groupService;
        User = user;
        Groups = new ObservableCollection<GroupModel>();
        BindingContext = this;
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
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", "An error occurred while fetching user groups.\nError: "+ex.Message, "OK");
            Groups.Clear();
        }
        InitializeUI();
    }
    private void InitializeUI()
    {
        if (Groups.Count == 0)
        {
            // Show the "No groups were found" message
            var noGroupsLabel = new Label
            {
                Text = "No groups were found for user.",
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
            Content = noGroupsLabel;
        }
        else
        {
            // Create a StackLayout to hold the buttons
            var buttonLayout = new StackLayout
            {
                Padding = new Thickness(10),
                Spacing = 10
            };

            // Add buttons for each group
            foreach (var group in Groups)
            {
                var button = new Button
                {
                    Text = group.GroupName
                };
                button.Clicked += (sender, args) => OnGroupButtonClicked(group);
                buttonLayout.Children.Add(button);
            }

            Content = buttonLayout;
        }
    }

    // Show the menu when the 3-dot button is clicked
    // Command that is bound to the ToolbarItem
    public Command ShowMenuCommand => new Command(OnShowMenu);

    // Method that is triggered when the toolbar item is clicked
    private async void OnShowMenu()
    {
        // Show the options in an ActionSheet (a menu with multiple options)
        string action = await DisplayActionSheet("Select an option", "Cancel", null, "Log Out", "Update User Details", "Add Group", "Invites");

        // Navigate based on the selected action
        switch (action)
        {
            case "Log Out":
                OnLogOut();
                break;
            case "Update User Details":
                OnUpdateUserDetails();
                break;
            case "Add Group":
                OnAddGroup();
                break;
            case "Invites":
                onInvites();
                break;
            default:
                break;
        }
    }

    // Handle group button click
    private async void OnGroupButtonClicked(GroupModel group)
    {
        await Navigation.PushAsync(new GroupViewPage(_userService, _groupService, group));
    }

    // Methods for menu actions

    private async void OnLogOut()
    {
        try
        {
            // Call the server to logout the user
            await _userService.LogoutUserAsync(User.Username);

            await DisplayAlert("Success", "User logged out successfully!", "OK");

            // Navigate to the main page
            await Navigation.PushAsync(new MainPage(_userService, _groupService));
        }
        catch (Exception ex)
        {
            // If the server returns error, display the error message to the user.
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnUpdateUserDetails()
    {
        // Navigate to a password updater page
        await Navigation.PushAsync(new UpdateUserDetailsPage(_userService, User));
    }

    private async void OnAddGroup()
    {
        // Navigate to a group creation page
        await Navigation.PushAsync(new CreateGroupPage(_userService, User));
    }

    private async void onInvites()
    {
        // Navigate to a page that shows the invites
        await Navigation.PushAsync(new InvitesPage(_userService, User.Username));
    }
}
