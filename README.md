# ApiForge

ApiForge generates a .NET solution from an OpenAPI (Swagger) specification and returns it as a zipped download. It provides a small API to upload an OpenAPI JSON/YAML file, parse it, and generate a multi-project .NET solution (Domain, Application, Infrastructure, Client) using configurable architecture conventions.

Features
- Parse OpenAPI 3.0 documents (JSON/YAML).
- Detect or override architecture style (Clean Architecture or Hexagonal).
- Generate projects, source files, and a .sln file.
- Return generated solution as a .zip file.
- Unit and integration test coverage (NUnit).

Repository layout
- ApiForge/                 — ASP.NET Web API project (controllers, Program).
- ApiForge.Infrastructure/  — Parser and code generation logic.
- ApiForge.Application/     — Application interfaces and contracts.
- ApiForge.Domain/          — Domain models and DTOs used by generators.
- ApiForge.Tests/           — NUnit test project (unit + integration tests).

Prerequisites
- .NET 10 SDK installed (dotnet --version should report a 10.x SDK).
- Optional: Visual Studio 2026 or later for IDE experience.

Build and run locally (CLI)
1. Restore and build:
   dotnet restore
   dotnet build ApiForge.sln -c Debug

2. Run the API:
   dotnet run --project ApiForge/ApiForge.Api.csproj --configuration Debug

   The API listens on the URLs configured in ApiForge/Properties/launchSettings.json (or console output).

Endpoints
- GET /status
  - Returns a JSON status response with version, environment and uptime.

- POST /convert
  - Form field `file`: OpenAPI spec file (JSON or YAML).
  - Form field `architecture` (optional): "auto" (default), "clean", or "hexagonal".
  - Returns: application/zip with generated solution.

Example usage (curl):
curl -F "file=@./openapi.yaml" -F "architecture=auto" http://localhost:5000/convert --output generated.zip

Tests
Project test runner: NUnit via dotnet test.

Run all tests:
dotnet test ApiForge.sln -c Debug

Run tests for the test project only:
dotnet test ApiForge.Tests/ApiForge.Tests.csproj -c Debug

Test artifacts and coverage:
- CI workflow collects XPlat code coverage and uploads test results as artifacts.

Continuous Integration
A GitHub Actions workflow is included (.github/workflows/dotnet-test.yml) that:
- Restores, builds and runs tests on push and pull requests (Windows runners).
- Collects code coverage and uploads test results as artifacts.

Contributing
- Run and add tests for new generators or parser behaviors.
- Keep changes targeted and add unit tests for regressions.
- Follow repository coding conventions and run dotnet build and dotnet test before opening a PR.

Troubleshooting
- If Test Explorer cannot find test assemblies:
  - dotnet build ApiForge.sln -c Debug
  - dotnet test ApiForge.sln -c Debug
  - Remove the .vs folder and bin/obj directories and rebuild if Visual Studio caches stale project metadata.

License
Specify license terms here (if applicable).
