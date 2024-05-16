using System.Text.Json;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Orleans.NpgsqlTenancy.Migrations
{
    /// <inheritdoc />
    public partial class StateStore : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "States",
                columns: table => new
                {
                    StateName = table.Column<string>(type: "text", nullable: false),
                    Id = table.Column<string>(type: "text", nullable: false),
                    State = table.Column<JsonDocument>(type: "jsonb", nullable: false),
                    ETag = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_States", x => new { x.Id, x.StateName });
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "States");
        }
    }
}
