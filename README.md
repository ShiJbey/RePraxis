# Re:Praxis - Logic database and query language

Re:Praxis a reconstruction of the exclusion logic language used by the [Versu social simulation engine](https://versu.com/). We use this logic language to create in-memory databases where information is stored. It uses the logic language as the keys to a Dictionary-style object, giving users a familiar interface to work with.

However, the real benefit comes with Re:Praxis' query API. It enables user to look for instances of keys that match certain preconditions.

## Building Re:Praxis from source

⚠️ **Warning:** The following instructions still need to be vetted and the build process still needs adjustment.

Building Re:Praxis from source requires that you have .Net net installed. Run the following commands and new `RePraxis.dll` and `RePraxis.pdb` files will be generated within the `src/RePraxis/bin/Debug/net7.0` directory.

```bash
# Step 1: Clone the repository
git clone https://github.com/ShiJBey/RePraxis.git

# Step 2: Change to rhe project repository
cd RePraxis

# Step 3: Build using dotnet CLI
dotnet build
```

## Usage

Projects that want to use RePraxis should include `RePraxis.dll` and `Antlr.Runtime.Standard.dll` in their projects.

Below is an example of how to create a new database, add data, retrieve, and query data.

```csharp
var db = new RePraxisDatabase();

db["astrid.relationships.jordan.reputation"] = 30;
db["astrid.relationships.jordan.type"] = "Rivalry";
db["astrid.relationships.britt.reputation"] = -10;
db["astrid.relationships.britt.type"] = "ExLover";
db["astrid.relationships.lee.reputation"] = 20;
db["astrid.relationships.lee.type"] = "Friend";
db["player.relationships.jordan.reputation"] = -20;
db["player.relationships.jordan.type"] = "enemy";

// The query below find all symbols in the database that meet the conditions
// required to bind to ?other
var results = new DBQuery()
    .Where("?speaker.relationships.?other.reputation >= 10")
    .Where("player.relationships.?other.reputation < 0")
    .Run(db);
```
