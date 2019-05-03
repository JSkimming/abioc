########################################################################################################################
# .NET Core 2.1
FROM mcr.microsoft.com/dotnet/core/sdk:2.1

ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true

WORKDIR /work

# Copy just the solution and proj files to make best use of docker image caching
COPY ./abioc.sln .
COPY ./src/Abioc/Abioc.csproj ./src/Abioc/Abioc.csproj
COPY ./test/Abioc.Tests/Abioc.Tests.csproj ./test/Abioc.Tests/Abioc.Tests.csproj
COPY ./test/Abioc.Tests.Internal/Abioc.Tests.Internal.csproj ./test/Abioc.Tests.Internal/Abioc.Tests.Internal.csproj

# Run restore on just the project files, this should cache the image after restore.
RUN dotnet restore

COPY . .

# Build to ensure the tests are their own distinct step
RUN dotnet build --no-restore -f netcoreapp2.1 -c Debug ./test/Abioc.Tests/Abioc.Tests.csproj

# Run unit tests
RUN dotnet test --no-restore --no-build -c Debug -f netcoreapp2.1 test/Abioc.Tests/Abioc.Tests.csproj /p:CollectCoverage=true /p:Include="[Abioc]*" /p:Exclude="[*.Tests]*"

########################################################################################################################
# .NET Core 2.2
FROM mcr.microsoft.com/dotnet/core/sdk:2.2

ENV DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true

WORKDIR /work

# Copy just the solution and proj files to make best use of docker image caching
COPY ./abioc.sln .
COPY ./src/Abioc/Abioc.csproj ./src/Abioc/Abioc.csproj
COPY ./test/Abioc.Tests/Abioc.Tests.csproj ./test/Abioc.Tests/Abioc.Tests.csproj
COPY ./test/Abioc.Tests.Internal/Abioc.Tests.Internal.csproj ./test/Abioc.Tests.Internal/Abioc.Tests.Internal.csproj

# Run restore on just the project files, this should cache the image after restore.
RUN dotnet restore

COPY . .

# Build to ensure the tests are their own distinct step
RUN dotnet build --no-restore -f netcoreapp2.1 -c Debug ./test/Abioc.Tests/Abioc.Tests.csproj

# Run unit tests
RUN dotnet test --no-restore --no-build -c Debug -f netcoreapp2.1 test/Abioc.Tests/Abioc.Tests.csproj /p:CollectCoverage=true /p:Include="[Abioc]*" /p:Exclude="[*.Tests]*"
