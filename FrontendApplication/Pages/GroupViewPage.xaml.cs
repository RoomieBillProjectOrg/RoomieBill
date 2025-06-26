using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Views;
using FrontendApplication.Models;
using FrontendApplication.Popups;
using FrontendApplication.Services;
using FrontendApplication.Models;
using System.Windows.Input;
namespace FrontendApplication.Pages;

public partial class GroupViewPage : ContentPage
{
    private readonly UserServiceApi _userService;
    private readonly GroupServiceApi _groupService;
    private readonly PaymentService _paymentService;
    private readonly PaymentReminderService _reminderService;
    private readonly UploadServiceApi _uploadService;


    public ObservableCollection<UserModel> Members { get; set; } = new ObservableCollection<UserModel>();
    public ObservableCollection<DebtModel> ShameTable { get; set; } = new ObservableCollection<DebtModel>();
    public ObservableCollection<DebtModel> YourOwnsTable { get; set; } = new ObservableCollection<DebtModel>();

    public bool IsShameTableEmpty => ShameTable.Count == 0;
    public bool IsYourOwnsTableEmpty => YourOwnsTable.Count == 0;
    public ObservableCollection<PaymentReminderModel> PaymentReminders { get; set; } = new ObservableCollection<PaymentReminderModel>();
    public bool HasPaymentReminders => PaymentReminders.Count > 0;
    public ICommand DeleteReminderCommand { get; }
    public bool IsNotAdmin => _group?.Admin?.Id != _currentUser?.Id;
    public bool IsAdmin => _group?.Admin?.Id == _currentUser?.Id;

    public GroupModel _group { get; set; }
    public UserModel _currentUser { get; }

    public GroupViewPage(UserServiceApi userService, GroupServiceApi groupService, PaymentService paymentService, 
        UploadServiceApi uploadService, PaymentReminderService reminderService, GroupModel group, UserModel CurrentUser)
    {
        InitializeComponent();

        _userService = userService;
        _groupService = groupService;
        _paymentService = paymentService;
        _reminderService = reminderService;
        _uploadService = uploadService;
        _group = group;
        _currentUser = CurrentUser;
        BindingContext = this;
        DeleteReminderCommand = new Command<int>(async (id) => await DeleteReminder(id));
    }

    protected override async void OnAppearing()
    {
        try
        {
            base.OnAppearing();
            ShowLoading("Loading group details...");
            await RefreshPageDataAsync();
        }
        catch (Exception)
        {
            await DisplayAlert("Error", 
                "Something went wrong while loading the group. Please try again.", "OK");
        }
        finally
        {
            HideLoading();
        }
    }

    private void ShowLoading(string message)
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LoadingOverlay.IsVisible = true;
                LoadingIndicator.IsRunning = true;
                LoadingIndicator.IsVisible = true;
                LoadingLabel.Text = message;
                LoadingLabel.IsVisible = true;
            });
        }
        catch
        {
            // Fail silently if UI update fails
        }
    }

    private void HideLoading()
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LoadingOverlay.IsVisible = false;
                LoadingIndicator.IsRunning = false;
                LoadingIndicator.IsVisible = false;
                LoadingLabel.IsVisible = false;
            });
        }
        catch
        {
            // Fail silently if UI update fails
        }
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
        try
        {
            var popup = new AddRoomiePopup(_userService, _groupService, _paymentService, _currentUser);
            var result = await this.ShowPopupAsync(popup);

            if (result is null)
                return;

            string email = result as string;
            if (string.IsNullOrWhiteSpace(email))
            {
                await DisplayAlert("Invalid Input", "Please provide a valid email address.", "OK");
                return;
            }

            ShowLoading("Sending invitation...");

            try
            {
                InviteToGroupByEmailDto invitedUser = new InviteToGroupByEmailDto
                {
                    InviterUsername = _currentUser.Username,
                    Email = email,
                    GroupId = _group.Id
                };

                await _groupService.InviteUserToGroupByEmailAsync(invitedUser);
                await DisplayAlert("Success", 
                    $"Invitation sent to {email}! They will receive an email to join the group.", "OK");
            }
            catch (HttpRequestException)
            {
                await DisplayAlert("Connection Error", 
                    "Unable to send invitation. Please check your internet connection.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", 
                    $"Failed to invite roomie: {ex.Message}", "OK");
            }
        }
        catch (Exception)
        {
            await DisplayAlert("Error", 
                "Something went wrong while processing the invitation. Please try again.", "OK");
        }
        finally
        {
            HideLoading();
        }
    }

    private async Task LoadShameTableAsync()
    {
        try
        {
            if (_group == null || _currentUser == null)
            {
                throw new InvalidOperationException("Group or user information is missing.");
            }

            ShameTable.Clear();

            try
            {
                var debts = await _groupService.GetDebtsForUserAsync(_group.Id, _currentUser.Id);

                if (debts != null)
                {
                    foreach (var debt in debts)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() => ShameTable.Add(debt));
                    }
                }
            }
            catch (HttpRequestException)
            {
                throw new Exception("Unable to load debts. Please check your internet connection.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load debts: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async Task LoadYourOwnsTableAsync()
    {
        try
        {
            if (_group == null || _currentUser == null)
            {
                throw new InvalidOperationException("Group or user information is missing.");
            }

            YourOwnsTable.Clear();

            try
            {
                var debts = await _groupService.GetDebtsOwedByUserAsync(_group.Id, _currentUser.Id);

                if (debts != null)
                {
                    foreach (var debt in debts)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() => YourOwnsTable.Add(debt));
                    }
                }
            }
            catch (HttpRequestException)
            {
                throw new Exception("Unable to load your debts. Please check your internet connection.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load your debts: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    public Command OnMemberButtonClicked => new Command<UserModel>((member) =>
    {
        DisplayAlert("Member Clicked", $"{member.Username} button clicked", "OK");
    });

    private async void OnViewTransactionClicked(object sender, EventArgs e)
    {
        try
        {
            ShowLoading("Loading transaction history...");
            var popup = new ViewHistoryTransactionsPopup(_group, _groupService, _uploadService);
            await this.ShowPopupAsync(popup);
        }
        catch (Exception)
        {
            await DisplayAlert("Error", 
                "Unable to load transaction history. Please try again.", "OK");
        }
        finally
        {
            HideLoading();
        }
    }

    private async void OnAddExpenseClicked(object sender, EventArgs e)
    {
        try
        {
            ShowLoading("Preparing expense form...");
            var popup = new AddExpensePopup(_group, _currentUser, _groupService, _uploadService);
            var res = await this.ShowPopupAsync(popup);

            if (res is string message)
            {
                if (message.StartsWith("Error"))
                {
                    await DisplayAlert("Error", message.Replace("Error: ", ""), "OK");
                    return;
                }

                ShowLoading("Updating group data...");
                try
                {
                    _group = await _groupService.GetGroup(_group.Id);
                    await RefreshPageDataAsync();
                    await DisplayAlert("Success", message, "OK");
                }
                catch (Exception)
                {
                    await DisplayAlert("Warning", 
                        "Expense was added but failed to refresh group data. Please reload the page.", "OK");
                }
            }
        }
        catch (Exception)
        {
            await DisplayAlert("Error", 
                "Something went wrong while adding the expense. Please try again.", "OK");
        }
        finally
        {
            HideLoading();
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
        if (selectedItem == null) return;

        try
        {
            ShowLoading("Sending payment reminder...");

            var snoozeToPayDto = new SnoozeToPayDto
            {
                snoozeToUsername = selectedItem.debtor.Username,
                snoozeInfo = $"{selectedItem.creditor.Username} wants you to pay {selectedItem.amount:C} NIS."
            };

            try
            {
                await _groupService.SnoozeMember(snoozeToPayDto);
                await DisplayAlert("Success", 
                    $"Payment reminder sent to {selectedItem.debtor.Username}", "OK");
            }
            catch (HttpRequestException)
            {
                await DisplayAlert("Connection Error", 
                    "Unable to send payment reminder. Please check your internet connection.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", 
                    $"Failed to send payment reminder: {ex.Message}", "OK");
            }
        }
        catch (Exception)
        {
            await DisplayAlert("Error", 
                "Something went wrong while sending the payment reminder. Please try again.", "OK");
        }
        finally
        {
            HideLoading();
        }
    });

    public Command<DebtModel> OnYourOwnsItemTapped => new Command<DebtModel>(async (selectedItem) =>
    {
        if (selectedItem == null) return;

        try
        {
            var proceed = await DisplayAlert("Debt Payment", 
                $"You owe {selectedItem.creditor.Username} {selectedItem.amount:C} NIS.\nWould you like to proceed with payment?", 
                "Yes", "No");

            if (!proceed) return;

            ShowLoading("Opening payment page...");
            try
            {
                await Navigation.PushAsync(new PaymentPage(
                    selectedItem, 
                    _group, 
                    _userService, 
                    _groupService, 
                    _paymentService, 
                    _uploadService, 
                    _currentUser));
            }
            catch (Exception)
            {
                await DisplayAlert("Error", 
                    "Unable to open payment page. Please try again.", "OK");
            }
        }
        catch (Exception)
        {
            await DisplayAlert("Error", 
                "Something went wrong. Please try again.", "OK");
        }
        finally
        {
            HideLoading();
        }
    });

    private async Task RefreshPageDataAsync()
    {
        await LoadGroupMembersAsync();
        await LoadShameTableAsync();
        await LoadYourOwnsTableAsync();
        await LoadPaymentRemindersAsync();
        OnPropertyChanged(nameof(IsShameTableEmpty));
        OnPropertyChanged(nameof(IsYourOwnsTableEmpty));
        OnPropertyChanged(nameof(HasPaymentReminders));
    }

    private async Task LoadPaymentRemindersAsync()
    {
        try
        {
            if (_group == null || _currentUser == null)
            {
                throw new InvalidOperationException("Group or user information is missing.");
            }

            PaymentReminders.Clear();

            try
            {
                var reminders = await _reminderService.GetUserReminders(_currentUser.Id);

                if (reminders != null)
                {
                    var groupReminders = reminders.Where(r => r.GroupId == _group.Id);
                    foreach (var reminder in groupReminders)
                    {
                        await MainThread.InvokeOnMainThreadAsync(() => PaymentReminders.Add(reminder));
                    }
                }
            }
            catch (HttpRequestException)
            {
                throw new Exception("Unable to load payment reminders. Please check your internet connection.");
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to load payment reminders: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private async void OnAddPaymentReminderClicked(object sender, EventArgs e)
    {
        try
        {
            ShowLoading("Preparing reminder form...");
            var popup = new AddPaymentReminderPopup(_reminderService, _currentUser.Id, _group.Id);
            var result = await this.ShowPopupAsync(popup);
            
            if (result != null)
            {
                ShowLoading("Updating reminders...");
                try
                {
                    await RefreshPageDataAsync();
                    await DisplayAlert("Success", "Payment reminder set successfully!", "OK");
                }
                catch (Exception)
                {
                    await DisplayAlert("Warning", 
                        "Reminder was added but failed to refresh data. Please reload the page.", "OK");
                }
            }
        }
        catch (Exception)
        {
            await DisplayAlert("Error", 
                "Unable to set payment reminder. Please try again.", "OK");
        }
        finally
        {
            HideLoading();
        }
    }

    private async Task DeleteReminder(int reminderId)
    {
        try
        {
            var confirm = await DisplayAlert("Confirm Delete", 
                "Are you sure you want to delete this reminder?", "Yes", "No");

            if (!confirm) return;

            ShowLoading("Deleting reminder...");
            try
            {
                await _reminderService.DeleteReminder(reminderId);
                ShowLoading("Updating reminders...");
                await RefreshPageDataAsync();
                await DisplayAlert("Success", "Reminder deleted successfully", "OK");
            }
            catch (HttpRequestException)
            {
                await DisplayAlert("Connection Error", 
                    "Unable to delete reminder. Please check your internet connection.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", 
                    $"Failed to delete reminder: {ex.Message}", "OK");
            }
        }
        catch (Exception)
        {
            await DisplayAlert("Error", 
                "Something went wrong while deleting the reminder. Please try again.", "OK");
        }
        finally
        {
            HideLoading();
        }
    }

    private async void OnHomePageButtonClicked(object sender, EventArgs e)
    {
        // Navigate to UserHomePage
        await Navigation.PushAsync(new UserHomePage(_userService, _groupService, _paymentService, _uploadService, _currentUser));
    }

    private async void OnGeminiFeedbackClicked(object sender, EventArgs e)
    {
        try
        {
            ShowLoading("Analyzing your group's expenses...");
            var feedback = await _groupService.GetGeiminiResponseForExpenses(_group.Id);

            if (string.IsNullOrWhiteSpace(feedback))
            {
                throw new Exception("No insights available at this time. Try again when you have more expenses.");
            }

            //await DisplayAlert("Gemini Insight 💡", feedback, "OK");
            GeminiPopup.Show(feedback);
        }
        catch (HttpRequestException)
        {
            await DisplayAlert("Connection Error",
                "Unable to get expense analysis. Please check your internet connection.", "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error",
                $"Failed to get expense analysis: {ex.Message}", "OK");
        }
        finally
        {
            HideLoading();
        }
    }

    private async void OnDeleteGroupClicked(object sender, EventArgs e)
    {
        try
        {
            bool confirm = await DisplayAlert("Delete Group", 
                "Are you sure you want to delete this group? This action cannot be undone, and all members will be notified.", 
                "Yes", "No");

            if (!confirm) return;

            ShowLoading("Deleting group...");
            try
            {
                await _groupService.DeleteGroupAsync(_group.Id, _currentUser.Id);
                await DisplayAlert("Success", "Group successfully deleted", "OK");
                await Navigation.PopAsync();
            }
            catch (HttpRequestException)
            {
                await DisplayAlert("Connection Error", 
                    "Unable to delete group. Please check your internet connection.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", 
                    $"Failed to delete group: {ex.Message}", "OK");
            }
        }
        catch (Exception)
        {
            await DisplayAlert("Error", 
                "Something went wrong while deleting the group. Please try again.", "OK");
        }
        finally
        {
            HideLoading();
        }
    }

    private async void OnExitGroupClicked(object sender, EventArgs e)
    {
        try
        {
            bool confirm = await DisplayAlert("Confirm Exit", 
                "Are you sure you want to exit the group? You won't be able to rejoin unless invited again.", 
                "Yes", "No");

            if (!confirm) return;

            ShowLoading("Leaving group...");
            try
            {
                await _groupService.ExitGroupAsync(_currentUser.Id, _group.Id);
                await DisplayAlert("Success", "Successfully left the group", "OK");
                await Navigation.PopAsync();
            }
            catch (HttpRequestException)
            {
                await DisplayAlert("Connection Error", 
                    "Unable to leave group. Please check your internet connection.", "OK");
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", 
                    $"Failed to leave group: {ex.Message}", "OK");
            }
        }
        catch (Exception)
        {
            await DisplayAlert("Error", 
                "Something went wrong while leaving the group. Please try again.", "OK");
        }
        finally
        {
            HideLoading();
        }
    }

}
