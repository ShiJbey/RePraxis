using System;
using System.Collections.Generic;
using System.Linq;

namespace RePraxis
{
	public class GreaterThanEqualToExpression : IQueryExpression
	{
		Sentence lhValue;
		Sentence rhValue;

		public GreaterThanEqualToExpression(string lhValue, string rhValue)
		{
			this.lhValue = new Sentence( lhValue );
			this.rhValue = new Sentence( rhValue );
		}

		public void Evaluate(QueryState state)
		{
			if ( lhValue.Nodes.Length > 1 )
			{
				throw new Exception(
					"Comparator expression may only be single variables, symbols, or constants. "
					+ $"{lhValue} has too many parts."
				);
			}

			if ( rhValue.Nodes.Length > 1 )
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
				&& (lhValue.HasVariables || rhValue.HasVariables)
			)
			{
				state.Success = false;
				return;
			}

			// Loop through through the bindings and find those where the bound left value
			// is greater than or equal to the right.
			List<Dictionary<string, INode>> validBindings = state.Bindings
						.Where( (binding) =>
						{
							INode leftNode = lhValue.BindVariables( binding ).Nodes[0];

							INode rightNode = rhValue.BindVariables( binding ).Nodes[0];

							return leftNode.GreaterThanEqualTo( rightNode );
						} ).ToList();

			if ( validBindings.Count == 0 )
			{
				state.Success = false;
				return;
			}

			state.Bindings = validBindings;
		}
	}
}
