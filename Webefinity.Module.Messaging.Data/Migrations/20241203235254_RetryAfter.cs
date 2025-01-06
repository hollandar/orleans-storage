using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Webefinity.Module.Messaging.Data.Migrations
{
    /// <inheritdoc />
    public partial class RetryAfter : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_SenderId_Status",
                table: "Messages");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "RetryAfter",
                table: "Messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "Messages",
                type: "integer",
                nullable: false,
                defaultValue: 10);

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Created",
                table: "Messages",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Status_RetryAfter_RetryCount",
                table: "Messages",
                columns: new[] { "Status", "RetryAfter", "RetryCount" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Messages_Created",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_SenderId",
                table: "Messages");

            migrationBuilder.DropIndex(
                name: "IX_Messages_Status_RetryAfter_RetryCount",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "RetryAfter",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "Messages");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId_Status",
                table: "Messages",
                columns: new[] { "SenderId", "Status" });
        }
    }
}
