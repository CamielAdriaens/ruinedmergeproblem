using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace FileStorage.Models
{
    public class FileMetadata
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("userId")]
        public string UserId { get; set; }

        [BsonElement("fileId")]
        public ObjectId FileId { get; set; }

        [BsonElement("fileName")]
        public string FileName { get; set; }

        [BsonElement("uploadDate")]
        public DateTime UploadDate { get; set; }
    }
}
