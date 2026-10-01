using ApiForge.Infrastructure.Generator;
using ApiForge.Domain.Models;
using Xunit;
using System.Linq;

namespace ApiForge.Tests.Generators
{
    public class CodeGeneratorTests
    {
        [Fact]
        public async Task GenerateAsync_ReturnsSolutionWithFiles()
        {
            var generator = new CodeGenerator();
            var def = new ApiDefinition { Title = "T", Version = "1" };

            var solution = await generator.GenerateAsync(def);

            Assert.NotNull(solution);
            Assert.NotEmpty(solution.Files);
            Assert.NotNull(solution.Name);
            Assert.True(solution.Files.Any(f => f.RelativePath.EndsWith(".sln") || f.RelativePath.EndsWith("Program.cs") || f.RelativePath.EndsWith(".csproj")));
        }
    }
}
