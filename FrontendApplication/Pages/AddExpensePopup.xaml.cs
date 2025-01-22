using CommunityToolkit.Maui.Views;

namespace FrontendApplication.Popups;

public partial class AddExpensePopup : Popup
{
    public AddExpensePopup()
    {
        InitializeComponent();
        BindingContext = this;
    }

    // Command for adding an expense
    public Command AddExpenseCommand => new Command(() =>
    {
        var amount = AmountEntry.Text;
        var description = DescriptionEntry.Text;

        if (string.IsNullOrWhiteSpace(amount) || string.IsNullOrWhiteSpace(description))
        {
            Close("Please fill in all fields.");
            return;
        }

        Close(new { Amount = amount, Description = description });
    });

    // Command for canceling
    public Command CancelCommand => new Command(() =>
    {
        Close(null); // Dismiss the popup without returning data
    });
}