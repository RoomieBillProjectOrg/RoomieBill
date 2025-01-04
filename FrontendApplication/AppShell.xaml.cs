using FrontendApplication.Pages;

namespace FrontendApplication
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            // Register the route for navigation
            Routing.RegisterRoute(nameof(RegisterPage), typeof(RegisterPage));
        }
    }
}