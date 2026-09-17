using Microsoft.AspNetCore.Http;
using System.Text;

namespace ApiForge.Tests.Helpers
{
    internal static class TestHelpers
    {
        public static IFormFile CreateFormFile(string content, string fileName = "test.yaml")
        {
            var bytes = Encoding.UTF8.GetBytes(content);
            var ms = new MemoryStream(bytes);
            return new FormFile(ms, 0, bytes.Length, "file", fileName);
        }
    }
}
