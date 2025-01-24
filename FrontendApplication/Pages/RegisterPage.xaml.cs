using FrontendApplication.Services;

namespace FrontendApplication.Pages
{
    public partial class RegisterPage : ContentPage
    {
        private readonly UserServiceApi _userService;
        private readonly GroupServiceApi _groupService;
        private readonly PaymentService _paymentService;

        public RegisterPage(UserServiceApi userService, GroupServiceApi groupService, PaymentService paymentService)
        {
            InitializeComponent();
            _userService = userService;
            _groupService = groupService;
            _paymentService = paymentService;
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            var email = EmailEntry.Text;
            var username = UsernameEntry.Text;
            var password = PasswordEntry.Text;
            var confirmPassword = PasswordConfirmationEntry.Text;

            if (password != confirmPassword)
            {
                await DisplayAlert("Error", "Passwords do not match.", "OK");
                return;
            }

            if (!NotRobotCheckBox.IsChecked)
            {
                await DisplayAlert("Error", "Please confirm that you're not a bot.", "OK");
                return;
            }

            try
            {
                // Try to register the user to the application using api call to the server.
                var user = await _userService.RegisterUserAsync(email, username, password);
                await DisplayAlert("Success", "User registered successfully!", "OK");

                // Navigate to LoginPage
                await Navigation.PushAsync(new LoginPage(_userService, _groupService, _paymentService));
            }
            catch (Exception ex)
            {
                // If the server returns error, display the error message to the user.
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}
