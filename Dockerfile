#
# Stage 1, Prebuild
#
FROM nexus.cha.rbxd.ds:8000/dotnet:2.2-sdk-chromium-phantomjs-node as builder

# RUN npm config set https-proxy http://outboundproxycha.cha.rbxd.ds:3128/
# RUN npm config set http-proxy http://outboundproxycha.cha.rbxd.ds:3128/

WORKDIR /src

COPY src/Squidex/package*.json /tmp/

# Install Node packages 
RUN cd /tmp && npm install --loglevel=error

COPY . .

# Build Frontend
RUN cp -a /tmp/node_modules src/Squidex/ \
 && cd src/Squidex \
# && npm run test:coverage \
 && npm run build
 
# Test Backend
RUN dotnet restore \
 && dotnet test --filter Category!=Dependencies tests/Squidex.Infrastructure.Tests/Squidex.Infrastructure.Tests.csproj \ 
 && dotnet test tests/Squidex.Domain.Apps.Core.Tests/Squidex.Domain.Apps.Core.Tests.csproj \ 
 && dotnet test tests/Squidex.Domain.Apps.Entities.Tests/Squidex.Domain.Apps.Entities.Tests.csproj \
 && dotnet test tests/Squidex.Domain.Users.Tests/Squidex.Domain.Users.Tests.csproj \
 && dotnet test tests/Squidex.Web.Tests/Squidex.Web.Tests.csproj

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
COPY --from=builder /out/alpine .
COPY src/Squidex/cha-ca.cer /usr/local/share/ca-certificates/cha-ca.cer

RUN update-ca-certificates

EXPOSE 80
EXPOSE 5000
# EXPOSE 33333
# EXPOSE 40000

ENTRYPOINT ["./Squidex"]