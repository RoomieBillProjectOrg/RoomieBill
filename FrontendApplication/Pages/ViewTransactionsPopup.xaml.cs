using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Maui.Views;
using FrontendApplication.Models;
using FrontendApplication.Services;

namespace FrontendApplication.Popups;
public partial class ViewTransactionsPopup : Popup
{
    public ObservableCollection<ExpenseModel> Transactions { get; set; } = new ObservableCollection<ExpenseModel>();

    private readonly GroupServiceApi _groupService;
    private readonly GroupModel _group;

    public ViewTransactionsPopup(GroupModel group, GroupServiceApi groupService)
    {
        InitializeComponent();
        BindingContext = this;
        _group = group;
        _groupService = groupService;

        LoadTransactionsAsync();
    }

    private async void LoadTransactionsAsync()
    {
        try
        {
            var expenses = await _groupService.GetExpensesForGroupAsync(_group.Id);
            Transactions.Clear();
            foreach (var expense in expenses)
            {
                Transactions.Add(expense);
            }
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load transactions: {ex.Message}", "OK");
        }
    }
    

    public Command CloseCommand => new Command(() =>
    {
        Close(); // Dismiss the popup
    });
}