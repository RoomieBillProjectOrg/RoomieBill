using FrontendApplication.Pages;
using FrontendApplication.Services;
using Plugin.Firebase.CloudMessaging;

namespace FrontendApplication
{
    public partial class MainPage : ContentPage
    {
        private readonly UserServiceApi _userService;
        private readonly GroupServiceApi _groupService;
        public MainPage(UserServiceApi userService, GroupServiceApi groupService)
        {
            InitializeComponent();
            _userService = userService;
            _groupService = groupService;
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
                Text = "Welcome to RoomieBill!",
                FontSize = 18,
                HorizontalTextAlignment = TextAlignment.Center,
                TextColor = Colors.Black // Adjust color as needed
            };
            layout.Children.Add(welcomeLabel);

            // Add a button to navigate to RegisterPage
            Button registerPageButton = new Button
            {
                Text = "Register User",
                BackgroundColor = Colors.Blue, // Adjust button color
                TextColor = Colors.White // Adjust text color
            };
            registerPageButton.Clicked += async (sender, e) =>
            {
                //await Shell.Current.GoToAsync(nameof(RegisterPage));
                await Navigation.PushAsync(new RegisterPage(_userService));
                await CrossFirebaseCloudMessaging.Current.CheckIfValidAsync();
                var token = await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
                Console.WriteLine($"FCM token: {token}");
            };
            layout.Children.Add(registerPageButton);

            // Add a button to navigate to LoginPage
            Button loginPageButton = new Button
            {
                Text = "Login User",
                BackgroundColor = Colors.Blue, // Adjust button color
                TextColor = Colors.White // Adjust text color
            };
            loginPageButton.Clicked += async (sender, e) =>
            {
                //await Shell.Current.GoToAsync(nameof(LoginPage));
                await Navigation.PushAsync(new LoginPage(_userService, _groupService));
            };
            layout.Children.Add(loginPageButton);

            Content = layout;
        }
    }
}