using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SseDemo.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionIdForMergeJob : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SessionId",
                table: "MergeJobs",
                type: "TEXT",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // backfill SessionIds
            migrationBuilder.Sql("UPDATE MergeJobs SET SessionId = RANDOMBLOB(16) WHERE SessionId = '00000000-0000-0000-0000-000000000000'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionId",
                table: "MergeJobs");
        }
    }
}
