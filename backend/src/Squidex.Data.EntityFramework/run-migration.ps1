# Stop the script when a cmdlet or a native command fails
$ErrorActionPreference = 'Stop'

$migrationName = $args[0]

dotnet ef migrations add $migrationName --context MysqlAppDbContext -o Providers/MySql/App/Migrations
if (!$?) { 
    Write-Error "Creating migration failed for MySql"
    exit 1 
}

dotnet ef migrations add $migrationName --context PostgresAppDbContext -o Providers/Postgres/App/Migrations
if (!$?) { 
    Write-Error "Creating migration failed for Postgres"
    exit 1 
}

dotnet ef migrations add $migrationName --context SqlServerAppDbContext -o Providers/SqlServer/App/Migrations
if (!$?) { 
    Write-Error "Creating migration failed for SqlServer"
    exit 1 
}