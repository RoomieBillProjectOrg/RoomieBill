using System.Net.Http;
using FrontendApplication.Models;
using FrontendApplication.Services;
using Plugin.Firebase.CloudMessaging;

namespace FrontendApplication.Pages;

public partial class LoginPage : ContentPage
{
    private readonly UserServiceApi _userService;
    private readonly GroupServiceApi _groupService;
    private readonly PaymentService _paymentService;
    private readonly UploadServiceApi _uploadService;
    private bool _isLoading;

    public LoginPage(UserServiceApi userService, GroupServiceApi groupService, PaymentService paymentService, UploadServiceApi uploadService)
    {
        InitializeComponent();
        _userService = userService;
        _groupService = groupService;
        _paymentService = paymentService;
        _uploadService = uploadService;
    }

    private void ShowLoading(string message)
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LoadingIndicator.IsRunning = true;
                LoadingOverlay.IsVisible = true;
                LoadingMessage.Text = message;
                LoadingMessage.IsVisible = true;
                LoginButton.IsEnabled = false;
            });
            _isLoading = true;
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
                LoadingIndicator.IsRunning = false;
                LoadingOverlay.IsVisible = false;
                LoadingMessage.IsVisible = false;
                LoginButton.IsEnabled = true;
            });
            _isLoading = false;
        }
        catch
        {
            // Fail silently if UI update fails
        }
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        if (_isLoading) return;

        var username = UsernameEntry?.Text?.Trim();
        var password = PasswordEntry?.Text;

        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            await DisplayAlert("Validation Error", "Please enter both username and password.", "OK");
            return;
        }

        ShowLoading("Signing in...");
        string firebaseToken = null;
        UserModel user = null;

        try
        {
            // Try to get Firebase token but don't block login if it fails
            try
            {
                firebaseToken = await GetUserFirebaseToken();
            }
            catch (Exception ex)
            {
                // Log the error but continue - user can still login without push notifications
                System.Diagnostics.Debug.WriteLine($"Failed to get Firebase token: {ex.Message}");
            }

            // Attempt to login
            try
            {
                user = await _userService.LoginUserAsync(username, password, firebaseToken);

                // Check if password is expired (older than 3 months)
                if (user.LastPasswordChangedDate < DateTime.UtcNow.AddMonths(-3))
                {
                    await DisplayAlert("Password Expired",
                        "Your password has not been updated in the last 3 months. Please update your password.",
                        "OK"
                    );

                    await Navigation.PushAsync(new UpdateUserDetailsPage(_userService, _groupService, _paymentService, _uploadService, user));
                }
                else
                {
                    await Navigation.PushAsync(new UserHomePage(_userService, _groupService, _paymentService, _uploadService, user));
                }
            }
            catch (HttpRequestException)
            {
                await DisplayAlert("Connection Error", 
                    "Unable to connect to the server. Please check your internet connection and try again.", 
                    "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Login Failed", ex.Message, "OK");
            }
        }
        finally
        {
            HideLoading();
        }
    }

    private async Task<string> GetUserFirebaseToken()
    {
        try
        {
            await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
            // If CheckIfValidAsync() completes without throwing an exception, Firebase is valid
            var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                throw new Exception("Failed to retrieve Firebase token.");
            }
            return token;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Firebase token error: {ex.Message}");
            throw new Exception("Failed to initialize push notifications.", ex);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        try
        {
            // Clear any existing values
            UsernameEntry.Text = string.Empty;
            PasswordEntry.Text = string.Empty;
            
            // Set focus to username entry
            MainThread.BeginInvokeOnMainThread(() => UsernameEntry?.Focus());
        }
        catch
        {
            // Fail silently if UI update fails
        }
    }
}
