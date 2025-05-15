using Roomiebill.Server.Models;
using Roomiebill.Server.Common.Enums;

namespace Roomiebill.Server.Common.Validators
{
    /// <summary>
    /// Validates expense data based on category-specific requirements.
    /// Other category requires description only, while all other categories require valid date ranges.
    /// </summary>
    public static class ExpenseValidator
    {
        /// <summary>
        /// Validates an expense's fields according to its category rules.
        /// </summary>
        /// <param name="expense">The expense to validate.</param>
        /// <exception cref="ArgumentNullException">When expense is null.</exception>
        /// <exception cref="InvalidOperationException">When validation rules are violated.</exception>
        public static void ValidateExpenseFields(Expense expense)
        {
            // Validate expense is not null
            if (expense == null)
            {
                throw new ArgumentNullException(nameof(expense), "Expense cannot be null.");
            }

            // Validate Other category specific rules
            if (expense.Category == Category.Other)
            {
                // Description is mandatory for Other category
                if (string.IsNullOrWhiteSpace(expense.Description))
                {
                    throw new InvalidOperationException("Description is required for 'Other' category expenses.");
                }

                // Dates must be null for Other category
                if (expense.StartMonth != null || expense.EndMonth != null)
                {
                    throw new InvalidOperationException("Start and end months must be null for 'Other' category expenses.");
                }
                return;
            }

            // For non-Other categories, validate date requirements
            if (expense.StartMonth == null || expense.EndMonth == null)
            {
                throw new InvalidOperationException($"Start and end months are required for {expense.Category} category expenses.");
            }

            // Ensure month range is valid (end must be strictly after start)
            if (expense.EndMonth.Value.Year == expense.StartMonth.Value.Year && 
                expense.EndMonth.Value.Month == expense.StartMonth.Value.Month)
            {
                throw new InvalidOperationException("Start and end months cannot be the same.");
            }
            if (expense.EndMonth < expense.StartMonth)
            {
                throw new InvalidOperationException("End month must be after start month.");
            }

            // Enforce first day of month requirement
            if (expense.StartMonth?.Day != 1 || expense.EndMonth?.Day != 1)
            {
                throw new InvalidOperationException("Dates must be set to the first day of the month.");
            }
        }
    }
}
