using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Crashlytics.Internal.Model;
using Firebase.Messaging;
using FrontendApplication.Models;
using FrontendApplication.Services;
using Roomiebill.FrontendApplication.Models;

namespace FrontendApplication.Pages
{
    public partial class InvitesPage : ContentPage
    {
        private readonly UserServiceApi _userService;
        private readonly GroupServiceApi _groupService;
        private readonly PaymentService _paymentService;
        private readonly UploadServiceApi _uploadService;
        private readonly UserModel _user;
        private ObservableCollection<InviteModel> _invitations;

        public InvitesPage(UserServiceApi userService, GroupServiceApi groupService, PaymentService paymentService, UploadServiceApi uploadService, UserModel user)
        {
            InitializeComponent();

            _userService = userService;
            _groupService = groupService;
            _paymentService = paymentService;
            _uploadService = uploadService;
            _user = user;

            _invitations = new ObservableCollection<InviteModel>();

            var collectionView = new CollectionView
            {
                ItemsSource = _invitations,
                ItemTemplate = new DataTemplate(() =>
                {
                    var grid = new Grid
                    {
                        ColumnDefinitions = { new ColumnDefinition(), new ColumnDefinition(), new ColumnDefinition() },
                        Padding = new Thickness(10),
                        Margin = new Thickness(5),
                        BackgroundColor = Colors.LightGray
                    };

                    var nameLabel = new Label { VerticalOptions = LayoutOptions.Center };
                    nameLabel.SetBinding(Label.TextProperty, "Group.GroupName");

                    var acceptButton = new Button
                    {
                        Text = "\u2713", // Checkmark
                        BackgroundColor = Colors.Green,
                        TextColor = Colors.White
                    };
                    acceptButton.SetBinding(Button.CommandParameterProperty, new Binding("."));
                    acceptButton.Clicked += AcceptButton_Clicked;

                    var rejectButton = new Button
                    {
                        Text = "\u2717", // Cross
                        BackgroundColor = Colors.Red,
                        TextColor = Colors.White
                    };
                    rejectButton.SetBinding(Button.CommandParameterProperty, new Binding("."));
                    rejectButton.Clicked += RejectButton_Clicked;

                    grid.Children.Add(nameLabel);
                    Grid.SetColumn(nameLabel, 0);
                    grid.Children.Add(acceptButton);
                    Grid.SetColumn(acceptButton, 1);
                    grid.Children.Add(rejectButton);
                    Grid.SetColumn(rejectButton, 2);

                    return new ContentView { Content = grid };
                })
            };

            Content = new StackLayout { Children = { collectionView } };

            LoadInvites(user.Username);
        }

        private async void LoadInvites(string username)
        {
            try
            {
                var response = await _userService.ShowUserInvites(username);
                foreach (var invite in response)
                {
                    if (invite.Status == StatusModel.Pending)
                        _invitations.Add(invite);
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async void AcceptButton_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is InviteModel invite)
            {
                AnswerInviteByUserDto answer = new AnswerInviteByUserDto(invite.Id, invite.Invited.Username, true);
                await _userService.AnswerInviteAsync(answer);
                _invitations.Remove(invite);
                FirebaseMessaging.Instance.SubscribeToTopic($"Group_{invite.Group.Id}");
                await DisplayAlert("Accepted", $"You accepted an invite from {invite.Inviter.Username}", "OK");
            }
        }

        private async void RejectButton_Clicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.CommandParameter is InviteModel invite)
            {
                AnswerInviteByUserDto answer = new AnswerInviteByUserDto(invite.Id, invite.Invited.Username, false);
                await _userService.AnswerInviteAsync(answer);
                _invitations.Remove(invite);
                await DisplayAlert("Rejected", $"You rejected an invite from {invite.Inviter.Username}", "OK");
            }
        }

        private async void OnHomePageButtonClicked(object sender, EventArgs e)
        {
            // Navigate to UserHomePage
            await Navigation.PushAsync(new UserHomePage(_userService, _groupService, _paymentService, _uploadService, _user));
        }
    }

}