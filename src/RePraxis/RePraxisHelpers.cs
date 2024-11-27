namespace RePraxis
{
	public static class RePraxisHelpers
	{
		/// <summary>
		/// Create a new node for the given value.
		/// </summary>
		/// <param name="value"></param>
		/// <param name="cardinality"></param>
		/// <returns></returns>
		public static INode CreateNode(object value, NodeCardinality cardinality = NodeCardinality.NONE)
		{
			if ( value is int )
			{
				return new IntNode( (int)value, NodeCardinality.NONE );
			}
			else if ( value is long )
			{
				return new IntNode( (int)(long)value, NodeCardinality.NONE );
			}
			else if ( value is float )
			{
				return new FloatNode( (float)value, NodeCardinality.NONE );
			}
			else if ( value is double )
			{
				return new FloatNode( (float)(double)value, NodeCardinality.NONE );
			}
			else if ( value is string )
			{
				string token = (string)value;

				if ( token[0] == '?' )
				{
					return new VariableNode( token, cardinality );
				}

				if ( int.TryParse( token, out var intValue ) )
				{
					return new IntNode( intValue, cardinality );
				}

				if ( float.TryParse( token, out var floatValue ) )
				{
					return new FloatNode( floatValue, cardinality );
				}

				return new SymbolNode( token, cardinality );
			}

			throw new System.ArgumentException(
				$"Object ({value}) cannot be converted to a valid node type." );
		}
	}
}
