namespace RePraxis
{
	public interface IQueryExpression
	{
		/// <summary>
		/// Evaluate the expression and update the result
		/// </summary>
		/// <param name="database"></param>
		/// <param name="state"></param>
		public QueryState Evaluate(RePraxisDatabase database, QueryState state);
	}
}
