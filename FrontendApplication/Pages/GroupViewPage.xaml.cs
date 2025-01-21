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