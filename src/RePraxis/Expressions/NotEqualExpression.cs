using System;
using System.Collections.Generic;
using System.Linq;

namespace RePraxis
{
	public class NotEqualExpression : IQueryExpression
	{
		string lhValue;
		string rhValue;

		public NotEqualExpression(string lhValue, string rhValue)
		{
			this.lhValue = lhValue;
			this.rhValue = rhValue;
		}

		public QueryState Evaluate(RePraxisDatabase database, QueryState state)
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
				state.Bindings.Count() == 0
				&& (RePraxisHelpers.HasVariables( lhValue ) || RePraxisHelpers.HasVariables( rhValue ))
			)
			{
				return new QueryState( false );
			}

			// Loop through through the bindings and find those where the bound values
			// are not equivalent.
			Dictionary<string, INode>[] validBindings = state.Bindings
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
				return new QueryState( false );
			}

			return new QueryState( true, validBindings );
		}
	}
}
