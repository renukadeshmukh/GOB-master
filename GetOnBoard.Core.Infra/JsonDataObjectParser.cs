using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace GetOnBoard.Core.Infra
{
    public class JsonDataObjectParser
    {
        internal static DataObject ParseInstanceData(string jsonString)
        {
            var data = new DataObject();
            if (string.IsNullOrWhiteSpace(jsonString) == true)
                return data;
            var json = JObject.Parse(jsonString);
            foreach (var property in json.Properties())
            {
                if (property.Value.Type != JTokenType.Null)
                    data[property.Name] = property.Value.ToString();
            }
            return data;
        }
    }
}
