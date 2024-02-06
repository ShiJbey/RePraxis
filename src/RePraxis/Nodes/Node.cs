using System;
using System.Collections.Generic;

namespace RePraxis
{
	public abstract class Node<T> : INode where T : notnull
	{
		#region Fields

		/// <summary>
		/// A map of the node's children's symbols mapped to their instances
		/// </summary>
		private Dictionary<string, INode> _children;

		#endregion

		#region Properties

		public string Symbol { get; }
		public T Value { get; }
		public NodeCardinality Cardinality { get; }
		public abstract NodeType NodeType { get; }
		public IEnumerable<INode> Children => _children.Values;
		public INode? Parent { get; set; }

		#endregion

		#region Constructors

		public Node(string symbol, T value, NodeCardinality cardinality)
		{
			Symbol = symbol;
			Value = value;
			Cardinality = cardinality;
			_children = new Dictionary<string, INode>();
			Parent = null;
		}

		#endregion

		#region Operator Overloads

		public static bool operator ==(Node<T> a, INode b)
		{
			return a.EqualTo( b );
		}

		public static bool operator !=(Node<T> a, INode b)
		{
			return a.NotEqualTo( b );
		}

		public static bool operator >=(Node<T> a, INode b)
		{
			return a.GreaterThanEqualTo( b );
		}

		public static bool operator <=(Node<T> a, INode b)
		{
			return a.LessThanEqualTo( b );
		}

		public static bool operator >(Node<T> a, INode b)
		{
			return a.GreaterThan( b );
		}

		public static bool operator <(Node<T> a, INode b)
		{
			return a.LessThan( b );
		}

		public override bool Equals(object? obj)
		{
			if ( obj == null ) return false;

			if ( ReferenceEquals( this, obj ) )
			{
				return true;
			}

			if ( ReferenceEquals( obj, null ) )
			{
				return false;
			}

			throw new NotImplementedException();
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		#endregion

		#region Public Methods

		public object GetValue()
		{
			return Value;
		}

		public override string ToString()
		{
			return Symbol;
		}

		/// <summary>
		/// Add a new child node to this node
		/// </summary>
		/// <param name="node"></param>
		public void AddChild(INode node)
		{
			if ( Cardinality == NodeCardinality.NONE )
			{
				throw new System.Exception(
					"Cannot add child to node with cardinality NONE"
				);
			}

			if ( Cardinality == NodeCardinality.ONE && _children.Count >= 1 )
			{
				throw new System.Exception(
					"Cannot add additional child to node with cardinality ONE"
				);
			}

			_children.Add( node.Symbol, node );
			node.Parent = this;
		}

		/// <summary>
		/// Removes a child node from the DBNode
		/// </summary>
		/// <param name="symbol"></param>
		/// <returns>True if successful</returns>
		public bool RemoveChild(string symbol)
		{
			if ( _children.ContainsKey( symbol ) )
			{
				var child = _children[symbol];
				child.Parent = null;
				_children.Remove( symbol );
				return true;
			}
			return false;
		}

		/// <summary>
		/// Get a child node
		/// </summary>
		/// <param name="symbol"></param>
		/// <returns>The node with the given symbol</returns>
		public INode GetChild(string symbol)
		{
			return _children[symbol];
		}

		/// <summary>
		/// Check if the node has a child
		/// </summary>
		/// <param name="symbol"></param>
		/// <returns>
		/// True if a child is present with the given symbol.
		/// False otherwise.
		/// </returns>
		public bool HasChild(string symbol)
		{
			return _children.ContainsKey( symbol );
		}

		/// <summary>
		/// Remove all children and from this node
		/// </summary>
		public void ClearChildren()
		{
			foreach ( var (_, child) in _children )
			{
				child.ClearChildren();
			}
			_children.Clear();
		}

		/// <summary>
		/// Get the database sentence this node represents
		/// </summary>
		public string GetPath()
		{
			if ( Parent == null || Parent.Symbol == "root" )
			{
				return Symbol;
			}
			else
			{
				string parentCardinalityOp = Parent.Cardinality == NodeCardinality.ONE ? "!" : ".";
				return Parent.GetPath() + parentCardinalityOp + Symbol;
			}
		}

		public abstract INode Copy();

		public abstract bool EqualTo(INode other);

		public abstract bool NotEqualTo(INode other);

		public abstract bool GreaterThanEqualTo(INode other);

		public abstract bool LessThanEqualTo(INode other);

		public abstract bool LessThan(INode other);

		public abstract bool GreaterThan(INode other);

		#endregion
	}

}
