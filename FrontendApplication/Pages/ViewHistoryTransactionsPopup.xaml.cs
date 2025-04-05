using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Maui.Views;
using FrontendApplication.Models;
using FrontendApplication.Services;

namespace FrontendApplication.Popups;
public partial class ViewTransactionsPopup : Popup
{
    public ObservableCollection<ExpenseModel> Transactions { get; set; } = new ObservableCollection<ExpenseModel>();
    public ObservableCollection<string> AvailableCategories { get; set; } = new();
    private List<ExpenseModel> _allExpenses = new(); // store unfiltered data

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
            _allExpenses = expenses.ToList();

            Transactions.Clear();
            foreach (var expense in _allExpenses)
                Transactions.Add(expense);

            // Populate category filter
            var categories = _allExpenses
                .Where(e => e.Category != null)
                .Select(e => e.Category.GetName())
                .Distinct()
                .OrderBy(name => name)
                .ToList();

            AvailableCategories.Clear();
            AvailableCategories.Add("All"); // Default option
            foreach (var cat in categories)
                AvailableCategories.Add(cat);
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error", $"Failed to load transactions: {ex.Message}", "OK");
        }
    }

    private void OnCategorySelected(object sender, EventArgs e)
    {
        var selectedCategoryName = CategoryPicker.SelectedItem as string;

        Transactions.Clear();

        var filtered = string.IsNullOrEmpty(selectedCategoryName) || selectedCategoryName == "All"
            ? _allExpenses
            : _allExpenses.Where(e => e.Category != null && e.Category.GetName() == selectedCategoryName);

        foreach (var expense in filtered)
            Transactions.Add(expense);
    }

    public Command CloseCommand => new Command(() =>
    {
        Close(); // Dismiss the popup
    });
    private void OnCloseClicked(object sender, EventArgs e)
    {
        // Close the popup
        Close();
    }
}