using System;

namespace RePraxis
{
	/// <summary>
	/// Exception raised when trying to set a value in the database with an incorrect cardinality.
	/// </summary>
	public class CardinalityException : Exception
	{
		public CardinalityException() { }

		public CardinalityException(string message) : base( message ) { }

		public CardinalityException(string message, Exception inner) : base( message, inner ) { }
	}

	/// <summary>
	/// Exception raised when there is an error regarding the type of a RePraxis node.
	/// </summary>
	public class NodeTypeException : Exception
	{
		public NodeTypeException() { }

		public NodeTypeException(string message) : base( message ) { }

		public NodeTypeException(string message, Exception inner) : base( message, inner ) { }
	}
}
