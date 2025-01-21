using System.Collections.ObjectModel;
using FrontendApplication.Models;
using FrontendApplication.Services;
namespace FrontendApplication.Pages;

public partial class GroupViewPage : ContentPage
{

	private readonly UserServiceApi _userService;
	private readonly GroupServiceApi _groupService;

	public ObservableCollection<UserModel> Members { get; set; } = new ObservableCollection<UserModel>();

	public GroupModel _group { get; set; }
	public GroupViewPage(UserServiceApi userService, GroupServiceApi groupService, GroupModel group)
	{
		InitializeComponent();

		_userService = userService;
		_groupService = groupService;
		_group = group;


	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await LoadGroupDetailsAsync();
	}

	// private async Task LoadGroupDetailsAsync()
	// {
	// 	try
	// 	{
	// 		var group = await _groupService.GetGroup(_group.Id);
	// 		_group.Members = group.Members; // Ensure members are loaded
	// 		OnPropertyChanged(nameof(_group)); // Notify the UI that the group data has changed
	// 	}
	// 	catch (Exception ex)
	// 	{
	// 		await DisplayAlert("Error", $"Failed to load group details: {ex.Message}", "OK");
	// 	}
	// }
	private async Task LoadGroupDetailsAsync()
	{
		try
		{
			var group = await _groupService.GetGroup(_group.Id);
			Members.Clear();
			foreach (var member in group.Members)
			{
				Members.Add(member);
			}
		}
		catch (Exception ex)
		{
			await DisplayAlert("Error", $"Failed to load group details: {ex.Message}", "OK");
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