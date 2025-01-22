using FrontendApplication.Models;
using FrontendApplication.Services;

namespace FrontendApplication.Pages;

public partial class PaymentPage : ContentPage
{
	public DebtModel _debt { get; }
	public GroupModel _groupOfUsers { get; }
	private readonly PaymentService _paymentService;
	public PaymentPage(DebtModel debt, GroupModel groupOfUsers, PaymentService paymentService)
	{
		InitializeComponent();
		_paymentService = paymentService;
		_debt = debt;
		_groupOfUsers = groupOfUsers;

		// Set the label text dynamically
        UserDebt.Text = $"You owe {_debt.amount}$ to {_debt.creditor.Username}";
	}

	// Event handler for Pay button
	private async void OnPayClicked(object sender, EventArgs e)
	{
		// Show the details of the debt when clicked
		await DisplayAlert("Payment", 
			$"You will pay {_debt.amount}$ to {_debt.creditor}", "OK");
		//TODO: for now currenct is always NIS and method is CARD
		try{
			PaymentRequestModel request = new PaymentRequestModel(_debt.amount, "NIS", _debt.creditor, _debt.debtor, "CARD", _groupOfUsers.Id);
			await _paymentService.ProcessPaymentAsync(request);
		}catch(Exception ex){
			await DisplayAlert("Error", ex.Message, "OK");
		}
	}
}