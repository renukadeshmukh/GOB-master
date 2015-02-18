using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Appacitive.Sdk;
using Appacitive.Sdk.Net45;

namespace GetOnBoard.Data.Provider.Appacitive.AppHelper
{
    public static class AppInitializer
    {
        private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();
        private static bool _isAppInitialized = false;

        public static bool Initialize()
        {
            if (_isAppInitialized)
            {
                return true;
            }
            Lock.EnterReadLock();
            try
            {
                if (_isAppInitialized == false)
                {
                    App.Initialize(WindowsHost.Instance, AppConfigurations.AppacitiveKey, AppConfigurations.AppacitiveEnviornment);
                    if (AppConfigurations.IsLoggingEnabled)
                    {
                        App.Debug.ApiLogging.LogFailures().LogSlowCalls(AppConfigurations.LogCallsSlowerThan);
                    }
                    _isAppInitialized = true;
                }
            }
            finally
            {
                Lock.ExitReadLock();
            }
            return _isAppInitialized;
        }
    }
}
