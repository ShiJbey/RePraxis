using System;
using System.Linq;
using System.Collections.Generic;

namespace RePraxis
{
	/// <summary>
	/// The intermediate state of a query while processing expressions
	/// </summary>
	public class QueryState
	{
		#region Properties

		/// <summary>
		/// The database the query is against.
		/// </summary>
		public RePraxisDatabase Database { get; }

		/// <summary>
		/// Paths that have not had their before access callbacks executed
		/// </summary>
		public List<string> UnexecutedBeforeAccessCallbacks { get; }

		/// <summary>
		/// Did all statements in the query pass or evaluate to true
		/// </summary>
		public bool Success { get; set; }

		/// <summary>
		/// Bindings for any variables present in the query
		/// </summary>
		public List<Dictionary<string, INode>> Bindings { get; set; }

		#endregion

		#region Constructors

		public QueryState(RePraxisDatabase database, bool success, IEnumerable<Dictionary<string, INode>> bindings)
		{
			Database = database;
			Database = database;
			Success = success;
			Bindings = new List<Dictionary<string, INode>>( bindings );
			UnexecutedBeforeAccessCallbacks =
				new List<string>( database.BeforeAccessListeners.Keys );
		}

		public QueryState(RePraxisDatabase database, bool success, IEnumerable<Dictionary<string, object>> bindings)
		{
			Database = database;
			Success = success;
			Bindings = new List<Dictionary<string, INode>>();
			UnexecutedBeforeAccessCallbacks =
				new List<string>( database.BeforeAccessListeners.Keys );

			foreach ( var entry in bindings )
			{
				Dictionary<string, INode> convertedBindings = new Dictionary<string, INode>();

				foreach ( var (varName, value) in entry )
				{
					convertedBindings[varName] = RePraxisHelpers.CreateNode( value );
				}

				Bindings.Add( convertedBindings );
			}
		}

		public QueryState(RePraxisDatabase database, bool success)
		{
			Database = database;
			Success = success;
			Bindings = new List<Dictionary<string, INode>>();
			UnexecutedBeforeAccessCallbacks =
				new List<string>( database.BeforeAccessListeners.Keys );
		}

		#endregion

		#region Public Methods

		/// <summary>
		/// Convert the <c>QueryState</c> to a corresponding <c>QueryResult</c>
		/// </summary>
		/// <returns></returns>
		public QueryResult ToResult()
		{
			if ( !Success ) return new QueryResult( false );

			Dictionary<string, object>[] results = new Dictionary<string, object>[Bindings.Count];

			for ( int i = 0; i < Bindings.Count; i++ )
			{
				results[i] = new Dictionary<string, object>();
				foreach ( var (varName, node) in Bindings[i] )
				{
					results[i][varName] = node.GetValue();
				}
			}

			return new QueryResult( true, results );
		}

		public void ExecuteOnBeforeAccessCallbacks(Sentence sentence)
		{
			// Loop backward through list and remove when invoked
			for ( int i = UnexecutedBeforeAccessCallbacks.Count - 1; i >= 0; i-- )
			{
				string path = UnexecutedBeforeAccessCallbacks[i];
				Sentence parsedPath = new Sentence( path );

				if ( sentence.StartsWith( parsedPath ) )
				{
					foreach ( var cb in Database.BeforeAccessListeners[path] )
					{
						cb?.Invoke( Database );
					}

					UnexecutedBeforeAccessCallbacks.RemoveAt( i );
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
		public bool DatabaseAssert(Sentence sentence)
		{
			// Perform callback checks
			ExecuteOnBeforeAccessCallbacks( sentence );

			INode currentNode = Database.Root;

			for ( int i = 0; i < sentence.Nodes.Length; i++ )
			{
				INode node = sentence.Nodes[i];

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
				if ( i == sentence.Nodes.Length - 1 ) return true;

				// Update the current node for a cardinality check
				currentNode = currentNode.GetChild( node.Symbol );

				// The cardinalities of all intermediate nodes need to match
				if ( currentNode.Cardinality != node.Cardinality ) return false;
			}

			return true;
		}

		/// <summary>
		/// Generates potential bindings from the database for a single sentence
		///
		/// This method does not take the current bindings into consideration. It
		/// should only be called by the UnifyAll method
		/// </summary>
		/// <param name="sentence"></param>
		/// <returns></returns>
		public List<Dictionary<string, INode>> Unify(Sentence sentence)
		{
			List<QueryBindingContext> unified = new List<QueryBindingContext>
				{
					new QueryBindingContext(Database.Root)
				};

			foreach ( var token in sentence.Nodes )
			{
				List<QueryBindingContext> nextUnified = new List<QueryBindingContext>();

				foreach ( var entry in unified )
				{
					// Need to run database before access listeners on the given current subtree
					// and cache the ones that have already been used
					ExecuteOnBeforeAccessCallbacks( new Sentence( entry.SubTree.GetPath() ) );

					foreach ( var child in entry.SubTree.Children )
					{
						if ( token.NodeType == NodeType.VARIABLE )
						{
							var unification =
								new QueryBindingContext(
									new Dictionary<string, INode>( entry.Bindings ), child );

							unification.Bindings[token.Symbol] = child;

							nextUnified.Add( unification );
						}
						else if ( token.Symbol == child.Symbol )
						{
							nextUnified.Add( new QueryBindingContext( entry.Bindings, child ) );
						}
					}
				}

				unified = nextUnified;
			}

			return unified
				.Select( unification => unification.Bindings )
				.Where( binding => binding.Count() > 0 )
				.ToList();
		}

		/// <summary>
		/// Generates potential bindings from the database unifying across all sentences.
		///
		/// This method takes into consideration the bindings from the current results.
		/// </summary>
		/// <param name="sentences"></param>
		/// <returns></returns>
		public List<Dictionary<string, INode>> UnifyAll(Sentence[] sentences)
		{
			List<Dictionary<string, INode>> possibleBindings = Bindings.ToList();

			foreach ( var sentence in sentences )
			{
				var iterativeBindings = new List<Dictionary<string, INode>>();
				var newBindings = Unify( sentence );

				if ( possibleBindings.Count == 0 )
				{
					// Copy the new bindings to the iterative bindings list
					foreach ( var binding in newBindings )
					{
						iterativeBindings.Add(
							new Dictionary<string, INode>( binding )
						);
					}
				}
				else
				{
					foreach ( var oldBinding in possibleBindings )
					{
						foreach ( var binding in newBindings )
						{
							var newKeys = binding.Keys.Where( k => !oldBinding.ContainsKey( k ) );
							var oldKeys = binding.Keys.Where( k => oldBinding.ContainsKey( k ) );
							bool existsIncompatibleKey = oldKeys.Any( k => !oldBinding[k].EqualTo( binding[k] ) );

							if ( existsIncompatibleKey )
							{
								continue;
							}
							else
							{
								var nextUnification = new Dictionary<string, INode>( oldBinding );

								foreach ( var k in newKeys )
								{
									nextUnification[k] = binding[k];
								}

								iterativeBindings.Add( nextUnification );
							}
						}
					}
				}

				possibleBindings = iterativeBindings;
			}

			return possibleBindings.Where( bindings => bindings.Count() > 0 ).ToList();
		}

		#endregion

		#region Helper Classes

		/// <summary>
		/// Used internally to track bindings for a single sentence and the database.
		/// </summary>
		private class QueryBindingContext
		{
			#region Properties

			/// <summary>
			/// Variable names mapped to their bound node values
			/// </summary>
			public Dictionary<string, INode> Bindings { get; }

			/// <summary>
			/// The subtree of the database that these bindings apply to.
			/// </summary>
			public INode SubTree { get; }

			#endregion

			#region Constructors

			public QueryBindingContext(Dictionary<string, INode> bindings, INode subTree)
			{
				Bindings = bindings;
				SubTree = subTree;
			}

			public QueryBindingContext(INode subtree)
				: this( new Dictionary<string, INode>(), subtree ) { }

			#endregion
		}

		#endregion
	}
}
