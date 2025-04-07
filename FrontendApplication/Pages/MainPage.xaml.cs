using FrontendApplication.Pages;
using FrontendApplication.Services;
using Plugin.Firebase.CloudMessaging;

namespace FrontendApplication
{
    public partial class MainPage : ContentPage
    {
        private readonly UserServiceApi _userService;
        private readonly GroupServiceApi _groupService;
        private readonly PaymentService _paymentService;
        private readonly UploadServiceApi _uploadService;
        public MainPage(UserServiceApi userService, GroupServiceApi groupService, PaymentService paymentService, UploadServiceApi uploadService)
        {
            InitializeComponent();
            _userService = userService;
            _groupService = groupService;
            _paymentService = paymentService;
            _uploadService = uploadService;
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {  
            await Navigation.PushAsync(new RegisterPage(_userService, _groupService, _paymentService));          
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            await Navigation.PushAsync(new LoginPage(_userService, _groupService, _paymentService, _uploadService));
        }

    }
}