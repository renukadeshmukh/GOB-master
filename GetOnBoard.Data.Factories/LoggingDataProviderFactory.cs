using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GetOnBoard.Data.Interfaces;
using GetOnBoard.Data.Provider.Appacitive;

namespace GetOnBoard.Data.Factories
{
    public static class LoggingDataProviderFactory
    {
        private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
        private static ILoggingDataProvider _provider = null;

        public static ILoggingDataProvider GetLoggingDataProvider()
        {
            if (_provider != null)
                return _provider;
            Lock.EnterReadLock();
            try
            {
                if (_provider == null)
                {
                    _provider = new LoggingDataProvider();
                }
            }
            finally
            {
                Lock.ExitReadLock();
            }
            return _provider;
        }
    }
}
