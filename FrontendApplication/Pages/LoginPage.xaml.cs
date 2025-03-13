using FrontendApplication.Models;
using FrontendApplication.Services;
using Plugin.Firebase.CloudMessaging;

namespace FrontendApplication.Pages;

public partial class LoginPage : ContentPage
{
    private readonly UserServiceApi _userService;
    private readonly GroupServiceApi _groupService;
    private readonly PaymentService _paymentService;

    public LoginPage(UserServiceApi userService, GroupServiceApi groupService, PaymentService paymentService)
	{
		InitializeComponent();
        _userService = userService;
        _groupService = groupService;
        _paymentService = paymentService;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var username = UsernameEntry.Text;
        var password = PasswordEntry.Text;
        var firebaseToken = await GetUserFirebaseToken();

        UserModel user = null;

        try
        {
            // Try to login the user using api call to the server.
            user = await _userService.LoginUserAsync(username, password, firebaseToken);

            // Check if user’s password is older than 3 months
            if(user.LastPasswordChangedDate < DateTime.UtcNow.AddMonths(-3))
            {
                await DisplayAlert("Password Expired",
                    "Your password has not been updated in the last 3 months. Please update your password.",
                    "OK"
                );

                // 3. Navigate to UpdateUserDetailsPage (instead of the home page).
                await Navigation.PushAsync(new UpdateUserDetailsPage(_userService, _groupService, _paymentService, user));
            }
            else
            {
                await DisplayAlert("Success", "User logged in successfully!", "OK");

                // Navigate to UserHomePage
                await Navigation.PushAsync(new UserHomePage(_userService, _groupService, _paymentService, user));
            }
        }
        catch(Exception ex)
        {
            // If the server returns error, display the error message to the user.
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async Task<string> GetUserFirebaseToken()
        {
            await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
            return await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
        }
}