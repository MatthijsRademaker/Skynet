﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Arnold.SkyNet.Migrations
{
    /// <inheritdoc />
    public partial class Update2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "KnowledgeTestPassed",
                schema: "core",
                table: "Customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "KnowledgeTestPassed",
                schema: "core",
                table: "Customers");
        }
    }
}
