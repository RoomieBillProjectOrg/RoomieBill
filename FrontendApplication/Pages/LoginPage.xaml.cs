using FrontendApplication.Services;

namespace FrontendApplication.Pages;

public partial class LoginPage : ContentPage
{
    private readonly UserServiceApi _userService;

    public LoginPage()
	{
		InitializeComponent();
        _userService = new UserServiceApi();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var username = UsernameEntry.Text;
        var password = PasswordEntry.Text;

        var success = await _userService.LoginUserAsync(username, password);

        if (success)
        {
            await DisplayAlert("Success", "User logged in successfully!", "OK");
        }
        else
        {
            await DisplayAlert("Error", "Failed to logged in user.", "OK");
        }
    }
}