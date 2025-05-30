using Xunit;
using Roomiebill.Server.Common.Enums;
using Roomiebill.Server.Common.Validators;
using Roomiebill.Server.Models;
using System;

namespace ServerTests.UnitTests
{
    public class ExpenseValidatorTests
    {
        [Fact]
        public void ValidateExpenseFields_OtherCategory_WithNullDates_AndDescription_Success()
        {
            // Arrange
            var expense = new Expense
            {
                Category = Category.Other,
                StartMonth = null,
                EndMonth = null,
                Description = "Valid description"
            };

            // Act & Assert
            ExpenseValidator.ValidateExpenseFields(expense); // Should not throw
        }

        [Fact]
        public void ValidateExpenseFields_OtherCategory_WithoutDescription_ThrowsException()
        {
            // Arrange
            var expense = new Expense
            {
                Category = Category.Other,
                StartMonth = null,
                EndMonth = null,
                Description = ""  // Empty description
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                ExpenseValidator.ValidateExpenseFields(expense));
            Assert.Equal("Description is required for 'Other' category expenses.", exception.Message);
        }

        [Fact]
        public void ValidateExpenseFields_OtherCategory_WithWhitespaceDescription_ThrowsException()
        {
            // Arrange
            var expense = new Expense
            {
                Category = Category.Other,
                StartMonth = null,
                EndMonth = null,
                Description = "   "  // Whitespace only
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                ExpenseValidator.ValidateExpenseFields(expense));
            Assert.Equal("Description is required for 'Other' category expenses.", exception.Message);
        }

        [Fact]
        public void ValidateExpenseFields_OtherCategory_WithDates_ThrowsException()
        {
            // Arrange
            var expense = new Expense
            {
                Category = Category.Other,
                StartMonth = new DateTime(2025, 1, 1),
                EndMonth = new DateTime(2025, 2, 1),
                Description = "Valid description"
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                ExpenseValidator.ValidateExpenseFields(expense));
            Assert.Equal("Start and end months must be null for 'Other' category expenses.", exception.Message);
        }

        [Fact]
        public void ValidateExpenseFields_NonOtherCategory_WithValidDates_Success()
        {
            // Arrange
            var expense = new Expense
            {
                Category = Category.Electricity,
                StartMonth = new DateTime(2025, 1, 1),
                EndMonth = new DateTime(2025, 2, 1),
                Description = null  // Description not required
            };

            // Act & Assert
            ExpenseValidator.ValidateExpenseFields(expense); // Should not throw
        }

        [Fact]
        public void ValidateExpenseFields_NonOtherCategory_WithNullDates_ThrowsException()
        {
            // Arrange
            var expense = new Expense
            {
                Category = Category.Gas,
                StartMonth = null,
                EndMonth = null
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                ExpenseValidator.ValidateExpenseFields(expense));
            Assert.Equal($"Start and end months are required for {expense.Category} category expenses.", exception.Message);
        }

        [Fact]
        public void ValidateExpenseFields_NonOtherCategory_WithOnlyStartDate_ThrowsException()
        {
            // Arrange
            var expense = new Expense
            {
                Category = Category.Water,
                StartMonth = new DateTime(2025, 1, 1),
                EndMonth = null
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                ExpenseValidator.ValidateExpenseFields(expense));
            Assert.Equal($"Start and end months are required for {expense.Category} category expenses.", exception.Message);
        }

        [Fact]
        public void ValidateExpenseFields_NonOtherCategory_EndBeforeStart_ThrowsException()
        {
            // Arrange
            var expense = new Expense
            {
                Category = Category.Water,
                StartMonth = new DateTime(2025, 2, 1),
                EndMonth = new DateTime(2025, 1, 1)
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                ExpenseValidator.ValidateExpenseFields(expense));
            Assert.Equal("End month must be after start month.", exception.Message);
        }

        [Fact]
        public void ValidateExpenseFields_NonOtherCategory_SameMonth_ThrowsException()
        {
            // Arrange
            var expense = new Expense
            {
                Category = Category.PropertyTaxes,
                StartMonth = new DateTime(2025, 1, 1),
                EndMonth = new DateTime(2025, 1, 1)
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                ExpenseValidator.ValidateExpenseFields(expense));
            Assert.Equal("Start and end months cannot be the same.", exception.Message);
        }

        [Fact]
        public void ValidateExpenseFields_NonOtherCategory_NonFirstDayOfMonth_ThrowsException()
        {
            // Arrange
            var expense = new Expense
            {
                Category = Category.PropertyTaxes,
                StartMonth = new DateTime(2025, 1, 15),
                EndMonth = new DateTime(2025, 2, 1)
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                ExpenseValidator.ValidateExpenseFields(expense));
            Assert.Equal("Dates must be set to the first day of the month.", exception.Message);
        }

        [Fact]
        public void ValidateExpenseFields_NonOtherCategory_EmptyDescription_Success()
        {
            // Arrange
            var expense = new Expense
            {
                Category = Category.Electricity,
                StartMonth = new DateTime(2025, 1, 1),
                EndMonth = new DateTime(2025, 2, 1),
                Description = string.Empty  // Empty description should be allowed for non-Other categories
            };

            // Act & Assert
            ExpenseValidator.ValidateExpenseFields(expense); // Should not throw
        }

        [Fact]
        public void ValidateExpenseFields_NonOtherCategory_LastDayOfMonth_ThrowsException()
        {
            // Arrange
            var expense = new Expense
            {
                Category = Category.Electricity,
                StartMonth = new DateTime(2025, 1, 1),
                EndMonth = new DateTime(2025, 2, 28)
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                ExpenseValidator.ValidateExpenseFields(expense));
            Assert.Equal("Dates must be set to the first day of the month.", exception.Message);
        }

        [Fact]
        public void ValidateExpenseFields_NonOtherCategory_DifferentYears_Success()
        {
            // Arrange
            var expense = new Expense
            {
                Category = Category.PropertyTaxes,
                StartMonth = new DateTime(2025, 12, 1),
                EndMonth = new DateTime(2026, 1, 1)
            };

            // Act & Assert
            ExpenseValidator.ValidateExpenseFields(expense); // Should not throw
        }

        [Fact]
        public void ValidateExpenseFields_NonOtherCategory_MultiYearSpan_Success()
        {
            // Arrange
            var expense = new Expense
            {
                Category = Category.PropertyTaxes,
                StartMonth = new DateTime(2025, 1, 1),
                EndMonth = new DateTime(2027, 1, 1)
            };

            // Act & Assert
            ExpenseValidator.ValidateExpenseFields(expense); // Should not throw
        }

        [Fact]
        public void ValidateExpenseFields_NonOtherCategory_EndDateLastDayPreviousMonth_ThrowsException()
        {
            // Arrange
            var expense = new Expense
            {
                Category = Category.PropertyTaxes,
                StartMonth = new DateTime(2025, 2, 1),
                EndMonth = new DateTime(2025, 1, 31)  // Last day of previous month
            };

            // Act & Assert
            var exception = Assert.Throws<InvalidOperationException>(() =>
                ExpenseValidator.ValidateExpenseFields(expense));
            Assert.Equal("End month must be after start month.", exception.Message);
        }

        [Fact]
        public void ValidateExpenseFields_Null_ThrowsArgumentNullException()
        {
            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() =>
                ExpenseValidator.ValidateExpenseFields(null));
            Assert.Equal("Expense cannot be null. (Parameter 'expense')", exception.Message);
        }
    }
}
