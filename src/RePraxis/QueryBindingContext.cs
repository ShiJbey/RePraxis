namespace RePraxis
{
	/// <summary>
	/// Used internally to track bindings for a single sentence and the database.
	/// </summary>
	public class QueryBindingContext
	{
		public Dictionary<string, string> Bindings { get; }
		public INode SubTree { get; }

		public QueryBindingContext(Dictionary<string, string> bindings, INode subTree)
		{
			Bindings = bindings;
			SubTree = subTree;
		}

		public QueryBindingContext(INode subtree)
			: this( new Dictionary<string, string>(), subtree ) { }
	}
}
