#
# Stage 1, Prebuild
#
FROM squidex/dotnet:2.2-sdk-chromium-phantomjs-node as builder

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
RUN dotnet publish src/Squidex/Squidex.csproj --output /out/alpine --configuration Release -r alpine.3.7-x64

#
# Stage 2, Build runtime
#
FROM microsoft/dotnet:2.2-runtime-deps-alpine

# Default AspNetCore directory
WORKDIR /app

# add libuv & curl
RUN apk update \
 && apk add --no-cache libc6-compat \
 && apk add --no-cache libuv \
 && apk add --no-cache curl \
 && ln -s /usr/lib/libuv.so.1 /usr/lib/libuv.so

# Copy from build stage
COPY --from=builder /out/alpine .

EXPOSE 80
EXPOSE 11111

ENTRYPOINT ["./Squidex"]