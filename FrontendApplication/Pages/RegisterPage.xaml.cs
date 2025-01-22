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

            var success = await _userService.RegisterUserAsync(email, username, password);
            if (success)
            {
                await DisplayAlert("Success", "User registered successfully!", "OK");

                // Navigate to LoginPage
                await Navigation.PushAsync(new LoginPage(_userService, _groupService, _paymentService));
            }
            else
            {
                await DisplayAlert("Error", "Failed to register user.", "OK");
            }
        }
    }
}