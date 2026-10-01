using ApiForge.Infrastructure.Generator.Planning;
using Xunit;

namespace ApiForge.Tests.Planning
{
    public class ProjectNamespacesTests
    {
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
