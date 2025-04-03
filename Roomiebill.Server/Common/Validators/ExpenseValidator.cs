using Roomiebill.Server.Models;
using Roomiebill.Server.Common.Enums;

namespace Roomiebill.Server.Common.Validators
{
    public static class ExpenseValidator
    {
        public static void ValidateExpenseFields(Expense expense)
        {
            // Description is required only for Other category
            if (expense.Category == Category.Other)
            {
                if (string.IsNullOrWhiteSpace(expense.Description))
                {
                    throw new InvalidOperationException("Description is required for 'Other' category expenses.");
                }

                if (expense.StartMonth != null || expense.EndMonth != null)
                {
                    throw new InvalidOperationException("Start and end months must be null for 'Other' category expenses.");
                }
                return;
            }

            // For non-Other categories, description is optional but months are required
            if (expense.StartMonth == null || expense.EndMonth == null)
            {
                throw new InvalidOperationException($"Start and end months are required for {expense.Category} category expenses.");
            }

            // Validate that EndMonth is strictly after StartMonth (no same month allowed)
            if (expense.EndMonth.Value.Year == expense.StartMonth.Value.Year && 
                expense.EndMonth.Value.Month == expense.StartMonth.Value.Month)
            {
                throw new InvalidOperationException("Start and end months cannot be the same.");
            }
            if (expense.EndMonth < expense.StartMonth)
            {
                throw new InvalidOperationException("End month must be after start month.");
            }

            // Round dates to first day of month for consistency
            if (expense.StartMonth?.Day != 1 || expense.EndMonth?.Day != 1)
            {
                throw new InvalidOperationException("Dates must be set to the first day of the month.");
            }
        }
    }
}
