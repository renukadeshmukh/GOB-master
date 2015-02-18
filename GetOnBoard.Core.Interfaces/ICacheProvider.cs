using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetOnBoard.Core.Interfaces
{
    public interface ICacheProvider
    {
        bool AddValue(string key, object value);

        bool Exists(string key);

        object GetValue(string key);

        bool Remove(string key);
    }
}
