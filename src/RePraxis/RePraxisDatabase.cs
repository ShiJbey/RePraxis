using System;

namespace RePraxis
{
	/// <summary>
	/// A database instance. Users can insert, delete, and assert logical statements. Inserted
	/// statements are stored in a tree structure containing nodes of various templated data types.
	/// Users can query the database for specific patterns using the <c>DBQuery</c> class.
	/// </summary>
	public class RePraxisDatabase
	{
		#region Properties

		/// <summary>
		/// A reference to the root node of the database
		/// </summary>
		public INode Root { get; }

		#endregion

		#region Constructors

		public RePraxisDatabase()
		{
			Root = new SymbolNode( "root", NodeCardinality.MANY );
		}

		#endregion

		#region Methods

		/// <summary>
		/// Add a value to the database under the given sentence.
		/// </summary>
		/// <param name="sentence"></param>
		/// <exception cref="ArgumentException">
		/// Thrown when the sentence contains variables.
		/// </exception>
		public void Insert(string sentence)
		{
			INode[] nodes = RePraxisHelpers.ParseSentence( sentence );

			INode subtree = Root;

			for ( int i = 0; i < nodes.Length; i++ )
			{
				var node = nodes[i].Copy();

				if ( node.NodeType == NodeType.VARIABLE )
				{
					throw new ArgumentException( @$"
                        Found variable {node.Symbol} in sentence '({sentence})'.
                        Sentence cannot contain variables when inserting a value."
					);
				}

				if ( !subtree.HasChild( node.Symbol ) )
				{
					if ( subtree.Cardinality == NodeCardinality.ONE )
					{
						// Replace the existing child
						subtree.ClearChildren();
					}

					subtree.AddChild( node );
					subtree = node;
				}
				else
				{
					// We need to get the existing node, check cardinalities, and establish new
					// nodes
					var existingNode = subtree.GetChild( node.Symbol );

					if ( existingNode.Cardinality != node.Cardinality )
					{
						throw new CardinalityException(
							$"Cardinality mismatch on {node.Symbol} in sentence '{sentence}'."
						);
					}

					subtree = existingNode;
				}
			}
		}

		/// <summary>
		/// Check if a given sentence exists within the database.
		/// </summary>
		/// <param name="sentence"></param>
		/// <returns></returns>
		/// <exception cref="ArgumentException">
		/// Thrown when a sentence contains variables.
		/// </exception>
		public bool Assert(string sentence)
		{
			INode[] nodes = RePraxisHelpers.ParseSentence( sentence );

			INode currentNode = Root;

			for ( int i = 0; i < nodes.Length; i++ )
			{
				INode node = nodes[i];

				if ( node.NodeType == NodeType.VARIABLE )
				{
					throw new ArgumentException( @$"
                        Found variable {node.Symbol} in sentence.
                        Sentence cannot contain variables when retrieving a value."
					);
				}

				// Return early if there is not a corresponding node in the database
				if ( !currentNode.HasChild( node.Symbol ) ) return false;

				// We can stop iterating since we don't care about the cardinality of the last node
				if ( i == nodes.Length - 1 ) return true;

				// Update the current node for a cardinality check
				currentNode = currentNode.GetChild( node.Symbol );

				// The cardinalities of all intermediate nodes need to match
				if ( currentNode.Cardinality != node.Cardinality ) return false;
			}

			return true;
		}

		/// <summary>
		/// Remove all values and sub-trees under the given sentence
		/// </summary>
		/// <param name="sentence"></param>
		/// <returns>
		/// True if something was removed. False otherwise.
		/// </returns>
		public bool Delete(string sentence)
		{

			INode[] nodes = RePraxisHelpers.ParseSentence( sentence );

			INode currentNode = Root;

			// Loop until we get to the second to last node
			for ( int i = 0; i < nodes.Length - 1; ++i )
			{
				var node = nodes[i];
				currentNode = currentNode.GetChild( node.Symbol );
			}

			// Get a reference to the final node in the sentence
			var lastToken = nodes[nodes.Length - 1];

			// Remove the child
			return currentNode.RemoveChild( lastToken.Symbol );
		}

		/// <summary>
		/// Clear the contents of the database.
		/// </summary>
		public void Clear()
		{
			Root.ClearChildren();
		}

		#endregion
	}
}
