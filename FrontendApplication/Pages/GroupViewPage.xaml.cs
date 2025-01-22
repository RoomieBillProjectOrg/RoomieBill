using System.Collections.ObjectModel;
using FrontendApplication.Models;
using FrontendApplication.Services;
namespace FrontendApplication.Pages;

public partial class GroupViewPage : ContentPage
{

	private readonly UserServiceApi _userService;
	private readonly GroupServiceApi _groupService;

	public ObservableCollection<UserModel> Members { get; set; } = new ObservableCollection<UserModel>();
	public ObservableCollection<DebtModel> ShameTable { get; set; } = new ObservableCollection<DebtModel>();
	public ObservableCollection<DebtModel> YourOwnsTable { get; set; } = new ObservableCollection<DebtModel>();



	public GroupModel _group { get; set; }
	public GroupViewPage(UserServiceApi userService, GroupServiceApi groupService, GroupModel group)
	{
		InitializeComponent();

		_userService = userService;
		_groupService = groupService;
		_group = group;

		// Set BindingContext to this page
		BindingContext = this;


	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await LoadGroupMembersAsync();
		await LoadShameTableAsync();
		await LoadYourOwnsTableAsync();
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
			var debts = await _groupService.GetDebtsForUserAsync(_group.Id, 2);

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
			var debts = await _groupService.GetDebtsOwedByUserAsync(_group.Id, 1);

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


	private void InitializeUI()
	{

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
}