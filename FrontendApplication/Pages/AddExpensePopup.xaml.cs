using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using CommunityToolkit.Maui.Views;
using FrontendApplication.Models;
using FrontendApplication.Services;

namespace FrontendApplication.Popups
{
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

            // Initialize category picker
            CategoryPicker.ItemsSource = Enum.GetNames(typeof(Category));
            CategoryPicker.SelectedIndex = 0; // Default to first category

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
            // Show/hide custom amount layout based on selected division type
            if (DividePicker.SelectedIndex == 1) // Custom division selected
            {
                CustomAmountLayout.IsVisible = true;
            }
            else
            {
                CustomAmountLayout.IsVisible = false;
            }
        }

        // Command for adding an expense
        public Command AddExpenseCommand => new Command(async () =>
        {
            // Validate Amount
            var amount = AmountEntry?.Text;
            if (string.IsNullOrWhiteSpace(amount) || !double.TryParse(amount, out double parsedAmount) || parsedAmount <= 0)
            {
                DisplayError("Please enter a valid amount.");
                return;
            }

            // Validate Description
            var description = DescriptionEntry?.Text;
            if (string.IsNullOrWhiteSpace(description))
            {
                DisplayError("Please enter a description.");
                return;
            }

            List<ExpenseSplitModel> expenseSplitsDtos;

            // Check Custom Division Logic
            if (DividePicker.SelectedIndex == 1) // Custom division selected
            {
                // Ensure at least one amount is provided
                var contributingMembers = Members.Where(m => !string.IsNullOrWhiteSpace(m.CustomAmount) && double.TryParse(m.CustomAmount, out double amountValue) && amountValue > 0).ToList();
                if (!contributingMembers.Any())
                {
                    DisplayError("Please provide at least one valid amount for custom division.");
                    return;
                }

                // Ensure all amounts sum up to total expense amount
                var totalAmount = contributingMembers.Sum(m =>
                {
                    if (double.TryParse(m.CustomAmount, out double parsedValue))
                    {
                        return parsedValue;
                    }
                    return 0;
                });

                if (Math.Abs(totalAmount - parsedAmount) > 0.01) // Using small epsilon for double comparison
                {
                    DisplayError($"The sum of split amounts ({totalAmount}) must equal the total expense amount ({parsedAmount}).");
                    return;
                }

                // Set the expense splits dtos
                expenseSplitsDtos = contributingMembers.Select(m => new ExpenseSplitModel
                {
                    Id = -1,
                    ExpenseId = -1,
                    UserId = m.Member.Id,
                    Amount = Convert.ToDouble(m.CustomAmount)
                }).ToList();
            }
            else
            {
                // Split equally
                var splitAmount = parsedAmount / _group.Members.Count;
                expenseSplitsDtos = _group.Members.Select(m => new ExpenseSplitModel
                {
                    Id = -1,
                    ExpenseId = -1,
                    UserId = m.Id,
                    Amount = splitAmount
                }).ToList();
            }

            // Set the expense dto
            ExpenseModel expense = new ExpenseModel
            {
                Id = -1,
                Amount = parsedAmount,
                Description = description,
                IsPaid = false,
                PayerId = _payer.Id,
                GroupId = _group.Id,
                Category = (Category)Enum.Parse(typeof(Category), CategoryPicker.SelectedItem.ToString()),
                ExpenseSplits = expenseSplitsDtos
            };

            try
            {
                await _groupService.addExpenseAsync(expense);
                Close("Expense added successfully!");
            }
            catch (Exception ex)
            {
                DisplayError($"Error: {ex.Message}");
            }
        });

        // Command for canceling - do nothing
        public Command CancelCommand => new Command(() =>
        {
            Close();
        });


        private void DisplayError(string message)
        {
            ErrorLabel.Text = message;
            ErrorLabel.IsVisible = true;
        }
    }

    public class MemberViewModel
    {
        public UserModel Member { get; set; }
        public bool IsSelected { get; set; } = false;
        public string CustomAmount { get; set; } = string.Empty;
    }
}
