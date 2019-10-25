#
# Stage 1, Prebuild
#
FROM squidex/dotnet:3.0-buster-chromium-phantomjs-node as builder

ARG SQUIDEX__VERSION=1.0.0

WORKDIR /src

# Copy Node project files.
COPY src/Squidex/package*.json /tmp/

# Install Node packages 
RUN cd /tmp && npm install --loglevel=error

# Copy nuget project files.
COPY /**/**/*.csproj /tmp/
# Copy nuget.config for package sources.
COPY NuGet.Config /tmp/

# Install nuget packages
RUN bash -c 'pushd /tmp; for p in *.csproj; do dotnet restore $p --verbosity quiet; true; done; popd'

COPY . .

# Build Frontend
RUN cp -a /tmp/node_modules src/Squidex/ \
 && cd src/Squidex \
 && npm run test:coverage \
 && npm run build
 
# Test Backend
RUN dotnet test tests/Squidex.Infrastructure.Tests/Squidex.Infrastructure.Tests.csproj --filter Category!=Dependencies \ 
 && dotnet test tests/Squidex.Domain.Apps.Core.Tests/Squidex.Domain.Apps.Core.Tests.csproj \ 
 && dotnet test tests/Squidex.Domain.Apps.Entities.Tests/Squidex.Domain.Apps.Entities.Tests.csproj \
 && dotnet test tests/Squidex.Domain.Users.Tests/Squidex.Domain.Users.Tests.csproj \
 && dotnet test tests/Squidex.Web.Tests/Squidex.Web.Tests.csproj

# Publish
RUN dotnet publish src/Squidex/Squidex.csproj --output /out/ --configuration Release -p:version=$SQUIDEX__VERSION

#
# Stage 2, Build runtime
#
FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim

# Default AspNetCore directory
WORKDIR /app

# Copy from build stage
COPY --from=builder /out/ .

EXPOSE 80
EXPOSE 11111

ENTRYPOINT ["dotnet", "Squidex.dll"]