using CommunityToolkit.Maui.Views;
using System.Windows.Input;
using FrontendApplication.Models;
using FrontendApplication.Services;

namespace FrontendApplication.Popups
{
    public partial class AddPaymentReminderPopup : CommunityToolkit.Maui.Views.Popup
    {
        private readonly PaymentReminderService _paymentReminderService;
        private readonly int _userId;
        private readonly int _groupId;
        private bool _isValidInput = true;

        public List<Category> Categories { get; } = Enum.GetValues(typeof(Category)).Cast<Category>().ToList();
        public List<RecurrencePattern> RecurrencePatterns { get; } = Enum.GetValues(typeof(RecurrencePattern)).Cast<RecurrencePattern>().ToList();

        public Category SelectedCategory { get; set; }
        public RecurrencePattern SelectedRecurrencePattern { get; set; }
        public string DayOfMonth { get; set; }

        public AddPaymentReminderPopup(PaymentReminderService paymentReminderService, int userId, int groupId)
        {
            InitializeComponent();
            BindingContext = this;

            _paymentReminderService = paymentReminderService;
            _userId = userId;
            _groupId = groupId;

            // Set default values
            SelectedCategory = Categories.First();
            SelectedRecurrencePattern = RecurrencePatterns.First();
        }

        private void OnDayOfMonthTextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.NewTextValue))
            {
                ErrorLabel.IsVisible = false;
                _isValidInput = false;
                return;
            }

            if (!int.TryParse(e.NewTextValue, out int day) || day < 1 || day > 28)
            {
                ErrorLabel.IsVisible = true;
                _isValidInput = false;
            }
            else
            {
                ErrorLabel.IsVisible = false;
                _isValidInput = true;
            }
        }
        
        // Command for canceling - do nothing
        public Command CancelCommand => new Command(() =>
        {
            Close();
        });

        // Command for saving the reminder
        public Command SaveCommand => new Command(async () =>
        {
            await SaveReminder();
        });

        private async Task SaveReminder()
        {
            if (!ValidateInput())
                return;

            try
            {
                var request = new CreatePaymentReminderRequest
                {
                    UserId = _userId,
                    GroupId = _groupId,
                    Category = SelectedCategory,
                    RecurrencePattern = SelectedRecurrencePattern,
                    DayOfMonth = int.Parse(DayOfMonth)
                };

                var reminder = await _paymentReminderService.CreateReminder(request);
                Close(reminder);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(DayOfMonth))
            {
                ErrorLabel.Text = "Please enter day of month";
                ErrorLabel.IsVisible = true;
                return false;
            }

            if (!int.TryParse(DayOfMonth, out int day) || day < 1 || day > 28)
            {
                ErrorLabel.Text = "Day must be between 1 and 28";
                ErrorLabel.IsVisible = true;
                return false;
            }

            ErrorLabel.IsVisible = false;
            return true;
        }
    }
}
