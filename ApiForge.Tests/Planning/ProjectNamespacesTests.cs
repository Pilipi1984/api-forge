using ApiForge.Infrastructure.Generator.Planning;

namespace ApiForge.Tests.Planning
{
    /// <summary>
    /// Tests for the ProjectNamespaces class
    /// </summary>
    public class ProjectNamespacesTests
    {
        /// <summary>
        /// Tests that the ClientsExtensionMethodName property uses the conventions defined in the ArchitectureConventions class
        /// </summary>
        [Fact]
        public void ClientsExtensionMethodName_UsesConventions()
        {
            var conventions = ArchitectureConventions.For(ApiForge.Domain.Enums.ArchitectureStyle.Clean);
            var ns = ProjectNamespaces.From("Acme.Root", conventions);

            var method = ns.ClientsExtensionMethodName;

            Assert.StartsWith("Add", method);
            Assert.EndsWith("Clients", method);
        }
    }
}
