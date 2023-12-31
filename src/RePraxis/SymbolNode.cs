namespace RePraxis
{
	/// <summary>
	/// A node within a RePraxis database that stores a symbol/string value
	/// </summary>
	public class SymbolNode : Node<string>
	{
		public override NodeType NodeType => NodeType.SYMBOL;

		public SymbolNode(string symbol, NodeCardinality cardinality)
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
			if ( other.NodeType != NodeType )
			{
				throw new NodeTypeException(
					$">= not defined between nodes of type {NodeType} and {other.NodeType}"
				);
			};

			int result = Value.CompareTo( ((SymbolNode)other).Value );

			return result > 0 || result == 0;
		}

		public override bool LessThanEqualTo(INode other)
		{
			if ( other.NodeType != NodeType )
			{
				throw new NodeTypeException(
					$">= not defined between nodes of type {NodeType} and {other.NodeType}"
				);
			};

			int result = Value.CompareTo( ((SymbolNode)other).Value );

			return result < 0 || result == 0;
		}

		public override bool LessThan(INode other)
		{
			if ( other.NodeType != NodeType )
			{
				throw new NodeTypeException(
					$">= not defined between nodes of type {NodeType} and {other.NodeType}"
				);
			};

			int result = Value.CompareTo( ((SymbolNode)other).Value );

			return result < 0;
		}

		public override bool GreaterThan(INode other)
		{
			if ( other.NodeType != NodeType )
			{
				throw new NodeTypeException(
					$">= not defined between nodes of type {NodeType} and {other.NodeType}"
				);
			};

			int result = Value.CompareTo( ((SymbolNode)other).Value );

			return result > 0;
		}

		public override INode Copy()
		{
			return new SymbolNode( Symbol, Cardinality );
		}

	}
}
