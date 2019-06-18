#
# Stage 1a, Build frontend
#
FROM squidex/dotnet:2.2-sdk-chromium-phantomjs-node as frontend-builder

WORKDIR /frontend

COPY src/Squidex/package*.json /tmp/

# Install Node packages 
RUN cd /tmp && npm install --loglevel=error

COPY src/Squidex src/Squidex

# Build Frontend
RUN cp -a /tmp/node_modules src/Squidex/ \
 && cd src/Squidex \
 && npm run test:coverage \
 && npm run build


#
# Stage 1b, Build frontend
#
FROM squidex/dotnet:2.2-sdk-chromium-phantomjs-node as backend-builder

WORKDIR /backend

COPY /**/**/*.csproj /tmp/
# Also copy nuget.config for package sources.
COPY NuGet.Config /tmp/

# Install Nuget packages
RUN bash -c 'pushd /tmp; for p in *.csproj; do dotnet restore $p --verbosity quiet; true; done; popd'

COPY . .

# Test Backend
RUN dotnet test tests/Squidex.Infrastructure.Tests/Squidex.Infrastructure.Tests.csproj --filter Category!=Dependencies \ 
 && dotnet test tests/Squidex.Domain.Apps.Core.Tests/Squidex.Domain.Apps.Core.Tests.csproj \ 
 && dotnet test tests/Squidex.Domain.Apps.Entities.Tests/Squidex.Domain.Apps.Entities.Tests.csproj \
 && dotnet test tests/Squidex.Domain.Users.Tests/Squidex.Domain.Users.Tests.csproj \
 && dotnet test tests/Squidex.Web.Tests/Squidex.Web.Tests.csproj

COPY --from=frontend-builder /frontend/src/Squidex/wwwroot src/Squidex/wwwroot

RUN cd src/Squidex/wwwroot && ls

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
COPY --from=backend-builder /out/alpine .

EXPOSE 80
EXPOSE 11111

ENTRYPOINT ["./Squidex"]