using System;
using System.Collections.Generic;
using System.Linq;

namespace RePraxis
{
	public class AssertExpression : IQueryExpression
	{
		string statement;

		public AssertExpression(string statement)
		{
			this.statement = statement;
		}

		public QueryResult Evaluate(RePraxisDatabase database, QueryResult result)
		{

			if ( DBQuery.HasVariables( statement ) )
			{
				var bindings = DBQuery.UnifyAll( database, result, new string[] { statement } );

				if ( bindings.Count() == 0 ) return new QueryResult( false );

				var validBindings = bindings
					.Where(
						(binding) =>
						{
							return database.Assert(
								RePraxisHelpers.BindSentence( statement, binding )
							);
						}
					)
					.ToArray();

				if ( validBindings.Length == 0 ) return new QueryResult( false );

				return new QueryResult( true, validBindings );
			}

			if ( !database.Assert( statement ) ) return new QueryResult( false );

			return result;
		}
	}

	public class NotExpression : IQueryExpression
	{
		string statement;

		public NotExpression(string statement)
		{
			this.statement = statement;
		}

		public QueryResult Evaluate(RePraxisDatabase database, QueryResult result)
		{
			if ( DBQuery.HasVariables( statement ) )
			{
				// If there are no existing bindings, then this is the first statement in the query
				// or no previous statements contained variables.
				if ( result.Bindings.Count() == 0 )
				{
					// We need to find bindings for all of the variables in this expression
					var bindings = DBQuery.UnifyAll( database, result, new string[] { statement } );

					// If bindings for variables are found then we know this expression fails
					// because we want to ensure that the statement is never true
					if ( bindings.Count > 0 ) return new QueryResult( false );

					// Continue the query.
					return result;
				}

				// If we have existing bindings, we need to filter the existing bindings
				var validBindings = result.Bindings
					.Where(
						(binding) =>
						{
							// Try to build a new sentence from the bindings and the expression's
							// statement.
							var sentence = RePraxisHelpers.BindSentence( statement, binding );

							if ( DBQuery.HasVariables( sentence ) )
							{
								// Treat the new sentence like its first in the query
								// and do a sub-unification, swapping out the result for an empty
								// one without existing bindings
								var scopedBindings = DBQuery.UnifyAll(
									database,
									new QueryResult( true ),
									new string[] { sentence }
								);

								// If any of the remaining variables are bound in the scoped
								// bindings, then the entire binding fails
								if ( scopedBindings.Count > 0 ) return false;

								return true;
							}

							return !database.Assert( sentence );
						}
					)
					.ToArray();

				if ( validBindings.Length == 0 ) return new QueryResult( false );

				return new QueryResult( true, validBindings );
			}

			if ( database.Assert( statement ) ) return new QueryResult( false );

			return result;
		}
	}

	public class EqualsExpression : IQueryExpression
	{
		string lhValue;
		string rhValue;

		public EqualsExpression(string lhValue, string rhValue)
		{
			this.lhValue = lhValue;
			this.rhValue = rhValue;
		}

		public QueryResult Evaluate(RePraxisDatabase database, QueryResult result)
		{
			INode[] lhNodes = RePraxisHelpers.ParseSentence( lhValue );
			INode[] rhNodes = RePraxisHelpers.ParseSentence( rhValue );

			if ( lhNodes.Length > 1 )
			{
				throw new Exception(
					"Comparator expression may only be single variables, symbols, or constants. "
					+ $"{lhValue} has too many parts."
				);
			}

			if ( rhNodes.Length > 1 )
			{
				throw new Exception(
					"Comparator expression may only be single variables, symbols, or constants. "
					+ $"{rhValue} has too many parts."
				);
			}

			// If no bindings are found and at least one of the values is a variables,
			// then the query has failed.
			if (
				result.Bindings.Count() == 0
				&& (DBQuery.HasVariables( lhValue ) || DBQuery.HasVariables( rhValue ))
			)
			{
				return new QueryResult( false );
			}

			// Loop through through the bindings and find those where the bound values
			// are equivalent.
			Dictionary<string, string>[] validBindings = result.Bindings
						.Where( (binding) =>
						{
							INode leftNode = RePraxisHelpers.ParseSentence(
								RePraxisHelpers.BindSentence( lhValue, binding )
							)[0];

							INode rightNode = RePraxisHelpers.ParseSentence(
								RePraxisHelpers.BindSentence( rhValue, binding )
							)[0];

							return leftNode.EqualTo( rightNode );
						} ).ToArray();

			if ( validBindings.Length == 0 )
			{
				return new QueryResult( false );
			}

			return new QueryResult( true, validBindings );
		}
	}

	public class NotEqualExpression : IQueryExpression
	{
		string lhValue;
		string rhValue;

		public NotEqualExpression(string lhValue, string rhValue)
		{
			this.lhValue = lhValue;
			this.rhValue = rhValue;
		}

		public QueryResult Evaluate(RePraxisDatabase database, QueryResult result)
		{
			INode[] lhNodes = RePraxisHelpers.ParseSentence( lhValue );
			INode[] rhNodes = RePraxisHelpers.ParseSentence( rhValue );

			if ( lhNodes.Length > 1 )
			{
				throw new Exception(
					"Comparator expression may only be single variables, symbols, or constants. "
					+ $"{lhValue} has too many parts."
				);
			}

			if ( rhNodes.Length > 1 )
			{
				throw new Exception(
					"Comparator expression may only be single variables, symbols, or constants. "
					+ $"{rhValue} has too many parts."
				);
			}

			// If no bindings are found and at least one of the values is a variables,
			// then the query has failed.
			if (
				result.Bindings.Count() == 0
				&& (DBQuery.HasVariables( lhValue ) || DBQuery.HasVariables( rhValue ))
			)
			{
				return new QueryResult( false );
			}

			// Loop through through the bindings and find those where the bound values
			// are not equivalent.
			Dictionary<string, string>[] validBindings = result.Bindings
						.Where( (binding) =>
						{
							INode leftNode = RePraxisHelpers.ParseSentence(
								RePraxisHelpers.BindSentence( lhValue, binding )
							)[0];

							INode rightNode = RePraxisHelpers.ParseSentence(
								RePraxisHelpers.BindSentence( rhValue, binding )
							)[0];

							return leftNode.NotEqualTo( rightNode );
						} ).ToArray();

			if ( validBindings.Length == 0 )
			{
				return new QueryResult( false );
			}

			return new QueryResult( true, validBindings );
		}
	}

	public class GreaterThanEqualToExpression : IQueryExpression
	{
		string lhValue;
		string rhValue;

		public GreaterThanEqualToExpression(string lhValue, string rhValue)
		{
			this.lhValue = lhValue;
			this.rhValue = rhValue;
		}

		public QueryResult Evaluate(RePraxisDatabase database, QueryResult result)
		{
			INode[] lhNodes = RePraxisHelpers.ParseSentence( lhValue );
			INode[] rhNodes = RePraxisHelpers.ParseSentence( rhValue );

			if ( lhNodes.Length > 1 )
			{
				throw new Exception(
					"Comparator expression may only be single variables, symbols, or constants. "
					+ $"{lhValue} has too many parts."
				);
			}

			if ( rhNodes.Length > 1 )
			{
				throw new Exception(
					"Comparator expression may only be single variables, symbols, or constants. "
					+ $"{rhValue} has too many parts."
				);
			}

			// If no bindings are found and at least one of the values is a variables,
			// then the query has failed.
			if (
				result.Bindings.Count() == 0
				&& (DBQuery.HasVariables( lhValue ) || DBQuery.HasVariables( rhValue ))
			)
			{
				return new QueryResult( false );
			}

			// Loop through through the bindings and find those where the bound left value
			// is greater than or equal to the right.
			Dictionary<string, string>[] validBindings = result.Bindings
						.Where( (binding) =>
						{
							INode leftNode = RePraxisHelpers.ParseSentence(
								RePraxisHelpers.BindSentence( lhValue, binding )
							)[0];

							INode rightNode = RePraxisHelpers.ParseSentence(
								RePraxisHelpers.BindSentence( rhValue, binding )
							)[0];

							return leftNode.GreaterThanEqualTo( rightNode );
						} ).ToArray();

			if ( validBindings.Length == 0 )
			{
				return new QueryResult( false );
			}

			return new QueryResult( true, validBindings );
		}
	}

	public class GreaterThanExpression : IQueryExpression
	{
		string lhValue;
		string rhValue;

		public GreaterThanExpression(string lhValue, string rhValue)
		{
			this.lhValue = lhValue;
			this.rhValue = rhValue;
		}

		public QueryResult Evaluate(RePraxisDatabase database, QueryResult result)
		{
			INode[] lhNodes = RePraxisHelpers.ParseSentence( lhValue );
			INode[] rhNodes = RePraxisHelpers.ParseSentence( rhValue );

			if ( lhNodes.Length > 1 )
			{
				throw new Exception(
					"Comparator expression may only be single variables, symbols, or constants. "
					+ $"{lhValue} has too many parts."
				);
			}

			if ( rhNodes.Length > 1 )
			{
				throw new Exception(
					"Comparator expression may only be single variables, symbols, or constants. "
					+ $"{rhValue} has too many parts."
				);
			}

			// If no bindings are found and at least one of the values is a variables,
			// then the query has failed.
			if (
				result.Bindings.Count() == 0
				&& (DBQuery.HasVariables( lhValue ) || DBQuery.HasVariables( rhValue ))
			)
			{
				return new QueryResult( false );
			}

			// Loop through through the bindings and find those where the bound left value
			// is greater than the right.
			Dictionary<string, string>[] validBindings = result.Bindings
						.Where( (binding) =>
						{
							INode leftNode = RePraxisHelpers.ParseSentence(
								RePraxisHelpers.BindSentence( lhValue, binding )
							)[0];

							INode rightNode = RePraxisHelpers.ParseSentence(
								RePraxisHelpers.BindSentence( rhValue, binding )
							)[0];

							return leftNode.GreaterThan( rightNode );
						} ).ToArray();

			if ( validBindings.Length == 0 )
			{
				return new QueryResult( false );
			}

			return new QueryResult( true, validBindings );
		}
	}

	public class LessThanEqualToExpression : IQueryExpression
	{
		string lhValue;
		string rhValue;

		public LessThanEqualToExpression(string lhValue, string rhValue)
		{
			this.lhValue = lhValue;
			this.rhValue = rhValue;
		}

		public QueryResult Evaluate(RePraxisDatabase database, QueryResult result)
		{
			INode[] lhNodes = RePraxisHelpers.ParseSentence( lhValue );
			INode[] rhNodes = RePraxisHelpers.ParseSentence( rhValue );

			if ( lhNodes.Length > 1 )
			{
				throw new Exception(
					"Comparator expression may only be single variables, symbols, or constants. "
					+ $"{lhValue} has too many parts."
				);
			}

			if ( rhNodes.Length > 1 )
			{
				throw new Exception(
					"Comparator expression may only be single variables, symbols, or constants. "
					+ $"{rhValue} has too many parts."
				);
			}

			// If no bindings are found and at least one of the values is a variables,
			// then the query has failed.
			if (
				result.Bindings.Count() == 0
				&& (DBQuery.HasVariables( lhValue ) || DBQuery.HasVariables( rhValue ))
			)
			{
				return new QueryResult( false );
			}

			// Loop through through the bindings and find those where the bound left value
			// is less than or equal to the right.
			Dictionary<string, string>[] validBindings = result.Bindings
						.Where( (binding) =>
						{
							INode leftNode = RePraxisHelpers.ParseSentence(
								RePraxisHelpers.BindSentence( lhValue, binding )
							)[0];

							INode rightNode = RePraxisHelpers.ParseSentence(
								RePraxisHelpers.BindSentence( rhValue, binding )
							)[0];

							return leftNode.LessThanEqualTo( rightNode );
						} ).ToArray();

			if ( validBindings.Length == 0 )
			{
				return new QueryResult( false );
			}

			return new QueryResult( true, validBindings );
		}
	}

	public class LessThanExpression : IQueryExpression
	{
		string lhValue;
		string rhValue;

		public LessThanExpression(string lhValue, string rhValue)
		{
			this.lhValue = lhValue;
			this.rhValue = rhValue;
		}

		public QueryResult Evaluate(RePraxisDatabase database, QueryResult result)
		{
			INode[] lhNodes = RePraxisHelpers.ParseSentence( lhValue );
			INode[] rhNodes = RePraxisHelpers.ParseSentence( rhValue );

			if ( lhNodes.Length > 1 )
			{
				throw new Exception(
					"Comparator expression may only be single variables, symbols, or constants. "
					+ $"{lhValue} has too many parts."
				);
			}

			if ( rhNodes.Length > 1 )
			{
				throw new Exception(
					"Comparator expression may only be single variables, symbols, or constants. "
					+ $"{rhValue} has too many parts."
				);
			}

			// If no bindings are found and at least one of the values is a variables,
			// then the query has failed.
			if (
				result.Bindings.Count() == 0
				&& (DBQuery.HasVariables( lhValue ) || DBQuery.HasVariables( rhValue ))
			)
			{
				return new QueryResult( false );
			}

			// Loop through through the bindings and find those where the bound left value
			// is less than the right.
			Dictionary<string, string>[] validBindings = result.Bindings
						.Where( (binding) =>
						{
							INode leftNode = RePraxisHelpers.ParseSentence(
								RePraxisHelpers.BindSentence( lhValue, binding )
							)[0];

							INode rightNode = RePraxisHelpers.ParseSentence(
								RePraxisHelpers.BindSentence( rhValue, binding )
							)[0];

							return leftNode.LessThanEqualTo( rightNode );
						} ).ToArray();

			if ( validBindings.Length == 0 )
			{
				return new QueryResult( false );
			}

			return new QueryResult( true, validBindings );
		}
	}
}
