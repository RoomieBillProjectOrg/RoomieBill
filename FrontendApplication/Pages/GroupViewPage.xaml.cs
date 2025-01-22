using Firebase.Firestore.Auth;
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

	public GroupViewPage(UserServiceApi userService, GroupServiceApi groupService, PaymentService paymentService, GroupModel group, UserModel CurrentUser)
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
		await LoadGroupMembersAsync();
		await LoadShameTableAsync();
		await LoadYourOwnsTableAsync();
		OnPropertyChanged(nameof(IsShameTableEmpty));
		OnPropertyChanged(nameof(IsYourOwnsTableEmpty));
	}
	private async Task LoadGroupMembersAsync()
	{
		try
		{
			// var group = await _groupService.GetGroup(_group.Id);
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
	private async Task LoadShameTableAsync()
	{
		try
		{
			// Clear existing data
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
		// Handle the View Transaction button click
		await DisplayAlert("View Transaction", "View Transaction button clicked", "OK");
		// Navigate to the transaction page or perform other actions
	}

	//add a new pop up window to add an expense
	private async void OnAddExpenseClicked(object sender, EventArgs e)
	{
		var popup = new AddExpensePopup();
		var result = await this.ShowPopupAsync(popup);

		if (result is not null)
		{
			var expenseData = (dynamic)result;
			var amount = expenseData.Amount;
			var description = expenseData.Description;
			var expenseSplits = new List<ExpenseSplitModel>();
			foreach (var member in expenseData.Members)
			{
				expenseSplits.Add(new ExpenseSplitModel
				{
					UserId = member.Id,
					Percentage = member.Percentage
				});
			}

			var expenseModel = new ExpenseModel
			{
				PayerId = _currentUser.Id,
				Amount = amount,
				Description = description,
				GroupId = _group.Id,
				ExpenseSplits = expenseSplits
			};
			await DisplayAlert("Expense Added", $"Amount: {amount}\nDescription: {description}", "OK");
			// Add logic to handle the expense (e.g., save to the database or update UI)
		}
		else
		{
			await DisplayAlert("Canceled", "No expense was added.", "OK");
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

	public Command<DebtModel> OnShameTableItemTapped => new Command<DebtModel>((selectedItem) =>
	{
		if (selectedItem != null)
		{
			// Example action: Show an alert with the user ID
			DisplayAlert("Item Tapped", $"User: {_currentUser.Username} tapped.", "OK");

			// Add your logic here (e.g., navigation or additional functionality)
		}
	});

	public Command<DebtModel> OnYourOwnsItemTapped => new Command<DebtModel>(async (selectedItem) =>
	{
		if (selectedItem != null)
		{
			// Example action: Show an alert with the user ID
			await DisplayAlert("Item Tapped", $"You owe User : {_currentUser.Username}.", "OK");

			// Add your logic here (e.g., navigation or additional functionality)
			await Navigation.PushAsync(new PaymentPage(selectedItem, _group, _paymentService));
		}
	});


}