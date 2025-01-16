using System;

namespace Roomiebill.Server.Exceptions
{
    public class UnsettledDebtException : Exception
    {
        public UnsettledDebtException(string message) : base(message)
        {
        }
    }
}