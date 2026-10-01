FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copy everything and restore
COPY . ./

RUN dotnet restore ApiForge.sln
RUN dotnet build ApiForge.sln -c Release --no-restore

# Run tests with coverage collection
RUN dotnet test ApiForge.Tests/ApiForge.Tests.csproj --results-directory /output --collect:"XPlat Code Coverage" -v minimal || true

# Install reportgenerator tool and produce an HTML summary
RUN dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.1.16 || true
ENV PATH="$PATH:/root/.dotnet/tools"
RUN reportgenerator -reports:/output/*/coverage.cobertura.xml -targetdir:/output/coverage-report -reporttypes:HtmlSummary || true

WORKDIR /src
CMD ["/bin/bash"]
