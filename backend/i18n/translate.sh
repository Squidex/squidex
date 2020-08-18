#!/bin/bash
PATH=${1:-/Squidex}   

cd translator/Squidex.Translator

/usr/local/share/dotnet/dotnet run translate check-backend $1
/usr/local/share/dotnet/dotnet run translate check-frontend $1

/usr/local/share/dotnet/dotnet run translate gen-frontend $1
/usr/local/share/dotnet/dotnet run translate gen-backend $1