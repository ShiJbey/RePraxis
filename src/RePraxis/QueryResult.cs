using System.Collections.Generic;
using System.Linq;
using System.Text;

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

		/// <summary>
		/// Return a prettified string explaining the result
		/// </summary>
		/// <returns></returns>
		public string ToPrettyString()
		{
			StringBuilder sb = new StringBuilder();

			sb.AppendLine( $"Result Success: {Success}" );
			sb.AppendLine( "Bindings: [" );

			for ( int i = 0; i < Bindings.Length; i++ )
			{
				sb.Append( $"[{i}] " );
				sb.Append( "{" );
				sb.Append(
				string.Join(
					", ", Bindings[i].Select( b => b.Key + "=" + b.Value ).ToArray()
					)
				);
				sb.Append( "}\n" );
			}
			sb.AppendLine( "]" );

			return sb.ToString();
		}
	}
}
