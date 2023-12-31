namespace RePraxis
{
	public interface IQueryExpression
	{
		/// <summary>
		/// Evaluate the expression and update the result
		/// </summary>
		/// <param name="database"></param>
		/// <param name="result"></param>
		public QueryResult Evaluate(RePraxisDatabase database, QueryResult result);
	}
}
