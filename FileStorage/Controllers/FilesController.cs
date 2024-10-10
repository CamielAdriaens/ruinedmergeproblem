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
        private readonly MongoDbContext _mongoDbContext;

        public FilesController(MongoDbContext mongoDbContext)
        {
            _mongoDbContext = mongoDbContext;
        }
        [HttpGet()]
        public async Task<IActionResult> GetFiles()
        {
            var files = await _mongoDbContext.GetFilesAsync();

            var fileInfos = new List<object>();
            foreach (var file in files)
            {
                fileInfos.Add(new
                {
                    Id = file.Id.ToString(),
                    FileName = file.Filename,
                    UploadDate = file.UploadDateTime,
                    Length = file.Length
                });
            }

            return Ok(fileInfos);
        }
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is not provided");

            using var stream = file.OpenReadStream();
            var fileId = await _mongoDbContext.UploadFileAsync(stream, file.FileName);

            return Ok(new { FileId = fileId.ToString() });
        }

        [HttpGet("download/{id}")]
        public async Task<IActionResult> DownloadFile(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
                return BadRequest("Invalid file ID");

            var fileStream = await _mongoDbContext.DownloadFileAsync(objectId);

            if (fileStream == null)
                return NotFound();

            return File(fileStream, "application/octet-stream", $"{id}.file");
        }

        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> DeleteFile(string id)
        {
            if (!ObjectId.TryParse(id, out ObjectId objectId))
                return BadRequest("Invalid file ID");

            await _mongoDbContext.DeleteFileAsync(objectId);

            return NoContent();
        }
    }
}