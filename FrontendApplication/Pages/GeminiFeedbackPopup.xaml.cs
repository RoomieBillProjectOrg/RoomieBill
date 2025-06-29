using Firebase.Messaging;
using FrontendApplication.Models;
using FrontendApplication.Services;
using Roomiebill.Server.DataAccessLayer.Dtos;

namespace FrontendApplication.Pages;

public partial class GeminiFeedbackPopup : ContentView
{
    public GeminiFeedbackPopup()
    {
        InitializeComponent();
    }

    public void Show(string feedback)
    {
        FeedbackLabel.Text = feedback;
        IsVisible = true;
    }

    private void OnCloseClicked(object sender, EventArgs e)
    {
        IsVisible = false;
    }
}