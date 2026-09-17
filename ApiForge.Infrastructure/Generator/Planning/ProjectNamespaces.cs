using ApiForge.Infrastructure.Helpers;

namespace ApiForge.Infrastructure.Generator.Planning
{
    /// <summary>
    /// Represents the namespaces used in the generated C# solution, based on a given root namespace
    /// and the <see cref="ArchitectureConventions"/> for the detected/selected architecture style.
    /// Suffix-derived namespaces (interfaces/ports, implementations/adapters, dependency injection)
    /// come exclusively from <see cref="Conventions"/>, so this stays the single source of truth
    /// for both Clean Architecture and Hexagonal naming — no duplicated if/else per generator.
    /// </summary>
    public sealed record ProjectNamespaces
    {
        public required string RootNamespace { get; init; }

        public required ArchitectureConventions Conventions { get; init; }

        public string DomainNamespace => $"{RootNamespace}.Domain";
        public string DomainModelsNamespace => $"{DomainNamespace}.Models";

        public string ApplicationNamespace => $"{RootNamespace}.Application";
        public string ApplicationInterfacesNamespace => $"{RootNamespace}.{Conventions.InterfaceNamespaceSuffix}";

        public string InfrastructureNamespace => $"{RootNamespace}.Infrastructure";
        public string InfrastructureClientsNamespace => $"{RootNamespace}.{Conventions.ImplementationNamespaceSuffix}";
        public string InfrastructureDependencyInjectionNamespace => $"{RootNamespace}.{Conventions.DependencyInjectionNamespaceSuffix}";

        public string ClientNamespace => $"{RootNamespace}.Client";

        /// <summary>
        /// Namespaces can contain dots (valid in a namespace), but a C# identifier cannot.
        /// This property sanitizes the root namespace for use in method names, and delegates
        /// the actual naming convention (e.g. "AddXClients" vs "AddXAdapters") to Conventions.
        /// </summary>
        public string ClientsExtensionMethodName => Conventions.ExtensionMethodName(NameHelper.ToPascalCase(RootNamespace));

        public static ProjectNamespaces From(string rootNamespace, ArchitectureConventions conventions) =>
            new() { RootNamespace = rootNamespace, Conventions = conventions };
    }
}
