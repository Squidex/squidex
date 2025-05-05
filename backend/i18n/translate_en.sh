cd translator\Squidex.Translator

/usr/local/share/dotnet/dotnet run --no-restore run translate check-backend ..\..\..\.. -l en
/usr/local/share/dotnet/dotnet run --no-restore run translate check-frontend ..\..\..\.. -l en

/usr/local/share/dotnet/dotnet run --no-restore run translate gen-frontend ..\..\..\.. -l en
/usr/local/share/dotnet/dotnet run --no-restore run translate gen-backend ..\..\..\.. -l en

cd ..\..
