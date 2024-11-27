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

			QueryState state = new QueryState( db, true, bindings );

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
					new AssertExpression( expressionParts[0] ).Evaluate( state );
				}
				// This is probably a not expression
				else if ( expressionParts.Length == 2 )
				{
					if ( expressionParts[0] == "not" )
					{
						// This is a "not x.y.z" expression
						new NotExpression( expressionParts[1] ).Evaluate( state );
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
							new EqualsExpression( expressionParts[1], expressionParts[2] )
								.Evaluate( state );
							break;
						case "neq":
							new NotEqualExpression( expressionParts[1], expressionParts[2] )
								.Evaluate( state );
							break;
						case "lt":
							new LessThanExpression( expressionParts[1], expressionParts[2] )
								.Evaluate( state );
							break;
						case "gt":
							new GreaterThanExpression( expressionParts[1], expressionParts[2] )
								.Evaluate( state );
							break;
						case "lte":
							new LessThanEqualToExpression( expressionParts[1], expressionParts[2] )
								.Evaluate( state );
							break;
						case "gte":
							new GreaterThanEqualToExpression( expressionParts[1], expressionParts[2] )
								.Evaluate( state );
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
	}
}
