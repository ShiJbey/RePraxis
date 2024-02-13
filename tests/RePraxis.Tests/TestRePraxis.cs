using System.Xml.Serialization;

namespace RePraxis.Tests;

[TestFixture]
public class Tests
{
	private RePraxisDatabase _db;

	[SetUp]
	public void SetUp()
	{
		_db = new RePraxisDatabase();

		_db.Insert( "astrid.relationships.jordan.reputation!30" );
		_db.Insert( "astrid.relationships.jordan.tags.rivalry" );
		_db.Insert( "astrid.relationships.britt.reputation!-10" );
		_db.Insert( "astrid.relationships.britt.tags.ex_lover" );
		_db.Insert( "astrid.relationships.lee.reputation!20" );
		_db.Insert( "astrid.relationships.lee.tags.friend" );
		_db.Insert( "britt.relationships.player.tags.spouse" );
		_db.Insert( "player.relationships.jordan.reputation!-20" );
		_db.Insert( "player.relationships.jordan.tags.enemy" );
		_db.Insert( "player.relationships.britt.tags.spouse" );
	}

	/// <summary>
	/// Test that create, retrieval, updating, and deletion (CRUD)
	/// </summary>
	[Test]
	public void TestInsertSentence()
	{
		var db = new RePraxisDatabase();

		// Create values
		db.Insert( "A.relationships.B.reputation!10" );
		db.Insert( "A.relationships.B.type!rivalry" );

		// Retrieve values
		Assert.That( db.Assert( "A.relationships.B.reputation!19" ), Is.EqualTo( false ) );
		Assert.That( db.Assert( "A.relationships.B.type" ), Is.EqualTo( true ) );
		Assert.That( db.Assert( "A" ), Is.EqualTo( true ) );
	}

	/// <summary>
	/// Test that create, retrieval, updating, and deletion (CRUD)
	/// </summary>
	[Test]
	public void TestDeleteSentence()
	{
		var db = new RePraxisDatabase();

		// Create values
		db.Insert( "A.relationships.B.reputation!10" );

		// Delete a value
		db.Delete( "A.relationships.B.reputation" );
		Assert.That( db.Assert( "A.relationships.B.reputation" ), Is.EqualTo( false ) );
	}

	/// <summary>
	/// Test that create, retrieval, updating, and deletion (CRUD)
	/// </summary>
	[Test]
	public void TestUpdateSentence()
	{
		var db = new RePraxisDatabase();

		// Create values
		db.Insert( "A.relationships.B.reputation!10" );

		// Update a value
		db.Insert( "A.relationships.B.reputation!-99" );
		Assert.That( db.Assert( "A.relationships.B.reputation!-99" ), Is.EqualTo( true ) );
		Assert.That( db.Assert( "A.relationships.B.reputation.-99" ), Is.EqualTo( false ) );
	}

	[Test]
	public void TestAssertExpressionNoVars()
	{
		var query = new DBQuery()
					.Where( "astrid.relationships.britt" );

		var result = query.Run( _db );

		Assert.That( result.Success, Is.EqualTo( true ) );
		Assert.That( result.Bindings.Length, Is.EqualTo( 0 ) );
	}

	[Test]
	public void TestFailingAssertExpressionNoVars()
	{
		// Failing assertion without variables
		var query = new DBQuery()
			.Where( "astrid.relationships.haley" );

		var result = query.Run( _db );

		Assert.That( result.Success, Is.EqualTo( false ) );
		Assert.That( result.Bindings.Length, Is.EqualTo( 0 ) );
	}

	[Test]
	public void TestGteExpression()
	{
		var query = new DBQuery()
			.Where( "astrid.relationships.?other.reputation!?r" )
			.Where( "gte ?r 10" );

		var result = query.Run( _db );

		Assert.That( result.Success, Is.EqualTo( true ) );
		Assert.That( result.Bindings.Length, Is.EqualTo( 2 ) );
	}

	[Test]
	public void TestGteExpressionWithBindings()
	{
		var query = new DBQuery()
			.Where( "astrid.relationships.?other.reputation!?r" )
			.Where( "gte ?r 10" );

		var result = query.Run( _db, new Dictionary<string, object>() { { "?other", "lee" } } );

		Assert.That( result.Success, Is.EqualTo( true ) );
		Assert.That( result.Bindings.Length, Is.EqualTo( 1 ) );
	}

	[Test]
	public void TestLteWithMultipleVars()
	{
		// Relational expression with multiple variables
		var query = new DBQuery()
			.Where( "?A.relationships.?other.reputation!?r" )
			.Where( "lte ?r 0" );

		var result = query.Run( _db );

		Assert.That( result.Success, Is.EqualTo( true ) );
		Assert.That( result.Bindings.Length, Is.EqualTo( 2 ) );
	}

	[Test]
	public void TestNotExpression()
	{
		// Check that a sentence without variables is not true within the database
		var query = new DBQuery()
			.Where( "not player.relationships.jordan.reputation!30" );

		var result = query.Run( _db );

		Assert.That( result.Success, Is.EqualTo( true ) );
	}

	[Test]
	public void TestNotExpressionWithVars()
	{
		// For all relationships astrid has with all ?others,
		// no relationship has a reputation of 15
		var query = new DBQuery()
			.Where( "not astrid.relationships.?other.reputation!15" );

		var result = query.Run( _db );

		Assert.That( result.Success, Is.EqualTo( true ) );
	}

	[Test]
	public void TestNeqExpressionWithVars()
	{
		// For all relationships astrid has with an ?other,
		// filter for those where reputation is not 30
		var query = new DBQuery()
			.Where( "astrid.relationships.?other.reputation!?rep" )
			.Where( "neq ?rep 30" );

		var result = query.Run( _db );

		Assert.That( result.Success, Is.EqualTo( true ) );
		Assert.That( result.Bindings.Length, Is.EqualTo( 2 ) ); // britt and lee
	}

	[Test]
	public void TestNotExpressionWithBindings()
	{
		// Given that ?other is jordan, is the statement still not true?
		var query = new DBQuery()
			.Where( "not player.relationships.?other.reputation!30" );

		var result = query.Run( _db, new Dictionary<string, object>()
			{
				{"?other", "jordan"}
			} );

		Assert.That( result.Success, Is.EqualTo( true ) );
	}

	[Test]
	public void TestCompoundNotQueries()
	{
		DBQuery query;
		QueryResult result;

		// For all relationships astrid has with an ?other
		// filter for those where reputation is not 30
		query = new DBQuery()
			.Where( "astrid.relationships.?other" )
			.Where( "not astrid.relationships.?other.reputation!30" );

		result = query.Run( _db );

		Assert.That( result.Success, Is.EqualTo( true ) );
		Assert.That( result.Bindings.Length, Is.EqualTo( 2 ) ); // britt and lee

		// For all relationships astrid has with an ?other
		// filter for those where reputation from astrid to ?other is not 30
		// and ?other does not have a relationship with a spouse tag
		query = new DBQuery()
			.Where( "astrid.relationships.?other" )
			.Where( "not astrid.relationships.?other.reputation!30" )
			.Where( "not ?other.relationships.?others_spouse.tags.spouse" );

		result = query.Run( _db );

		Assert.That( result.Success, Is.EqualTo( true ) );
		Assert.That( result.Bindings.Length, Is.EqualTo( 1 ) ); // lee

		// For all relationships astrid has with an ?other
		// filter for those where reputation from astrid to ?other is not 30.
		// Also ensure that the player does not have a spouse
		query = new DBQuery()
			.Where( "astrid.relationships.?other" )
			.Where( "not astrid.relationships.?other.reputation!30" )
			// below will fail because player has spouse
			.Where( "not player.relationships.?x.tags.spouse" );

		result = query.Run( _db );

		Assert.That( result.Success, Is.EqualTo( false ) );

		// For all relationships astrid has with an ?other
		// filter for those where reputation from astrid to ?other is not 30.
		// Also ensure that the player does not have any friends
		query = new DBQuery()
			.Where( "astrid.relationships.?other" )
			.Where( "not astrid.relationships.?other.reputation!30" )
			.Where( "not player.relationships.?x.tags.friend" );

		result = query.Run( _db );

		Assert.That( result.Success, Is.EqualTo( true ) );
	}

	[Test]
	public void TestCompoundQueryWithVars()
	{
		// Compound query with multiple variables
		var query = new DBQuery()
			.Where( "?speaker.relationships.?other.reputation!?r0" )
			.Where( "gt ?r0 10" )
			.Where( "player.relationships.?other.reputation!?r1" )
			.Where( "lt ?r1 0" )
			.Where( "neq ?speaker player" );

		var result = query.Run( _db );

		Assert.That( result.Success, Is.EqualTo( true ) );
		Assert.That( result.Bindings.Length, Is.EqualTo( 1 ) );
	}

	[Test]
	public void TestResultsDataTypes()
	{
		// Compound query with multiple variables
		var query = new DBQuery()
			.Where( "?speaker.relationships.?other.reputation!?r0" )
			.Where( "gt ?r0 10" )
			.Where( "player.relationships.?other.reputation!?r1" )
			.Where( "lt ?r1 0" )
			.Where( "neq ?speaker player" );

		var result = query.Run( _db );

		Assert.That( result.Success, Is.True );
		Assert.That( result.Bindings.Length, Is.EqualTo( 1 ) );
		Assert.That( result.Bindings[0]["?speaker"] is string, Is.True );
		Assert.That( result.Bindings[0]["?other"] is string, Is.True );
		Assert.That( result.Bindings[0]["?r0"] is int, Is.True );
		Assert.That( result.Bindings[0]["?r1"] is int, Is.True );
	}

	/// <summary>
	/// Test insertion, deletion, and query of string literals
	/// </summary>
	[Test]
	public void TestStringLiteral()
	{
		RePraxisDatabase db = new RePraxisDatabase();

		db.Insert( "toph.fullName!Toph Beifong" );
		db.Insert( "toph.displayName!Toph: The greatest Earthbender in the world" );

		Assert.That( db.Assert( "toph.fullName!Toph Beifong" ) );

		QueryResult queryResult;

		queryResult = new DBQuery()
			.Where( "toph.fullName!?fullName" )
			.Run( db );

		Assert.That( queryResult.Bindings[0]["?fullName"], Is.EqualTo( "Toph Beifong" ) );
	}

	/// <summary>
	/// Ensure removing data that does not exist in the database never results
	/// in an error.
	/// </summary>
	[Test]
	public void TestRemoveNonexistentData()
	{
		RePraxisDatabase db = new RePraxisDatabase();

		db.Delete( "katara" );

		Assert.True( true );
	}

	/// <summary>
	/// Ensure running an empty query against the database will always yield
	/// a successful result.
	/// </summary>
	[Test]
	public void TestEmptyQuery()
	{
		RePraxisDatabase db = new RePraxisDatabase();

		QueryResult queryResult;

		queryResult = new DBQuery().Run( _db );

		Assert.That( queryResult.Success, Is.True );
	}
}
