using ApiForge.Domain.Enums;

namespace ApiForge.Infrastructure.Generator.Planning
{
    /// <summary>
    /// Record to store architecture conventions depending on ArchitectureStyle
    /// </summary>
    public sealed record ArchitectureConventions
    {
        /// <summary>
        /// Interface namespaces suffix
        /// </summary>
        public required string InterfaceNamespaceSuffix { get; init; }
        /// <summary>
        /// Implementation namespaces suffix
        /// </summary>
        public required string ImplementationNamespaceSuffix { get; init; }
        /// <summary>
        /// Dependency injection namespaces suffix
        /// </summary>
        public required string DependencyInjectionNamespaceSuffix { get; init; }
        /// <summary>
        /// Name of interfaces by group
        /// </summary>
        public required Func<string, string> InterfaceName { get; init; }
        /// <summary>
        /// Name of classes by group
        /// </summary>
        public required Func<string, string> ClassName { get; init; }
        /// <summary>
        /// Name of extension methods by group
        /// </summary>
        public required Func<string, string> ExtensionMethodName { get; init; }

        /// <summary>
        /// Gets ArchitectureConventions by ArchitectureStyle
        /// </summary>
        /// <param name="style">ArchitectureStyle</param>
        /// <returns>ArchitectureConventions</returns>
        public static ArchitectureConventions For(ArchitectureStyle style) => style switch
        {
            ArchitectureStyle.Hexagonal => Hexagonal(),
            _ => Clean()
        };

        /// <summary>
        /// Gets ArchitectureConventions for ArchitectureStyle.Clean
        /// </summary>
        /// <returns>ArchitectureConventions</returns>
        private static ArchitectureConventions Clean() => new()
        {
            InterfaceNamespaceSuffix = "Application.Interfaces",
            ImplementationNamespaceSuffix = "Infrastructure.Clients",
            DependencyInjectionNamespaceSuffix = "Infrastructure.DependencyInjection",
            InterfaceName = group => $"I{group}Client",
            ClassName = group => $"{group}Client",
            ExtensionMethodName = rootNamespace => $"Add{rootNamespace}Clients"
        };

        /// <summary>
        /// Gets ArchitectureConventions for ArchitectureStyle.Hexagonal
        /// </summary>
        /// <returns>ArchitectureConventions</returns>
        private static ArchitectureConventions Hexagonal() => new()
        {
            InterfaceNamespaceSuffix = "Application.Ports.Driven",
            ImplementationNamespaceSuffix = "Infrastructure.Adapters.Driven",
            DependencyInjectionNamespaceSuffix = "Infrastructure.Adapters.DependencyInjection",
            InterfaceName = group => $"I{group}ClientPort",
            ClassName = group => $"{group}ClientAdapter",
            ExtensionMethodName = rootNamespace => $"Add{rootNamespace}Adapters"
        };
    }
}