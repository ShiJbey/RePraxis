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
		#region Properties

		/// <summary>
		/// Did all statements in the query pass or evaluate to true
		/// </summary>
		public bool Success { get; }

		/// <summary>
		/// Bindings for any variables present in the query
		/// </summary>
		public Dictionary<string, object>[] Bindings { get; }

		#endregion

		#region Constructors

		public QueryResult(bool success, IEnumerable<Dictionary<string, object>> bindings)
		{
			Success = success;
			Bindings = bindings.ToArray();
		}

		public QueryResult(bool success)
		{
			Success = success;
			Bindings = new Dictionary<string, object>[0];
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Limit query bindings to a subset of select variables.
		/// </summary>
		/// <param name="vars"></param>
		/// <returns></returns>
		public QueryResult LimitToVar(params string[] vars)
		{
			if ( !Success )
			{
				return new QueryResult( false );
			}

			if ( vars.Length == 0 )
			{
				return new QueryResult( true );
			}

			Dictionary<string, object>[] filteredResults =
				new Dictionary<string, object>[Bindings.Length];

			for ( int i = 0; i < Bindings.Length; i++ )
			{
				filteredResults[i] = new Dictionary<string, object>();

				foreach ( var varName in vars )
				{
					filteredResults[i][varName] = Bindings[i][varName];
				}
			}

			return new QueryResult( true, filteredResults );
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

		#endregion
	}
}
