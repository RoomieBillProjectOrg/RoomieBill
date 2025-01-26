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
            var invitedUsername = UsernameEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(invitedUsername))
            {
                Application.Current.MainPage.DisplayAlert("Error", "Username cannot be empty.", "OK");
                return;
            }

            // Return the invited username as a result
            Close(invitedUsername);
        }

        private void OnCancelClicked(object sender, EventArgs e)
        {
            Close(null); // Close the popup without returning any result
        }
    }
}