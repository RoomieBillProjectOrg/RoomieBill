using CommunityToolkit.Maui.Views;

namespace FrontendApplication.Pages
{
    public partial class AddRoomiePopup : Popup
    {
        public AddRoomiePopup()
        {
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
