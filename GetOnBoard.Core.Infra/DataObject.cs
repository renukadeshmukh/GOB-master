using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetOnBoard.Core.Infra
{
    public class DataObject : IEnumerable<KeyValuePair<string, string>>
    {
        private IDictionary<string, string> _items = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        public IEnumerator<KeyValuePair<string, string>> GetEnumerator()
        {
            return _items.ToList().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Case insensitive indexer over the data object.
        /// </summary>
        /// <param name="property">Property name.</param>
        /// <returns>String value for the property. Returns null if the property does not exist.</returns>
        public string this[string property]
        {
            get
            {
                string value = null;
                if (_items.TryGetValue(property, out value) == true)
                    return value;
                else return null;
            }
            set
            {
                _items[property] = value;
            }
        }

        public IEnumerable<string> Properties
        {
            get
            {
                return _items.Keys;
            }
        }
    }
}
