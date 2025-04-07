using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Maui.Views;
using FrontendApplication.Models;
using FrontendApplication.Pages;
using FrontendApplication.Services;
using Plugin.Firebase.CloudMessaging;

namespace FrontendApplication.Popups
{
    public partial class RegisterVerifyEmailPopup : Popup
    {
        private UserServiceApi _userService;
        private GroupServiceApi _groupService;
        private PaymentService _paymentService;
        private readonly UploadServiceApi _uploadService;
        private VerifiyCodeModel _verificationCode;
        private RegisterUserDto _user;

        public RegisterVerifyEmailPopup(UserServiceApi userService, GroupServiceApi groupService,
            PaymentService paymentService, VerifiyCodeModel verificationCode, RegisterUserDto user)
        {
            InitializeComponent();
            _userService = userService;
            _groupService = groupService;
            _paymentService = paymentService;
            _verificationCode = verificationCode;
            _user = user;
        }

        private async void OnVerifyCodeClicked(object sender, EventArgs e)
        {
            var enteredCode = VerificationCodeEntry.Text;

            if (enteredCode == _verificationCode.VerifyCode)
            {
                try
                {
                    var success = await _userService.RegisterUserAsync(_user);
                }
                catch (Exception ex)
                {
                    Close($"Error: {ex.Message}");
                    return;
                }
            }
            else
            {
                Close("Verification code is incorrect. Please try again.");
                return;
            }

            Close();
        }
    }
}
