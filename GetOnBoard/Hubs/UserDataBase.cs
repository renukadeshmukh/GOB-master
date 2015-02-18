using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GetOnBoard.Hubs
{
    public static class UserDataBase
    {
        private static readonly ConcurrentDictionary<string, string> UserConnections = new ConcurrentDictionary<string, string>();

        public static void SaveUserConnection(string userName, string connectionId)
        {
            if(!string.IsNullOrWhiteSpace(userName))
                UserConnections[userName] = connectionId;
        }

        public static string GetUserConnectionId(string userName)
        {
            if (!string.IsNullOrWhiteSpace(userName) && UserConnections.ContainsKey(userName))
            {
                return UserConnections[userName];
            }
            return string.Empty;
        }

        public static void RemoveUserConnectionId(string userName)
        {
            if (!string.IsNullOrWhiteSpace(userName) && UserConnections.ContainsKey(userName))
            {
                string connectionId;
                UserConnections.TryRemove(userName, out connectionId);
            }
        }

        public static bool IsOnline(string userName)
        {
            if (UserConnections.ContainsKey(userName))
                return true;
            return false;
        }
    }
}