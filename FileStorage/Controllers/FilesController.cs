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
            try
            {
                // Fetch all files from GridFS
                var files = await _gridFS.Find(Builders<GridFSFileInfo>.Filter.Empty).ToListAsync();

                // Simplify the GridFSFileInfo object to avoid serialization issues
                var fileDtos = files.Select(file => new
                {
                    Id = file.Id.ToString(),  // Convert ObjectId to string
                    Filename = file.Filename,
                    Length = file.Length,     // Size of the file
                    UploadDate = file.UploadDateTime
                }).ToList();

                return Ok(fileDtos);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching files: {ex.Message}");
                return StatusCode(500, "An error occurred while fetching the files.");
            }
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

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadFile(string id)
        {
            try
            {
                Console.WriteLine($"Downloading file with ID: {id}");  // Log the ID for debugging
                var fileId = new ObjectId(id);  // Convert the string ID to MongoDB ObjectId

                var stream = new MemoryStream();
                await _gridFS.DownloadToStreamAsync(fileId, stream);
                stream.Position = 0;  // Reset stream position before returning the file

                var fileInfo = await _gridFS.Find(Builders<GridFSFileInfo>.Filter.Eq("_id", fileId)).FirstOrDefaultAsync();
                if (fileInfo == null)
                {
                    return NotFound();
                }

                return File(stream, "application/octet-stream", fileInfo.Filename);  // Download the file
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error downloading file: {ex.Message}");
                return StatusCode(500, "An error occurred while downloading the file.");
            }
        }
    }
}