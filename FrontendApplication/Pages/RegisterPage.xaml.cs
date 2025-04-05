using CommunityToolkit.Maui.Views;
using FrontendApplication.Models;
using FrontendApplication.Popups;
using FrontendApplication.Services;
using Plugin.Firebase.CloudMessaging;
using ZXing;
using ZXing.Common;
using SkiaSharp;
using System.Runtime.InteropServices;


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
            var username = UsernameEntry.Text;
            var email = EmailEntry.Text;
            var password = PasswordEntry.Text;
            var confirmPassword = PasswordConfirmationEntry.Text;
            var bitLink = BitLinkEntry.Text;



            // Alert on empty fields
            if (string.IsNullOrWhiteSpace(username))
            {
                await DisplayAlert("Error", "Username cannot be empty.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(email))
            {
                await DisplayAlert("Error", "Email cannot be empty.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(password))
            {
                await DisplayAlert("Error", "Password cannot be empty.", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(confirmPassword))
            {
                await DisplayAlert("Error", "Please confirm your password.", "OK");
                return;
            }

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
                RegisterUserDto user = new RegisterUserDto()
                {
                    Username = username,
                    Password = password,
                    Email = email,
                    FirebaseToken = await GetUserFirebaseToken(),
                    BitLink = BitLinkEntry.Text
                };

                _verificationCode = await _userService.VerifyUserRegisterDetails(user);

                // Show the verification code popup
                var popup = new RegisterVerifyEmailPopup(_userService, _groupService, _paymentService, _verificationCode, user);
                var res = await this.ShowPopupAsync(popup);

                await DisplayAlert("Success", "Your account has been verified and registered successfully!", "OK");
                await Navigation.PushAsync(new LoginPage(_userService, _groupService, _paymentService));
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", ex.Message, "OK");
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
            try
            {
                // Open file picker to select an image
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    FileTypes = FilePickerFileType.Images,
                    PickerTitle = "Select a QR Code Image"
                });

                if (result != null)
                {
                    // Process the selected image
                    var qrCodeLink = await ExtractQRCodeLink(result.FullPath);

                    if (!string.IsNullOrEmpty(qrCodeLink))
                    {
                        BitLinkEntry.Text = qrCodeLink; // Set the extracted link to the BitLink field
                        await DisplayAlert("Success", "QR Code link extracted successfully!", "OK");
                    }
                    else
                    {
                        await DisplayAlert("Error", "Could not extract a valid link from the QR Code.", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }
        private async Task<string> ExtractQRCodeLink(string imagePath)
        {
            try
            {
                // Load the image
                using var stream = File.OpenRead(imagePath);
                using var bitmap = SKBitmap.Decode(stream);

                if (bitmap == null || bitmap.IsEmpty)
                    return null;

                // Get the image data
                int width = bitmap.Width;
                int height = bitmap.Height;

                // Create byte array with RGB values
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

                // Create RGB luminance source
                var luminanceSource = new RGBLuminanceSource(rgbBytes, width, height);
                var binaryBitmap = new BinaryBitmap(new HybridBinarizer(luminanceSource));

                // Set up the reader with options
                var reader = new MultiFormatReader();
                var hints = new Dictionary<DecodeHintType, object>
        {
            { DecodeHintType.POSSIBLE_FORMATS, new List<BarcodeFormat> { BarcodeFormat.QR_CODE } },
            { DecodeHintType.TRY_HARDER, true }
        };

                // Attempt to decode
                var result = reader.decode(binaryBitmap, hints);

                if (result != null)
                {
                    return result.Text;
                }

                return null;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to decode QR code: {ex.Message}", "OK");
                return null;
            }
        }
        private async Task<string> GetUserFirebaseToken()
        {
            await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
            return await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
        }
    }
}
