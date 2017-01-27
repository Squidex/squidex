#!bin/bash
set -e
dotnet restore
dotnet test tests/Squidex.Core.Tests/project.json
dotnet test tests/Squidex.Infrastructure.Tests/project.json
dotnet test tests/Squidex.Read.Tests/project.json
dotnet test tests/Squidex.Write.Tests/project.json
rm -rf $(pwd)/publish/web
dotnet publish src/Squidex/project.json -c release -o $(pwd)/publish/web