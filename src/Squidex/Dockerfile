FROM microsoft/dotnet:2.1.0-aspnetcore-runtime

WORKDIR /app

# Copy from current directory
COPY . .

EXPOSE 80
EXPOSE 33333
EXPOSE 40000

ENTRYPOINT ["dotnet", "Squidex.dll"]