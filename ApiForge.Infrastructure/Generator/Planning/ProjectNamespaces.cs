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
        /// <summary>
        /// Name of the root namespace used as the default prefix for types and resources in the project.
        /// </summary>
        public required string RootNamespace { get; init; }

        /// <summary>
        /// Conjunto de convenciones de arquitectura aplicadas al diseño y comportamiento del sistema.
        /// </summary>
        /// <remarks>Requerida; debe fijarse durante la inicialización y permanece inmutable tras la
        /// construcción.</remarks>
        public required ArchitectureConventions Conventions { get; init; }

        /// <summary>
        /// Domain namespace
        /// </summary>
        public string DomainNamespace => $"{RootNamespace}.Domain";
        /// <summary>
        /// Domain models namespace
        /// </summary>
        public string DomainModelsNamespace => $"{DomainNamespace}.Models";

        /// <summary>
        /// Application namespace
        /// </summary>
        public string ApplicationNamespace => $"{RootNamespace}.Application";
        /// <summary>
        /// Application interfaces namespace
        /// </summary>
        public string ApplicationInterfacesNamespace => $"{RootNamespace}.{Conventions.InterfaceNamespaceSuffix}";

        /// <summary>
        /// Namespace of infrastructure
        /// </summary>
        public string InfrastructureNamespace => $"{RootNamespace}.Infrastructure";
        /// <summary>
        /// Infrastructure clients namespace
        /// </summary>
        public string InfrastructureClientsNamespace => $"{RootNamespace}.{Conventions.ImplementationNamespaceSuffix}";
        /// <summary>
        /// Infrastructure dependency injection namespaces
        /// </summary>
        public string InfrastructureDependencyInjectionNamespace => $"{RootNamespace}.{Conventions.DependencyInjectionNamespaceSuffix}";

        /// <summary>
        /// Client namespace
        /// </summary>
        public string ClientNamespace => $"{RootNamespace}.Client";

        /// <summary>
        /// Namespaces can contain dots (valid in a namespace), but a C# identifier cannot.
        /// This property sanitizes the root namespace for use in method names, and delegates
        /// the actual naming convention (e.g. "AddXClients" vs "AddXAdapters") to Conventions.
        /// </summary>
        public string ClientsExtensionMethodName => Conventions.ExtensionMethodName(NameHelper.ToPascalCase(RootNamespace));

        /// <summary>
        /// Generates a new instance of <see cref="ProjectNamespaces"/> with the specified root namespace and architecture conventions.
        /// </summary>
        /// <param name="rootNamespace">Root namespace</param>
        /// <param name="conventions">Architecture conventions</param>
        /// <returns>Project Namespaces</returns>
        public static ProjectNamespaces From(string rootNamespace, ArchitectureConventions conventions) =>
            new() { RootNamespace = rootNamespace, Conventions = conventions };
    }
}
