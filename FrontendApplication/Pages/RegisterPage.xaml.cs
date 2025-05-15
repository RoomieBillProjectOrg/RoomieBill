using CommunityToolkit.Maui.Views;
using FrontendApplication.Models;
using FrontendApplication.Popups;
using FrontendApplication.Services;
using Plugin.Firebase.CloudMessaging;
using ZXing;
using ZXing.Common;
using SkiaSharp;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace FrontendApplication.Pages
{
    public partial class RegisterPage : ContentPage
    {
        private readonly UserServiceApi _userService;
        private readonly GroupServiceApi _groupService;
        private readonly PaymentService _paymentService;
        private readonly UploadServiceApi _uploadService;
        private VerifiyCodeModel _verificationCode;
        private bool _isLoading;
        private readonly Regex _emailRegex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

        public RegisterPage(UserServiceApi userService, GroupServiceApi groupService, PaymentService paymentService)
        {
            InitializeComponent();
            _userService = userService;
            _groupService = groupService;
            _paymentService = paymentService;
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
                    RegisterButton.IsEnabled = false;
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
                    RegisterButton.IsEnabled = true;
                });
                _isLoading = false;
            }
            catch
            {
                // Fail silently if UI update fails
            }
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            if (_isLoading) return;

            var username = UsernameEntry?.Text?.Trim();
            var email = EmailEntry?.Text?.Trim();
            var password = PasswordEntry?.Text;
            var confirmPassword = PasswordConfirmationEntry?.Text;
            var bitLink = BitLinkEntry?.Text?.Trim();

            try
            {
                // Validate all inputs
                if (string.IsNullOrWhiteSpace(username))
                {
                    await DisplayAlert("Validation Error", "Username cannot be empty.", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    await DisplayAlert("Validation Error", "Email cannot be empty.", "OK");
                    return;
                }

                if (!_emailRegex.IsMatch(email))
                {
                    await DisplayAlert("Validation Error", "Please enter a valid email address.", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    await DisplayAlert("Validation Error", "Password cannot be empty.", "OK");
                    return;
                }

                if (string.IsNullOrWhiteSpace(confirmPassword))
                {
                    await DisplayAlert("Validation Error", "Please confirm your password.", "OK");
                    return;
                }

                if (password != confirmPassword)
                {
                    await DisplayAlert("Validation Error", "Passwords do not match.", "OK");
                    return;
                }

                if (!NotRobotCheckBox.IsChecked)
                {
                    await DisplayAlert("Validation Error", "Please confirm that you're not a bot.", "OK");
                    return;
                }

                ShowLoading("Creating your account...");

                string firebaseToken = null;
                try
                {
                    firebaseToken = await GetUserFirebaseToken();
                }
                catch (Exception ex)
                {
                    // Log the error but continue - user can still register without push notifications
                    System.Diagnostics.Debug.WriteLine($"Failed to get Firebase token: {ex.Message}");
                }

                RegisterUserDto user = new RegisterUserDto()
                {
                    Username = username,
                    Password = password,
                    Email = email,
                    FirebaseToken = firebaseToken,
                    BitLink = bitLink
                };

                _verificationCode = await _userService.VerifyUserRegisterDetails(user);

                // Show the verification code popup
                var popup = new RegisterVerifyEmailPopup(_userService, _groupService, _paymentService, _verificationCode, user);
                var res = await this.ShowPopupAsync(popup);

                await DisplayAlert("Success", "Your account has been verified and registered successfully!", "OK");
                await Navigation.PushAsync(new LoginPage(_userService, _groupService, _paymentService, _uploadService));
            }
            catch (HttpRequestException)
            {
                await DisplayAlert("Connection Error", 
                    "Unable to connect to the server. Please check your internet connection and try again.", 
                    "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Registration Failed", ex.Message, "OK");
            }
            finally
            {
                HideLoading();
            }
        }

        private async void OnBitLinkInfoClicked(object sender, EventArgs e)
        {
            string message =
                "🔗 **How to Get Your Bit Payment Link:**\n\n" +
                "1️⃣ Open the **Bit** app.\n\n" +
                "2️⃣ Tap **More**.\n\n" +
                "3️⃣ Select **Permanent QR to receive money**.\n\n" +
                "4️⃣ Tap the QR code — the link is now copied to your clipboard.\n\n" +
                "💡 You can paste it into the BitLink field above.";

            await DisplayAlert("How to Find Your Bit Link", message, "Got it!");
        }

        private async void OnUploadQRCodeClicked(object sender, EventArgs e)
        {
            if (_isLoading) return;

            try
            {
                ShowLoading("Selecting QR code...");

                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Select a QR Code Image"
                });

                if (result != null)
                {
                    ShowLoading("Processing QR code...");
                    var qrCodeLink = await ExtractQRCodeLink(result.FullPath);

                    if (!string.IsNullOrEmpty(qrCodeLink))
                    {
                        BitLinkEntry.Text = qrCodeLink;
                        await DisplayAlert("Success", "QR Code link extracted successfully!", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "Could not detect a valid QR code in the image.", "OK");
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                await DisplayAlert("Permission Error", 
                    "Unable to access your photos. Please grant permission and try again.", 
                    "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", 
                    $"Failed to process QR code: {ex.Message}", 
                    "OK");
            }
            finally
            {
                HideLoading();
            }
        }

        private async Task<string> ExtractQRCodeLink(string imagePath)
        {
            try
            {
                if (!File.Exists(imagePath))
                {
                    throw new FileNotFoundException("The selected image file was not found.");
                }

                using var stream = File.OpenRead(imagePath);
                using var bitmap = SKBitmap.Decode(stream);

                if (bitmap == null || bitmap.IsEmpty)
                {
                    throw new Exception("Unable to load the image. The file might be corrupted.");
                }

                int width = bitmap.Width;
                int height = bitmap.Height;
                byte[] rgbBytes = new byte[width * height * 3];
                int index = 0;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        SKColor pixel = bitmap.GetPixel(x, y);
                        rgbBytes[index++] = pixel.Red;
                        rgbBytes[index++] = pixel.Green;
                        rgbBytes[index++] = pixel.Blue;
                    }
                }

                var luminanceSource = new RGBLuminanceSource(rgbBytes, width, height);
                var binaryBitmap = new BinaryBitmap(new HybridBinarizer(luminanceSource));
                var reader = new MultiFormatReader();
                var hints = new Dictionary<DecodeHintType, object>
                {
                    { DecodeHintType.POSSIBLE_FORMATS, new List<BarcodeFormat> { BarcodeFormat.QR_CODE } },
                    { DecodeHintType.TRY_HARDER, true }
                };

                var result = reader.decode(binaryBitmap, hints);
                return result?.Text;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"QR code extraction error: {ex.Message}");
                throw new Exception("Failed to process the QR code image.", ex);
            }
        }

        private async Task<string> GetUserFirebaseToken()
        {
            try
            {
                try
                {
                    await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
                }
                catch
                {
                    throw new Exception("Firebase messaging is not properly configured.");
                }

                var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new Exception("Failed to retrieve Firebase token.");
                }

                return token;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Firebase token error: {ex.Message}");
                throw new Exception("Failed to initialize push notifications.", ex);
            }
        }
    }
}
