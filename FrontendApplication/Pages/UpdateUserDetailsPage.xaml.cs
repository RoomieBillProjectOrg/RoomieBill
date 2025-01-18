using FrontendApplication.Models;
using FrontendApplication.Services;

namespace FrontendApplication.Pages;

public partial class UpdateUserDetailsPage : ContentPage
{
    private readonly UserServiceApi _userService;
    private UserModel _user;

    public UpdateUserDetailsPage(UserServiceApi userService, UserModel user)
    {
        InitializeComponent();
        _userService = userService;
        _user = user;
    }

    private async void OnUpdatePasswordClicked(object sender, EventArgs e)
    {
        var oldPassword = OldPasswordEntry.Text;
        var newPassword = NewPasswordEntry.Text;
        var verifyNewPassword = VeriftNewPasswordEntry.Text;

        // Check if the new password and verify new password are the same
        if (newPassword != verifyNewPassword)
        {
            await DisplayAlert("Error", "New password and verify new password do not match.", "OK");

            // Delete the new password and verify new password entries
            NewPasswordEntry.Text = "";
            VeriftNewPasswordEntry.Text = "";

            return;
        }

        UpdatePasswordDto updatePasswordDto = new UpdatePasswordDto()
        {
            Username = _user.Username,
            OldPassword = oldPassword,
            NewPassword = newPassword,
            VerifyNewPassword = verifyNewPassword
        };

        try
        {
            await _userService.UpdateUserPasswordAsync(updatePasswordDto);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}