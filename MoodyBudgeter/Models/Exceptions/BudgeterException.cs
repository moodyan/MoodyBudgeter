using System;

namespace MoodyBudgeter.Models.Exceptions
{
    public class BudgeterException : Exception
    {
        public BudgeterException(string message) : base(message) { }

        public BudgeterException(string message, Exception innerException) : base(message, innerException) { }
    }
}
