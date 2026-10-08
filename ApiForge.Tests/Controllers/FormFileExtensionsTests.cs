#nullable enable
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using ApiForge.Api.Controllers;
using Microsoft.AspNetCore.Http;
using Xunit;

namespace ApiForge.Tests.Controllers
{
    public class FormFileExtensionsTests
    {
        [Fact]
        public async Task CreateFormFileFromPathAsync_KnownExtension_SetsContentAndMetadataAsync()
        {
            // Arrange
            var content = "Hello, world!";
            var bytes = Encoding.UTF8.GetBytes(content);
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".txt");
            try
            {
                await File.WriteAllBytesAsync(tempPath, bytes, TestContext.Current.CancellationToken);

                // Act
                var formFile = await FormFileExtensions.CreateFormFileFromPathAsync(tempPath, "myField");

                // Assert
                Assert.Equal(Path.GetFileName(tempPath), formFile.FileName);
                Assert.Equal("myField", formFile.Name);
                Assert.Equal(bytes.Length, formFile.Length);
                Assert.Equal("text/plain", formFile.ContentType);

                // Verify content can be read and matches
                using var stream = formFile.OpenReadStream();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms, TestContext.Current.CancellationToken);
                var read = ms.ToArray();
                Assert.Equal(bytes.Length, read.Length);
                Assert.Equal(bytes, read);

                Assert.NotNull(formFile.Headers);
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }

        [Fact]
        public async Task CreateFormFileFromPathAsync_UnknownExtension_UsesOctetStreamAndDefaultFieldNameAsync()
        {
            // Arrange
            var content = "Data with unknown extension";
            var bytes = Encoding.UTF8.GetBytes(content);
            var tempPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".unknownext");
            try
            {
                await File.WriteAllBytesAsync(tempPath, bytes, TestContext.Current.CancellationToken);

                // Act
                var formFile = await FormFileExtensions.CreateFormFileFromPathAsync(tempPath);

                // Assert
                Assert.Equal(Path.GetFileName(tempPath), formFile.FileName);
                Assert.Equal("file", formFile.Name); // default form field name
                Assert.Equal(bytes.Length, formFile.Length);
                Assert.Equal("application/octet-stream", formFile.ContentType);

                using var stream = formFile.OpenReadStream();
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms, TestContext.Current.CancellationToken);
                var read = ms.ToArray();
                Assert.Equal(bytes, read);
            }
            finally
            {
                if (File.Exists(tempPath)) File.Delete(tempPath);
            }
        }

    }
}
