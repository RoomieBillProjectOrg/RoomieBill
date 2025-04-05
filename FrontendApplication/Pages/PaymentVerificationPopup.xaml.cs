namespace FrontendApplication.Pages;

public partial class PaymentVerificationPopup : ContentPage
{
    public bool IsPaymentConfirmed { get; private set; } = false;

    public PaymentVerificationPopup()
    {
        InitializeComponent();
    }

    private async void OnConfirmClicked(object sender, EventArgs e)
    {
        if (PaymentCheckBox.IsChecked)
        {
            IsPaymentConfirmed = true;
            await DisplayAlert("Success", "Payment confirmed!", "OK");
            await Navigation.PopModalAsync(); // Close the pop-up
        }
        else
        {
            await DisplayAlert("Error", "Please check the box to confirm payment.", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync(); // Close the pop-up
    }
}