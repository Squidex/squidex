#
# Stage 1, Build Backend
#
FROM mcr.microsoft.com/dotnet/core/sdk:3.0-buster as backend

ARG SQUIDEX__VERSION=1.0.0

WORKDIR /src

# Copy nuget project files.
COPY backend/**/**/*.csproj /tmp/
# Copy nuget.config for package sources.
COPY backend/NuGet.Config /tmp/

# Install nuget packages
RUN bash -c 'pushd /tmp; for p in *.csproj; do dotnet restore $p --verbosity quiet; true; done; popd'

COPY backend .
 
# Test Backend
RUN dotnet test tests/Squidex.Infrastructure.Tests/Squidex.Infrastructure.Tests.csproj --filter Category!=Dependencies \ 
 && dotnet test tests/Squidex.Domain.Apps.Core.Tests/Squidex.Domain.Apps.Core.Tests.csproj \ 
 && dotnet test tests/Squidex.Domain.Apps.Entities.Tests/Squidex.Domain.Apps.Entities.Tests.csproj \
 && dotnet test tests/Squidex.Domain.Users.Tests/Squidex.Domain.Users.Tests.csproj \
 && dotnet test tests/Squidex.Web.Tests/Squidex.Web.Tests.csproj

# Publish
RUN dotnet publish src/Squidex/Squidex.csproj --output /build/ --configuration Release -p:version=$SQUIDEX__VERSION


#
# Stage 2, Build Frontend
#
FROM buildkite/puppeteer:latest as frontend

WORKDIR /src

# Copy Node project files.
COPY frontend/package*.json /tmp/

# Install Node packages 
RUN cd /tmp && npm install --loglevel=error

COPY frontend .

# Build Frontend
RUN cp -a /tmp/node_modules . \
 && npm run test:coverage \
 && npm run build

RUN cp -a build /build/


#
# Stage 3, Build runtime
#
FROM mcr.microsoft.com/dotnet/core/aspnet:3.0-buster-slim

# Add curl
RUN apt-get install curl

# Default AspNetCore directory
WORKDIR /app

# Copy from build stages
COPY --from=backend /build/ .
COPY --from=frontend /build/ wwwroot/build/

EXPOSE 80
EXPOSE 11111

ENTRYPOINT ["dotnet", "Squidex.dll"]
