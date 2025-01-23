using FrontendApplication.Models;
using FrontendApplication.Services;

namespace FrontendApplication.Pages;

public partial class UpdateUserDetailsPage : ContentPage
{
    private readonly UserServiceApi _userService;
    private readonly GroupServiceApi _groupService;
    private UserModel _user;

    public UpdateUserDetailsPage(UserServiceApi userService, GroupServiceApi groupServiceApi, UserModel user)
    {
        InitializeComponent();
        _userService = userService;
        _groupService = groupServiceApi;
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
            UserModel userNewPass = await _userService.UpdateUserPasswordAsync(updatePasswordDto);

            await DisplayAlert("Success", "The password was updated successfuly.", "OK");

            // Navigate back to the user home page
            await Navigation.PushAsync(new UserHomePage(_userService, _groupService, userNewPass));
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }
}