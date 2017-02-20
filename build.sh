#!bin/bash
set -e
cd src/Squidex
npm install
npm rebuild node-sass
npm rebuild phantomjs-prebuilt
npm run test:coverage
npm run build:copy
npm run build
cd ./../..
dotnet restore
dotnet test tests/Squidex.Core.Tests/Squidex.Core.Tests.csproj
dotnet test tests/Squidex.Infrastructure.Tests/Squidex.Infrastructure.Tests.csproj
dotnet test tests/Squidex.Read.Tests/Squidex.Read.Tests.csproj
dotnet test tests/Squidex.Write.Tests/Squidex.Write.Tests.csproj
rm -rf $(pwd)/publish/web
dotnet publish src/Squidex/Squidex.csproj -c release -o $(pwd)/publish/web