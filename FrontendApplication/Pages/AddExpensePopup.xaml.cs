using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Maui.Views;
using FrontendApplication.Models;
using FrontendApplication.Services;

namespace FrontendApplication.Popups;

public partial class AddExpensePopup : Popup
{
    public ObservableCollection<MemberViewModel> Members { get; set; } = new ObservableCollection<MemberViewModel>();
    public ObservableCollection<MemberViewModel> SelectedMembers => new ObservableCollection<MemberViewModel>(Members.Where(m => m.IsSelected));

    private readonly GroupServiceApi _groupService;
    private readonly UserModel _payer;
    private readonly GroupModel _group;

    public AddExpensePopup(GroupModel group, UserModel payer, GroupServiceApi groupService)
    {
        InitializeComponent();
        BindingContext = this;
        _payer = payer;
        _groupService = groupService;
        _group = group;

        // Initialize members
        foreach (var member in _group.Members)
        {
            Members.Add(new MemberViewModel
            {
                Member = member
            });
        }
    }

    private void OnDividePickerChanged(object sender, EventArgs e)
    {
        if (DividePicker.SelectedIndex == 1) // Custom division selected
        {
            
            CustomPercentageLayout.IsVisible = true;
        }
        else
        {
            CustomPercentageLayout.IsVisible = false;
        }
    }

    // Command for adding an expense
    public Command AddExpenseCommand => new Command(async () =>
    {
        // Validate Amount
        var amount = AmountEntry?.Text;
        if (string.IsNullOrWhiteSpace(amount) || !double.TryParse(amount, out double parsedAmount) || parsedAmount <= 0)
        {
            Close("Please enter a valid amount.");
            return;
        }

        // Validate Description
        var description = DescriptionEntry?.Text;
        if (string.IsNullOrWhiteSpace(description))
        {
            Close("Please enter a description.");
            return;
        }
        List<ExpenseSplitModel> expenseSplitsDtos;
        // Check Custom Division Logic
        if (DividePicker.SelectedIndex == 1) // Custom division selected
        {
            // Ensure at least one percentage is provided
            var contributingMembers = Members.Where(m => !string.IsNullOrWhiteSpace(m.CustomPercentage) && double.TryParse(m.CustomPercentage, out double percentage) && percentage > 0).ToList();
            if (!contributingMembers.Any())
            {
                Close("Please provide at least one valid percentage for custom division.");
                return;
            }

            // Ensure all percentages are valid and sum up to 100%
            var totalPercentage = contributingMembers.Sum(m => decimal.Parse(m.CustomPercentage));
            if (totalPercentage != 100)
            {
                Close("The total percentage for custom split must equal 100%.");
                return;
            }

            // Set the expense splits dtos
            expenseSplitsDtos = contributingMembers.Select(m => new ExpenseSplitModel
                {
                    Id = -1,
                    ExpenseId = -1,
                    UserId = m.Member.Id,
                    Percentage = Convert.ToDouble(m.CustomPercentage)
                }).ToList();
        }else{
            expenseSplitsDtos = _group.Members.Select(m => new ExpenseSplitModel
                {
                    Id = -1,
                    ExpenseId = -1,
                    UserId = m.Id,
                    Percentage = 100 / _group.Members.Count
                }).ToList();
        }

        // Set the expense dto
        ExpenseModel expense = new ExpenseModel{
            Id = -1,
            Amount = parsedAmount,
            Description = description,
            IsPaid = false,
            PayerId = _payer.Id,
            GroupId = _group.Id,
            ExpenseSplits = expenseSplitsDtos
        };

        try{
            await _groupService.addExpenseAsync(expense);
            Close("Expense added successfuly!");
        }catch(Exception ex){
            Close($"Expense add failed: {ex.Message}");
        }
    });


    // Command for canceling
    public Command CancelCommand => new Command(() =>
    {
        Close(null); // Dismiss the popup without returning data
    });
}

public class MemberViewModel
{
    public UserModel Member { get; set; }
    public bool IsSelected { get; set; } = false;
    public string CustomPercentage { get; set; } = string.Empty;

}