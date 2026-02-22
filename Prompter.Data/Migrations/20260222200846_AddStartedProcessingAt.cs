using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Prompter.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStartedProcessingAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "StartedProcessingAt",
                table: "Prompts",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StartedProcessingAt",
                table: "Prompts");
        }
    }
}
