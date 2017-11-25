#
# Stage 1, Prebuild
#
FROM microsoft/aspnetcore-build:2.0.3-jessie as builder

COPY . .

WORKDIR /

# Test Backend
RUN dotnet test tests/Squidex.Infrastructure.Tests/Squidex.Infrastructure.Tests.csproj \ 
 && dotnet test tests/Squidex.Domain.Apps.Core.Tests/Squidex.Domain.Apps.Core.Tests.csproj \ 
 && dotnet test tests/Squidex.Domain.Apps.Read.Tests/Squidex.Domain.Apps.Read.Tests.csproj \
 && dotnet test tests/Squidex.Domain.Apps.Write.Tests/Squidex.Domain.Apps.Write.Tests.csproj \
 && dotnet test tests/Squidex.Domain.Users.Tests/Squidex.Domain.Users.Tests.csproj

# Publish
RUN dotnet publish src/Squidex/Squidex.csproj --output /out/ --configuration Release

#
# Stage 2, Build runtime
#
FROM microsoft/aspnetcore:2.0.3-jessie

# Default AspNetCore directory
WORKDIR /app

# Copy from nuild stage
COPY --from=builder /out/ .

EXPOSE 80
EXPOSE 33333
EXPOSE 40000

ENTRYPOINT ["dotnet", "Squidex.dll"]