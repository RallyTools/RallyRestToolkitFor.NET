using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace Rally.RestApi.Json
{
	internal class DynamicJsonConverter : JavaScriptConverter
	{
		public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
		{
			if (dictionary == null)
				throw new ArgumentNullException("dictionary");

			return type == typeof(object) ? new DynamicJsonObject(dictionary) : null;
		}

		public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
		{
			throw new NotImplementedException();
		}

		public override IEnumerable<Type> SupportedTypes
		{
			get { return new List<Type>(new[] { typeof(object) }).AsReadOnly(); }
		}
	}
}
