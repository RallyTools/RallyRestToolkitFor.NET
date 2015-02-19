using Rally.RestApi.Exceptions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Web.Script.Serialization;

namespace Rally.RestApi.Json
{
	/// <summary>
	/// A class for serializing/deserizalizing dynamic JSON objects.
	/// </summary>
	internal class DynamicJsonSerializer
	{
		readonly JavaScriptSerializer deSerializer;
		/// <summary>
		/// Constructor
		/// </summary>
		internal DynamicJsonSerializer()
		{
			deSerializer = new JavaScriptSerializer();
			deSerializer.MaxJsonLength = int.MaxValue;
			deSerializer.RegisterConverters(new JavaScriptConverter[] { new DynamicJsonConverter() });
		}

		/// <summary>
		/// Deserializes a JSON data string into a dynamic JSON Object.
		/// </summary>
		/// <exception cref="RallyUnavailableException">Rally returned an HTML page. This usually occurs when Rally is off-line. Please check the ErrorMessage property for more information.</exception>
		/// <exception cref="RallyFailedToDeserializeJson">The JSON returned by Rally was not able to be deserialized. Please check the JsonData property for what was returned by Rally.</exception>
		internal DynamicJsonObject Deserialize(string json)
		{
			try
			{
				return deSerializer.Deserialize(json, typeof(object)) as DynamicJsonObject;
			}
			catch (Exception e)
			{
				if ((!String.IsNullOrWhiteSpace(json)) && (json.StartsWith("<!DOCTYPE HTML")))
				{
					throw new RallyUnavailableException(e, json);
				}
				else
				{
					throw new RallyFailedToDeserializeJson(e, json);
				}
			}
		}

		/// <summary>
		/// Serializes a dynamic JSON Object into a string.
		/// </summary>
		internal string Serialize(DynamicJsonObject value)
		{
			return SerializeDictionary(value.Dictionary);
		}

		/// <summary>
		/// Serializes a dictionary into a string.
		/// </summary>
		internal string SerializeDictionary(IDictionary<string, object> dictionary)
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
			if (obj == null)
			{
				return "null";
			}

			if (obj is string)
			{
				return "\"" + ((String)obj).Replace(@"\", @"\\").Replace("\"", "\\\"") + "\"";
			}
			return obj.ToString();
		}
	}
}
