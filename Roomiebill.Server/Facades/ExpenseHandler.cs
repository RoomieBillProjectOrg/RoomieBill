using Roomiebill.Server.Models;
using Roomiebill.Server.DataAccessLayer.Dtos;
using System.Collections.Generic;
using Roomiebill.Server.Exceptions;

namespace Roomiebill.Server.Facades
{
    public class ExpenseHandler
    {
        // private int[] debtArray; // 1D array to store debts
        private Dictionary<int, int> _userIndexMap; //<UserId,Array index> map user id to index in the _debtMatrix to avoid not consistent user ids.
        private int _userCount; // number of users

        public ExpenseHandler(List<int> userIds)
        {
            _userIndexMap = new Dictionary<int, int>();
            for (int i = 0; i < _userCount; i++)
            {
                _userIndexMap[userIds[i]] = i;
            }
        }

        public ExpenseHandler(List<User> member)
        {
            List<int> userIds = new List<int>();
            this._userCount = member.Count;
            foreach (User user in member)
            {
                userIds.Add(user.Id);
            }
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
        //Get the index of the cell in the new debtArray 
        private int GetNewIndex(int i, int j, int newUserCount)
        {
            if (i > j) (i, j) = (j, i); // i should be less than j
            return i * (newUserCount - 1) - i * (i + 1) / 2 + (j - 1);
        }

        // Get the debt between 2 users - i owns to j- so if it is negative i owns j 0
        public int GetDebtBetween(int i, int j, int[] debtArray)
        {
            int index = GetIndex(i, j);
            int debt = debtArray[index];
            if (i < j)
            {
                return debt > 0 ? debt : 0; // Positive: i owes j; Zero or negative: i owes j nothing
            }
            else
            {
                return debt < 0 ? -debt : 0; // Negative: j owes i; Zero or positive: j owes i nothing
            }
        }
       
       // Get the debt between 2 users - i owns to j- so if it is negative i owns j 0
        public int GetDebtBetweenIndex(int i, int j, int[] debtArray)
        {
            int indexi = _userIndexMap[i];
            int indexj = _userIndexMap[j];
            int index = GetIndex(indexi, indexj);
            int debt = debtArray[index];
            if (indexi < indexj)
            {
                return debt > 0 ? debt : 0; // Positive: i owes j; Zero or negative: i owes j nothing
            }
            else
            {
                return debt < 0 ? -debt : 0; // Negative: j owes i; Zero or positive: j owes i nothing
            }
        }
        private void UpdateDebtArray(int i, int j, int amount, int[] debtArray)
        {
            int index = GetIndex(i, j);
            if (i < j)
            {
                debtArray[index] -= amount;
            }
            else
            {
                debtArray[index] += amount;
            }
        }

         public Expense AddExpense(Expense expense, int[] debtArray){
            int payerId = expense.PayerId;
            int payerIndex = _userIndexMap[payerId];
            foreach (ExpenseSplit split in expense.ExpenseSplits)
            {
                int userId = split.UserId;
                int userIndex = _userIndexMap[userId];
                if (userIndex == payerIndex)
                {
                    continue;
                }
                int amount = (int)(expense.Amount * (split.Percentage / 100.0));
                UpdateDebtArray(payerIndex, userIndex, amount, debtArray);
            }
            return expense;
         }

        // Get all debts between users (both sides  -i owns j and j owns i)
        public Dictionary<(int, int), int> GetAllDebts(int[] debtArray)
        {
            Dictionary<(int, int), int> debts = new Dictionary<(int, int), int>();
            for (int i = 0; i < _userCount; i++)
            {
                for (int j = 0; j < _userCount; j++)
                {
                    if (i != j)
                    {
                        int debt = GetDebtBetween(i, j, debtArray);
                        // if (debt > 0)
                        // {
                        debts.Add((i, j), debt);
                        // }
                    }
                }
            }
            return debts;
        }
        //settle all debt for a user and all other  users
        public void SettleAllDebts(int userId, int[] debtArray)
        {
            for (int i = 0; i < _userIndexMap.Keys.Count; i++)
            {
                if (i != userId)
                {
                    SettleDebt(userId, i, debtArray);
                }
            }
        }

        // Settle debt between two users
        public void SettleDebt(int i, int j, int[] debtArray)
        {
            int index = GetIndex(_userIndexMap[i], _userIndexMap[j]);
            debtArray[index] = 0;
        }

        public void DeleteExpense(Expense expense, int[] debtArray){
            int payerId = expense.PayerId;
            int payerIndex = _userIndexMap[payerId];
            foreach (ExpenseSplit split in expense.ExpenseSplits)
            {
                int userId = split.UserId;
                int userIndex = _userIndexMap[userId];
                if (userIndex == payerIndex)
                {
                    continue;
                }
                int amount = (int)(expense.Amount * (split.Percentage / 100.0));
                UpdateDebtArray(payerIndex, userIndex, -amount, debtArray); // Reverse the debt
            }
        }

        public Expense UpdateExpense(Expense oldExpense, Expense newExpense, int[] debtArray){
            DeleteExpense(oldExpense, debtArray); // Reverse the debt
            Expense newAddedExpense = AddExpense(newExpense, debtArray);
            return newAddedExpense;
        }

        // Get total debt for a specific user (the sum all users have to pay to the user)
        public double GetTotalDebtOwedToUser(int userId, int[] debtArray)
        {
            int userIndex = _userIndexMap[userId];
            double totalDebt = 0;
            for (int i = 0; i < _userCount; i++)
            {
                if (i != userIndex)
                {
                    totalDebt += GetDebtBetween(i, userIndex, debtArray);
                }
            }
            return totalDebt;
        }
         

        // Get total debt a specific user owes to all other users
        public double GetTotalDebtUserOwes(int userId, int[] debtArray)
        {
            int userIndex = _userIndexMap[userId];
            double totalDebt = 0;
            for (int i = 0; i < _userCount; i++)
            {
                if (i != userIndex)
                {
                    totalDebt += GetDebtBetween(userIndex, i, debtArray);
                }
            }
            return totalDebt;
        }

        public int[] EnlargeDebtArraySize(int newUserCount, int oldUserCount, int[] debtArray)
        {
            int newSize = (newUserCount * (newUserCount - 1)) / 2;
            int[] newDebtArray = new int[newSize];
            // Copy existing debt value from the old arrayto the position in the new array.
            for (int i = 0; i < oldUserCount; i++)
            {
                for (int j = i + 1; j < oldUserCount; j++)
                {
                    int oldIndex = GetIndex(i, j);
                    int newIndex = GetNewIndex(i, j,newUserCount);
                    newDebtArray[newIndex] = debtArray[oldIndex];
                }
            }
            this._userCount = newUserCount;
            return newDebtArray;
        }
        //TODO: add throw not settle debt exception


        public int[] ReduceDebtArraySize(int newUserCount, int oldUserCount, List<int> removedUsers, int[] debtArray)
        {
            int newSize = (newUserCount * (newUserCount - 1)) / 2;
            int[] newDebtArray = new int[newSize];
           

            // Create a set of removed user indices for quick lookup
            HashSet<int> removedUserIndices = new HashSet<int>();
            foreach (int userId in removedUsers)
            {
                removedUserIndices.Add(_userIndexMap[userId]);
            }
            //check for unsettled debts
            checkUnsettledDebts(removedUsers,oldUserCount, debtArray);

            // Copy existing data to the new array, excluding the removed users
            for (int i = 0; i < oldUserCount; i++)
            {
                if (removedUserIndices.Contains(i)) continue;
                for (int j = i + 1; j < oldUserCount; j++)
                {
                    if (removedUserIndices.Contains(j)) continue;
                    //Adjust Indices for Removed Users:
                    int oldIndex = GetIndex(i, j);
                    int newI = i - removedUserIndices.Count(u => u < i); //calculates the new index by subtracting the number of removed users that come before i.
                    int newJ = j - removedUserIndices.Count(u => u < j);
                    int newIndex = GetNewIndex(newI, newJ, newUserCount);
                    newDebtArray[newIndex] = debtArray[oldIndex];
                }
            }
            this._userCount = newUserCount;
            return newDebtArray;
        }

        private void checkUnsettledDebts(List<int> removedUsers,int oldUserCount,int[] debtArray)
        {
             // Check for unsettled debts
            foreach (int userId in removedUsers)
            {
                int userIndex = _userIndexMap[userId];
                for (int i = 0; i < oldUserCount; i++)
                {
                    if (i == userIndex) continue;
                    int debt = GetDebtBetween(userIndex, i, debtArray);
                    if (debt != 0)
                    {
                        int id = GetUserId(i);
                        throw new UnsettledDebtException($"User {userId} owns {debt} to user {id}.");
                    }
                }
            }
        }
        //
        private int GetUserId(int userIndex)
        {
            int userId = -1;
            foreach (KeyValuePair<int, int> entry in _userIndexMap)
            {
                if (entry.Value == userIndex)
                {
                    userId = entry.Key;
                }
        }
            return userId;
        }

        public int[]  UpdateDebtBetweenIndex(int payerIndex, int userIndex, int amount, int[] debtArray)
        {
            UpdateDebtArray(payerIndex, userIndex, amount, debtArray);
            return debtArray;
        }

        public void AddUserToUserIndexMap(int userId)
        {
            _userIndexMap[userId] = userId;
        }
    }
}