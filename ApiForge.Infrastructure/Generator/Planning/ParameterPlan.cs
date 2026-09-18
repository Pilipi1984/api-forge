using ApiForge.Domain.Models.ApiParameters;

namespace ApiForge.Infrastructure.Generator.Planning
{
    /// <summary>
    /// Represents a plan for a parameter, grouping the parameter's name, its C# type, and its source in the API.
    /// </summary>
    public sealed record ParameterPlan
    {
        /// <summary>
        /// Name
        /// </summary>
        public required string Name { get; init; }
        /// <summary>
        /// Csharp type
        /// </summary>
        public required string CSharpType { get; init; }
        /// <summary>
        /// Source ApiParameter
        /// </summary>
        public required ApiParameter Source { get; init; }
    }
}
