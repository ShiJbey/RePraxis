using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace RePraxis
{
	public static class RePraxisHelpers
	{
		/// <summary>
		/// Return true if the sentence contains variables.
		/// </summary>
		/// <param name="sentence"></param>
		/// <returns></returns>
		public static bool HasVariables(string sentence)
		{
			return ParseSentence( sentence )
				.Where( node => node.NodeType == NodeType.VARIABLE ).Count() > 0;
		}

		/// <summary>
		/// Creates a new sentence by binding variables to entries within the bindings.
		/// </summary>
		/// <param name="sentence"></param>
		/// <param name="bindings"></param>
		/// <returns></returns>
		public static string BindSentence(string sentence, Dictionary<string, INode> bindings)
		{
			INode[] nodes = ParseSentence( sentence );
			string finalSentence = "";

			for ( int i = 0; i < nodes.Length; ++i )
			{
				INode node = nodes[i];

				if ( node.NodeType == NodeType.VARIABLE )
				{
					if ( bindings.ContainsKey( node.Symbol ) )
					{
						finalSentence += bindings[node.Symbol].Symbol;
					}
					else
					{
						finalSentence += node.Symbol;
					}
				}
				else
				{
					finalSentence += node.Symbol;
				}

				if ( i < nodes.Length - 1 )
				{
					finalSentence += node.Cardinality == NodeCardinality.ONE ? "!" : ".";
				}
			}

			return finalSentence;
		}

		/// <summary>
		/// Create a database sentence from the given nodes
		/// </summary>
		/// <param name="nodes"></param>
		/// <returns></returns>
		public static string CreateSentence(INode[] nodes)
		{
			StringBuilder sentence = new StringBuilder();

			for ( int i = 0; i < nodes.Length; i++ )
			{
				var node = nodes[i];
				sentence.Append( node.Symbol );

				if ( i != nodes.Length - 1 )
				{
					sentence.Append( node.Cardinality == NodeCardinality.ONE ? "!" : "." );
				}
			}

			return sentence.ToString();
		}

		/// <summary>
		/// Create a new node for the given token.
		/// </summary>
		/// <param name="token"></param>
		/// <param name="cardinality"></param>
		/// <returns></returns>
		public static INode CreateNode(string token, NodeCardinality cardinality)
		{
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

		/// <summary>
		/// Create a new node for the given value.
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static INode CreateNode(object value)
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
				return new SymbolNode( (string)value, NodeCardinality.NONE );
			}

			throw new System.ArgumentException(
				$"Object ({value}) cannot be converted to a valid node type." );
		}

		/// <summary>
		/// Breakup a database sentence into component parts for use when looking though
		/// a database.
		/// </summary>
		/// <param name="sentence"></param>
		/// <returns>The tokens of a database sentence</returns>
		public static INode[] ParseSentence(string sentence)
		{
			List<INode> nodes = new List<INode>();

			string currentToken = "";

			// Divide the sentence into tokens by reaching from one character at a time.
			foreach ( char ch in sentence )
			{
				// If we reach the exclusion or dot operator, we have reached the end of a token
				if ( ch == '!' || ch == '.' )
				{
					var cardinality = ch == '!' ? NodeCardinality.ONE : NodeCardinality.MANY;

					nodes.Add( CreateNode( currentToken, cardinality ) );

					currentToken = "";
				}
				// We are still in the middle of a token
				else
				{
					currentToken += ch;
				}
			}

			// Whatever characters remain become the final token in the sentence
			nodes.Add( CreateNode( currentToken, NodeCardinality.MANY ) );

			return nodes.ToArray();
		}

	}
}
