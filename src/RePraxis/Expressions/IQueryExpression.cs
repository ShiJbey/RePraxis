namespace RePraxis
{
	public interface IQueryExpression
	{
		/// <summary>
		/// Evaluate the expression and update the result
		/// </summary>
		/// <param name="state"></param>
		public void Evaluate(QueryState state);
	}
}
