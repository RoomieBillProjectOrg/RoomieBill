using Roomiebill.Server.Models;
using Roomiebill.Server.DataAccessLayer.Dtos;
using System.Collections.Generic;

namespace Roomiebill.Server.Facades
{
    public class ExpenseFacade
    {
        private int[] _debtArray; // 1D array to store debts
        private readonly Dictionary<int, int> _userIndexMap; //<UserId,Array index> map user id to index in the _debtMatrix
        private readonly int _userCount; // number of users

        public ExpenseFacade(List<int> userIds)
        {
            _userCount = userIds.Count;
            int size = (_userCount * (_userCount - 1)) / 2; // size of the debtArray
            _debtArray = new int[size];
            _userIndexMap = new Dictionary<int, int>();
            for (int i = 0; i < _userCount; i++)
            {
                _userIndexMap[userIds[i]] = i;
            }
        }

        // Get the index of the cell in the debtArray given 2 ids of users
        private int GetIndex(int i, int j)
        {
            if (i > j) (i, j) = (j, i); // i should be less than j
            return i * (_userCount - 1) - i * (i + 1) / 2 + (j - 1);
        }

        // Get the debt between 2 users - i owns to j- so if it is negative i owns j 0
        private int GetDebtBetween(int i, int j)
        {
            int index = GetIndex(i, j);
            int debt = _debtArray[index];
            if (i < j)
            {
                return debt > 0 ? debt : 0; // Positive: i owes j; Zero or negative: i owes j nothing
            }
            else
            {
                return debt < 0 ? -debt : 0; // Negative: j owes i; Zero or positive: j owes i nothing
            

        private void UpdateDebtArray(int i, int j, int amount)
        {
            int index = GetIndex(i, j);
            if (i < j)
            {
                _debtArray[index] -= amount; 
            }
            else
            {
                _debtArray[index] += amount; 
            }
        }

        public ExpenseDto CreateExpense(ExpenseDto expenseDto)
        {
            int payerId = expenseDto.PayerId;
            int payerIndex = _userIndexMap[payerId];
            foreach (var split in expenseDto.SplitBetween)
            {
                int userId = split.Key;
                int userIndex = _userIndexMap[userId];
                int amount = (int)(expenseDto.Amount * (split.Value / 100.0));

                UpdateDebtArray(payerIndex, userIndex, amount);
            }
            return expenseDto;
        }

                // Get all debts between users (both sides  -i owns j and j owns i)
        public Dictionary<(int, int), int> GetAllDebts()
        {
            Dictionary<(int, int), int> debts = new Dictionary<(int, int), int>();
            for (int i = 0; i < _userCount; i++)
            {
                for (int j = 0; j < _userCount; j++)
                {
                    if (i != j)
                    {
                        int debt = GetDebtBetween(i, j);
                        if (debt > 0)
                        {
                            debts.Add((i, j), debt);
                        }
                    }
                }
            }
            return debts;
        }

        // Settle debt between two users
        public void SettleDebt(int i, int j)
        {
            int index = GetIndex(i, j);
            _debtArray[index] = 0;
        }

        public void DeleteExpense(ExpenseDto expenseDto)
        {
            int payerId = expenseDto.PayerId;
            int payerIndex = _userIndexMap[payerId];
            foreach (KeyValuePair<int, double> split in expenseDto.SplitBetween)
            {
                int userId = split.Key;
                int userIndex = _userIndexMap[userId];
                int amount = (int)(expenseDto.Amount * (split.Value / 100.0));
        
                UpdateDebtArray(payerIndex, userIndex, -amount); // Reverse the debt
            }
        }

            // Update an existing expense
        public void UpdateExpense(ExpenseDto oldExpenseDto, ExpenseDto newExpenseDto)
        {
            // First, reverse the old expense
            DeleteExpense(oldExpenseDto);
            // Then, add the new expense
            CreateExpense(newExpenseDto);
        }

         // Get total debt for a specific user
        public double GetTotalDebtForUser(int userId)
        {
            int userIndex = _userIndexMap[userId];
            double totalDebt = 0;
            for (int i = 0; i < _userCount; i++)
            {
                if (i != userIndex)
                {
                    totalDebt += GetDebtBetween(i,userIndex);
                }
            }
            return totalDebt;
        }


        
    }
}