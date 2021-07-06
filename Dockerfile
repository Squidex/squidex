#
# Stage 1, Build Backend
#
FROM mcr.microsoft.com/dotnet/sdk:5.0 as backend

ARG SQUIDEX__VERSION=5.0.0

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
RUN dotnet publish --no-restore src/Squidex/Squidex.csproj --output /build/ --configuration Release -p:version=$SQUIDEX__VERSION


#
# Stage 2, Build Frontend
#
FROM buildkite/puppeteer:5.2.1 as frontend

WORKDIR /src

ENV CONTINUOUS_INTEGRATION=1

# Copy Node project files.
COPY frontend/package*.json /tmp/

# Copy patches for broken npm packages
COPY frontend/patches /tmp/patches

# Install Node packages 
RUN cd /tmp && npm set unsafe-perm true && npm install --loglevel=error

COPY frontend .

# Build Frontend
RUN cp -a /tmp/node_modules . \
 && npm run test:coverage \
 && npm run build

RUN cp -a build /build/


#
# Stage 3, Build runtime
#
FROM mcr.microsoft.com/dotnet/aspnet:5.0.0-buster-slim

# Curl for debugging and libc-dev for Protobuf
RUN apt-get update \
 && apt-get install -y curl libc-dev

# Default AspNetCore directory
WORKDIR /app

# Copy from backend build stages
COPY --from=backend /build/ .

# Copy from backend build stages to webserver folder
COPY --from=frontend /build/ wwwroot/build/

EXPOSE 80
EXPOSE 11111

ENTRYPOINT ["dotnet", "Squidex.dll"]
