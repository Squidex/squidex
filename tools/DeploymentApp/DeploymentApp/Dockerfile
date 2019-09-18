# Build step
FROM nexus.cha.rbxd.ds:8000/dotnet/core/sdk:2.2 as builder

ARG SQUIDEX__VERSION=1.0.0

WORKDIR /src

COPY DeploymentApp/*.csproj /tmp/
RUN bash -c 'pushd /tmp; for p in *.csproj; do dotnet restore $p; true; done; popd'

COPY ./tools/DeploymentApp/DeploymentApp .

# Publish
RUN dotnet publish DeploymentApp/DeploymentApp.csproj /p:version=$SQUIDEX__VERSION --output /out/alpine --configuration Release

# Stage 2, Build runtime
FROM nexus.cha.rbxd.ds:8000/dotnet/core/runtime:2.2.5-alpine3.9

# Default AspNetCore directory
WORKDIR /app

# Copy from build stage
COPY --from=builder /out/alpine .
COPY cha-ca.cer /usr/local/share/ca-certificates/cha-ca.cer

RUN update-ca-certificates

ENTRYPOINT ["dotnet","./DeploymentApp.dll"]
