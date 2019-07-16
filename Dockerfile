#
# Stage 1, Prebuild
#
FROM nexus.cha.rbxd.ds:8000/dotnet:2.2-sdk-chromium-phantomjs-node as builder

ARG SQUIDEX__VERSION=1.0.0

RUN exit 1

WORKDIR /src

COPY src/Squidex/package*.json /tmp/

# Install Node packages 
RUN cd /tmp && npm install --loglevel=error

COPY src/Squidex src/Squidex

# Build Frontend
RUN cp -a /tmp/node_modules src/Squidex/ \
 && cd src/Squidex \
 && npm run test:coverage \
 && npm run build
 
# Test Backend
FROM nexus.cha.rbxd.ds:8000/dotnet:2.2-sdk-chromium-phantomjs-node as builder_backend

ARG SQUIDEX__VERSION=1.0.0

WORKDIR /src

# Install Screen
#RUN apt-get update \
 #&& apt-get install screen

# Install OpenJDK-8
# RUN apt-get update && \
#     apt-get install -y openjdk-8-jdk && \
#     apt-get install -y ant && \
#     apt-get clean;

# Fix certificate issues
# RUN apt-get update && \
#     apt-get install ca-certificates-java && \
#     apt-get clean && \
#     update-ca-certificates -f;

# Setup JAVA_HOME -- useful for docker commandline
# ENV JAVA_HOME /usr/lib/jvm/java-8-openjdk-amd64/
# RUN export JAVA_HOME

COPY src/**/*.csproj /tmp/
COPY tests/**/*.csproj /tmp/
RUN bash -c 'pushd /tmp; for p in *.csproj; do dotnet restore $p; true; done; popd'

COPY . .

RUN dotnet restore && dotnet test -s ../../.runsettings --filter Category!=Dependencies

COPY --from=builder /src/src/Squidex/wwwroot src/Squidex/wwwroot

# Run Functional Test Cases
# COPY cosmos-func-tests/package*.json /tmp/
# RUN cd /tmp && npm install --loglevel=error
# RUN cp -a /tmp/node_modules cosmos-func-tests/ \
#  && cd cosmos-func-tests \
#  && mkdir database \
#  && chmod +x setup-app.sh \
#  && npm run test

# Publish
RUN dotnet publish src/Squidex/Squidex.csproj /p:version=$SQUIDEX__VERSION --output /out/alpine --configuration Release 

#
# Stage 2, Build runtime
#
FROM nexus.cha.rbxd.ds:8000/dotnet/core/runtime:2.2.5-alpine3.9

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
EXPOSE 11111

ENTRYPOINT ["dotnet","./Squidex.dll"] 