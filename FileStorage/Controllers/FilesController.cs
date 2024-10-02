using FileStorage.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.Security.Claims;



// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace FileStorage.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/files")]
    public class FilesController : ControllerBase
    {
        private readonly MongoDbContext _dbContext;

        public FilesController(MongoDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var bucket = new GridFSBucket(_dbContext.GetDatabase());

            using var stream = file.OpenReadStream();
            var fileId = await bucket.UploadFromStreamAsync(file.FileName, stream);

            // Save metadata linking file to user
            var fileInfo = new BsonDocument
        {
            { "userId", userId },
            { "fileId", fileId },
            { "fileName", file.FileName }
        };
            await _dbContext.GetFileCollection().InsertOneAsync(fileInfo);

            return Ok(new { FileId = fileId });
        }

        [HttpGet("my-files")]
        public async Task<IActionResult> GetUserFiles()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var files = await _dbContext.GetFileCollection()
                .Find(f => f["userId"] == userId)
                .ToListAsync();

            return Ok(files);
        }
    }

}
