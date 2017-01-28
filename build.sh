#!bin/bash
set -e
cd src/Squidex
npm install
npm rebuild node-sass
npm rebuild phantomjs-prebuilt
npm run test:coverage
npm run build:copy
npm run build
cd ../..
dotnet restore
dotnet test tests/Squidex.Core.Tests/project.json
dotnet test tests/Squidex.Infrastructure.Tests/project.json
dotnet test tests/Squidex.Read.Tests/project.json
dotnet test tests/Squidex.Write.Tests/project.json
rm -rf $(pwd)/publish/web
dotnet publish src/Squidex/project.json -c release -o $(pwd)/publish/web