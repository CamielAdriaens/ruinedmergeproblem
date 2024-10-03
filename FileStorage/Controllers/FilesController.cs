using FileStorage.Database;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using System.IO;
using System.Threading.Tasks;

namespace FileStorage.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IGridFSBucket _gridFS;

        public FilesController(MongoDbContext dbContext)
        {
            _gridFS = dbContext.GridFSBucket; // Inject GridFS from MongoDbContext
        }

        // List all files stored in GridFS
        [HttpGet]
        public async Task<IActionResult> GetFiles()
        {
            var files = await _gridFS.Find(Builders<GridFSFileInfo>.Filter.Empty).ToListAsync();
            return Ok(files);
        }

        // Upload a new file to GridFS
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("File not selected.");
            }

            using (var stream = file.OpenReadStream())
            {
                var fileId = await _gridFS.UploadFromStreamAsync(file.FileName, stream);
                return Ok(new { FileId = fileId });
            }
        }

        // Download a file from GridFS by its ObjectId
        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadFile(string id)
        {
            var fileId = new ObjectId(id);
            var stream = new MemoryStream();
            await _gridFS.DownloadToStreamAsync(fileId, stream);
            stream.Position = 0;  // Reset the stream position before returning the file

            var fileInfo = await _gridFS.Find(Builders<GridFSFileInfo>.Filter.Eq("_id", fileId)).FirstOrDefaultAsync();
            if (fileInfo == null)
            {
                return NotFound();
            }

            return File(stream, "application/octet-stream", fileInfo.Filename); // Download the file
        }
    }
}
