using System.Collections.Generic;

namespace RePraxis
{
	/// <summary>
	/// Used internally to track bindings for a single sentence and the database.
	/// </summary>
	public class QueryBindingContext
	{
		#region Properties

		/// <summary>
		/// Variable names mapped to their bound node values
		/// </summary>
		public Dictionary<string, INode> Bindings { get; }

		/// <summary>
		/// The subtree of the database that these bindings apply to.
		/// </summary>
		public INode SubTree { get; }

		#endregion

		#region Constructors

		public QueryBindingContext(Dictionary<string, INode> bindings, INode subTree)
		{
			Bindings = bindings;
			SubTree = subTree;
		}

		public QueryBindingContext(INode subtree)
			: this( new Dictionary<string, INode>(), subtree ) { }

		#endregion
	}
}
