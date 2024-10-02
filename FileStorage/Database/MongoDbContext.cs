using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Microsoft.Extensions.Configuration;
using FileStorage.Models;

namespace FileStorage.Database
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly IConfiguration _configuration;

        public MongoDbContext(IConfiguration configuration)
        {
            _configuration = configuration;
            var client = new MongoClient(_configuration["MongoDB:ConnectionString"]);
            _database = client.GetDatabase(_configuration["MongoDB:DatabaseName"]);
        }

        // This method provides access to the IMongoDatabase instance
        public IMongoDatabase GetDatabase()
        {
            return _database;
        }

        // Return the file metadata collection (e.g., storing metadata about files)
        public IMongoCollection<FileMetadata> GetFileCollection() =>
            _database.GetCollection<FileMetadata>(_configuration["MongoDB:FileCollection"]);
    }
}
