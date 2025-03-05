using MongoDB.Driver;
using PfeProjet.Models;

namespace PfeProjet
{
   
        public class MongoDbContext
        {
            private readonly IMongoDatabase _database;

            public MongoDbContext(IMongoClient client, string databaseName)
            {
                _database = client.GetDatabase(databaseName);
            }

            public IMongoCollection<Pipeline> Pipelines => _database.GetCollection<Pipeline>("Pipelines");
        public IMongoCollection<Release> Releases => _database.GetCollection<Release>("Releases");
        public IMongoCollection<AgentPool> AgentPools => _database.GetCollection<AgentPool>("AgentPools");
    }
}