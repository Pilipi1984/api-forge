using Microsoft.AspNetCore.StaticFiles;

namespace ApiForge.Api.Controllers
{
    public class FormFileExtensions
    {
        public static async Task<IFormFile> CreateFormFileFromPathAsync(string path, string formFieldName = "file")
        {
            var provider = new FileExtensionContentTypeProvider();
            if (!provider.TryGetContentType(path, out var contentType))
            {
                contentType = "application/octet-stream";
            }

            var fileName = Path.GetFileName(path);
            var ms = new MemoryStream();
            using (var fs = File.OpenRead(path))
            {
                await fs.CopyToAsync(ms);
            }
            ms.Position = 0;
            var formFile = new FormFile(ms, 0, ms.Length, formFieldName, fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };

            return formFile;
        }
    }
}
