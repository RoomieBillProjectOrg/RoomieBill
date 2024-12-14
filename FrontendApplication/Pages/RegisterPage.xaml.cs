using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrontendApplication.Models;
using FrontendApplication.Services;

namespace FrontendApplication.Pages
{
    public partial class RegisterPage : ContentPage
    {
        private readonly UserServiceApi _userService;

        public RegisterPage()
        {
            InitializeComponent();
            _userService = new UserServiceApi();
        }

        private async void OnRegisterClicked(object sender, EventArgs e)
        {
            var email = EmailEntry.Text;
            var username = UsernameEntry.Text;
            var password = PasswordEntry.Text;

            var success = await _userService.RegisterUserAsync(email, username, password);

            if (success)
            {
                await DisplayAlert("Success", "User registered successfully!", "OK");
            }
            else
            {
                await DisplayAlert("Error", "Failed to register user.", "OK");
            }
        }
    }
}