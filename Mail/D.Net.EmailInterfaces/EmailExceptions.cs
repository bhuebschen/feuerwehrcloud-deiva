using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace D.Net.EmailInterfaces
{
    public enum EMAIL_EXCEPTION_TYPE
    {
        NONE,
        NO_FOLDER_SELECTED,
        ERROR_ON_GET_MESSAGES,
        ERROR_ON_GET_MESSAGE,
        ERROR_ON_CONNECTION,
        NOT_CONNECTED
    }

    public class EMailException : Exception
    {
        public EMAIL_EXCEPTION_TYPE ExceptionType { get; set; }
        public Exception InnerException { get; set; }
    }
}
