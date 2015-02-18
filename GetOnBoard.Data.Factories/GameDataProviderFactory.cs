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
    public static class GameDataProviderFactory
    {
        private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
        private static IGameDataProvider _provider = null;

        public static IGameDataProvider GetGameDataProvider()
        {
            if (_provider != null)
                return _provider;
            Lock.EnterReadLock();
            try
            {
                if (_provider == null)
                {
                    _provider = new GameDataProvider();
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
