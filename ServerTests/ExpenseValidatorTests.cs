using Xunit;
using Roomiebill.Server.Common.Enums;
using Roomiebill.Server.Common.Validators;
using Roomiebill.Server.Models;
using System;

namespace ServerTests
{
    public class ExpenseValidatorTests
    {
        [Fact]
        public void ValidateExpenseFields_OtherCategory_WithNullDates_AndDescription_Success()
        {
            var expense = new Expense
            {
                Category = Category.Other,
                StartMonth = null,
                EndMonth = null,
                Description = "Valid description"
            };

            ExpenseValidator.ValidateExpenseFields(expense); // Should not throw
        }

        [Fact]
        public void ValidateExpenseFields_OtherCategory_WithoutDescription_ThrowsException()
        {
            var expense = new Expense
            {
                Category = Category.Other,
                StartMonth = null,
                EndMonth = null,
                Description = ""  // Empty description
            };

            var exception = Assert.Throws<InvalidOperationException>(() => 
                ExpenseValidator.ValidateExpenseFields(expense));
            Assert.Equal("Description is required for 'Other' category expenses.", exception.Message);
        }

        [Fact]
        public void ValidateExpenseFields_OtherCategory_WithDates_ThrowsException()
        {
            var expense = new Expense
            {
                Category = Category.Other,
                StartMonth = new DateTime(2025, 1, 1),
                EndMonth = new DateTime(2025, 2, 1),
                Description = "Valid description"
            };

            var exception = Assert.Throws<InvalidOperationException>(() => 
                ExpenseValidator.ValidateExpenseFields(expense));
            Assert.Equal("Start and end months must be null for 'Other' category expenses.", exception.Message);
        }

        [Fact]
        public void ValidateExpenseFields_NonOtherCategory_WithValidDates_Success()
        {
            var expense = new Expense
            {
                Category = Category.Electricity,
                StartMonth = new DateTime(2025, 1, 1),
                EndMonth = new DateTime(2025, 2, 1),
                Description = null  // Description not required
            };

            ExpenseValidator.ValidateExpenseFields(expense); // Should not throw
        }

        [Fact]
        public void ValidateExpenseFields_NonOtherCategory_WithNullDates_ThrowsException()
        {
            var expense = new Expense
            {
                Category = Category.Gas,
                StartMonth = null,
                EndMonth = null
            };

            var exception = Assert.Throws<InvalidOperationException>(() => 
                ExpenseValidator.ValidateExpenseFields(expense));
            Assert.Equal($"Start and end months are required for {expense.Category} category expenses.", exception.Message);
        }

        [Fact]
        public void ValidateExpenseFields_NonOtherCategory_WithOnlyStartDate_ThrowsException()
        {
            var expense = new Expense
            {
                Category = Category.Water,
                StartMonth = new DateTime(2025, 1, 1),
                EndMonth = null
            };

            var exception = Assert.Throws<InvalidOperationException>(() => 
                ExpenseValidator.ValidateExpenseFields(expense));
            Assert.Equal($"Start and end months are required for {expense.Category} category expenses.", exception.Message);
        }

        [Fact]
        public void ValidateExpenseFields_NonOtherCategory_EndBeforeStart_ThrowsException()
        {
            var expense = new Expense
            {
                Category = Category.Water,
                StartMonth = new DateTime(2025, 2, 1),
                EndMonth = new DateTime(2025, 1, 1)
            };

            var exception = Assert.Throws<InvalidOperationException>(() => 
                ExpenseValidator.ValidateExpenseFields(expense));
            Assert.Equal("End month must be after start month.", exception.Message);
        }

        [Fact]
        public void ValidateExpenseFields_NonOtherCategory_SameMonth_ThrowsException()
        {
            var expense = new Expense
            {
                Category = Category.PropertyTaxes,
                StartMonth = new DateTime(2025, 1, 1),
                EndMonth = new DateTime(2025, 1, 1)
            };

            var exception = Assert.Throws<InvalidOperationException>(() => 
                ExpenseValidator.ValidateExpenseFields(expense));
            Assert.Equal("Start and end months cannot be the same.", exception.Message);
        }

        [Fact]
        public void ValidateExpenseFields_NonOtherCategory_NonFirstDayOfMonth_ThrowsException()
        {
            var expense = new Expense
            {
                Category = Category.PropertyTaxes,
                StartMonth = new DateTime(2025, 1, 15),
                EndMonth = new DateTime(2025, 2, 1)
            };

            var exception = Assert.Throws<InvalidOperationException>(() => 
                ExpenseValidator.ValidateExpenseFields(expense));
            Assert.Equal("Dates must be set to the first day of the month.", exception.Message);
        }

        [Fact]
        public void ValidateExpenseFields_NonOtherCategory_EmptyDescription_Success()
        {
            var expense = new Expense
            {
                Category = Category.Electricity,
                StartMonth = new DateTime(2025, 1, 1),
                EndMonth = new DateTime(2025, 2, 1),
                Description = string.Empty  // Empty description should be allowed for non-Other categories
            };

            ExpenseValidator.ValidateExpenseFields(expense); // Should not throw
        }
    }
}
