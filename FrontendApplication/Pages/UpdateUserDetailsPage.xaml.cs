using FrontendApplication.Models;
using FrontendApplication.Services;

namespace FrontendApplication.Pages;

public partial class UpdateUserDetailsPage : ContentPage
{
    private readonly UserServiceApi _userService;
    private readonly GroupServiceApi _groupService;
    private readonly PaymentService _paymentService;
    private readonly UploadServiceApi _uploadService;
    private UserModel _user;

    public UpdateUserDetailsPage(UserServiceApi userService, GroupServiceApi groupServiceApi, PaymentService paymentService, UploadServiceApi uploadService, UserModel user)
    {
        InitializeComponent();
        _userService = userService;
        _groupService = groupServiceApi;
        _paymentService = paymentService;
        _uploadService = uploadService;
        _user = user;
    }

    private async void OnUpdatePasswordClicked(object sender, EventArgs e)
    {
        var oldPassword = OldPasswordEntry.Text;
        var newPassword = NewPasswordEntry.Text;
        var verifyNewPassword = VeriftNewPasswordEntry.Text;

        // Validate if the new password matches the confirmation
        if (newPassword != verifyNewPassword)
        {
            await DisplayAlert("Error", "New password and confirmation do not match.", "OK");

            // Clear the new password fields
            NewPasswordEntry.Text = string.Empty;
            VeriftNewPasswordEntry.Text = string.Empty;
            return;
        }

        // Check if the new password is the same as the old one.
        if (newPassword == oldPassword)
        {
            await DisplayAlert("Error", "New password cannot be the same as the old password.", "OK");
            NewPasswordEntry.Text = string.Empty;
            VeriftNewPasswordEntry.Text = string.Empty;
            return;
        }

        var updatePasswordDto = new UpdatePasswordDto
        {
            Username = _user.Username,
            OldPassword = oldPassword,
            NewPassword = newPassword,
            VerifyNewPassword = verifyNewPassword
        };

        try
        {
            // Attempt to update the password
            UserModel updatedUser = await _userService.UpdateUserPasswordAsync(updatePasswordDto);

            await DisplayAlert("Success", "Password updated successfully.", "OK");

            // Navigate to the user's home page after success
            await Navigation.PushAsync(new UserHomePage(_userService, _groupService, _paymentService, _uploadService, updatedUser));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnHomePageButtonClicked(object sender, EventArgs e)
    {
        // Navigate to UserHomePage
        await Navigation.PushAsync(new UserHomePage(_userService, _groupService, _paymentService, _uploadService, _user));
    }
}
