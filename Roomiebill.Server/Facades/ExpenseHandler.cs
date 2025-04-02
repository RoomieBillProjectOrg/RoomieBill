using Roomiebill.Server.Models;
using Roomiebill.Server.Exceptions;

namespace Roomiebill.Server.Facades
{
    public class ExpenseHandler
    {
        public Dictionary<int, int> _userIndexMap = new Dictionary<int, int>(); //<UserId,Array index> map user id to index in the _debtMatrix to avoid not consistent user ids.
        
        public int _userCount = 0; // number of users

        public ExpenseHandler() { }

        public ExpenseHandler(List<User> member)
        {
            List<int> userIds = new List<int>();

            _userCount = member.Count;

            foreach (User user in member)
            {
                userIds.Add(user.Id);
            }

            // Ensure userIds has the same count as _userCount
            if (userIds.Count != _userCount)
            {
                throw new InvalidOperationException("The number of user IDs does not match the user count.");
            }

            for (int i = 0; i < _userCount; i++)
            {
                _userIndexMap[userIds[i]] = i;
            }
        }

        // Get the index of the cell in the debtArray given 2 ids of users
        public int GetIndex(int i, int j)
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
        public double GetDebtBetween(int i, int j, double[] debtArray)
        {
            int index = GetIndex(i, j);
            double debt = debtArray[index];
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
        public double GetDebtBetweenIndex(int i, int j, double[] debtArray)
        {
            int indexi = _userIndexMap[i];
            int indexj = _userIndexMap[j];
            int index = GetIndex(indexi, indexj);
            double debt = debtArray[index];
            if (indexi < indexj)
            {
                return debt > 0 ? debt : 0; // Positive: i owes j; Zero or negative: i owes j nothing
            }
            else
            {
                return debt < 0 ? -debt : 0; // Negative: j owes i; Zero or positive: j owes i nothing
            }
        }

        private void UpdateDebtArray(int i, int j, double amount, double[] debtArray)
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

        public Expense AddExpense(Expense expense, double[] debtArray)
        {
            int payerId = expense.PayerId;
            int payerIndex = _userIndexMap[payerId];
            
            // Verify total split amounts equal expense amount
            double totalSplitAmount = expense.ExpenseSplits.Sum(s => s.Amount);
            if (Math.Abs(totalSplitAmount - expense.Amount) > 0.01) // Using small epsilon for double comparison
            {
                throw new InvalidOperationException($"Sum of split amounts ({totalSplitAmount}) must equal total expense amount ({expense.Amount})");
            }

            foreach (ExpenseSplit split in expense.ExpenseSplits)
            {
                int userId = split.UserId;
                int userIndex = _userIndexMap[userId];
                if (userIndex == payerIndex)
                {
                    continue;
                }
                double amount = split.Amount;
                if (split.UserId == payerId)
                {
                    continue;
                }
                UpdateDebtArray(payerIndex, userIndex, amount, debtArray);
            }
            return expense;
        }

        // Get all debts between users (both sides  -i owns j and j owns i)
        public Dictionary<(int, int), double> GetAllDebts(double[] debtArray)
        {
            Dictionary<(int, int), double> debts = new Dictionary<(int, int), double>();
            for (int i = 0; i < _userCount; i++)
            {
                for (int j = 0; j < _userCount; j++)
                {
                    if (i != j)
                    {
                        double debt = GetDebtBetween(i, j, debtArray);
                        debts.Add((i, j), debt);
                    }
                }
            }
            return debts;
        }

        //settle all debt for a user and all other users
        public void SettleAllDebts(int userId, double[] debtArray)
        {
            if (!_userIndexMap.ContainsKey(userId))
            {
                throw new KeyNotFoundException($"User ID {userId} not found in the user index map.");
            }

            for (int i = 0; i < _userIndexMap.Keys.Count; i++)
            {
                if (i != userId)
                {
                    SettleDebt(userId, i, debtArray);
                }
            }
        }

        // Settle debt between two users
        public void SettleDebt(int i, int j, double[] debtArray)
        {
            if (!_userIndexMap.ContainsKey(i) || !_userIndexMap.ContainsKey(j))
            {
                throw new KeyNotFoundException($"User ID {i} or {j} not found in the user index map.");
            }

            int index = GetIndex(_userIndexMap[i], _userIndexMap[j]);
            debtArray[index] = 0;
        }

        public void DeleteExpense(Expense expense, double[] debtArray)
        {
            int payerId = expense.PayerId;
            int payerIndex = _userIndexMap[payerId];
            
            // Verify total split amounts equal expense amount
            double totalSplitAmount = expense.ExpenseSplits.Sum(s => s.Amount);
            if (Math.Abs(totalSplitAmount - expense.Amount) > 0.01)
            {
                throw new InvalidOperationException($"Sum of split amounts ({totalSplitAmount}) must equal total expense amount ({expense.Amount})");
            }

            foreach (ExpenseSplit split in expense.ExpenseSplits)
            {
                int userId = split.UserId;
                int userIndex = _userIndexMap[userId];
                if (userIndex == payerIndex)
                {
                    continue;
                }
                double amount = split.Amount;
                UpdateDebtArray(payerIndex, userIndex, -amount, debtArray); // Reverse the debt
            }
        }

        public Expense UpdateExpense(Expense oldExpense, Expense newExpense, double[] debtArray)
        {
            DeleteExpense(oldExpense, debtArray); // Reverse the debt
            Expense newAddedExpense = AddExpense(newExpense, debtArray);
            return newAddedExpense;
        }

        // Get total debt for a specific user (the sum all users have to pay to the user)
        public double GetTotalDebtOwedToUser(int userId, double[] debtArray)
        {
            if (!_userIndexMap.ContainsKey(userId))
            {
                throw new KeyNotFoundException($"User ID {userId} not found in the user index map.");
            }

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
        public double GetTotalDebtUserOwes(int userId, double[] debtArray)
        {
            if (!_userIndexMap.ContainsKey(userId))
            {
                throw new KeyNotFoundException($"User ID {userId} not found in the user index map.");
            }

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

        public double[] EnlargeDebtArraySize(int newUserCount, int oldUserCount, double[] debtArray)
        {
            int newSize = (newUserCount * (newUserCount - 1)) / 2;
            double[] newDebtArray = new double[newSize];
            // Copy existing debt value from the old arrayto the position in the new array.
            for (int i = 0; i < oldUserCount; i++)
            {
                for (int j = i + 1; j < oldUserCount; j++)
                {
                    int oldIndex = GetIndex(i, j);
                    int newIndex = GetNewIndex(i, j, newUserCount);
                    newDebtArray[newIndex] = debtArray[oldIndex];
                }
            }
            this._userCount = newUserCount;
            return newDebtArray;
        }

        public double[] ReduceDebtArraySize(int newUserCount, int oldUserCount, List<int> removedUsers, double[] debtArray)
        {
            int newSize = (newUserCount * (newUserCount - 1)) / 2;
            double[] newDebtArray = new double[newSize];

            // Create a set of removed user indices for quick lookup
            HashSet<int> removedUserIndices = new HashSet<int>();
            foreach (int userId in removedUsers)
            {
                removedUserIndices.Add(_userIndexMap[userId]);
            }
            //check for unsettled debts
            checkUnsettledDebts(removedUsers, oldUserCount, debtArray);

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

        private void checkUnsettledDebts(List<int> removedUsers, int oldUserCount, double[] debtArray)
        {
            // Check for unsettled debts
            foreach (int userId in removedUsers)
            {
                int userIndex = _userIndexMap[userId];
                for (int i = 0; i < oldUserCount; i++)
                {
                    if (i == userIndex) continue;
                    double debt = GetDebtBetween(userIndex, i, debtArray);
                    if (Math.Abs(debt) > 0.01)
                    {
                        int id = GetUserId(i);
                        throw new UnsettledDebtException($"User {userId} owns {debt} to user {id}.");
                    }
                }
            }
        }

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

        public double[] UpdateDebtBetweenIndex(int payerIndex, int userIndex, double amount, double[] debtArray)
        {
            UpdateDebtArray(payerIndex, userIndex, amount, debtArray);
            return debtArray;
        }

        public void AddUserToUserIndexMap(int userId)
        {
            int maxId = -1;
            foreach (int mID in _userIndexMap.Values)
            {
                if (mID > maxId)
                {
                    maxId = mID;
                }
            }
            _userIndexMap[userId] = maxId + 1;
        }
    }
}
