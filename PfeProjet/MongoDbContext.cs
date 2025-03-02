using MongoDB.Driver;
using PfeProjet.Models;
namespace PfeProjet
{
    public class MongoDbContext
    {

        
            private readonly IMongoDatabase _database;

            public MongoDbContext(MongoClient client, string databaseName)
            {
                _database = client.GetDatabase(databaseName);
            }

            public IMongoCollection<Pipeline> Pipelines => _database.GetCollection<Pipeline>("Pipelines");
    }
}

