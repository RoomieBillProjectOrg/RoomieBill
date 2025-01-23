using Firebase.Messaging;
using FrontendApplication.Services;
using Plugin.Firebase.CloudMessaging;

namespace FrontendApplication.Pages
{
    public partial class RegisterPage : ContentPage
    {
        private readonly UserServiceApi _userService;
        private readonly GroupServiceApi _groupService;

        public RegisterPage(UserServiceApi userService, GroupServiceApi groupService)
        {
            InitializeComponent();
            _userService = userService;
            _groupService = groupService;
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            var email = EmailEntry.Text;
            var username = UsernameEntry.Text;
            var password = PasswordEntry.Text;
            var firebaseToken = await GetUserFirebaseToken();

            try
            {
                // Try to register the user to the application using api call to the server.
                var success = await _userService.RegisterUserAsync(email, username, password, firebaseToken);

                await DisplayAlert("Success", "User registered successfully!", "OK");

                // Navigate to LoginPage
                await Navigation.PushAsync(new LoginPage(_userService, _groupService));
            }
            catch (Exception ex)
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
}