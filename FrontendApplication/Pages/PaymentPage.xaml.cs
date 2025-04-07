using FrontendApplication.Models;
using FrontendApplication.Services;

namespace FrontendApplication.Pages;

public partial class PaymentPage : ContentPage
{
    private readonly UserServiceApi _userService;
    private readonly GroupServiceApi _groupService;
    private readonly PaymentService _paymentService;
    private readonly UploadServiceApi _uploadService;
    private UserModel _user;

    public DebtModel _debt { get; }
    public GroupModel _groupOfUsers { get; }

    public PaymentPage(DebtModel debt, GroupModel groupOfUsers, UserServiceApi userService, GroupServiceApi groupService, PaymentService paymentService, UploadServiceApi uploadService, UserModel user)
    {
        InitializeComponent();

        _debt = debt;
        _groupOfUsers = groupOfUsers;
        _userService = userService;
        _groupService = groupService;
        _paymentService = paymentService;
        _uploadService = uploadService;
        _user = user;

        // Dynamically display debt details
        UserDebt.Text = $"You owe {_debt.amount:N2} NIS to {_debt.creditor.Username}";
    }

    // Handle payment process
    private async void OnPayClicked(object sender, EventArgs e)
    {
        var confirmationMessage = $"You will pay {_debt.amount:N2} NIS to {_debt.creditor.Username}. Proceed?";
        var confirm = await DisplayAlert("Confirm Payment", confirmationMessage, "Yes", "No");

        if (!confirm) return;

        try
        {
            // Show loading indicator while processing the payment
            IsBusy = true;

            // Create and process payment request
            var request = new PaymentRequestModel(_debt.amount, "NIS", _debt.creditor, _debt.debtor, "CARD", _groupOfUsers.Id);
            await _paymentService.ProcessPaymentAsync(request);

            // Open the payment verification pop-up
            var paymentVerificationPopup = new PaymentVerificationPopup();
            await Application.Current.MainPage.Navigation.PushModalAsync(paymentVerificationPopup);

            // Wait for the user to confirm payment in the pop-up
            while (!paymentVerificationPopup.IsPaymentConfirmed)
            {
                await Task.Delay(500); // Poll every 500ms
            }

            // Success feedback
            await DisplayAlert("Success", $"You successfully transferred {_debt.amount:N2} NIS to {_debt.creditor.Username}.", "OK");

            // Navigate back
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            // Error feedback
            await DisplayAlert("Payment Failed", ex.Message, "OK");
        }
        finally
        {
            // Hide loading indicator
            IsBusy = false;
        }
    }

    // Handle cancel button
    private async void OnCancelClicked(object sender, EventArgs e)
    {
        // Confirm cancellation
        var confirm = await DisplayAlert("Cancel Payment", "Are you sure you want to cancel?", "Yes", "No");
        if (confirm)
        {
            await Navigation.PopAsync();
        }
    }

    private async void OnHomePageButtonClicked(object sender, EventArgs e)
    {
        // Navigate to UserHomePage
        await Navigation.PushAsync(new UserHomePage(_userService, _groupService, _paymentService, _uploadService, _user));
    }
}
