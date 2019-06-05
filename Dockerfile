#
# Stage 1, Prebuild
#
FROM nexus.cha.rbxd.ds:8000/dotnet:2.2-sdk-chromium-phantomjs-node as builder

WORKDIR /src

COPY src/Squidex/package*.json /tmp/

# Install Node packages 
RUN cd /tmp && npm install --loglevel=error

COPY src/Squidex src/Squidex

# Build Frontend
RUN cp -a /tmp/node_modules src/Squidex/ \
 && cd src/Squidex \
# && npm run test:coverage \
 && npm run build
 
# Test Backend
FROM nexus.cha.rbxd.ds:8000/dotnet:2.2-sdk-chromium-phantomjs-node as builder_backend

WORKDIR /src

COPY src/**/*.csproj /tmp/
COPY tests/**/*.csproj /tmp/
RUN bash -c 'pushd /tmp; for p in *.csproj; do dotnet restore $p; true; done; popd'

COPY . .

RUN dotnet test --no-restore -s ../../.runsettings --filter Category!=Dependencies

COPY --from=builder /src/src/Squidex/wwwroot src/Squidex/wwwroot

# Publish
RUN dotnet publish src/Squidex/Squidex.csproj --output /out/alpine --configuration Release -r alpine.3.7-x64

#
# Stage 2, Build runtime
#
FROM nexus.cha.rbxd.ds:8000/dotnet:2.2-runtime-deps-alpine

# Default AspNetCore directory
WORKDIR /app

# add libuv & curl
RUN apk update \
 && apk add --no-cache libc6-compat \
 && apk add --no-cache libuv \
 && apk add --no-cache curl \
 && ln -s /usr/lib/libuv.so.1 /usr/lib/libuv.so

# Copy from build stage
COPY --from=builder_backend /out/alpine .
COPY src/Squidex/cha-ca.cer /usr/local/share/ca-certificates/cha-ca.cer

RUN update-ca-certificates

EXPOSE 80
EXPOSE 5000
# EXPOSE 33333
# EXPOSE 40000

ENTRYPOINT ["./Squidex"]   