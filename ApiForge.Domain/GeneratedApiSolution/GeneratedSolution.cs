namespace ApiForge.Domain.GeneratedApiSolution
{
    /// <summary>
    /// The output generated solution from OpenApi definition
    /// </summary>
    public sealed class GeneratedSolution
    {
        /// <summary>
        /// The name of the solution
        /// </summary>
        public required string Name { get; init; }

        /// <summary>
        /// Root namespace
        /// </summary>
        public required string RootNamespace { get; init; }

        /// <summary>
        /// The files that belong to the solution
        /// </summary>

        public IReadOnlyList<GeneratedFile> Files { get; init; } = [];
    }

    /// <summary>
    /// Generated .zip archive containing the solution files, ready for download.
    /// </summary>
    public sealed record GeneratedZipArchive
    {
        /// <summary>
        /// Name of the generated solution (used as the .zip file name).
        /// </summary>
        public required string Name { get; init; }
        /// <summary>
        /// Content of the generated .zip file.
        /// </summary>
        public required byte[] Content { get; init; }
    }
}
