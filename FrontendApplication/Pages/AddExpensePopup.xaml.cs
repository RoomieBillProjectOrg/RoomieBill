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

            // Initialize category picker and handle selection changes
            var categories = Enum.GetNames(typeof(Category)).ToList();
            // Reorder to put "Other" first
            categories.Remove("Other");
            categories.Insert(0, "Other");
            CategoryPicker.ItemsSource = categories;
            CategoryPicker.SelectedIndex = 0; // Default to "Other"
            CategoryPicker.SelectedIndexChanged += OnCategoryPickerChanged;

            // Initialize visibility state
            UpdateFieldsVisibility(Category.Other);

            // Initialize date pickers
            var today = DateTime.Today;
            StartMonthPicker.Date = new DateTime(today.Year, today.Month, 1);
            EndMonthPicker.Date = StartMonthPicker.Date.AddMonths(1);
            
            StartMonthPicker.MinimumDate = new DateTime(today.Year, today.Month, 1);
            EndMonthPicker.MinimumDate = StartMonthPicker.Date.AddMonths(1);
            
            // Add handlers for date changes
            StartMonthPicker.DateSelected += OnStartMonthSelected;
            EndMonthPicker.DateSelected += OnEndMonthSelected;

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

            // Validate Description only for Other category
            var selectedCategory = (Category)Enum.Parse(typeof(Category), CategoryPicker.SelectedItem.ToString());
            var description = DescriptionEntry?.Text;
            if (selectedCategory == Category.Other && string.IsNullOrWhiteSpace(description))
            {
                DisplayError("Please enter a description for Other category expenses.");
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
            // Validate dates for non-Other categories
            if (selectedCategory != Category.Other)
            {
                if (EndMonthPicker.Date <= StartMonthPicker.Date)
                {
                    DisplayError("End month must be after start month.");
                    return;
                }

                // Ensure we're using the first day of the month
                if (StartMonthPicker.Date.Day != 1 || EndMonthPicker.Date.Day != 1)
                {
                    DisplayError("Dates must be set to the first day of the month.");
                    return;
                }
            }

            ExpenseModel expense = new ExpenseModel
            {
                Id = -1,
                Amount = parsedAmount,
                Description = description ?? string.Empty,
                IsPaid = false,
                PayerId = _payer.Id,
                GroupId = _group.Id,
                Category = selectedCategory,
                ExpenseSplits = expenseSplitsDtos,
                StartMonth = selectedCategory != Category.Other ? StartMonthPicker.Date : null,
                EndMonth = selectedCategory != Category.Other ? EndMonthPicker.Date : null
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

        private void OnStartMonthSelected(object sender, DateChangedEventArgs e)
        {
            // Ensure first day of month
            StartMonthPicker.Date = new DateTime(e.NewDate.Year, e.NewDate.Month, 1);
            
            // Update end month minimum if needed
            var minEndDate = StartMonthPicker.Date.AddMonths(1);
            EndMonthPicker.MinimumDate = minEndDate;
            if (EndMonthPicker.Date <= StartMonthPicker.Date)
            {
                EndMonthPicker.Date = minEndDate;
            }
        }

        private void OnEndMonthSelected(object sender, DateChangedEventArgs e)
        {
            // Ensure first day of month
            var selectedDate = new DateTime(e.NewDate.Year, e.NewDate.Month, 1);
            
            // Validate against start date
            if (selectedDate.Year == StartMonthPicker.Date.Year && 
                selectedDate.Month == StartMonthPicker.Date.Month)
            {
                DisplayError("Start and end months cannot be the same");
                EndMonthPicker.Date = StartMonthPicker.Date.AddMonths(1);
                return;
            }
            
            if (selectedDate < StartMonthPicker.Date)
            {
                DisplayError("End month must be after start month");
                EndMonthPicker.Date = StartMonthPicker.Date.AddMonths(1);
                return;
            }

            EndMonthPicker.Date = selectedDate;
            ErrorLabel.IsVisible = false; // Clear error if valid
        }

        private void UpdateFieldsVisibility(Category category)
        {
            MonthsLayout.IsVisible = category != Category.Other;
            
            // Clear description if switching to non-Other category
            if (category != Category.Other)
            {
                DescriptionEntry.Text = string.Empty;
            }

            // Ensure EndMonthPicker is always at least one month after StartMonthPicker
            if (category != Category.Other && EndMonthPicker.Date <= StartMonthPicker.Date)
            {
                EndMonthPicker.Date = StartMonthPicker.Date.AddMonths(1);
            }
        }

        private void OnCategoryPickerChanged(object sender, EventArgs e)
        {
            var selectedCategory = (Category)Enum.Parse(typeof(Category), CategoryPicker.SelectedItem.ToString());
            UpdateFieldsVisibility(selectedCategory);
        }

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
