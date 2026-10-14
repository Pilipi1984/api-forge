# ApiForge.Generator

[![NuGet](https://img.shields.io/nuget/v/ApiForge.Generator.svg)](https://www.nuget.org/packages/ApiForge.Generator)
[![License: MIT](https://img.shields.io/badge/license-MIT-blue.svg)](https://github.com/Pilipi1984/api-forge/blob/main/LICENSE)

Generate a typed C# client solution from an OpenAPI specification, **in-process**: no HTTP call, no external tooling.

Give it an OpenAPI 3.x document (JSON or YAML) and it returns a complete multi-project .NET solution (Domain, Application, Infrastructure and Client), laid out following **Clean Architecture** or **Hexagonal (Ports & Adapters)** conventions.

## Features

- Parses OpenAPI 3.0 / 3.1 documents in JSON or YAML.
- Generates a ready-to-build solution: `.sln`, `.csproj` files, domain models, client contracts, HTTP implementations and DI registration.
- Two architecture styles: Clean Architecture and Hexagonal.
- Architecture can be declared in the spec (`x-architecture`) or forced from code.
- Returns the result either as in-memory files or as a `.zip`.

## Installation

```bash
dotnet add package ApiForge.Generator
```

Requires **.NET 10** or later.

## Quick start

```csharp
using ApiForge.Domain.Enums;
using ApiForge.Generator;

var generator = new ApiForgeGenerator();

await using var spec = File.OpenRead("openapi.yaml");
var zip = await generator.GenerateZipAsync(spec);

await File.WriteAllBytesAsync($"{zip.Name}.zip", zip.Content);
```

### Get the files in memory

```csharp
await using var spec = File.OpenRead("openapi.yaml");
var solution = await generator.GenerateAsync(spec);

foreach (var file in solution.Files)
{
    var path = Path.Combine("output", file.RelativePath);
    Directory.CreateDirectory(Path.GetDirectoryName(path)!);
    await File.WriteAllTextAsync(path, file.Content);
}
```

### Force an architecture style

```csharp
var solution = await generator.GenerateAsync(spec, ArchitectureStyle.Hexagonal);
```

An explicit style always wins over whatever the spec declares.

### Use with dependency injection

`ApiForgeGenerator` implements `IApiForgeGenerator` (namespace `ApiForge.Application.Interfaces`), so it can be registered and injected:

```csharp
services.AddScoped<IOpenApiParser, OpenApiParser>();
services.AddScoped<ICodeGenerator, CodeGenerator>();
services.AddScoped<IApiForgeGenerator, ApiForgeGenerator>();
```

You can also pass your own `IOpenApiParser` / `ICodeGenerator` implementations to the constructor, which is handy for testing.

## Choosing the architecture

The style is resolved in this order:

1. **Explicit override** passed to `GenerateAsync` / `GenerateZipAsync`.
2. **`x-architecture` extension** in the spec, either at the document root or inside `info`.
3. **Tag heuristic**: if any tag name contains `port`, `adapter`, `driven` or `driving`, Hexagonal is assumed.
4. **Clean Architecture** as the default.

```yaml
openapi: 3.0.0
x-architecture: hexagonal   # hexagonal | ports-and-adapters | clean | clean-architecture
info:
  title: Shop
  version: 1.0.0
```

Values are case-insensitive.

## What gets generated

For a spec titled `Shop` with a `/pets` group of endpoints:

| | Clean | Hexagonal |
|---|---|---|
| Contract | `Shop.Application/Interfaces/IPetsClient.cs` | `Shop.Application/Ports/Driven/IPetsClientPort.cs` |
| Implementation | `Shop.Infrastructure/Clients/PetsClient.cs` | `Shop.Infrastructure/Adapters/Driven/PetsClientAdapter.cs` |
| DI extension | `Shop.Infrastructure/DependencyInjection/ServiceCollectionExtensions.cs` | `Shop.Infrastructure/Adapters/DependencyInjection/ServiceCollectionExtensions.cs` |
| DI method | `AddShopClients(...)` | `AddShopAdapters(...)` |

Common to both styles:

```
Shop.sln
Shop.Domain/          # models (Shop.Domain/Models/*.cs)
Shop.Application/     # contracts
Shop.Infrastructure/  # HttpClient-based implementations + DI
Shop.Client/          # console app wiring everything together
```

The solution name and root namespace come from `info.title` (sanitized into a valid namespace; falls back to `GeneratedApi`).

### Conventions

- **Grouping**: endpoints are grouped by the first non-parameter segment of their route (`/pets/{id}` → `Pets`). Routes with no such segment go to `Root`.
- **Method names**: PascalCase `operationId` + `Async`. Without an `operationId`, the HTTP method and route are combined. Duplicates get a numeric suffix.
- **Parameters**: order is path, query, header, request body, then `CancellationToken`. Optional parameters are nullable.
- **Type mapping**:

| OpenAPI | C# |
|---|---|
| `string` | `string` |
| `string` + `date-time` | `DateTimeOffset` |
| `string` + `date` | `DateOnly` |
| `string` + `uuid` | `Guid` |
| `integer` | `int` |
| `integer` + `int64` | `long` |
| `number` | `double` |
| `number` + `float` | `float` |
| `boolean` | `bool` |
| `array` | `List<T>` |
| `$ref` | generated model type |

### Example output

Generated contract:

```csharp
public interface IPetsClient
{
    /// <summary>
    /// Gets a pet
    /// </summary>
    Task<Shop.Domain.Models.Pet> GetPetAsync(int petId, int? limit, string? traceId, CancellationToken cancellationToken = default);

    Task<string> CreatePetAsync(Shop.Domain.Models.Pet request, CancellationToken cancellationToken = default);
}
```

Registering the generated clients in a consuming application:

```csharp
services.AddShopClients(new Uri("https://api.example.com"));
```

## Current limitations

- Only the first response and the first content type of each operation are used.
- Cookie parameters are parsed but not emitted in the generated clients.
- Enums are generated as `string`; inline object schemas resolve to `object`.
- Model properties are mapped from primitive types; nested `$ref` or array properties fall back to `object`.

Contributions and issues are welcome.

## Also available as a web API

The repository includes an ASP.NET Core API that exposes the same functionality over HTTP (`POST /v1/convert`, with a `file` and an optional `architecture` field). See the [repository](https://github.com/Pilipi1984/api-forge) for details.

## License

[MIT](https://github.com/Pilipi1984/api-forge/blob/main/LICENSE)
