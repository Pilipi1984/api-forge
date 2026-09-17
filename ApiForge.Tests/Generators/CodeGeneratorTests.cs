using ApiForge.Infrastructure.Generator;
using ApiForge.Domain.Models;
using NUnit.Framework;
using System.Linq;

namespace ApiForge.Tests.Generators
{
    public class CodeGeneratorTests
    {
        [Test]
        public async Task GenerateAsync_ReturnsSolutionWithFiles()
        {
            var generator = new CodeGenerator();
            var def = new ApiDefinition { Title = "T", Version = "1" };

            var solution = await generator.GenerateAsync(def);

            Assert.IsNotNull(solution);
            Assert.IsNotEmpty(solution.Files);
            Assert.IsNotNull(solution.Name);
            Assert.IsTrue(solution.Files.Any(f => f.RelativePath.EndsWith(".sln") || f.RelativePath.EndsWith("Program.cs") || f.RelativePath.EndsWith(".csproj")));
        }
    }
}
