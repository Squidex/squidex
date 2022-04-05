cd translator\Squidex.Translator

dotnet run translate check-backend ..\..\..\.. -l en
dotnet run translate check-frontend ..\..\..\.. -l en

dotnet run translate gen-frontend ..\..\..\.. -l en
dotnet run translate gen-backend ..\..\..\.. -l en

