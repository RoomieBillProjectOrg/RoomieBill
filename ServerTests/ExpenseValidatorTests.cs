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
        public void ValidateMonthFields_OtherCategory_WithNullDates_Success()
        {
            var expense = new Expense
            {
                Category = Category.Other,
                StartMonth = null,
                EndMonth = null
            };

            ExpenseValidator.ValidateExpenseFields(expense); // Should not throw
        }

        [Fact]
        public void ValidateMonthFields_OtherCategory_WithDates_ThrowsException()
        {
            var expense = new Expense
            {
                Category = Category.Other,
                StartMonth = new DateTime(2025, 1, 1),
                EndMonth = new DateTime(2025, 2, 1)
            };

            Assert.Throws<InvalidOperationException>(() => ExpenseValidator.ValidateExpenseFields(expense));
        }

        [Fact]
        public void ValidateMonthFields_NonOtherCategory_WithValidDates_Success()
        {
            var expense = new Expense
            {
                Category = Category.Electricity,
                StartMonth = new DateTime(2025, 1, 1),
                EndMonth = new DateTime(2025, 2, 1)
            };

            ExpenseValidator.ValidateExpenseFields(expense); // Should not throw
        }

        [Fact]
        public void ValidateMonthFields_NonOtherCategory_WithNullDates_ThrowsException()
        {
            var expense = new Expense
            {
                Category = Category.Gas,
                StartMonth = null,
                EndMonth = null
            };

            Assert.Throws<InvalidOperationException>(() => ExpenseValidator.ValidateExpenseFields(expense));
        }

        [Fact]
        public void ValidateMonthFields_NonOtherCategory_EndBeforeStart_ThrowsException()
        {
            var expense = new Expense
            {
                Category = Category.Water,
                StartMonth = new DateTime(2025, 2, 1),
                EndMonth = new DateTime(2025, 1, 1)
            };

            Assert.Throws<InvalidOperationException>(() => ExpenseValidator.ValidateExpenseFields(expense));
        }

        [Fact]
        public void ValidateMonthFields_NonOtherCategory_NonFirstDayOfMonth_ThrowsException()
        {
            var expense = new Expense
            {
                Category = Category.PropertyTaxes,
                StartMonth = new DateTime(2025, 1, 15),
                EndMonth = new DateTime(2025, 2, 1)
            };

            Assert.Throws<InvalidOperationException>(() => ExpenseValidator.ValidateExpenseFields(expense));
        }
    }
}
