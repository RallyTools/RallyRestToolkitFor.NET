using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;

namespace Rally.RestApi
{
    public class DynamicJsonSerializer
    {
        readonly JavaScriptSerializer deSerializer;
        public DynamicJsonSerializer()
        {
            deSerializer = new JavaScriptSerializer();
            deSerializer.MaxJsonLength = int.MaxValue;
            deSerializer.RegisterConverters(new JavaScriptConverter[] { new DynamicJsonConverter() });
        }
        // as dynamic
        public DynamicJsonObject Deserialize(string json)
        {
            return deSerializer.Deserialize(json, typeof(object)) as DynamicJsonObject;
        }
        public string Serialize(DynamicJsonObject value)
        {
            return SerializeDictionary(value.Dictionary);
        }
        public string SerializeDictionary(IDictionary<string, object> dictionary)
        {
            var builder = new StringBuilder();
            builder.Append("{");
            var first = true;
            foreach (var key in dictionary.Keys)
            {
                if (first) first = false;
                else builder.Append(",");
                builder.Append("\"");
                builder.Append(key);
                builder.Append("\"");
                builder.Append(":");
                if (dictionary[key] is IDictionary<string, object>)
                    builder.Append(SerializeDictionary(dictionary[key] as IDictionary<string, object>));
                else if (dictionary[key] is ArrayList)
                    builder.Append(SerializeArray(dictionary[key] as ArrayList));
                else
                    builder.Append(SerializeObject(dictionary[key]));
            }
            builder.Append("}");
            return builder.ToString();
        }
        private string SerializeArray(ArrayList list)
        {
            var builder = new StringBuilder();
            var first = true;
            builder.Append("[");
            foreach (var obj in list)
            {
                if (first) first = false;
                else builder.Append(",");
                if (obj is IDictionary<string, object>)
                {
                    builder.Append(SerializeDictionary(obj as IDictionary<string, object>));
                }
                else if (obj is ArrayList)
                {
                    builder.Append(SerializeArray(obj as ArrayList));
                }
                else
                {
                    builder.Append(SerializeObject(obj));
                }
            }
            builder.Append("]");
            return builder.ToString();
        }
        private static string SerializeObject(object obj)
        {
            if (obj is string)
            {
                return "\"" + ((String)obj).Replace("\"", "\\\"") + "\"";
            }
            return obj.ToString();
        }
    }
}
