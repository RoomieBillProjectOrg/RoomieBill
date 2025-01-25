using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Maui.Views;
using FrontendApplication.Models;

namespace FrontendApplication.Popups;

public partial class AddExpensePopup : Popup
{
    public ObservableCollection<MemberViewModel> Members { get; set; } = new ObservableCollection<MemberViewModel>();
    public ObservableCollection<MemberViewModel> SelectedMembers => new ObservableCollection<MemberViewModel>(Members.Where(m => m.IsSelected));

    public AddExpensePopup(GroupModel group)
    {
        InitializeComponent();
        BindingContext = this;

        // Initialize members
        foreach (var member in group.Members)
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
    public Command AddExpenseCommand => new Command(() =>
    {
        // Validate Amount
        var amount = AmountEntry?.Text;
        if (string.IsNullOrWhiteSpace(amount) || !decimal.TryParse(amount, out decimal parsedAmount) || parsedAmount <= 0)
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

        // Check Custom Division Logic
        if (DividePicker.SelectedIndex == 1) // Custom division selected
        {
            // Ensure at least one percentage is provided
            var contributingMembers = Members.Where(m => !string.IsNullOrWhiteSpace(m.CustomPercentage) && decimal.TryParse(m.CustomPercentage, out decimal percentage) && percentage > 0).ToList();
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

            // Return data with custom percentages
            Close(new
            {
                Amount = parsedAmount,
                Description = description,
                Members = contributingMembers.Select(m => new
                {
                    m.Member.Id,
                    m.Member.Username,
                    m.CustomPercentage
                }).ToList()
            });
            return;
        }

        // If not custom division, equally split
        var equalSplit = Members.Select(m => new
        {
            m.Member.Id,
            m.Member.Username,
            CustomPercentage = (100m / Members.Count).ToString("F2") // Equal percentage
        }).ToList();

        // Return data for equal split
        Close(new
        {
            Amount = parsedAmount,
            Description = description,
            Members = equalSplit
        });
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