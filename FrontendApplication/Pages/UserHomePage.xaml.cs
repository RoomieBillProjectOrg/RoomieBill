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
        _userService = userServiceApi;
        User = user;
        Groups = user.GroupsUserIsMemberAt ?? new List<GroupModel>();

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

        // Set up the 3-dots menu in the upper right corner
        ToolbarItems.Add(new ToolbarItem
        {
            Text = "•••",
            Command = new Command(ShowMenu)
        });
    }

    // Show the menu when the 3-dot button is clicked
    private async void ShowMenu()
    {
        // Create a new MenuFlyout to hold the options
        var menu = new MenuFlyout();

        // Add items to the menu
        menu.Add(new MenuFlyoutItem { Text = "Log Out", Command = new Command(OnLogOut) });
        menu.Add(new MenuFlyoutItem { Text = "Update User Details", Command = new Command(OnUpdateUserDetails) });
        menu.Add(new MenuFlyoutItem { Text = "Add Group", Command = new Command(OnAddGroup) });

        // Show the menu at the current toolbar item
        await DisplayActionSheet("Menu", "Cancel", null, "Log Out", "Update User Details", "Add Group");
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

    private void OnAddGroup()
    {
        // TODO: Navigate to a new page
        // await Navigation.PushAsync(new AddNewGroupPage(_userService));
    }

    // Handle group button click
    private void OnGroupButtonClicked(GroupModel group)
    {
        // TODO: Navigate to a new page based on the group
        // await Navigation.PushAsync(new GroupPage(_userService));
    }
}