using FrontendApplication.Models;
using FrontendApplication.Services;
namespace FrontendApplication.Pages;

public partial class GroupViewPage : ContentPage
{

	private readonly UserServiceApi _userService;
    private readonly GroupServiceApi _groupService;

	public GroupModel _group { get; set; }
	public GroupViewPage(UserServiceApi userService, GroupServiceApi groupService, GroupModel group)
	{
		InitializeComponent();

		_userService = userService;
        _groupService = groupService;
		_group = group;

		InitializeUI();
	}

	private void InitializeUI(){
		
		// This list should be fetched from database (Metar will add)
		var debts = new List<DebtDto>
		{
			new DebtDto { recipient = "Vladi" , amount = 50 },
			new DebtDto { recipient = "Metar" , amount = 30 },
			new DebtDto { recipient = "Inbar" , amount = 100 }
		};

		// Create buttons dynamically based on the debts
		foreach (var debt in debts)
		{
			var button = new Button
			{
				Text = $"You own {debt.amount}$ to {debt.recipient}",
				Command = new Command(() => OnOwnsButtonClicked(debt))
			};
			OwnsButtonsLayout.Children.Add(button);
		}
	}

	private async void OnViewTransactionClicked(object sender, EventArgs e)
	{
		// Handle the View Transaction button click
		await DisplayAlert("View Transaction", "View Transaction button clicked", "OK");
		// Navigate to the transaction page or perform other actions
	}

	private async void OnAddExpenseClicked(object sender, EventArgs e)
	{
		// Handle the Add Expense button click
		await DisplayAlert("Add Expense", "Add Expense button clicked", "OK");
		// Navigate to the add expense page or perform other actions
	}

	// Event handler for "You own..." buttons
	private async Task OnOwnsButtonClicked(DebtDto debt)
	{
		await Navigation.PushAsync(new PaymentPage(debt));
		// Add further logic for handling payment or other actions
	}
}