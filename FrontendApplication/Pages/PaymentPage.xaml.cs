using FrontendApplication.Models;

namespace FrontendApplication.Pages;

public partial class PaymentPage : ContentPage
{
	public DebtDto _debt { get; }
	public PaymentPage(DebtDto debt)
	{
		InitializeComponent();
		_debt = debt;

		// Set the label text dynamically
        UserDebt.Text = $"You owe {_debt.amount}$ to {_debt.recipient}";
	}

	// Event handler for Pay button
	private async void OnPayClicked(object sender, EventArgs e)
	{
		// Show the details of the debt when clicked
		await DisplayAlert("Payment", 
			$"You will pay {_debt.amount}$ to {_debt.recipient}", "OK");
	}
}