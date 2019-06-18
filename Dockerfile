#
# Stage 1a, Build frontend
#
FROM squidex/dotnet:2.2-sdk-chromium-phantomjs-node as frontend-builder

WORKDIR /src

COPY /**/**/*.csproj /tmp/

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

WORKDIR /

COPY /**/*.csproj /tmp/

# Install Nuget packages
RUN bash -c 'pushd /tmp; for p in *.csproj; do dotnet restore $p; true; done; popd'

COPY . .

# Test Backend
RUN dotnet restore \
 && dotnet test -s ../../.testrunsettings --filter Category!=Dependencies

COPY --from=frontend-builder /src/src/Squidex/wwwroot src/Squidex/wwwroot

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