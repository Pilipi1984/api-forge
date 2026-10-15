using ApiForge.Domain.Enums;
using ApiForge.Domain.Models;
using ApiForge.Domain.Models.ApiParameters;
using ApiForge.Domain.Models.Schema;
using ApiForge.Infrastructure.Generator;
using ApiForge.Infrastructure.Generator.Planning;
using Microsoft.OpenApi;

namespace ApiForge.Tests.Generators
{
    /// <summary>
    /// Tests for the contract and client generators
    /// </summary>
    public class ContractAndClientGeneratorTests
    {
        /// <summary>
        /// Builds a solution plan for the given architecture style
        /// </summary>
        /// <param name="style">The architecture style</param>
        /// <returns>The solution plan</returns>
        private static SolutionPlan BuildPlan(ArchitectureStyle style) =>
            SolutionPlanner.CreatePlan(new ApiDefinition
            {
                Title = "Shop",
                Version = "1",
                Architecture = style,
                Endpoints =
                [
                    new ApiEndpoint
                    {
                        Route = "/pets/{petId}",
                        HttpMethod = "GET",
                        OperationId = "getPet",
                        Summary = "Gets a pet",
                        Parameters =
                        [
                            new ApiPathParameter { Name = "petId", Type = "int", Required = true, Location = ParameterLocation.Path },
                            new ApiQueryParameter { Name = "limit", Type = "int", Location = ParameterLocation.Query },
                            new ApiHeaderParameter { Name = "traceId", Type = "string", Location = ParameterLocation.Header }
                        ],
                        Response = new ReferenceSchema { ReferenceName = "Pet", OpenApiType = "object", ClrType = "Pet" }
                    },
                    new ApiEndpoint
                    {
                        Route = "/pets",
                        HttpMethod = "POST",
                        OperationId = "createPet",
                        RequestBody = new ReferenceSchema { ReferenceName = "Pet", OpenApiType = "object", ClrType = "Pet" },
                        Response = new PrimitiveSchema { OpenApiType = "string", ClrType = "string" }
                    }
                ]
            }, "Shop");

        /// <summary>
        /// Tests that the contract generator generates an interface with typed methods for the Clean architecture style
        /// </summary>
        [Fact]
        public void Contract_Clean_GeneratesInterfaceWithTypedMethods()
        {
            var file = Assert.Single(ApiContractGenerator.Generate(BuildPlan(ArchitectureStyle.Clean)));

            Assert.Equal("Shop.Application/Interfaces/IPetsClient.cs", file.RelativePath);
            Assert.Contains("namespace Shop.Application.Interfaces", file.Content);
            Assert.Contains("public interface IPetsClient", file.Content);
            Assert.Contains("/// Gets a pet", file.Content);
            Assert.Contains(
                "Task<Shop.Domain.Models.Pet> GetPetAsync(int petId, int? limit, string? traceId, CancellationToken cancellationToken = default);",
                file.Content);
            Assert.Contains(
                "Task<string> CreatePetAsync(Shop.Domain.Models.Pet request, CancellationToken cancellationToken = default);",
                file.Content);
        }

        /// <summary>
        /// Tests that the contract generator generates a port interface for the Hexagonal architecture style
        /// </summary>
        [Fact]
        public void Contract_Hexagonal_GeneratesPort()
        {
            var file = Assert.Single(ApiContractGenerator.Generate(BuildPlan(ArchitectureStyle.Hexagonal)));

            Assert.Equal("Shop.Application/Ports/Driven/IPetsClientPort.cs", file.RelativePath);
            Assert.Contains("namespace Shop.Application.Ports.Driven", file.Content);
            Assert.Contains("public interface IPetsClientPort", file.Content);
        }

        /// <summary>
        /// Tests that the client implementation generator generates a client class that 
        /// implements the interface for the Clean architecture style
        /// </summary>
        [Fact]
        public void Client_Clean_GeneratesHttpImplementation()
        {
            var file = Assert.Single(ApiClientImplementationGenerator.Generate(BuildPlan(ArchitectureStyle.Clean)));

            Assert.Equal("Shop.Infrastructure/Clients/PetsClient.cs", file.RelativePath);
            Assert.Contains("using Shop.Application.Interfaces;", file.Content);
            Assert.Contains("public sealed class PetsClient : IPetsClient", file.Content);
            Assert.Contains("var path = $\"pets/{petId}\";", file.Content);
            Assert.Contains("queryParameters.Add($\"limit=", file.Content);
            Assert.Contains("TryAddWithoutValidation(\"traceId\"", file.Content);
            Assert.Contains("new HttpRequestMessage(HttpMethod.Get, path)", file.Content);
            Assert.Contains("new HttpRequestMessage(HttpMethod.Post, path)", file.Content);
            Assert.Contains("JsonContent.Create(request)", file.Content);
            Assert.Contains("ReadFromJsonAsync<Shop.Domain.Models.Pet>", file.Content);
            Assert.Contains("ReadFromJsonAsync<string>", file.Content);
        }

        /// <summary>
        /// Tests that the client implementation generator generates an adapter class 
        /// that implements the port interface for the Hexagonal architecture style
        /// </summary>
        [Fact]
        public void Client_Hexagonal_GeneratesAdapter()
        {
            var file = Assert.Single(ApiClientImplementationGenerator.Generate(BuildPlan(ArchitectureStyle.Hexagonal)));

            Assert.Equal("Shop.Infrastructure/Adapters/Driven/PetsClientAdapter.cs", file.RelativePath);
            Assert.Contains("public sealed class PetsClientAdapter : IPetsClientPort", file.Content);
        }

        /// <summary>
        /// Tests that the client implementation generator maps HTTP methods to the correct HttpMethod expression
        /// </summary>
        /// <param name="method">The HTTP method</param>
        /// <param name="expected">The expected HttpMethod expression</param>
        [Theory]
        [InlineData("GET", "HttpMethod.Get")]
        [InlineData("post", "HttpMethod.Post")]
        [InlineData("PUT", "HttpMethod.Put")]
        [InlineData("DELETE", "HttpMethod.Delete")]
        [InlineData("PATCH", "HttpMethod.Patch")]
        [InlineData("HEAD", "HttpMethod.Head")]
        [InlineData("OPTIONS", "HttpMethod.Options")]
        [InlineData("TRACE", "HttpMethod.Trace")]
        [InlineData("FOO", "HttpMethod.Get")]
        public void Client_MapsHttpMethods(string method, string expected)
        {
            var plan = SolutionPlanner.CreatePlan(new ApiDefinition
            {
                Title = "Shop",
                Endpoints = [new ApiEndpoint { Route = "/items", HttpMethod = method, OperationId = "op" }]
            }, "Shop");

            var file = Assert.Single(ApiClientImplementationGenerator.Generate(plan));

            Assert.Contains(expected, file.Content);
        }

        /// <summary>
        /// Tests that the generators produce no files when there are no endpoints defined in the API definition
        /// </summary>
        [Fact]
        public void Generators_NoEndpoints_ProduceNoFiles()
        {
            var plan = SolutionPlanner.CreatePlan(new ApiDefinition { Title = "Shop" }, "Shop");

            Assert.Empty(ApiContractGenerator.Generate(plan));
            Assert.Empty(ApiClientImplementationGenerator.Generate(plan));
        }

        /// <summary>
        /// Tests that the solution planner uses the root group name for endpoints with a root route
        /// </summary>
        [Fact]
        public void Planner_RootRoute_UsesRootGroup()
        {
            var plan = SolutionPlanner.CreatePlan(new ApiDefinition
            {
                Title = "Shop",
                Endpoints = [new ApiEndpoint { Route = "/{id}", HttpMethod = "GET" }]
            }, "Shop");

            Assert.Equal("Root", Assert.Single(plan.Groups).GroupName);
        }
    }
}