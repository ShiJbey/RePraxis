using System.Collections.Generic;
using System.Linq;

namespace RePraxis
{
	/// <summary>
	/// A series of nodes representing a path in the database.
	/// </summary>
	public class Sentence
	{
		/// <summary>
		/// The string version of the sentence.
		/// </summary>
		private string path;

		/// <summary>
		/// The nodes that make up the sentence.
		/// </summary>
		private INode[] nodes;

		/// <summary>
		/// The nodes that make up the sentence.
		/// </summary>
		public INode[] Nodes => nodes;

		/// <summary>
		/// Does the sentence contain variables.
		/// </summary>
		public bool HasVariables { get; }

		public Sentence(string path)
		{
			this.path = path;
			nodes = PathStringToNodes( path );
			HasVariables = nodes.Where( node => node.NodeType == NodeType.VARIABLE ).Count() > 0;
		}

		public override string ToString()
		{
			return path;
		}

		/// <summary>
		/// Check if a sentence starts with another sentence.
		/// </summary>
		/// <param name="sentence"></param>
		/// <returns></returns>
		public bool StartsWith(Sentence test)
		{
			if ( test.nodes.Length > this.nodes.Length ) return false;

			for ( int i = 0; i < test.nodes.Length; i++ )
			{
				INode subjectNode = this.nodes[i];
				INode testNode = test.nodes[i];

				if ( subjectNode.NodeType != testNode.NodeType ) return false;

				if ( !subjectNode.EqualTo( testNode ) ) return false;

				if ( i < test.nodes.Length - 1 && subjectNode.Cardinality != testNode.Cardinality )
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Create a new sentence with the given variables bound.
		/// </summary>
		/// <param name="bindings"></param>
		/// <returns></returns>
		public Sentence BindVariables(Dictionary<string, INode> bindings)
		{
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

			return new Sentence( finalSentence );
		}

		/// <summary>
		/// Breakup a database sentence into component parts for use when looking though
		/// a database.
		/// </summary>
		/// <param name="path"></param>
		/// <returns>The tokens of a database sentence</returns>
		private static INode[] PathStringToNodes(string path)
		{
			List<INode> nodes = new List<INode>();

			string currentToken = "";

			// Divide the sentence into tokens by reaching from one character at a time.
			foreach ( char ch in path )
			{
				// If we reach the exclusion or dot operator, we have reached the end of a token
				if ( ch == '!' || ch == '.' )
				{
					var cardinality = ch == '!' ? NodeCardinality.ONE : NodeCardinality.MANY;

					nodes.Add( RePraxisHelpers.CreateNode( currentToken, cardinality ) );

					currentToken = "";
				}
				// We are still in the middle of a token
				else
				{
					currentToken += ch;
				}
			}

			// Whatever characters remain become the final token in the sentence
			nodes.Add( RePraxisHelpers.CreateNode( currentToken, NodeCardinality.MANY ) );

			return nodes.ToArray();
		}
	}
}
