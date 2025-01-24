using FrontendApplication.Pages;
using FrontendApplication.Services;
using Plugin.Firebase.CloudMessaging;

namespace FrontendApplication
{
    public partial class MainPage : ContentPage
    {
        private readonly UserServiceApi _userService;
        private readonly GroupServiceApi _groupService;
        public MainPage(UserServiceApi userService, GroupServiceApi groupService)
        {
            InitializeComponent();
            _userService = userService;
            _groupService = groupService;
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new RegisterPage(_userService, _groupService));
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            //await Shell.Current.GoToAsync(nameof(LoginPage));
            await Navigation.PushAsync(new LoginPage(_userService, _groupService));
        }
    }
}