FROM microsoft/aspnetcore:2.0.0-jessie

WORKDIR /app

# Copy from current directory
COPY . .

EXPOSE 80
ENTRYPOINT ["dotnet", "Squidex.dll"]