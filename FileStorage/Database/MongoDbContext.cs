using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace FileStorage.Database
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly IGridFSBucket _gridFsBucket;

        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            if (settings == null || settings.Value == null)
            {
                throw new ArgumentNullException(nameof(settings), "MongoDB settings cannot be null");
            }

            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
            _gridFsBucket = new GridFSBucket(_database);
        }
        public IMongoDatabase Database => _database;

        public IGridFSBucket GridFsBucket => _gridFsBucket;
        public async Task<List<GridFSFileInfo>> GetFilesAsync()
        {
            var filter = Builders<GridFSFileInfo>.Filter.Empty;  // No filter for all files
            using var cursor = await _gridFsBucket.FindAsync(filter);
            return await cursor.ToListAsync();
        }
        public async Task<ObjectId> UploadFileAsync(Stream sourceStream, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentException("FileName cannot be null or empty", nameof(fileName));
            }

            var fileId = await _gridFsBucket.UploadFromStreamAsync(fileName, sourceStream);
            return fileId;
        }

        public async Task<Stream> DownloadFileAsync(ObjectId fileId)
        {
            var destinationStream = new MemoryStream();
            await _gridFsBucket.DownloadToStreamAsync(fileId, destinationStream);
            destinationStream.Position = 0;  // Reset stream position after download
            return destinationStream;
        }

        public async Task DeleteFileAsync(ObjectId fileId)
        {
            await _gridFsBucket.DeleteAsync(fileId);
        }
    }
}