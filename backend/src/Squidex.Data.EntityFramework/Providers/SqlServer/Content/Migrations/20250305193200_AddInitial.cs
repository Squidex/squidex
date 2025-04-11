using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Squidex.Providers.SqlServer.Content.Migrations
{
    /// <inheritdoc />
    public partial class AddInitial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: $"{TableName.Prefix}ContentsAll",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    AppId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    SchemaId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NewStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScheduleJob = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IndexedAppId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IndexedSchemaId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ScheduledAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NewData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TranslationStatus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey($"PK_{TableName.Prefix}ContentsAll", x => x.DocumentId);
                });

            migrationBuilder.CreateTable(
                name: $"{TableName.Prefix}ContentsPublished",
                columns: table => new
                {
                    DocumentId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Id = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    AppId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    SchemaId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NewStatus = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScheduleJob = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IndexedAppId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IndexedSchemaId = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ScheduledAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    NewData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TranslationStatus = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey($"PK_{TableName.Prefix}ContentsPublished", x => x.DocumentId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: $"{TableName.Prefix}ContentsAll");

            migrationBuilder.DropTable(
                name: $"{TableName.Prefix}ContentsPublished");
        }
    }
}
