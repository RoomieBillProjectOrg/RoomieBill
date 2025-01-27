using FrontendApplication.Models;
using FrontendApplication.Services;
using Plugin.Firebase.CloudMessaging;

namespace FrontendApplication.Pages
{
    public partial class RegisterPage : ContentPage
    {
        private readonly UserServiceApi _userService;
        private readonly GroupServiceApi _groupService;
        private readonly PaymentService _paymentService;
        private VerifiyCodeModel _verificationCode;

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
                VerifiyCodeModel _verificationCode = await _userService.VerifyEmailRegister(email);

                // Show verification section
                VerificationSection.IsVisible = true;
                await DisplayAlert("Success", "Verification code sent to your email. Please check your inbox.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private async void OnVerifyCodeClicked(object sender, EventArgs e)
        {
            var enteredCode = VerificationCodeEntry.Text;

            if (enteredCode == _verificationCode.VerifyCode)
            {
                try
                {
                    // Complete registration process
                    var email = EmailEntry.Text;
                    var username = UsernameEntry.Text;
                    var password = PasswordEntry.Text;
                    var firebaseToken = await GetUserFirebaseToken();

                    var success = await _userService.RegisterUserAsync(email, username, password, firebaseToken);

                    await DisplayAlert("Success", "Your account has been verified and registered successfully!", "OK");
                    await Navigation.PushAsync(new LoginPage(_userService, _groupService, _paymentService));
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", ex.Message, "OK");
                }
            }
            else
            {
                await DisplayAlert("Error", "Verification code is incorrect. Please try again.", "OK");
            }
        }

        private async Task<string> GetUserFirebaseToken()
        {
            await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
            return await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
        }
    }
}
