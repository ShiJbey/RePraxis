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

		public QueryState Evaluate(RePraxisDatabase database, QueryState state)
		{

			if ( RePraxisHelpers.HasVariables( statement ) )
			{
				var bindings = DBQuery.UnifyAll( database, state, new string[] { statement } );

				if ( bindings.Count() == 0 ) return new QueryState( false );

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

				if ( validBindings.Length == 0 ) return new QueryState( false );

				return new QueryState( true, validBindings );
			}

			if ( !database.Assert( statement ) ) return new QueryState( false );

			return state;
		}
	}
}
