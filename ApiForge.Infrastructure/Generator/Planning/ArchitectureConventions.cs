using ApiForge.Domain.Enums;

namespace ApiForge.Infrastructure.Generator.Planning
{
    public sealed record ArchitectureConventions
    {
        public required string InterfaceNamespaceSuffix { get; init; }
        public required string ImplementationNamespaceSuffix { get; init; }
        public required string DependencyInjectionNamespaceSuffix { get; init; }
        public required Func<string, string> InterfaceName { get; init; }
        public required Func<string, string> ClassName { get; init; }
        public required Func<string, string> ExtensionMethodName { get; init; }

        public static ArchitectureConventions For(ArchitectureStyle style) => style switch
        {
            ArchitectureStyle.Hexagonal => Hexagonal(),
            _ => Clean()
        };

        private static ArchitectureConventions Clean() => new()
        {
            InterfaceNamespaceSuffix = "Application.Interfaces",
            ImplementationNamespaceSuffix = "Infrastructure.Clients",
            DependencyInjectionNamespaceSuffix = "Infrastructure.DependencyInjection",
            InterfaceName = group => $"I{group}Client",
            ClassName = group => $"{group}Client",
            ExtensionMethodName = rootNamespace => $"Add{rootNamespace}Clients"
        };

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