# Stop the script when a cmdlet or a native command fails
$ErrorActionPreference = 'Stop'

$migrationName = $args[0]

dotnet ef migrations add $migrationName --context MysqlDbContext -o Providers/MySql/Migrations
if (!$?) { 
    Write-Error "Creating migration failed for MySql"
    exit 1 
}

dotnet ef migrations add $migrationName --context PostgresDbContext -o Providers/Postgres/Migrations
if (!$?) { 
    Write-Error "Creating migration failed for Postgres"
    exit 1 
}

dotnet ef migrations add $migrationName --context SqlServerDbContext -o Providers/SqlServer/Migrations
if (!$?) { 
    Write-Error "Creating migration failed for SqlServer"
    exit 1 
}