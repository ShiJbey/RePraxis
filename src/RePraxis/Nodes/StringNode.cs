namespace RePraxis
{
	public class StringNode: Node<string>
	{
		public StringNode(string value, NodeCardinality cardinality) : base( $"\"{value}\"", value, cardinality )
		{ }

		public override NodeType NodeType => NodeType.STRING;

		public override INode Copy()
		{
			return new StringNode( Value, Cardinality );
		}

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
	}
}
