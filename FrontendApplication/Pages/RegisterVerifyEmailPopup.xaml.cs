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
        private readonly UserServiceApi _userService;
        private readonly GroupServiceApi _groupService;
        private readonly PaymentService _paymentService;
        private readonly UploadServiceApi _uploadService;
        private readonly VerifiyCodeModel _verificationCode;
        private readonly RegisterUserDto _user;
        private bool _isLoading;

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

        private void ShowLoading(string message)
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LoadingIndicator.IsRunning = true;
                    LoadingOverlay.IsVisible = true;
                    LoadingMessage.Text = message;
                    LoadingMessage.IsVisible = true;
                    VerifyButton.IsEnabled = false;
                    VerificationCodeEntry.IsEnabled = false;
                });
                _isLoading = true;
            }
            catch
            {
                // Fail silently if UI update fails
            }
        }

        private void HideLoading()
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    LoadingIndicator.IsRunning = false;
                    LoadingOverlay.IsVisible = false;
                    LoadingMessage.IsVisible = false;
                    VerifyButton.IsEnabled = true;
                    VerificationCodeEntry.IsEnabled = true;
                });
                _isLoading = false;
            }
            catch
            {
                // Fail silently if UI update fails
            }
        }

        private async void OnVerifyCodeClicked(object sender, EventArgs e)
        {
            if (_isLoading) return;

            var enteredCode = VerificationCodeEntry?.Text?.Trim();

            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(enteredCode))
                {
                    Close("Error: Please enter the verification code.");
                    return;
                }

                // Compare codes
                if (enteredCode != _verificationCode.VerifyCode)
                {
                    Close("Error: The verification code is incorrect. Please check your email and try again.");
                    return;
                }

                ShowLoading("Creating your account...");

                try
                {
                    var success = await _userService.RegisterUserAsync(_user);
                    if (!success)
                    {
                        throw new Exception("Registration failed for unknown reason.");
                    }
                }
                catch (HttpRequestException)
                {
                    Close("Error: Unable to complete registration. Please check your internet connection.");
                    return;
                }
                catch (Exception ex)
                {
                    Close($"Error: {ex.Message}");
                    return;
                }

                Close(); // Success - close without error message
            }
            catch (Exception ex)
            {
                Close($"Error: An unexpected error occurred: {ex.Message}");
            }
            finally
            {
                HideLoading();
            }
        }

        private void CleanupResources()
        {
            try
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    VerificationCodeEntry.Text = string.Empty;
                    HideLoading();
                });
            }
            catch
            {
                // Fail silently if cleanup fails
            }
        }

        public new void Close(string result = null)
        {
            CleanupResources();
            base.Close(result);
        }
    }
}
