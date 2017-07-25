FROM microsoft/aspnetcore:1.1.2

WORKDIR /app

# Copy from current directory
COPY . .

EXPOSE 80
ENTRYPOINT ["dotnet", "Squidex.dll"]