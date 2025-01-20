using FrontendApplication.Models;
using FrontendApplication.Services;

namespace FrontendApplication.Pages;

public partial class UserHomePage : ContentPage
{
    private readonly UserServiceApi _userService;

    public UserModel User { get; set; }
    public List<GroupModel> Groups { get; set; }

    public UserHomePage(UserServiceApi userServiceApi, UserModel user)
    {
        InitializeComponent();

        _userService = userServiceApi;
        User = user;
        Groups = user.GroupsUserIsMemberAt ?? new List<GroupModel>();

        BindingContext = this;

        // Initialize the UI based on the groups
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
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.CenterAndExpand
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
    private void OnGroupButtonClicked(GroupModel group)
    {
        // TODO: Navigate to a new page based on the group
        // await Navigation.PushAsync(new GroupPage(_userService));
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
            await Navigation.PushAsync(new MainPage(_userService));
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
        await Navigation.PushAsync(new InvitesPage(_userService, User));
    }
}