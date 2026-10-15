using ApiForge.Domain.Enums;
using ApiForge.Infrastructure.Generator.Planning;

namespace ApiForge.Tests.Planning
{
    /// <summary>
    /// Tests for the ArchitectureConventions class
    /// </summary>
    public class ArchitectureConventionsTests
    {
        /// <summary>
        /// Test cases for the For method
        /// </summary>
        public static TheoryData<ArchitectureStyle, string, string, string, string, string> Cases => new()
        {
            { ArchitectureStyle.Clean, "IPaymentsClient", "PaymentsClient", "AddAcmeClients",
              "Application.Interfaces", "Infrastructure.Clients" },
            { ArchitectureStyle.Hexagonal, "IPaymentsClientPort", "PaymentsClientAdapter", "AddAcmeAdapters",
              "Application.Ports.Driven", "Infrastructure.Adapters.Driven" }
        };

        /// <summary>
        /// Tests that the For method returns the expected conventions for each architecture style
        /// </summary>
        /// <param name="style">The architecture style.</param>
        /// <param name="iface">The expected interface name.</param>
        /// <param name="cls">The expected class name.</param>
        /// <param name="ext">The expected extension method name.</param>
        /// <param name="ifaceNs">The expected interface namespace suffix.</param>
        /// <param name="implNs">The expected implementation namespace suffix.</param>
        [Theory]
        [MemberData(nameof(Cases))]
        public void For_ReturnsExpectedConventions(
            ArchitectureStyle style, string iface, string cls, string ext, string ifaceNs, string implNs)
        {
            var c = ArchitectureConventions.For(style);

            Assert.Equal(iface, c.InterfaceName("Payments"));
            Assert.Equal(cls, c.ClassName("Payments"));
            Assert.Equal(ext, c.ExtensionMethodName("Acme"));
            Assert.Equal(ifaceNs, c.InterfaceNamespaceSuffix);
            Assert.Equal(implNs, c.ImplementationNamespaceSuffix);
        }

        /// <summary>
        /// Tests that the For method falls back to Clean conventions for unknown architecture styles
        /// </summary>
        [Fact]
        public void For_UnknownStyle_FallsBackToClean()
        {
            var c = ArchitectureConventions.For((ArchitectureStyle)99);

            Assert.Equal("IPaymentsClient", c.InterfaceName("Payments"));
        }

        /// <summary>
        /// Tests that the ProjectNamespaces class uses the correct namespaces for Hexagonal architecture
        /// </summary>
        [Fact]
        public void ProjectNamespaces_Hexagonal_UsesPortsAndAdaptersNamespaces()
        {
            var ns = ProjectNamespaces.From("Shop", ArchitectureConventions.For(ArchitectureStyle.Hexagonal));

            Assert.Equal("Shop.Application.Ports.Driven", ns.ApplicationInterfacesNamespace);
            Assert.Equal("Shop.Infrastructure.Adapters.Driven", ns.InfrastructureClientsNamespace);
            Assert.Equal("Shop.Infrastructure.Adapters.DependencyInjection", ns.InfrastructureDependencyInjectionNamespace);
        }
    }
}