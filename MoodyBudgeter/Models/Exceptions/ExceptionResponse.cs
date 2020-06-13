using System;

namespace MoodyBudgeter.Models.Exceptions
{
    public class ExceptionResponse
    {
        public string Message { get; set; }
        public Exception Exception { get; set; }
    }
}
