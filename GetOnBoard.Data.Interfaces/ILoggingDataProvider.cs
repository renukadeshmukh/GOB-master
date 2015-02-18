using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GetOnBoard.Core.Model;

namespace GetOnBoard.Data.Interfaces
{
    public interface ILoggingDataProvider
    {
        void LogException(Exception exception, string source, string method, Severity severity);
        void LogMessage(Log log);
    }
}
