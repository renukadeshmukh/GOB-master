using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;
using GetOnBoard.Core.Interfaces;

namespace GetOnBoard.Core.Caching.Http
{
    public class HttpRuntimeCacheProvider : ICacheProvider
    {
        #region ICacheProvider Members

        private const string Source = "HttpRuntimeCache";
        private const string CompletedSuffix = "Completed";

        public bool AddValue(string key, object value)
        {
            try
            {
                HttpRuntime.Cache.Add(key, value, null, Cache.NoAbsoluteExpiration, new TimeSpan(1, 0, 0),
                                      CacheItemPriority. NotRemovable,
                                      null);
            }
            catch (Exception ex)
            {
                return false;
            }
            return true;
        }

        public bool Exists(string key)
        {
            try
            {
                return HttpRuntime.Cache[key] != null;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public object GetValue(string key)
        {
            try
            {
                return HttpRuntime.Cache[key];
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool Remove(string key)
        {
            try
            {
                return HttpRuntime.Cache.Remove(key) != null;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        #endregion
    }
}
