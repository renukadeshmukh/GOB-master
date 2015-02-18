using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using GetOnBoard.Data.Interfaces;
using GetOnBoard.Data.Provider.Appacitive;

namespace GetOnBoard.Data.Factories
{
    public static class AccountDataProviderFactory
    {
        private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
        private static IAccountDataProvider _provider = null;

        public static IAccountDataProvider GetAccountDataProvider()
        {
            if (_provider != null)
            {
                return _provider;
            }
            Lock.EnterReadLock();
            try
            {
                if (_provider == null)
                {
                    _provider = new AccountDataProvider();
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
