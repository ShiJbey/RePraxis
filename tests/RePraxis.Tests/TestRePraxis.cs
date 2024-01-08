namespace RePraxis.Tests;

public class Tests
{
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
	public void TestQuery()
	{
		var db = new RePraxisDatabase();

		db.Insert( "astrid.relationships.jordan.reputation!30" );
		db.Insert( "astrid.relationships.jordan.tags.rivalry" );
		db.Insert( "astrid.relationships.britt.reputation!-10" );
		db.Insert( "astrid.relationships.britt.tags.ex_lover" );
		db.Insert( "astrid.relationships.lee.reputation!20" );
		db.Insert( "astrid.relationships.lee.tags.friend" );
		db.Insert( "britt.relationships.player.tags.spouse" );
		db.Insert( "player.relationships.jordan.reputation!-20" );
		db.Insert( "player.relationships.jordan.tags.enemy" );
		db.Insert( "player.relationships.britt.tags.spouse" );

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
		Assert.That( r2.Bindings.Length, Is.EqualTo( 0 ) );

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

		// Check that a sentence without variables is not true within the database
		var r5 = new DBQuery()
			.Where( "not player.relationships.jordan.reputation!30" )
			.Run( db );

		Assert.That( r5.Success, Is.EqualTo( true ) );

		// Given that ?other is jordan, is the statement still not true?
		var r6 = new DBQuery()
			.Where( "not player.relationships.?other.reputation!30" )
			.Run( db, new Dictionary<string, string>()
			{
				{"?other", "jordan"}
			} );

		Assert.That( r6.Success, Is.EqualTo( true ) );

		// For all relationships astrid has with all ?others,
		// no relationship has a reputation of 15
		var r7 = new DBQuery()
			.Where( "not astrid.relationships.?other.reputation!15" )
			.Run( db );

		Assert.That( r7.Success, Is.EqualTo( true ) );

		// For all relationships astrid has with an ?other,
		// filter for those where reputation is not 30
		var r7_b = new DBQuery()
			.Where( "astrid.relationships.?other.reputation!?rep" )
			.Where( "neq ?rep 30" )
			.Run( db );

		Assert.That( r7_b.Success, Is.EqualTo( true ) );
		Assert.That( r7_b.Bindings.Length, Is.EqualTo( 2 ) ); // britt and lee


		// For all relationships astrid has with an ?other
		// filter for those where reputation is not 30
		var r8 = new DBQuery()
			.Where( "astrid.relationships.?other" )
			.Where( "not astrid.relationships.?other.reputation!30" )
			.Run( db );

		Assert.That( r8.Success, Is.EqualTo( true ) );
		Assert.That( r8.Bindings.Length, Is.EqualTo( 2 ) ); // britt and lee

		// For all relationships astrid has with an ?other
		// filter for those where reputation from astrid to ?other is not 30
		// and ?other does not have a relationship with a spouse tag
		var r9 = new DBQuery()
			.Where( "astrid.relationships.?other" )
			.Where( "not astrid.relationships.?other.reputation!30" )
			.Where( "not ?other.relationships.?others_spouse.tags.spouse" )
			.Run( db );

		Assert.That( r9.Success, Is.EqualTo( true ) );
		Assert.That( r9.Bindings.Length, Is.EqualTo( 1 ) ); // lee

		// For all relationships astrid has with an ?other
		// filter for those where reputation from astrid to ?other is not 30.
		// Also ensure that the player does not have a spouse
		var r10 = new DBQuery()
			.Where( "astrid.relationships.?other" )
			.Where( "not astrid.relationships.?other.reputation!30" )
			// below will fail because player has spouse
			.Where( "not player.relationships.?x.tags.spouse" )
			.Run( db );

		Assert.That( r10.Success, Is.EqualTo( false ) );

		// For all relationships astrid has with an ?other
		// filter for those where reputation from astrid to ?other is not 30.
		// Also ensure that the player does not have any friends
		var r11 = new DBQuery()
			.Where( "astrid.relationships.?other" )
			.Where( "not astrid.relationships.?other.reputation!30" )
			.Where( "not player.relationships.?x.tags.friend" )
			.Run( db );

		Assert.That( r11.Success, Is.EqualTo( true ) );
	}
}
