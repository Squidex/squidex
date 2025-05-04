cd translator\Squidex.Translator

dotnet run translate --no-restore check-backend ..\..\..\.. -l en
dotnet run translate --no-restore check-frontend ..\..\..\.. -l en

dotnet run translate --no-restore gen-frontend ..\..\..\.. -l en
dotnet run translate --no-restore gen-backend ..\..\..\.. -l en

cd ..\..