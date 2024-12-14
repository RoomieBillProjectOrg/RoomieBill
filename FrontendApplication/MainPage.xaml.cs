using FrontendApplication.Pages;

namespace FrontendApplication
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

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
            };
            layout.Children.Add(registerPageButton);

            Content = layout;
        }
    }
}