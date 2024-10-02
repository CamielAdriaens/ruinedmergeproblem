using FileStorage.Database;
using FileStorage.Models; // Import your FileMetadata model
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.Security.Claims;

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
            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            // Use GetDatabase to initialize the GridFSBucket
            var bucket = new GridFSBucket(_dbContext.GetDatabase());

            using var stream = file.OpenReadStream();
            var fileId = await bucket.UploadFromStreamAsync(file.FileName, stream);

            // Save metadata linking file to user
            var fileMetadata = new FileMetadata
            {
                UserId = userId,
                FileId = fileId,
                FileName = file.FileName,
                UploadDate = DateTime.UtcNow
            };
            await _dbContext.GetFileCollection().InsertOneAsync(fileMetadata);

            return Ok(new { FileId = fileId });
        }

        [HttpGet("my-files")]
        public async Task<IActionResult> GetUserFiles()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            var files = await _dbContext.GetFileCollection()
                .Find(f => f.UserId == userId)
                .ToListAsync();

            return Ok(files);
        }
        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadFile(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            var fileMetadata = await _dbContext.GetFileCollection().Find(f => f.Id == id && f.UserId == userId).FirstOrDefaultAsync();
            if (fileMetadata == null)
            {
                return NotFound("File not found.");
            }

            var bucket = new GridFSBucket(_dbContext.GetDatabase());
            var fileStream = await bucket.OpenDownloadStreamAsync(fileMetadata.FileId);

            return File(fileStream, "application/octet-stream", fileMetadata.FileName);
        }
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteFile(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("User is not authenticated.");
            }

            var fileMetadata = await _dbContext.GetFileCollection().Find(f => f.Id == id && f.UserId == userId).FirstOrDefaultAsync();
            if (fileMetadata == null)
            {
                return NotFound("File not found.");
            }

            var bucket = new GridFSBucket(_dbContext.GetDatabase());
            await bucket.DeleteAsync(fileMetadata.FileId);

            // Remove the metadata
            await _dbContext.GetFileCollection().DeleteOneAsync(f => f.Id == id);

            return Ok("File deleted.");
        }

    }
}
