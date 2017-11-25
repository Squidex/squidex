FROM microsoft/aspnetcore:2.0.3-jessie

WORKDIR /app

# Copy from current directory
COPY . .

EXPOSE 80
EXPOSE 33333
EXPOSE 40000

ENTRYPOINT ["dotnet", "Squidex.dll"]