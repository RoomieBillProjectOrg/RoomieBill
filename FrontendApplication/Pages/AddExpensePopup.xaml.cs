using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using CommunityToolkit.Maui.Views;
using FrontendApplication.Models;
using FrontendApplication.Services;
using Microsoft.Maui.Storage;
using System.Net.Http;
using System.Net.Http.Headers;
using System.IO;
using System.Threading.Tasks;

namespace FrontendApplication.Popups
{
    public partial class AddExpensePopup : Popup
    {
        public ObservableCollection<MemberViewModel> Members { get; set; } = new ObservableCollection<MemberViewModel>();
        public ObservableCollection<MemberViewModel> SelectedMembers => new ObservableCollection<MemberViewModel>(Members.Where(m => m.IsSelected));

        private readonly GroupServiceApi _groupService;
        private readonly UploadServiceApi _uploadService;
        private readonly UserModel _payer;
        private readonly GroupModel _group;
        private string receiptFilePath = "";
        string receiptUrl = "";
        const string OCR_PROCESSOR_MSG = "Data extracted automaticlly when uploaded!";

        public AddExpensePopup(GroupModel group, UserModel payer, GroupServiceApi groupService, UploadServiceApi uploadService)
        {
            InitializeComponent();
            BindingContext = this;
            _payer = payer;
            _groupService = groupService;
            _uploadService = uploadService;
            _group = group;

            // Initialize category picker and handle selection changes
            var categories = Enum.GetNames(typeof(Category)).ToList();
            CategoryPicker.ItemsSource = categories;
            CategoryPicker.SelectedIndex = 0; // Default to "Other"
            CategoryPicker.SelectedIndexChanged += OnCategoryPickerChanged;

            // Initialize visibility state
            UpdateFieldsVisibility(Category.Other);

            // Initialize date pickers
            var today = DateTime.Today;
            StartMonthPicker.Date = new DateTime(today.Year, today.Month, 1);
            EndMonthPicker.Date = StartMonthPicker.Date.AddMonths(1);
            
            StartMonthPicker.MinimumDate = new DateTime(today.Year-1, 1, 1);
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

        private async void OnUploadReceiptClicked(object sender, EventArgs e)
        {
            try
            {
                var result = await FilePicker.PickAsync(new PickOptions
                {
                    PickerTitle = "Select a Receipt",
                    FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
                    {
                        //{ DevicePlatform.iOS, new[] { "public.image", "com.adobe.pdf" } }, // Only if we decide to support ios later.
                        { DevicePlatform.Android, new[] { "image/*", "application/pdf" } }
                    })
                });

                if (result != null)
                {
                    receiptFilePath = result.FullPath;
                    receiptUrl = await _uploadService.UploadReceiptAsync(receiptFilePath);

                    // Extract Info from file
                    var selectedCategory = (Category)Enum.Parse(typeof(Category), CategoryPicker.SelectedItem.ToString());
                    if (selectedCategory != Category.Other)
                    {
                        // Try to extract the data using uploadService
                        try
                        {
                            ShowLoading();
                            BillData data = await _uploadService.ExtractData(receiptUrl);
                            HideLoading();
                            if (data != null)
                            {
                                string message = $"📅 Start Date: {data.StartDate:yyyy-MM-dd}\n" +
                                                $"📅 End Date: {data.EndDate:yyyy-MM-dd}\n" +
                                                $"📝 Description: {data.Description}\n" +
                                                $"💰 Total Price: {data.TotalPrice:F2} ILS\n\n" +
                                                $"Do you want to apply this data?";

                                bool apply = await Application.Current.MainPage.DisplayAlert("Data Extracted", message, "Yes", "No");

                                if (apply)
                                {
                                    // Apply the extracted data to your UI or model
                                    StartMonthPicker.Date = new DateTime(data.StartDate.Year, data.StartDate.Month, data.StartDate.Day);
                                    EndMonthPicker.Date = new DateTime(data.EndDate.Year, data.EndDate.Month, data.EndDate.Day);
                                    AmountEntry.Text = data.TotalPrice.ToString("F2");
                                    DescriptionEntry.Text = data.Description;
                                }
                            }
                            else
                            {
                                await Application.Current.MainPage.DisplayAlert("Note", $"Couldn't extract your data. You can try again if you want :)", "OK");
                            }

                            // ✅ Update button UI to indicate success
                            UploadedReceiptLabel.Text = "[Click to select another file]";
                            UploadedReceiptLabel.IsVisible = true;
                            // UploadedReceiptLabel.BackgroundColor = Colors.Green;
                            UploadedReceiptLabel.TextColor = Colors.White;

                            // Optional: show the uploaded filename
                            UploadReceiptButton.Text = "Receipt Chosen ✔";
                            UploadReceiptButton.BackgroundColor = Colors.BlueViolet;
                        }
                        catch (Exception ex)
                        {
                            await Application.Current.MainPage.DisplayAlert("Note", $"Couldn't extract your data. You can try again if you want :)", "OK");
                            HideLoading();
                        }
                    }
                }
                
                //await Application.Current.MainPage.DisplayAlert("Success", "Receipt uploaded successfully!", "OK");
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Receipt choose failed: {ex.Message}", "OK");
            }
        }


        // Command for adding an expense
        public Command AddExpenseCommand => new Command(async () =>
        {
            try
            {
                // Validate Category selection
                if (CategoryPicker.SelectedItem == null)
                {
                    DisplayError("Oops! Please select an expense category.");
                    return;
                }

                // Validate Amount
                var amount = AmountEntry?.Text;
                if (string.IsNullOrWhiteSpace(amount))
                {
                    DisplayError("Don't forget to enter the expense amount!");
                    return;
                }

                if (!double.TryParse(amount, out double parsedAmount))
                {
                    DisplayError("The amount seems to be in an incorrect format. Please enter a valid number.");
                    return;
                }

                if (parsedAmount <= 0)
                {
                    DisplayError("The amount should be greater than zero.");
                    return;
                }

                var selectedCategory = (Category)Enum.Parse(typeof(Category), CategoryPicker.SelectedItem.ToString());

                // Validate Description only for Other category
                var description = DescriptionEntry?.Text;
                if (selectedCategory == Category.Other)
                {
                    if (string.IsNullOrWhiteSpace(description))
                    {
                        DisplayError("Please add a brief description of the expense.");
                        return;
                    }
                    if (description.Length < 3)
                    {
                        DisplayError("Description should be at least 3 characters long.");
                        return;
                    }
                }

                List<ExpenseSplitModel> expenseSplitsDtos;

                // Handle expense splitting
                try
                {
                    if (DividePicker.SelectedIndex == 1) // Custom division selected
                    {
                        expenseSplitsDtos = await HandleCustomDivision(parsedAmount);
                        if (expenseSplitsDtos == null) return; // Error was already displayed
                    }
                    else
                    {
                        // Split equally
                        if (_group.Members.Count == 0)
                        {
                            DisplayError("Unable to split the expense. No group members found.");
                            return;
                        }

                        var splitAmount = Math.Round(parsedAmount / _group.Members.Count, 2);
                        expenseSplitsDtos = _group.Members.Select(m => new ExpenseSplitModel
                        {
                            Id = -1,
                            ExpenseId = -1,
                            UserId = m.Id,
                            Amount = splitAmount
                        }).ToList();
                    }
                }
                catch (Exception ex)
                {
                    DisplayError($"Something went wrong while splitting the expense. Please try again.");
                    return;
                }

                // Handle receipt upload
                try
                {
                    if (!string.IsNullOrEmpty(receiptFilePath))
                    {
                        ShowLoading();
                        receiptUrl = await _uploadService.UploadReceiptAsync(receiptFilePath);
                        HideLoading();
                    }
                }
                catch (Exception ex)
                {
                    HideLoading();
                    DisplayError("We couldn't upload your receipt. You can try again or continue without it.");
                    return;
                }

                // Validate dates for non-Other categories
                if (selectedCategory != Category.Other)
                {
                    if (EndMonthPicker.Date <= StartMonthPicker.Date)
                    {
                        DisplayError("Please make sure the end month comes after the start month.");
                        return;
                    }

                    if (StartMonthPicker.Date.Day != 1 || EndMonthPicker.Date.Day != 1)
                    {
                        DisplayError("Please select the first day of each month for billing periods.");
                        return;
                    }
                }

                // Create and save expense
                try
                {
                    parsedAmount = 0;
                    foreach (var expenseSplit in expenseSplitsDtos)
                    {
                        parsedAmount += expenseSplit.Amount;
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
                        EndMonth = selectedCategory != Category.Other ? EndMonthPicker.Date : null,
                        ReceiptString = receiptUrl
                    };

                    ShowLoading();
                    await _groupService.addExpenseAsync(expense);
                    HideLoading();
                    Close("Your expense has been added successfully!");
                }
                catch (Exception ex)
                {
                    HideLoading();
                    DisplayError("We couldn't save your expense. Please check your connection and try again.");
                }
            }
            catch (Exception ex)
            {
                HideLoading();
                DisplayError("Something unexpected happened. Please try again.");
            }
        });

        private async Task<List<ExpenseSplitModel>> HandleCustomDivision(double totalAmount)
        {
            try
            {
                var contributingMembers = Members.Where(m => 
                    !string.IsNullOrWhiteSpace(m.CustomAmount) && 
                    double.TryParse(m.CustomAmount, out double amountValue) && 
                    amountValue > 0
                ).ToList();

                if (!contributingMembers.Any())
                {
                    DisplayError("Please enter at least one valid amount for the custom split.");
                    return null;
                }

                double splitTotal = 0;
                var splits = new List<ExpenseSplitModel>();

                foreach (var member in contributingMembers)
                {
                    if (!double.TryParse(member.CustomAmount, out double amount))
                    {
                        DisplayError($"Invalid amount format for {member.Member.Username}");
                        return null;
                    }
                    splitTotal += amount;
                    splits.Add(new ExpenseSplitModel
                    {
                        Id = -1,
                        ExpenseId = -1,
                        UserId = member.Member.Id,
                        Amount = amount
                    });
                }
                
                // No need to check this, rounding can cause errors
                // if (Math.Abs(splitTotal - totalAmount) > 0.01)
                // {
                //     DisplayError($"The individual amounts ({splitTotal:C}) don't add up to the total expense ({totalAmount:C})");
                //     return null;
                // }

                return splits;
            }
            catch (Exception ex)
            {
                DisplayError("Something went wrong while processing the custom split. Please try again.");
                return null;
            }
        }

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
                // Notify the user that he can upload the bill and the data will be filled automaticlly
                UploadedReceiptLabel.Text = OCR_PROCESSOR_MSG;
                UploadedReceiptLabel.IsVisible = true;
                UploadedReceiptLabel.TextColor = Colors.Red;
            }else{
                UploadedReceiptLabel.IsVisible = false;
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
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    ErrorLabel.Text = message;
                    ErrorLabel.IsVisible = true;
                    // Auto-hide error after 5 seconds
                    Task.Delay(5000).ContinueWith(_ =>
                    {
                        MainThread.BeginInvokeOnMainThread(() =>
                        {
                            ErrorLabel.IsVisible = false;
                        });
                    });
                }
                catch
                {
                    // Fail silently if UI update fails
                }
            });
        }

        private void ShowLoading()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LoadingOverlay.IsVisible = true;
                RootGrid.InputTransparent = true;
            });
        }

        private void HideLoading()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                LoadingOverlay.IsVisible = false;
                RootGrid.InputTransparent = false;
            });
        }
    }

    

    public class MemberViewModel
    {
        public UserModel Member { get; set; }
        public bool IsSelected { get; set; } = false;
        public string CustomAmount { get; set; } = string.Empty;
    }
}
