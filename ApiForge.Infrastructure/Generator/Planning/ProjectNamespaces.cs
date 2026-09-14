using ApiForge.Infrastructure.Helpers;

namespace ApiForge.Infrastructure.Generator.Planning
{
    /// <summary>
    /// Represents the namespaces used in the generated C# solution, based on a given root namespace.
    /// </summary>
    public sealed record ProjectNamespaces
    {
        public required string RootNamespace { get; init; }

        public string DomainNamespace => $"{RootNamespace}.Domain";
        public string DomainModelsNamespace => $"{DomainNamespace}.Models";

        public string ApplicationNamespace => $"{RootNamespace}.Application";
        public string ApplicationInterfacesNamespace => $"{ApplicationNamespace}.Interfaces";

        public string InfrastructureNamespace => $"{RootNamespace}.Infrastructure";
        public string InfrastructureClientsNamespace => $"{InfrastructureNamespace}.Clients";
        public string InfrastructureDependencyInjectionNamespace => $"{InfrastructureNamespace}.DependencyInjection";

        public string ClientNamespace => $"{RootNamespace}.Client";

        /// <summary>
        /// Namespaces can contain dots (valid in a namespace), but a C# identifier cannot. 
        /// This property sanitizes the root namespace for use in method names.
        /// </summary>
        public string ClientsExtensionMethodName => $"Add{NameHelper.ToPascalCase(RootNamespace)}Clients";

        public static ProjectNamespaces From(string rootNamespace) => new() { RootNamespace = rootNamespace };
    }
}