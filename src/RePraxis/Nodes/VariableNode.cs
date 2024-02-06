namespace RePraxis
{
	/// <summary>
	/// A database node containing a variable
	/// </summary>
	public class VariableNode : Node<string>
	{
		public override NodeType NodeType => NodeType.VARIABLE;

		public VariableNode(string symbol, NodeCardinality cardinality)
			: base( symbol, symbol, cardinality )
		{ }

		public override bool EqualTo(INode other)
		{
			if ( other.NodeType != NodeType ) return false;

			return Value == ((SymbolNode)other).Value;
		}

		public override bool NotEqualTo(INode other)
		{
			if ( other.NodeType != NodeType ) return true;

			return Value != ((SymbolNode)other).Value;
		}

		public override bool GreaterThanEqualTo(INode other)
		{
			throw new NodeTypeException(
				$">= not defined between nodes of type {NodeType} and {other.NodeType}"
			);
		}

		public override bool LessThanEqualTo(INode other)
		{
			throw new NodeTypeException(
				$"<= not defined between nodes of type {NodeType} and {other.NodeType}"
			);
		}

		public override bool LessThan(INode other)
		{
			throw new NodeTypeException(
				$"< not defined between nodes of type {NodeType} and {other.NodeType}"
			);
		}

		public override bool GreaterThan(INode other)
		{
			throw new NodeTypeException(
				$"> not defined between nodes of type {NodeType} and {other.NodeType}"
			);
		}

		public override INode Copy()
		{
			return new VariableNode( Symbol, Cardinality );
		}
	}
}
