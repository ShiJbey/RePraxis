namespace RePraxis.Tests;

public class Tests
{
	/// <summary>
	/// Test that create, retrieval, updating, and deletion (CRUD)
	/// </summary>
	[Test]
	public void TestDatabaseCRUD()
	{
		var db = new RePraxisDatabase();

		// Create values
		db.Insert( "A.relationships.B.reputation!10" );
		db.Insert( "A.relationships.B.type!rivalry" );

		// Retrieve values
		Assert.That( db.Assert( "A.relationships.B.reputation!19" ), Is.EqualTo( false ) );
		Assert.That( db.Assert( "A.relationships.B.type" ), Is.EqualTo( true ) );
		Assert.That( db.Assert( "A" ), Is.EqualTo( true ) );

		// Update a value
		db.Insert( "A.relationships.B.reputation!-99" );
		Assert.That( db.Assert( "A.relationships.B.reputation.-99" ), Is.EqualTo( false ) );

		// Delete a value
		db.Delete( "A.relationships.B.reputation" );
		Assert.That( db.Assert( "A.relationships.B.reputation" ), Is.EqualTo( false ) );
	}

	[Test]
	public void TestQuery()
	{
		var db = new RePraxisDatabase();

		db.Insert( "astrid.relationships.jordan.reputation!30" );
		db.Insert( "astrid.relationships.jordan.tags.rivalry" );
		db.Insert( "astrid.relationships.britt.reputation!-10" );
		db.Insert( "astrid.relationships.britt.tags.ex_lover" );
		db.Insert( "astrid.relationships.lee.reputation!20" );
		db.Insert( "astrid.relationships.lee.tags.friend" );
		db.Insert( "player.relationships.jordan.reputation!-20" );
		db.Insert( "player.relationships.jordan.tags.enemy" );

		// Relational expression with a single variable
		var r0 = new DBQuery()
			.Where( "astrid.relationships.?other.reputation!?r" )
			.Where( "gte ?r 10" )
			.Run( db );

		Assert.That( r0.Success, Is.EqualTo( true ) );
		Assert.That( r0.Bindings.Length, Is.EqualTo( 2 ) );

		// Relational expression with a single variable
		var r0_b = new DBQuery()
			.Where( "astrid.relationships.?other.reputation!?r" )
			.Where( "gte ?r 10" )
			.Run( db, new Dictionary<string, string>() { { "?other", "lee" } } );

		Assert.That( r0_b.Success, Is.EqualTo( true ) );
		Assert.That( r0_b.Bindings.Length, Is.EqualTo( 1 ) );

		// Relational expression with multiple variables
		var r1 = new DBQuery()
			.Where( "?A.relationships.?other.reputation!?r" )
			.Where( "lte ?r 0" )
			.Run( db );

		Assert.That( r1.Success, Is.EqualTo( true ) );
		Assert.That( r1.Bindings.Length, Is.EqualTo( 2 ) );

		// Assertion expression without variables
		var r2 = new DBQuery()
			.Where( "astrid.relationships.britt" )
			.Run( db );

		Assert.That( r2.Success, Is.EqualTo( true ) );
		Assert.That( r2.Bindings.Length, Is.EqualTo( 1 ) );

		// Failing assertion without variables
		var r3 = new DBQuery()
			.Where( "astrid.relationships.haley" )
			.Run( db );

		Assert.That( r3.Success, Is.EqualTo( false ) );
		Assert.That( r3.Bindings.Length, Is.EqualTo( 0 ) );

		// Compound query with multiple variables
		var r4 = new DBQuery()
			.Where( "?speaker.relationships.?other.reputation!?r0" )
			.Where( "gt ?r0 10" )
			.Where( "player.relationships.?other.reputation!?r1" )
			.Where( "lt ?r1 0" )
			.Where( "neq ?speaker player" )
			.Run( db );

		Assert.That( r4.Success, Is.EqualTo( true ) );
		Assert.That( r4.Bindings.Length, Is.EqualTo( 1 ) );

		// Check for negation
		var r5 = new DBQuery()
			.Where( "not player.relationships.jordan.reputation!30" )
			.Run( db );

		Assert.That( r5.Success, Is.EqualTo( true ) );
	}
}
