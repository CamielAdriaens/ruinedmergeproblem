using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using FileStorage.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using System.Security.Claims;

namespace FileStorage.Database
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IConfiguration configuration)
        {
            var client = new MongoClient(configuration["MongoDB:ConnectionString"]);
            _database = client.GetDatabase(configuration["MongoDB:DatabaseName"]);
        }

        public IMongoCollection<GridFSFileInfo> GetFileCollection() =>
            _database.GetCollection<GridFSFileInfo>(configuration["MongoDB:FileCollection"]);
    }

}
