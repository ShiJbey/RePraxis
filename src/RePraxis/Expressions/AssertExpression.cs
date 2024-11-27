using System.Linq;

namespace RePraxis
{
	public class AssertExpression : IQueryExpression
	{
		Sentence statement;

		public AssertExpression(string statement)
		{
			this.statement = new Sentence( statement );
		}

		public void Evaluate(QueryState state)
		{

			if ( statement.HasVariables )
			{
				var bindings = state.UnifyAll( new Sentence[] { statement } );

				if ( bindings.Count() == 0 )
				{
					state.Success = false;
					return;
				}

				var validBindings = bindings
					.Where(
						(binding) =>
						{
							return state.DatabaseAssert(
								statement.BindVariables( binding )
							);
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

			if ( !state.DatabaseAssert( statement ) )
			{
				state.Success = false;
			}
		}
	}
}
