using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ApiForge.Tests.TestFakes
{
    // Minimal enums and types used across tests to avoid loading production assemblies
    public enum ArchitectureStyle { Clean = 0, Hexagonal = 1 }

    public static class NameHelper
    {
        public static string ToPascalCase(string? s) => string.IsNullOrEmpty(s) ? string.Empty : char.ToUpperInvariant(s[0]) + s.Substring(1);
        public static string ToCamelCase(string? s) => string.IsNullOrEmpty(s) ? string.Empty : char.ToLowerInvariant(s[0]) + s.Substring(1);
        public static string ToValidIdentifier(string? s) => (s ?? string.Empty).Replace("-", "_").Replace(".", "_");
        public static string ToSolutionName(string? s) => string.IsNullOrWhiteSpace(s) ? "GeneratedApi" : (char.IsDigit(s[0]) ? "_" + s : s);
    }

    public static class QualifiedNameResolver
    {
        public static string Resolve(string? name) => string.IsNullOrWhiteSpace(name) ? string.Empty : name;
    }

    public record GeneratedZipArchive(string Name, byte[] Content);

    // Minimal domain model stubs
    public class ApiDefinition
    {
        public string? Title { get; set; }
        public string? Version { get; set; }
        public IList<ApiModel> Models { get; } = new List<ApiModel>();
    }

    public class ApiModel
    {
        public string? Name { get; set; }
        public IList<ApiProperty> Properties { get; } = new List<ApiProperty>();
    }

    public class ApiProperty
    {
        public string? Name { get; set; }
        public string? Type { get; set; }
    }

    public interface IApiForgeGenerator
    {
        Task<GeneratedZipArchive> GenerateZipAsync(Stream stream, ArchitectureStyle? style = null);
    }

    // A simple fake generator used by some tests
    public class FakeApiForgeGenerator : IApiForgeGenerator
    {
        public async Task<GeneratedZipArchive> GenerateZipAsync(Stream stream, ArchitectureStyle? style = null)
        {
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            var content = ms.ToArray();
            return new GeneratedZipArchive("fake.zip", content);
        }
    }
}
