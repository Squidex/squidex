$migrationName = $args[0]

dotnet ef migrations add $migrationName --context MysqlDbContext -o Providers/MySql/Migrations
dotnet ef migrations add $migrationName --context PostgresDbContext -o Providers/Postgres/Migrations
dotnet ef migrations add $migrationName --context SqlServerDbContext -o Providers/SqlServer/Migrations