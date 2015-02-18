using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GetOnBoard.Core.Caching.Http;
using GetOnBoard.Core.Interfaces;

namespace GetOnBoard.Core.Factories
{
    public static class CacheProviderFactory
    {
        private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
        private static ICacheProvider _provider = null;

        public static ICacheProvider GetCacheProvider()
        {
            if (_provider != null)
                return _provider;
            Lock.EnterReadLock();
            try
            {
                if (_provider == null)
                {
                    _provider = new HttpRuntimeCacheProvider();
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
