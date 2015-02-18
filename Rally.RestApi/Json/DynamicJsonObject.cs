using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Collections;
using System.Globalization;
using System.Web.Script.Serialization;

namespace Rally.RestApi.Json
{
	/// <summary>
	/// Representation of a Rally object
	/// </summary>
	[Serializable]
	public class DynamicJsonObject : DynamicObject
	{
		[ScriptIgnore]
		internal IDictionary<string, object> Dictionary { get; set; }

		/// <summary>
		/// Create a new object from the specified dictionary
		/// </summary>
		/// <param name="dictionary">A dictionary of members and values
		/// with which to initialize this object</param>
		public DynamicJsonObject(IDictionary<string, object> dictionary)
		{
			Dictionary = dictionary;
		}

		/// <summary>
		/// Create a new empty object
		/// </summary>
		public DynamicJsonObject()
			: this(new Dictionary<string, object>())
		{
		}

		/// <summary>
		/// Try to get the specified member
		/// </summary>
		/// <param name="binder">The member to get</param>
		/// <param name="result">The value</param>
		/// <returns>true if successful, false otherwise</returns>
		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			result = GetMember(binder.Name);
			return (result != null) ? true : false;
		}

		/// <summary>
		/// Get the value of the specified member
		/// Equivalent to using [name].
		/// </summary>
		/// <param name="name">The specified member</param>
		/// <returns>The value of the specified member</returns>
		private dynamic GetMember(string name)
		{
			// Prevent exceptions
			if (!Dictionary.ContainsKey(name))
				return null;

			var result = Dictionary[name];

			if (result is IDictionary<string, object>)
			{
				result = new DynamicJsonObject(result as IDictionary<string, object>);
			}
			else if (result is ArrayList)
			{
				var list = new ArrayList();
				foreach (var i in result as ArrayList)
				{
					if (i is IDictionary<string, object>)
					{
						list.Add(new DynamicJsonObject(i as IDictionary<string, object>));
					}
					else
						list.Add(i);
				}
				result = list;
			}

			return result;
		}

		/// <summary>
		/// Format the specified value into a type
		/// that is compatible with DynamicJsonObject
		/// </summary>
		/// <param name="value">The value to be formatted</param>
		/// <returns>The formatted item</returns>
		private object FormatSetValue(object value)
		{
			if (value == null)
			{
				return null;
			}
			if (value is DynamicJsonObject)
			{
				var valueDictionary = (value as DynamicJsonObject).Dictionary;
				return new Dictionary<string, object>(valueDictionary);
			}
			if (value is string)
			{
				return value;
			}
			if (value is IEnumerable)
			{
				var list = new ArrayList();
				foreach (var i in value as IEnumerable)
				{
					list.Add(FormatSetValue(i));
				}
				return list;
			}
			if (value is bool)
			{
				return value;
			}
			if (IsNumeric(value))
			{
				return value;
			}
			throw new ArgumentException("Attempt to set property to an unsupported type.");
		}

		/// <summary>
		/// Attempt to the specified member's value
		/// </summary>
		/// <param name="binder">The member</param>
		/// <param name="value">The value</param>
		/// <returns>true if successful, false otherwise</returns>
		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			try
			{
				SetMember(binder.Name, value);
				return true;
			}
			catch
			{
				return false;
			}
		}

		/// <summary>
		/// Set the specified member's value.
		/// Equivalent to using [name] = value.
		/// </summary>
		/// <param name="name">The member</param>
		/// <param name="value">The value</param>
		private void SetMember(string name, dynamic value)
		{
			if (Dictionary.ContainsKey(name))
			{
				Dictionary[name] = FormatSetValue(value);
			}
			else
			{
				Dictionary.Add(name, FormatSetValue(value));
			}

		}

		/// <summary>
		/// Determine if the specified expression is numeric
		/// </summary>
		/// <param name="expression">The expression to be evaluated</param>
		/// <returns>true if numeric, false otherwise</returns>
		public static bool IsNumeric(object expression)
		{
			if (expression == null)
				return false;

			double number;
			return Double.TryParse(Convert.ToString(expression, CultureInfo.InvariantCulture), NumberStyles.Any, NumberFormatInfo.InvariantInfo, out number);
		}

		/// <summary>
		/// Get the hash code for this object
		/// </summary>
		/// <returns>The hash code for this object</returns>
		public override int GetHashCode()
		{
			return Dictionary.GetHashCode();
		}

		/// <summary>
		/// Get the value of the specified member
		/// </summary>
		/// <param name="key">The member to retrieve</param>
		/// <returns>The value of the specified member</returns>
		public dynamic this[string key]
		{
			get
			{
				return GetMember(key);
			}
			set
			{
				SetMember(key, value);
			}
		}

		/// <summary>
		/// Determine if this object contains the specified member
		/// </summary>
		/// <param name="key">The member to search for</param>
		/// <returns>true if it is present, false otherwise</returns>
		public bool HasMember(string key)
		{
			return Dictionary.ContainsKey(key);
		}

		/// <summary>
		/// Get the members contained in this object.
		/// </summary>
		public IEnumerable<string> Fields { get { return Dictionary.Keys; } }

		#region Equality

		/// <summary>
		/// Return whether this object equals the specified object
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>true if equal, false otherwise</returns>
		public override bool Equals(object obj)
		{
			var json = obj as DynamicJsonObject;
			return json != null && CompareObject(Dictionary, json.Dictionary);
		}

		private bool CompareObject(object obj1, object obj2)
		{
			if (obj1.GetType() != obj2.GetType())
				return false;
			if (obj1 is IDictionary<string, object>)
			{
				return CompareLists((obj1 as IDictionary<string, object>), (obj2 as IDictionary<string, object>));
			}
			if (obj1 is string)
			{
				return (obj1 as string).Equals(obj2 as string);
			}
			if (obj1 is IEnumerable)
			{
				return CompareEnumerable((obj1 as IEnumerable), (obj2 as IEnumerable));
			}
			if (obj1 is int)
			{
				return ((int)obj1).Equals((int)obj2);
			}
			if (obj1 is decimal)
			{
				return ((decimal)obj1).Equals((decimal)obj2);
			}
			if (obj1 is double)
			{
				return ((double)obj1).Equals((double)obj2);
			}
			return obj1.Equals(obj2);
		}


		private bool CompareEnumerable(IEnumerable en1, IEnumerable en2)
		{
			var two = en2.GetEnumerator();
			var one = en1.GetEnumerator();
			while (one.MoveNext())
			{
				// returns if the second contains fewer elements
				if (!two.MoveNext())
					return false;
				if (!CompareObject(two.Current, one.Current))
				{
					return false;
				}
			}
			return true;
		}


		private bool CompareLists(IDictionary<string, object> list1, IDictionary<string, object> list2)
		{
			if (list1 == null && list2 == null)
				return true;
			if (list1 == null || list2 == null)
				return false;
			foreach (var key in list1.Keys)
			{
				if (!list2.ContainsKey(key))
					return false;
				if (!CompareObject(list1[key], list2[key]))
					return false;
			}
			return true;
		}
		#endregion

	}
}
