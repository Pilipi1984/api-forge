using ApiForge.Domain.Models;
using ApiForge.Domain.Models.ApiParameters;
using ApiForge.Domain.Models.Schema;
using ApiForge.Infrastructure.Generator.Planning;
using Xunit;

namespace ApiForge.Tests.Planning
{
    public class SolutionPlannerTests
    {
        [Fact]
        public void CreatePlan_GroupsEndpoints_ByRoute()
        {
            var def = new ApiDefinition
            {
                Title = "T",
                Version = "1",
                Endpoints = new System.Collections.Generic.List<ApiEndpoint>
                {
                    new ApiEndpoint { Route = "/payments/{id}", HttpMethod = "GET", OperationId = "GetPayment" },
                    new ApiEndpoint { Route = "/payments", HttpMethod = "POST", OperationId = "CreatePayment" },
                    new ApiEndpoint { Route = "/users/{id}", HttpMethod = "GET", OperationId = "GetUser" }
                }
            };

            var plan = SolutionPlanner.CreatePlan(def, "Acme.Root");

            Assert.NotNull(plan);
            // Expect two groups: payments and users
            Assert.Contains(true, plan.Groups.Select(g => g.GroupName == "Payments"));
            Assert.Contains(true, plan.Groups.Select(g => g.GroupName == "Users"));
        }

        [Fact]
        public void CreatePlan_ParametersAndRequestBody_AreMapped()
        {
            var def = new ApiDefinition
            {
                Title = "T",
                Version = "1",
                Endpoints = new System.Collections.Generic.List<ApiEndpoint>
                {
                    new ApiEndpoint {
                        Route = "/orders/{orderId}",
                        HttpMethod = "PUT",
                        OperationId = "UpdateOrder",
                        Parameters = new System.Collections.Generic.List<ApiParameter>
                        {
                            new ApiPathParameter { Name = "orderId", Type = "Int32", Required = true, Location = Microsoft.OpenApi.ParameterLocation.Path },
                            new ApiQueryParameter { Name = "verbose", Type = "Boolean", Required = false, Location = Microsoft.OpenApi.ParameterLocation.Query }
                        },
                        RequestBody = new ReferenceSchema { ReferenceName = "My.Company.Order", OpenApiType = "ref", ClrType = "Ref" },
                        Response = new PrimitiveSchema { OpenApiType = "primitive", ClrType = "String" }
                    }
                }
            };

            var plan = SolutionPlanner.CreatePlan(def, "Acme.Root");

            var group = plan.Groups.Any() ? plan.Groups[0] : null;
            Assert.NotNull(group);
            var ep = group?.Endpoints[0];
            Assert.Equal("UpdateOrder", ep?.MethodName);
            Assert.Equal("string", ep?.ReturnType);
            Assert.Contains(true, ep?.PathParameters.Select(p => p.Name == "orderId"));
            Assert.Contains(true, ep?.QueryParameters.Select(p => p.Name == "verbose"));
            Assert.NotNull(ep?.RequestBodyType);
            Assert.Contains("Order", ep.RequestBodyType);
        }
    }
}
