cd translator\Squidex.Translator

dotnet run --no-restore translate check-backend ..\..\..\..
dotnet run --no-restore translate check-frontend ..\..\..\..

dotnet run --no-restore translate gen-frontend ..\..\..\..
dotnet run --no-restore translate gen-backend ..\..\..\..

cd ..\..

