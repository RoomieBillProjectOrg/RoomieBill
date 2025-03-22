using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using FrontendApplication.Models;
using FrontendApplication.Popups;
using FrontendApplication.Services;
namespace FrontendApplication.Pages;

public partial class GroupViewPage : ContentPage
{
    private readonly UserServiceApi _userService;
    private readonly GroupServiceApi _groupService;
    private readonly PaymentService _paymentService;

    public ObservableCollection<UserModel> Members { get; set; } = new ObservableCollection<UserModel>();
    public ObservableCollection<DebtModel> ShameTable { get; set; } = new ObservableCollection<DebtModel>();
    public ObservableCollection<DebtModel> YourOwnsTable { get; set; } = new ObservableCollection<DebtModel>();

    public bool IsShameTableEmpty => ShameTable.Count == 0;
    public bool IsYourOwnsTableEmpty => YourOwnsTable.Count == 0;

    public GroupModel _group { get; set; }
    public UserModel _currentUser { get; }

    public GroupViewPage(UserServiceApi userService, GroupServiceApi groupService, PaymentService paymentService,
        GroupModel group, UserModel CurrentUser)
    {
        InitializeComponent();

        _userService = userService;
        _groupService = groupService;
        _paymentService = paymentService;
        _group = group;
        _currentUser = CurrentUser;
        BindingContext = this;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshPageDataAsync();
    }

    private async Task LoadGroupMembersAsync()
    {
        try
        {
            Members.Clear();
            foreach (var member in _group.Members)
            {
                Members.Add(member);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load group details: {ex.Message}", "OK");
        }
    }

    private async void OnAddRoomieClicked(object sender, EventArgs e)
    {
        // Open a popup for adding a roomie
        var popup = new AddRoomiePopup(_userService, _groupService, _paymentService, _currentUser); // Create a popup for adding roomie
        var result = await this.ShowPopupAsync(popup);

        if (result is null)
        {
            await DisplayAlert("Canceled", "No roomie was added.", "OK");
            return;
        }

        if (result is not null)
        {
            InviteToGroupByEmailDto invitedUser = new InviteToGroupByEmailDto
            {
                InviterUsername = _currentUser.Username,
                Email = (string)result,
                GroupId = _group.Id
            };

            try
            {
                await _groupService.InviteUserToGroupByEmailAsync(invitedUser);
                await DisplayAlert("Success", $"{(string)result} has been invited to the group!", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to invite roomie: {ex.Message}", "OK");
            }
        }
        else
        {
            await DisplayAlert("Canceled", "No roomie was invited.", "OK");
        }
    }

    private async Task LoadShameTableAsync()
    {
        try
        {
            ShameTable.Clear();
            // Fetch debts for the current user
            var debts = await _groupService.GetDebtsForUserAsync(_group.Id, _currentUser.Id);

            // Populate the ShameTable collection
            foreach (var debt in debts)
            {
                ShameTable.Add(debt);
            }

        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load shame table: {ex.Message}", "OK");
        }
    }

    private async Task LoadYourOwnsTableAsync()
    {
        try
        {
            // Clear existing data
            YourOwnsTable.Clear();

            // Fetch debts the current user owes
            //TODO: Change the user id to the current user id
            var debts = await _groupService.GetDebtsOwedByUserAsync(_group.Id, _currentUser.Id);

            // Populate the YourOwnsTable collection
            foreach (var debt in debts)
            {
                YourOwnsTable.Add(debt);
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Failed to load 'Your Owns' table: {ex.Message}", "OK");
        }
    }

    public Command OnMemberButtonClicked => new Command<UserModel>((member) =>
    {
        DisplayAlert("Member Clicked", $"{member.Username} button clicked", "OK");
    });

    private async void OnViewTransactionClicked(object sender, EventArgs e)
    {
        var popup = new ViewTransactionsPopup(_group, _groupService);
        await this.ShowPopupAsync(popup);
    }

    //add a new pop up window to add an expense
    private async void OnAddExpenseClicked(object sender, EventArgs e)
    {
        var popup = new AddExpensePopup(_group, _currentUser, _groupService);
        var res = await this.ShowPopupAsync(popup);
        await DisplayAlert("Expense", (string)res, "OK");
        if (res is string message && !message.StartsWith("Error"))
        {
            _group = await _groupService.GetGroup(_group.Id);
            await RefreshPageDataAsync();
        }
    }

    public Command<UserModel> OnMemberClicked => new Command<UserModel>((selectedMember) =>
    {
        if (selectedMember != null)
        {
            // Retrieve the ID of the selected member
            int userId = selectedMember.Id;

            // Example action: Show an alert with the user ID
            DisplayAlert("Member Clicked", $"Member ID: {userId}, Username: {selectedMember.Username}", "OK");

            // Add additional logic here (e.g., navigation or action)
        }
    });

    public Command<DebtModel> OnShameTableItemTapped => new Command<DebtModel>(async (selectedItem) =>
    {
        if (selectedItem != null)
        {
            // Example action: Show an alert with the user ID
            SnoozeToPayDto snoozeToPayDto = new SnoozeToPayDto
            {
                snoozeToUsername = selectedItem.debtor.Username,
                snoozeInfo = $"{selectedItem.creditor.Username} wants you to pay {selectedItem.amount} NIS."
            };
            await _groupService.SnoozeMember(snoozeToPayDto);
            // await DisplayAlert("Someone owns you money :)", $"{selectedItem.debtor.Username} owns you money. Lets snooze!", "OK");
            await DisplayAlert("Snoozed successfully", $"{selectedItem.debtor.Username} snoozed!", "OK");
            // Add your logic here (e.g., navigation or additional functionality)
        }
    });

    public Command<DebtModel> OnYourOwnsItemTapped => new Command<DebtModel>(async (selectedItem) =>
    {
        if (selectedItem != null)
        {
            // Example action: Show an alert with the user ID
            await DisplayAlert("Debt", $"You owe {selectedItem.creditor.Username} {selectedItem.amount} NIS.", "OK");

            // Add your logic here (e.g., navigation or additional functionality)
            await Navigation.PushAsync(new PaymentPage(selectedItem, _group, _paymentService));
        }
    });

    private async Task RefreshPageDataAsync()
    {
        await LoadGroupMembersAsync();
        await LoadShameTableAsync();
        await LoadYourOwnsTableAsync();
        OnPropertyChanged(nameof(IsShameTableEmpty));
        OnPropertyChanged(nameof(IsYourOwnsTableEmpty));
    }

    private async void OnHomePageButtonClicked(object sender, EventArgs e)
    {
        // Navigate to UserHomePage
        await Navigation.PushAsync(new UserHomePage(_userService, _groupService, _paymentService, _currentUser));
    }

}
