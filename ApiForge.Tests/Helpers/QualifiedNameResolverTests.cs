using ApiForge.Infrastructure.Helpers;

namespace ApiForge.Tests.Helpers
{
    /// <summary>
    /// Tests for the <see cref="QualifiedNameResolver"/> class.
    /// </summary>
    public class QualifiedNameResolverTests
    {
        /// <summary>
        /// Tests that the <see cref="QualifiedNameResolver.Resolve(string?)"/> method returns "Unnamed" for an empty input.
        /// </summary>
        [Fact]
        public void Resolve_ReturnsUnnamed_ForEmpty()
        {
            var res = QualifiedNameResolver.Resolve(null);
            Assert.Equal("Unnamed", res.ClassName);
            Assert.Empty(res.NamespaceSegments);
        }

        /// <summary>
        /// Tests that the <see cref="QualifiedNameResolver.Resolve(string?)"/> method splits 
        /// a qualified name into its class name and namespace segments.
        /// </summary>
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
