using System.Collections.Generic;

namespace RePraxis
{
	/// <summary>
	/// The intermediate state of a query while processing expressions
	/// </summary>
	public class QueryState
	{
		#region Properties

		/// <summary>
		/// Did all statements in the query pass or evaluate to true
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// Bindings for any variables present in the query
		/// </summary>
		public List<Dictionary<string, INode>> Bindings { get; }

		#endregion

		#region Constructors

		public QueryState(bool success, IEnumerable<Dictionary<string, INode>> bindings)
		{
			Success = success;
			Bindings = new List<Dictionary<string, INode>>( bindings );
		}

		public QueryState(bool success, IEnumerable<Dictionary<string, object>> bindings)
		{
			Success = success;
			Bindings = new List<Dictionary<string, INode>>();

			foreach ( var entry in bindings )
			{
				Dictionary<string, INode> convertedBindings = new Dictionary<string, INode>();

				foreach ( var (varName, value) in entry )
				{
					convertedBindings[varName] = RePraxisHelpers.CreateNode( value );
				}

				Bindings.Add( convertedBindings );
			}
		}

		public QueryState(bool success)
		{
			Success = success;
			Bindings = new List<Dictionary<string, INode>>();
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Convert the <c>QueryState</c> to a corresponding <c>QueryResult</c>
		/// </summary>
		/// <returns></returns>
		public QueryResult ToResult()
		{
			if ( !Success ) return new QueryResult( false );

			Dictionary<string, object>[] results = new Dictionary<string, object>[Bindings.Count];

			for ( int i = 0; i < Bindings.Count; i++ )
			{
				results[i] = new Dictionary<string, object>();
				foreach ( var (varName, node) in Bindings[i] )
				{
					results[i][varName] = node.GetValue();
				}
			}

			return new QueryResult( true, results );
		}

		#endregion
	}
}
