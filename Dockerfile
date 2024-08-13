#
# Stage 1, Build Backend
#

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0 AS backend

ARG SQUIDEX__BUILD__VERSION=7.0.0

WORKDIR /src

# Copy nuget project files.
COPY backend/*.sln ./

# Copy the main source project files
COPY backend/src/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p src/${file%.*}/ && mv $file src/${file%.*}/; done

# Copy the test project files
COPY backend/tests/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p tests/${file%.*}/ && mv $file tests/${file%.*}/; done

# Copy the extension project files
COPY backend/extensions/*/*.csproj ./
RUN for file in $(ls *.csproj); do mkdir -p extensions/${file%.*}/ && mv $file extensions/${file%.*}/; done

RUN dotnet restore

COPY backend .
 
# Test Backend
RUN dotnet test --no-restore --filter Category!=Dependencies

# Publish
RUN dotnet publish --no-restore src/Squidex/Squidex.csproj --output /build/ --configuration Release -p:version=$SQUIDEX__BUILD__VERSION

# Install tools
RUN dotnet tool install --tool-path /tools dotnet-counters \
 && dotnet tool install --tool-path /tools dotnet-dump \
 && dotnet tool install --tool-path /tools dotnet-gcdump \
 && dotnet tool install --tool-path /tools dotnet-trace


#
# Stage 2, Build Frontend
#
FROM squidex/frontend-build:20.9 AS frontend

WORKDIR /src

ENV CONTINUOUS_INTEGRATION=1

# Copy Node project files
COPY frontend/package*.json ./

# Install Node packages 
RUN npm install --loglevel=error --force

COPY frontend .

# Build Frontend
RUN npm run test:coverage \
 && npm run build

RUN cp -a build /build/

 
#
# Stage 3, Build runtime
#
FROM mcr.microsoft.com/dotnet/aspnet:8.0-bookworm-slim

ARG SQUIDEX__RUNTIME__VERSION=7.0.0

# Curl for debugging and libc-dev for protobuf
RUN apt-get update \
 && apt-get install -y curl libc-dev

# Default tool directory
WORKDIR /tools

# Copy tools from backend build stage.
COPY --from=backend /tools .

# Default app directory
WORKDIR /app

# Copy from build stages
COPY --from=backend /build/ .
COPY --from=frontend /build/browser wwwroot/build/

EXPOSE 80
EXPOSE 443

ENV DIAGNOSTICS__COUNTERSTOOL=/tools/dotnet-counters
ENV DIAGNOSTICS__DUMPTOOL=/tools/dotnet-dump
ENV DIAGNOSTICS__GCDUMPTOOL=/tools/dotnet-gcdump
ENV DIAGNOSTICS__TRACETOOL=/tools/dotnet-trace
ENV ASPNETCORE_HTTP_PORTS=80

ENTRYPOINT ["dotnet", "Squidex.dll"]

ENV EXPOSEDCONFIGURATION__VERSION=$SQUIDEX__RUNTIME__VERSION
