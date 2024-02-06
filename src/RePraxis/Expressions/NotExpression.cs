using System;
using System.Collections.Generic;
using System.Linq;

namespace RePraxis
{
	public class NotExpression : IQueryExpression
	{
		string statement;

		public NotExpression(string statement)
		{
			this.statement = statement;
		}

		public QueryState Evaluate(RePraxisDatabase database, QueryState state)
		{
			if ( RePraxisHelpers.HasVariables( statement ) )
			{
				// If there are no existing bindings, then this is the first statement in the query
				// or no previous statements contained variables.
				if ( state.Bindings.Count() == 0 )
				{
					// We need to find bindings for all of the variables in this expression
					var bindings = DBQuery.UnifyAll( database, state, new string[] { statement } );

					// If bindings for variables are found then we know this expression fails
					// because we want to ensure that the statement is never true
					if ( bindings.Count > 0 ) return new QueryState( false );

					// Continue the query.
					return state;
				}

				// If we have existing bindings, we need to filter the existing bindings
				var validBindings = state.Bindings
					.Where(
						(binding) =>
						{
							// Try to build a new sentence from the bindings and the expression's
							// statement.
							var sentence = RePraxisHelpers.BindSentence( statement, binding );

							if ( RePraxisHelpers.HasVariables( sentence ) )
							{
								// Treat the new sentence like its first in the query
								// and do a sub-unification, swapping out the state for an empty
								// one without existing bindings
								var scopedBindings = DBQuery.UnifyAll(
									database,
									new QueryState( true ),
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

				if ( validBindings.Length == 0 ) return new QueryState( false );

				return new QueryState( true, validBindings );
			}

			if ( database.Assert( statement ) ) return new QueryState( false );

			return state;
		}
	}
}
