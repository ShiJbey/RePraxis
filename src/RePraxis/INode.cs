namespace RePraxis
{
	/// <summary>
	/// An interface implemented by all nodes in the database
	/// </summary>
	public interface INode
	{
		/// <summary>
		/// Get the type of the node (simplifies reflection)
		/// </summary>
		public NodeType NodeType { get; }

		/// <summary>
		/// Get the symbol associated with the node in the database
		/// </summary>
		public string Symbol { get; }

		/// <summary>
		/// How many children is the node allowed to have at one time
		/// </summary>
		public NodeCardinality Cardinality { get; }

		/// <summary>
		/// The children of the node
		/// </summary>
		public IEnumerable<INode> Children { get; }

		/// <summary>
		/// A reference to the node's parent node.
		/// </summary>
		public INode? Parent { get; set; }

		/// <summary>
		/// Get the value associated with this node
		/// </summary>
		public object GetValue();

		/// <summary>
		/// Check if the nodes's value is equal to another
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool EqualTo(INode other);

		/// <summary>
		/// Check if the node's value is not equal to another
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool NotEqualTo(INode other);

		/// <summary>
		/// Check if the node's value is greater than or equal to another
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool GreaterThanEqualTo(INode other);

		/// <summary>
		/// Check if the node's value is less than or equal to another
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool LessThanEqualTo(INode other);

		/// <summary>
		/// Check if the node's value is less than another
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool LessThan(INode other);

		/// <summary>
		/// Check if the node's value is greater than another
		/// </summary>
		/// <param name="other"></param>
		/// <returns></returns>
		public bool GreaterThan(INode other);

		/// <summary>
		/// Add a new child node to this node
		/// </summary>
		/// <param name="node"></param>
		public void AddChild(INode node);

		/// <summary>
		/// Removes a child node from the DBNode
		/// </summary>
		/// <param name="symbol"></param>
		/// <returns>True if successful</returns>
		public bool RemoveChild(string symbol);

		/// <summary>
		/// Get a child node
		/// </summary>
		/// <param name="symbol"></param>
		/// <returns>The node with the given symbol</returns>
		public INode GetChild(string symbol);

		/// <summary>
		/// Check if the node has a child
		/// </summary>
		/// <param name="symbol"></param>
		/// <returns>
		/// True if a child is present with the given symbol.
		/// False otherwise.
		/// </returns>
		public bool HasChild(string symbol);

		/// <summary>
		/// Remove all children and from this node
		/// </summary>
		public void ClearChildren();

		/// <summary>
		/// Get the database sentence this node represents
		/// </summary>
		public string GetPath();

		/// <summary>
		/// Create a copy of the node
		/// </summary>
		/// <returns></returns>
		public INode Copy();
	}
}
