using System;

namespace MoodyBudgeter.Models.Exceptions
{
    /// <summary>
    /// Exception used to give feedback to the user. This generates a 409 error code. A ErrorCode must be used for front end localization
    /// </summary>
    public class FriendlyException : Exception
    {
        public string ErrorCode { get; set; }

        public FriendlyException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }

        public FriendlyException(string errorCode, string message, Exception innerException) : base(message, innerException)
        {
            ErrorCode = errorCode;
        }
    }
}
