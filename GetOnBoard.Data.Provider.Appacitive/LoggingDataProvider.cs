using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Appacitive.Sdk;
using GetOnBoard.Core.Model;
using GetOnBoard.Data.Interfaces;
using GetOnBoard.Data.Provider.Appacitive.Constants;

namespace GetOnBoard.Data.Provider.Appacitive
{
    public class LoggingDataProvider : ILoggingDataProvider
    {
        public void LogException(Exception exception, string source, string method, Severity severity)
        {
            var log = new ExceptionLog(exception, source, method, severity);
            Article article = new Article(Schemas.Exception);
            article.Set("title",log.Title);
            article.Set("type", log.Type);
            article.Set("severity", log.Severity);
            article.Set("message", log.Message);
            article.Set("sessionid", log.SessionId);
            article.Set("machine", log.MachineName);
            article.Set("appdomain", log.AppDomainName);
            article.Set("source", log.Source);
            article.Set("targetsite", log.TargetSite);
            article.Set("statcktrace", log.StackTrace);
            article.Set("additioninfo", log.AdditionalInfo);
            article.Set("innerexception", log.InnerException);
            article.SaveAsync();
        }

        public void LogMessage(Log log)
        {
            Article article = new Article(Schemas.Log);
            article.Set("name",log.Name);
            article.Set("machine", log.MachineName);
            article.Set("sessionid", log.SessionId);
            article.Set("request", log.Request);
            article.Set("response", log.Response);
            article.Set("servicename", log.ServiceName);
            article.Set("status", log.Status);
            article.Set("timetaken", log.TimeTaken);
            article.SaveAsync();
        }
    }
}
