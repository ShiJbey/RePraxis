using System;
using System.Collections.Generic;
using System.Linq;

namespace RePraxis
{
	public class NotExpression : IQueryExpression
	{
		Sentence statement;

		public NotExpression(string statement)
		{
			this.statement = new Sentence( statement );
		}

		public void Evaluate(QueryState state)
		{
			if ( statement.HasVariables )
			{
				// If there are no existing bindings, then this is the first statement in the query
				// or no previous statements contained variables.
				if ( state.Bindings.Count() == 0 )
				{
					// We need to find bindings for all of the variables in this expression
					var bindings = state.UnifyAll( new Sentence[] { statement } );

					// If bindings for variables are found then we know this expression fails
					// because we want to ensure that the statement is never true
					if ( bindings.Count > 0 )
					{
						state.Success = false;
						return;
					}

					// Continue the query.
					return;
				}

				// If we have existing bindings, we need to filter the existing bindings
				List<Dictionary<string, INode>> validBindings = state.Bindings
					.Where(
						(binding) =>
						{
							// Try to build a new sentence from the bindings and the expression's
							// statement.
							var sentence = statement.BindVariables( binding );

							if ( sentence.HasVariables )
							{
								// Treat the new sentence like its first in the query
								// and do a sub-unification, swapping out the state for an empty
								// one without existing bindings
								var scopedBindings = new QueryState( state.Database, true )
									.UnifyAll( new Sentence[] { sentence } );

								// If any of the remaining variables are bound in the scoped
								// bindings, then the entire binding fails
								if ( scopedBindings.Count > 0 ) return false;

								return true;
							}

							return !state.DatabaseAssert( sentence );
						}
					)
					.ToList();

				if ( validBindings.Count == 0 )
				{
					state.Success = false;
					return;
				}

				state.Bindings = validBindings;
				return;
			}

			if ( state.DatabaseAssert( statement ) )
			{
				state.Success = false;
				return;
			}
		}
	}
}
