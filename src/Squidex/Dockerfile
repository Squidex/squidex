FROM microsoft/aspnetcore:1.1.2

# Default AspNet Core Workdir
WORKDIR /app

# Copy from Build Stage
COPY --from=builder /out/ .

ENTRYPOINT ["dotnet", "Squidex.dll"]