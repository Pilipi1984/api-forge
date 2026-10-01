using ApiForge.Infrastructure.Helpers;

namespace ApiForge.Tests.Helpers
{
    public class QualifiedNameResolverTests
    {
        [Fact]
        public void Resolve_ReturnsUnnamed_ForEmpty()
        {
            var res = QualifiedNameResolver.Resolve(null);
            Assert.Equal("Unnamed", res.ClassName);
            Assert.Empty(res.NamespaceSegments);
        }

        [Fact]
        public void Resolve_SplitsAndPascalCases()
        {
            var full = "Company.Product.Service.MyClass";
            var res = QualifiedNameResolver.Resolve(full);

            Assert.Equal("MyClass", res.ClassName);
            Assert.Equal(["Company", "Product", "Service"], res.NamespaceSegments);
        }
    }
}
