using ApiForge.Domain.Models;
using ApiForge.Domain.Models.ApiParameters;
using ApiForge.Domain.Models.Schema;
using ApiForge.Infrastructure.Generator.Planning;

namespace ApiForge.Tests.Planning
{
    /// <summary>
    /// Tests for the SolutionPlanner class, which is responsible for creating 
    /// a plan for generating code from an API definition.
    /// </summary>
    public class SolutionPlannerTests
    {
        /// <summary>
        /// Tests that the CreatePlan method correctly groups endpoints by their route.
        /// </summary>
        [Fact]
        public void CreatePlan_GroupsEndpoints_ByRoute()
        {
            var def = new ApiDefinition
            {
                Title = "T",
                Version = "1",
                Endpoints =
                [
                    new ApiEndpoint { Route = "/payments/{id}", HttpMethod = "GET", OperationId = "GetPayment" },
                    new ApiEndpoint { Route = "/payments", HttpMethod = "POST", OperationId = "CreatePayment" },
                    new ApiEndpoint { Route = "/users/{id}", HttpMethod = "GET", OperationId = "GetUser" }
                ]
            };

            var plan = SolutionPlanner.CreatePlan(def, "Acme.Root");

            Assert.NotNull(plan);
            // Expect two groups: payments and users
            Assert.Contains(true, plan.Groups.Select(g => g.GroupName.Equals("Payments", StringComparison.InvariantCultureIgnoreCase)));
            Assert.Contains(true, plan.Groups.Select(g => g.GroupName.Equals("Users", StringComparison.InvariantCultureIgnoreCase)));
        }

        /// <summary>
        /// Tests that the CreatePlan method correctly maps parameters and request body from the API definition to the plan.
        /// </summary>
        [Fact]
        public void CreatePlan_ParametersAndRequestBody_AreMapped()
        {
            var def = new ApiDefinition
            {
                Title = "T",
                Version = "1",
                Endpoints =
                [
                    new ApiEndpoint {
                        Route = "/orders/{orderId}",
                        HttpMethod = "PUT",
                        OperationId = "UpdateOrder",
                        Parameters = [
                            new ApiPathParameter { Name = "orderId", Type = "Int32", Required = true, Location = Microsoft.OpenApi.ParameterLocation.Path },
                            new ApiQueryParameter { Name = "verbose", Type = "Boolean", Required = false, Location = Microsoft.OpenApi.ParameterLocation.Query }
                        ],
                        RequestBody = new ReferenceSchema { ReferenceName = "My.Company.Order", OpenApiType = "ref", ClrType = "Ref" },
                        Response = new PrimitiveSchema { OpenApiType = "primitive", ClrType = "String" }
                    }
               ]
            };

            var plan = SolutionPlanner.CreatePlan(def, "Acme.Root");

            var group = plan.Groups.Any() ? plan.Groups[0] : null;
            Assert.NotNull(group);
            var ep = group?.Endpoints[0];
            Assert.Equal("UpdateOrder", ep?.MethodName);
            Assert.Equal("string", ep?.ReturnType);
            Assert.True(ep?.PathParameters?.Any(p => p?.Name.Equals("orderId", StringComparison.InvariantCultureIgnoreCase) == true));
            Assert.True(ep?.QueryParameters?.Any(p => p?.Name.Equals("verbose", StringComparison.InvariantCultureIgnoreCase) == true));
            Assert.NotNull(ep?.RequestBodyType);
            Assert.Contains("Order", ep.RequestBodyType);
        }
    }
}
