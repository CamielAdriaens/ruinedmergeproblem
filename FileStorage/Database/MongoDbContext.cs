using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Microsoft.Extensions.Configuration;

namespace FileStorage.Database
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        public IGridFSBucket GridFSBucket { get; }

        public MongoDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["MongoDB:ConnectionString"]);
            _database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
            GridFSBucket = new GridFSBucket(_database);  // Initialize GridFS bucket
        }

        public IMongoDatabase GetDatabase()
        {
            return _database;
        }
    }
}
