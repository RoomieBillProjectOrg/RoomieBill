using FrontendApplication.Models;
using FrontendApplication.Services;

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

        UserModel user = null;

        try
        {
            // Try to login the user using api call to the server.
            user = await _userService.LoginUserAsync(username, password);

            await DisplayAlert("Success", "User logged in successfully!", "OK");

            // Navigate to UserHomePage
            await Navigation.PushAsync(new UserHomePage(_userService, _groupService, _paymentService, user));
        }
        catch(Exception ex)
        {
            // If the server returns error, display the error message to the user.
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}