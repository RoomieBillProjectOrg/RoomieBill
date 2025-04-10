﻿using Firebase.Auth;
using FrontendApplication.Services;

namespace FrontendApplication
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();
            MainPage = new NavigationPage(new MainPage(
                serviceProvider.GetService<UserServiceApi>(),
                serviceProvider.GetService<GroupServiceApi>(),
                serviceProvider.GetService<PaymentService>(),
                serviceProvider.GetService<UploadServiceApi>()));
        }
    }
}