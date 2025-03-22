using CommunityToolkit.Maui.Views;
using FrontendApplication.Models;
using FrontendApplication.Services;

namespace FrontendApplication.Pages
{
    public partial class AddRoomiePopup : Popup
    {
        private readonly UserServiceApi _userService;
        private readonly GroupServiceApi _groupService;
        private readonly PaymentService _paymentService;
        private readonly UserModel _user;
        
        public AddRoomiePopup(UserServiceApi userServiceApi, GroupServiceApi groupServiceApi, PaymentService paymentService, UserModel user)
        {
            _userService = userServiceApi;
            _groupService = groupServiceApi;
            _paymentService = paymentService;
            _user = user;
            InitializeComponent();
    }

        private void OnInviteClicked(object sender, EventArgs e)
        {
            var email = EmailEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(email))
            {
                Application.Current.MainPage.DisplayAlert("Error", "Email cannot be empty.", "OK");
                return;
            }

            // Return the email as a result
            Close(email);
        }

        private void OnCancelClicked(object sender, EventArgs e)
        {
            Close(null); // Close the popup without returning any result
        }
    }
}
