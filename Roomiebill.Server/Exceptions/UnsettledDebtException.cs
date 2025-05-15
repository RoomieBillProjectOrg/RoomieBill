using System;
using System.Diagnostics.CodeAnalysis;

namespace Roomiebill.Server.Exceptions
{
    [ExcludeFromCodeCoverage]
    public class UnsettledDebtException : Exception
    {
        public UnsettledDebtException(string message) : base(message)
        {
        }
    }
}
