using FrontendApplication.Pages;
using Plugin.Firebase.CloudMessaging;

namespace FrontendApplication
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();

            // Add your welcome message and NBA photos here
            StackLayout layout = new StackLayout
            {
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };

            Label welcomeLabel = new Label
            {
                Text = "Welcome to Roomiebill!",
                FontSize = 18,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Colors.Black // Adjust color as needed
            };
            layout.Children.Add(welcomeLabel);

            // Add a button to navigate to NBAPlayerSearchPage
            Button registerPageButton = new Button
            {
                Text = "Register User",
                BackgroundColor = Colors.Blue, // Adjust button color
                TextColor = Colors.White // Adjust text color
            };
            registerPageButton.Clicked += async (sender, e) =>
            {
                //await Shell.Current.GoToAsync(nameof(RegisterPage));
                await Navigation.PushAsync(new FrontendApplication.Pages.RegisterPage());
                await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
                var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
                Console.WriteLine($"FCM token: {token}");
            };
            layout.Children.Add(registerPageButton);

            Content = layout;
        }
    }
}