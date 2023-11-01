using RePraxis;

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
        db["A.relationships.B.reputation"] = 10;
        db["A.relationships.B.type"] = "Rivalry";

        // Retrieve values
        Assert.That(db["A.relationships.B.reputation"], Is.EqualTo(10));
        Assert.That(db["A.relationships.B.type"], Is.EqualTo("Rivalry"));
        Assert.That(db["A"], Is.EqualTo(true));

        // Update a value
        db["A.relationships.B.reputation"] = -99;
        Assert.That(db["A.relationships.B.reputation"], Is.EqualTo(-99));

        // Delete a value
        db.Remove("A.relationships.B.reputation");
        Assert.That(db["A.relationships.B.reputation"], Is.EqualTo(false));
    }

    [Test]
    public void TestQuery()
    {
        var db = new RePraxisDatabase();

        db["astrid.relationships.jordan.reputation"] = 30;
        db["astrid.relationships.jordan.type"] = "Rivalry";
        db["astrid.relationships.britt.reputation"] = -10;
        db["astrid.relationships.britt.type"] = "ExLover";
        db["astrid.relationships.lee.reputation"] = 20;
        db["astrid.relationships.lee.type"] = "Friend";
        db["player.relationships.jordan.reputation"] = -20;
        db["player.relationships.jordan.type"] = "enemy";

        // Relational expression with a single variable
        var r0 = new DBQuery()
            .Where("astrid.relationships.?other.reputation >= 10")
            .Run(db);

        Assert.That(r0.Success, Is.EqualTo(true));
        Assert.That(r0.Bindings.Length, Is.EqualTo(2));

        // Relational expression with multiple variables
        var r1 = new DBQuery()
            .Where("?A.relationships.?other.reputation <= 0")
            .Run(db);

        Assert.That(r1.Success, Is.EqualTo(true));
        Assert.That(r1.Bindings.Length, Is.EqualTo(2));

        // Assertion expression without variables
        var r2 = new DBQuery()
            .Where("astrid.relationships.britt")
            .Run(db);

        Assert.That(r2.Success, Is.EqualTo(true));
        Assert.That(r2.Bindings.Length, Is.EqualTo(0));

        // Failing assertion without variables
        var r3 = new DBQuery()
            .Where("astrid.relationships.haley")
            .Run(db);

        Assert.That(r3.Success, Is.EqualTo(false));
        Assert.That(r3.Bindings.Length, Is.EqualTo(0));

        // Compound query with multiple variables
        var r4 = new DBQuery()
            .Where("?speaker.relationships.?other.reputation >= 10")
            .Where("player.relationships.?other.reputation < 0")
            .Run(db);

        Assert.That(r4.Success, Is.EqualTo(true));
        Assert.That(r4.Bindings.Length, Is.EqualTo(1));
    }
}
