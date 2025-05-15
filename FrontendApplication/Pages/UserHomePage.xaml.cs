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
    private readonly UploadServiceApi _uploadService;

    public UserModel User { get; set; }
    public ObservableCollection<GroupModel> Groups { get; set; }

    public UserHomePage(UserServiceApi userService, GroupServiceApi groupService, PaymentService paymentService, UploadServiceApi uploadService, UserModel user)
    {
        InitializeComponent();

        _userService = userService;
        _groupService = groupService;
        _paymentService = paymentService;
        _uploadService = uploadService;
        User = user;
        Groups = new ObservableCollection<GroupModel>();

        BindingContext = this;

        // Dynamically set the title
        Title = $"Welcome, {user.Username}!";
    }

    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            ShowLoading("Loading your groups...");
            await InitializeAsync();
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "Something went wrong while loading the page. Please try again.", "OK");
        }
        finally
        {
            HideLoading();
        }
    }

    private async Task InitializeAsync()
    {
        try
        {
            if (User == null)
            {
                throw new InvalidOperationException("User session not found. Please log in again.");
            }

            // Fetch the group list
            var groups = await _groupService.GetUserGroups(User);
            
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Groups.Clear();
                foreach (var group in groups)
                {
                    Groups.Add(group);
                }
                UpdateUI();
            });
        }
        catch (HttpRequestException)
        {
            await DisplayAlert("Connection Error", 
                "We couldn't load your groups. Please check your internet connection and try again.", "OK");
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Groups.Clear();
                UpdateUI();
            });
        }
        catch (Exception ex)
        {
            string message = "Unable to load your groups at this time. ";
            if (ex is InvalidOperationException)
            {
                message = ex.Message;
            }
            await DisplayAlert("Error", message + "Please try again later.", "OK");
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Groups.Clear();
                UpdateUI();
            });
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
        try
        {
            if (sender is not Button button || button.CommandParameter is not GroupModel group)
            {
                throw new InvalidOperationException("Invalid group selection.");
            }

            ShowLoading("Opening group...");
            var reminderService = App.Current.Handler.MauiContext.Services.GetRequiredService<PaymentReminderService>();
            
            if (reminderService == null)
            {
                throw new InvalidOperationException("Required services are not available.");
            }

            await Navigation.PushAsync(new GroupViewPage(
                _userService, _groupService, _paymentService, _uploadService, reminderService, group, User));
        }
        catch (InvalidOperationException ex)
        {
            await DisplayAlert("Error", "Unable to open the group: " + ex.Message, "OK");
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "Something went wrong while opening the group. Please try again.", "OK");
        }
        finally
        {
            HideLoading();
        }
    }

    private async Task OnLogOut()
    {
        try
        {
            bool confirm = await DisplayAlert("Confirm Logout", 
                "Are you sure you want to log out?", "Yes", "No");

            if (!confirm) return;

            ShowLoading("Logging out...");
            await _userService.LogoutUserAsync(User.Username);
            
            // Navigate to the main page
            await Navigation.PushAsync(new MainPage(_userService, _groupService, _paymentService, _uploadService));
        }
        catch (HttpRequestException)
        {
            await DisplayAlert("Connection Error", 
                "Unable to complete logout. Please check your internet connection.", "OK");
        }
        catch (Exception)
        {
            await DisplayAlert("Error", 
                "Something went wrong during logout. Your session might still be active.", "OK");
        }
        finally
        {
            HideLoading();
        }
    }

    private async Task OnUpdateUserDetails()
    {
        try
        {
            ShowLoading("Loading settings...");
            await Navigation.PushAsync(new UpdateUserDetailsPage(
                _userService, _groupService, _paymentService, _uploadService, User));
        }
        catch (Exception)
        {
            await DisplayAlert("Error", 
                "Unable to open settings page. Please try again.", "OK");
        }
        finally
        {
            HideLoading();
        }
    }

    private async Task OnAddGroup()
    {
        try
        {
            ShowLoading("Opening create group page...");
            await Navigation.PushAsync(new CreateGroupPage(
                _userService, _groupService, _paymentService, _uploadService, User));
        }
        catch (Exception)
        {
            await DisplayAlert("Error", 
                "Unable to open group creation page. Please try again.", "OK");
        }
        finally
        {
            HideLoading();
        }
    }

    private async Task OnInvites()
    {
        try
        {
            ShowLoading("Loading invites...");
            await Navigation.PushAsync(new InvitesPage(
                _userService, _groupService, _paymentService, _uploadService, User));
        }
        catch (Exception)
        {
            await DisplayAlert("Error", 
                "Unable to load invites. Please try again.", "OK");
        }
        finally
        {
            HideLoading();
        }
    }

    private void ShowLoading(string message)
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LoadingOverlay.IsVisible = true;
                LoadingIndicator.IsRunning = true;
                LoadingIndicator.IsVisible = true;
                LoadingLabel.Text = message;
                LoadingLabel.IsVisible = true;
            });
        }
        catch
        {
            // Fail silently if UI update fails
        }
    }

    private void HideLoading()
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LoadingOverlay.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                LoadingLabel.IsVisible = false;
            });
        }
        catch
        {
            // Fail silently if UI update fails
        }
    }
}
