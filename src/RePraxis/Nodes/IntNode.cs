namespace RePraxis
{
	/// <summary>
	/// A database node containing an integer value
	/// </summary>
	public class IntNode : Node<int>
	{
		public override NodeType NodeType => NodeType.INT;

		public IntNode(int value, NodeCardinality cardinality)
			: base( value.ToString(), value, cardinality )
		{ }

		public override bool EqualTo(INode other)
		{
			if ( other.NodeType != NodeType ) return false;

			return Value == ((IntNode)other).Value;
		}

		public override bool NotEqualTo(INode other)
		{
			if ( other.NodeType != NodeType ) return true;

			return Value != ((IntNode)other).Value;
		}

		public override bool GreaterThanEqualTo(INode other)
		{
			if ( other.NodeType != NodeType.INT && other.NodeType != NodeType.FLOAT )
			{
				throw new NodeTypeException(
					$">= not defined between nodes of type {NodeType} and {other.NodeType}"
				);
			};

			if ( other.NodeType == NodeType.FLOAT )
			{
				return Value >= ((FloatNode)other).Value;
			}

			return Value >= ((IntNode)other).Value;
		}

		public override bool LessThanEqualTo(INode other)
		{
			if ( other.NodeType != NodeType.INT && other.NodeType != NodeType.FLOAT )
			{
				throw new NodeTypeException(
					$"<= not defined between nodes of type {NodeType} and {other.NodeType}"
				);
			};

			if ( other.NodeType == NodeType.FLOAT )
			{
				return Value <= ((FloatNode)other).Value;
			}

			return Value <= ((IntNode)other).Value;
		}

		public override bool LessThan(INode other)
		{
			if ( other.NodeType != NodeType.INT && other.NodeType != NodeType.FLOAT )
			{
				throw new NodeTypeException(
					$"< not defined between nodes of type {NodeType} and {other.NodeType}"
				);
			};

			if ( other.NodeType == NodeType.FLOAT )
			{
				return Value < ((FloatNode)other).Value;
			}

			return Value < ((IntNode)other).Value;
		}

		public override bool GreaterThan(INode other)
		{
			if ( other.NodeType != NodeType.INT && other.NodeType != NodeType.FLOAT )
			{
				throw new NodeTypeException(
					$"> not defined between nodes of type {NodeType} and {other.NodeType}"
				);
			};

			if ( other.NodeType == NodeType.FLOAT )
			{
				return Value > ((FloatNode)other).Value;
			}

			return Value > ((IntNode)other).Value;
		}

		public override INode Copy()
		{
			return new IntNode( Value, Cardinality );
		}
	}
}
