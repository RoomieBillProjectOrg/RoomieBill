using FrontendApplication.Services;

namespace FrontendApplication.Pages
{
    public partial class RegisterPage : ContentPage
    {
        private readonly UserServiceApi _userService;

        public RegisterPage(UserServiceApi userService)
        {
            InitializeComponent();
            _userService = userService;
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            var email = EmailEntry.Text;
            var username = UsernameEntry.Text;
            var password = PasswordEntry.Text;

            var success = await _userService.RegisterUserAsync(email, username, password);
            if (success)
            {
                await DisplayAlert("Success", "User registered successfully!", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Failed to register user.", "OK");
            }
        }
    }
}