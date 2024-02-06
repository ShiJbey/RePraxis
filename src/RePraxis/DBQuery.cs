using System;
using System.Collections.Generic;
using System.Linq;

namespace RePraxis
{
	/// <summary>
	/// Used to construct queries against a story database
	///
	/// <para>
	/// This class is immutable. So, additional calls to the Where method produce
	/// new DBQuery instances.
	/// </para>
	/// </summary>
	public class DBQuery
	{
		#region Fields

		/// <summary>
		/// 'Where' expressions contained within this query
		/// </summary>
		private string[] _expressions;

		#endregion

		#region Constructors

		public DBQuery(IEnumerable<string> expressions)
		{
			_expressions = expressions.ToArray();
		}

		public DBQuery()
		{
			_expressions = new string[0];
		}

		#endregion

		#region Methods

		/// <summary>
		/// Adds a new expression to the query
		/// </summary>
		/// <param name="expression"></param>
		/// <returns>
		/// A new DBQuery instance containing the provided expression and
		/// all expressions from the calling query.
		/// </returns>
		public DBQuery Where(string expression)
		{
			return new DBQuery( _expressions.ToList().Append( expression ) );
		}

		/// <summary>
		/// Run the query on the provided database
		/// </summary>
		/// <param name="db"></param>
		/// <returns>
		/// A <c>QueryResult</c> object with the final result of the query. If
		/// result.Success is true, then everything passes. Also, if there were
		/// any variables within the query, it returns all valid bindings for those
		/// variables as an array of dictionaries.
		/// </returns>
		public QueryResult Run(RePraxisDatabase db, Dictionary<string, object>[] bindings)
		{

			QueryState state = new QueryState( true, bindings );

			foreach ( string expressionStr in _expressions )
			{
				// Step 1: Split the expression into parts using whitespace
				string[] expressionParts = expressionStr
					.Split( " " ).Select( s => s.Trim() ).ToArray();

				// Step 2: Classify this expression string by the total number of parts
				//         and execute the expression

				// This is an assertion expression that may contain variables
				if ( expressionParts.Length == 1 )
				{
					state = new AssertExpression( expressionParts[0] ).Evaluate( db, state );
				}
				// This is probably a not expression
				else if ( expressionParts.Length == 2 )
				{
					if ( expressionParts[0] == "not" )
					{
						// This is a "not x.y.z" expression
						state = new NotExpression( expressionParts[1] ).Evaluate( db, state );
					}
					else
					{
						throw new Exception( $"Unrecognized query expression '{expressionStr}'." );
					}
				}
				// This is probably a comparator/inequality statement
				else if ( expressionParts.Length == 3 )
				{
					// Check to see what comparator is called
					string comparisonOp = expressionParts[0];

					switch ( comparisonOp )
					{
						case "eq":
							state = new EqualsExpression( expressionParts[1], expressionParts[2] )
								.Evaluate( db, state );
							break;
						case "neq":
							state = new NotEqualExpression( expressionParts[1], expressionParts[2] )
								.Evaluate( db, state );
							break;
						case "lt":
							state = new LessThanExpression( expressionParts[1], expressionParts[2] )
								.Evaluate( db, state );
							break;
						case "gt":
							state = new GreaterThanExpression( expressionParts[1], expressionParts[2] )
								.Evaluate( db, state );
							break;
						case "lte":
							state = new LessThanEqualToExpression( expressionParts[1], expressionParts[2] )
								.Evaluate( db, state );
							break;
						case "gte":
							state = new GreaterThanEqualToExpression( expressionParts[1], expressionParts[2] )
								.Evaluate( db, state );
							break;
						default:
							throw new Exception(
								$"Unrecognized comparison operator in {expressionStr}."
							);
					}
				}
				// This expression is not recognized
				else
				{
					throw new Exception( $"Unrecognized query expression '{expressionStr}'." );
				}

				// Step 3: Check if the query has failed and quit processing if true
				if ( !state.Success ) break;
			}

			return state.ToResult();
		}

		/// <summary>
		/// Run the query on the provided database
		/// </summary>
		/// <param name="db"></param>
		/// <returns></returns>
		public QueryResult Run(RePraxisDatabase db)
		{
			return Run( db, new Dictionary<string, object>[0] );
		}

		/// <summary>
		/// Run the query on the provided database
		/// </summary>
		/// <param name="db"></param>
		/// <param name="bindings"></param>
		/// <returns></returns>
		public QueryResult Run(RePraxisDatabase db, Dictionary<string, object> bindings)
		{
			return Run( db, new Dictionary<string, object>[] { bindings } );
		}

		#endregion

		#region Unification Methods

		/// <summary>
		/// Generates potential bindings from the database for a single sentence
		///
		/// This method does not take the current bindings into consideration. It
		/// should only be called by the UnifyAll method
		/// </summary>
		/// <param name="sentence"></param>
		/// <returns></returns>
		public static List<Dictionary<string, INode>> Unify(RePraxisDatabase database, string sentence)
		{
			List<QueryBindingContext> unified = new List<QueryBindingContext>
				{
					new QueryBindingContext(database.Root)
				};

			var tokens = RePraxisHelpers.ParseSentence( sentence );

			foreach ( var token in tokens )
			{
				List<QueryBindingContext> nextUnified = new List<QueryBindingContext>();

				foreach ( var entry in unified )
				{
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
						else
						{
							if ( token.Symbol == child.Symbol )
							{
								nextUnified.Add( new QueryBindingContext( entry.Bindings, child ) );
							}
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
		public static List<Dictionary<string, INode>> UnifyAll(RePraxisDatabase database, QueryState state, string[] sentences)
		{
			List<Dictionary<string, INode>> possibleBindings = state.Bindings.ToList();

			foreach ( var sentence in sentences )
			{
				var iterativeBindings = new List<Dictionary<string, INode>>();
				var newBindings = Unify( database, sentence );

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
	}
}
