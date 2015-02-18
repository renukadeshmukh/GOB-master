using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GetOnBoard.Core.Model;
using GetOnBoard.Data.Factories;

namespace GetOnBoard.Core.Infra
{
    public class AppacitiveTraceListener : TraceListener
    {
        public override void Write(string message)
        {
            LogMessage(message);
        }

        public override void WriteLine(string message)
        {
            LogMessage(message);
        }

        private static void LogMessage(string message)
        {
            var logData = JsonDataObjectParser.ParseInstanceData(message);
            if (!string.Equals(logData["url"], "https://apis.appacitive.com/article/log")) // avoid cycle.
            {
                Log log = new Log();
                log.Name = "AppacitiveTrace";
                log.ServiceName = string.Format("{0}|{1}", logData["method"], logData["url"]);
                log.SessionId = logData["referenceId"];
                log.Response = message;

                int timeTaken = int.MinValue;
                int.TryParse(logData["responseTime"], out timeTaken);
                log.TimeTaken = Convert.ToDecimal(new TimeSpan(0, 0, 0, 0, timeTaken).TotalSeconds);

                var responseData = JsonDataObjectParser.ParseInstanceData(logData["response"]);
                var statusData = JsonDataObjectParser.ParseInstanceData(responseData["status"]);
                if (string.Equals("200", statusData["code"]))
                {
                    log.Status = Status.Success;
                }
                else
                {
                    log.Status = Status.Failure;
                }
                var loggingDataProvider = LoggingDataProviderFactory.GetLoggingDataProvider();
                loggingDataProvider.LogMessage(log);
            }
        }
    }
}
