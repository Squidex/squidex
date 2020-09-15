cd translator\Squidex.Translator

dotnet run translate check-backend ..\..\..\..
dotnet run translate check-frontend ..\..\..\..

dotnet run translate gen-frontend ..\..\..\..
dotnet run translate gen-backend ..\..\..\..

