#!/bin/bash
cd translator/Squidex.Translator

/usr/local/share/dotnet/dotnet run --no-restore translate check-backend ../../../..
/usr/local/share/dotnet/dotnet run --no-restore translate check-frontend ../../../..

/usr/local/share/dotnet/dotnet run --no-restore translate gen-frontend ../../../..
/usr/local/share/dotnet/dotnet run --no-restore translate gen-backend ../../../..

cd ..\..