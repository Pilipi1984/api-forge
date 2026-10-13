using ApiForge.Domain.Enums;
using ApiForge.Infrastructure.Generator.Planning;

namespace ApiForge.Tests.Planning
{
    public class ArchitectureConventionsTests
    {
        public static TheoryData<ArchitectureStyle, string, string, string, string, string> Cases => new()
        {
            { ArchitectureStyle.Clean, "IPaymentsClient", "PaymentsClient", "AddAcmeClients",
              "Application.Interfaces", "Infrastructure.Clients" },
            { ArchitectureStyle.Hexagonal, "IPaymentsClientPort", "PaymentsClientAdapter", "AddAcmeAdapters",
              "Application.Ports.Driven", "Infrastructure.Adapters.Driven" }
        };

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

        [Fact]
        public void For_UnknownStyle_FallsBackToClean()
        {
            var c = ArchitectureConventions.For((ArchitectureStyle)99);

            Assert.Equal("IPaymentsClient", c.InterfaceName("Payments"));
        }

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