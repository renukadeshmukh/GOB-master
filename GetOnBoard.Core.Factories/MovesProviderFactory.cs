using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GetOnBoard.Core.GameMgmt;
using GetOnBoard.Core.Interfaces;
using GetOnBoard.Core.MovesMgmt;
using GetOnBoard.Data.Factories;
using GetOnBoard.Data.Interfaces;

namespace GetOnBoard.Core.Factories
{
    public static class MovesProviderFactory
    {
        private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
        private static IMovesProvider _provider = null;

        public static IMovesProvider GetGameProvider()
        {
            if (_provider != null)
                return _provider;
            Lock.EnterReadLock();
            try
            {
                if (_provider == null)
                {
                    _provider = new MovesProvider(CacheProviderFactory.GetCacheProvider(), GameProviderFactory.GetGameProvider(), MovesDataProviderFactory.GetMovesDataProvider());
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
