using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using CommunityToolkit.Maui.Views;
using FrontendApplication.Models;
using FrontendApplication.Services;
using System.Diagnostics;

namespace FrontendApplication.Popups{
    public partial class ViewHistoryTransactionsPopup : Popup
    {
        public ObservableCollection<ExpenseModel> Transactions { get; set; } = new ObservableCollection<ExpenseModel>();
        public ObservableCollection<string> AvailableCategories { get; set; } = new();
        private List<ExpenseModel> _allExpenses = new(); // store unfiltered data

        private readonly GroupServiceApi _groupService;
        private readonly UploadServiceApi _uploadService;
        private readonly GroupModel _group;
        private byte[] imageBytes { get; set; }

        public ViewHistoryTransactionsPopup(GroupModel group, GroupServiceApi groupService, UploadServiceApi uploadService)
        {
            InitializeComponent();
            BindingContext = this;
            _group = group;
            _groupService = groupService;
            _uploadService = uploadService;
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

        private void OnFilterTypeChanged(object sender, EventArgs e)
        {
            var selected = FilterTypePicker.SelectedItem as string;

            CategoryFilterSection.IsVisible = selected == "Category";
            DateFilterSection.IsVisible = selected == "Date Range";
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

        private void OnApplyDateFilterClicked(object sender, EventArgs e)
        {
            var startDate = StartDatePicker.Date;
            var endDate = EndDatePicker.Date;

            Transactions.Clear();

            var filtered = _allExpenses
                .Where(e => e.StartMonth >= startDate && e.EndMonth <= endDate)
                .ToList();

            foreach (var item in filtered)
                Transactions.Add(item);
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
        private async void OnViewReceiptClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is ExpenseModel expense && !string.IsNullOrEmpty(expense.ReceiptString))
            {
                try
                {
                    var stream = await _uploadService.DownloadReceiptAsync(expense.ReceiptString);
                    if (stream == null)
                    {
                        Debug.WriteLine("Downloaded stream is null.");
                        return;
                    }
                    
                    var ms = new MemoryStream();
                    await stream.CopyToAsync(ms);
                    imageBytes = ms.ToArray(); 

                    var streamSource = new StreamImageSource();
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        if (TransactionListView != null && ReceiptImage != null && ReceiptView != null)
                        {
                            TransactionListView.IsVisible = false;
                            streamSource = new StreamImageSource
                            {
                                Stream = token => Task.FromResult<Stream>(new MemoryStream(imageBytes))
                            };
                            ReceiptImage.Source = streamSource;
                            ReceiptImage.IsVisible = true;
                            ReceiptView.IsVisible = true;
                        }
                        else
                        {
                            Debug.WriteLine("One or more UI elements are null.");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error while downloading or processing receipt: {ex.Message}");
                    await Shell.Current.DisplayAlert("Error", $"Failed to load receipt: {ex.Message}", "OK");
                }
            }
        }
        private void OnCloseReceiptClicked(object sender, EventArgs e)
        {
            // Switch back to Transaction List View
            TransactionListView.IsVisible = true;
            ReceiptView.IsVisible = false;
            
            PdfLabel.IsVisible = false;
        }
    }
}