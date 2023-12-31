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
				var bindings = DBQuery.UnifyAll( database, result, new string[] { statement } );

				if ( bindings.Count() == 0 ) return new QueryResult( false );

				var validBindings = bindings
					.Where(
						(binding) =>
						{
							return !database.Assert(
								RePraxisHelpers.BindSentence( statement, binding )
							);
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
