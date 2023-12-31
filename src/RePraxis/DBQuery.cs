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
		public QueryResult Run(RePraxisDatabase db)
		{
			var result = new QueryResult( true, new Dictionary<string, string>[0] );

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
					result = new AssertExpression( expressionParts[0] ).Evaluate( db, result );
				}
				// This is probably a not expression
				else if ( expressionParts.Length == 2 )
				{
					if ( expressionParts[0] == "not" )
					{
						// This is a "not x.y.z" expression
						result = new NotExpression( expressionParts[1] ).Evaluate( db, result );
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
							result = new EqualsExpression( expressionParts[1], expressionParts[2] )
								.Evaluate( db, result );
							break;
						case "neq":
							result = new NotEqualExpression( expressionParts[1], expressionParts[2] )
								.Evaluate( db, result );
							break;
						case "lt":
							result = new LessThanExpression( expressionParts[1], expressionParts[2] )
								.Evaluate( db, result );
							break;
						case "gt":
							result = new GreaterThanExpression( expressionParts[1], expressionParts[2] )
								.Evaluate( db, result );
							break;
						case "lte":
							result = new LessThanEqualToExpression( expressionParts[1], expressionParts[2] )
								.Evaluate( db, result );
							break;
						case "gte":
							result = new GreaterThanEqualToExpression( expressionParts[1], expressionParts[2] )
								.Evaluate( db, result );
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
				if ( !result.Success ) break;
			}

			return result;
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
		public static List<Dictionary<string, string>> Unify(RePraxisDatabase database, string sentence)
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
									new Dictionary<string, string>( entry.Bindings ), child );

							unification.Bindings[token.Symbol] = child.Symbol;

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
		public static List<Dictionary<string, string>> UnifyAll(RePraxisDatabase database, QueryResult result, string[] sentences)
		{
			var possibleBindings = result.Bindings.ToList();

			foreach ( var key in sentences )
			{
				var iterativeBindings = new List<Dictionary<string, string>>();
				var newBindings = Unify( database, key );

				if ( possibleBindings.Count == 0 )
				{
					foreach ( var binding in newBindings )
					{
						var nextUnification = new Dictionary<string, string>();

						foreach ( var k in binding.Keys )
						{
							nextUnification[k] = binding[k];
						}

						iterativeBindings.Add( nextUnification );
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
							bool existsIncompatibleKey = oldKeys.Any( k => oldBinding[k] != binding[k] );

							if ( existsIncompatibleKey )
							{
								continue;
							}
							else
							{
								var nextUnification = new Dictionary<string, string>( oldBinding );

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

		/// <summary>
		/// Return true if the sentence contains variables.
		/// </summary>
		/// <param name="sentence"></param>
		/// <returns></returns>
		public static bool HasVariables(string sentence)
		{
			return RePraxisHelpers.ParseSentence( sentence )
				.Where( node => node.NodeType == NodeType.VARIABLE ).Count() > 0;
		}

		#endregion
	}
}
