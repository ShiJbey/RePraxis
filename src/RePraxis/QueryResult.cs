using System.Collections.Generic;
using System.Linq;

namespace RePraxis
{
	/// <summary>
	/// Returned by DBQueries indicating if they passed/failed
	/// </summary>
	public class QueryResult
	{
		/// <summary>
		/// Did all statements in the query pass or evaluate to true
		/// </summary>
		public bool Success { get; }

		/// <summary>
		/// Bindings for any variables present in the query
		/// </summary>
		public Dictionary<string, string>[] Bindings { get; }


		public QueryResult(bool success, IEnumerable<Dictionary<string, string>> bindings)
		{
			Success = success;
			Bindings = bindings.ToArray();
		}

		public QueryResult(bool success)
		{
			Success = success;
			Bindings = new Dictionary<string, string>[0];
		}
	}
}
