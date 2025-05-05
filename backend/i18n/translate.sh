#!/bin/bash
cd translator/Squidex.Translator

/usr/local/share/dotnet/dotnet run --no-restore run translate check-backend ../../../..
/usr/local/share/dotnet/dotnet run --no-restore run translate check-frontend ../../../..

/usr/local/share/dotnet/dotnet run --no-restore run translate gen-frontend ../../../..
/usr/local/share/dotnet/dotnet run --no-restore run translate gen-backend ../../../..

cd ..\..