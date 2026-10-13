FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

ENV DOTNET_CLI_TELEMETRY_OPTOUT=1 \
    DOTNET_COVERAGE_TELEMETRY_OPTOUT=1 \
    PATH="$PATH:/root/.dotnet/tools"

# Copiar solo lo necesario (nada de "COPY . ./")
COPY ApiForge.sln global.json ./
COPY ApiForge/ ApiForge/
COPY ApiForge.Application/ ApiForge.Application/
COPY ApiForge.Domain/ ApiForge.Domain/
COPY ApiForge.Infrastructure/ ApiForge.Infrastructure/
COPY ApiForge.Generator/ ApiForge.Generator/
COPY ApiForge.Tests/ ApiForge.Tests/

# README.md lo referencia ApiForge.Generator.csproj al empaquetar
COPY README.md ./

RUN dotnet restore ApiForge.sln \
 && dotnet build ApiForge.sln -c Release --no-restore \
# Tests + cobertura (falla el build si fallan los tests)
 && dotnet tool install --global dotnet-coverage \
 && dotnet tool install --global dotnet-reportgenerator-globaltool \
 && dotnet-coverage collect \
      "dotnet test --solution ApiForge.sln -c Release --no-build" \
      -f cobertura -o /output/coverage.cobertura.xml \
 && reportgenerator \
      -reports:/output/coverage.cobertura.xml \
      -targetdir:/output/coverage-report \
      -reporttypes:HtmlSummary

CMD ["/bin/bash"]