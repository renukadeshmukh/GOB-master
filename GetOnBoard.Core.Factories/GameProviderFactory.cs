using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GetOnBoard.Core.GameMgmt;
using GetOnBoard.Core.Interfaces;
using GetOnBoard.Data.Factories;
using GetOnBoard.Data.Interfaces;

namespace GetOnBoard.Core.Factories
{
    public static class GameProviderFactory
    {
        private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
        private static IGameProvider _provider = null;

        public static IGameProvider GetGameProvider()
        {
            if (_provider != null)
                return _provider;
            Lock.EnterReadLock();
            try
            {
                if (_provider == null)
                {
                    _provider = new GameProvider(CacheProviderFactory.GetCacheProvider(), GameDataProviderFactory.GetGameDataProvider(), MovesDataProviderFactory.GetMovesDataProvider());
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
