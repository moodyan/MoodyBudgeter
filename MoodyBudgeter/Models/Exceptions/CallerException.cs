using System;

namespace MoodyBudgeter.Models.Exceptions
{
    public class CallerException : Exception
    {
        public CallerException(string message) : base(message) { }

        public CallerException(string message, Exception innerException) : base(message, innerException) { }
    }
}
