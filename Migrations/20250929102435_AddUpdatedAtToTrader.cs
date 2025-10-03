using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TradeSphere3.Migrations
{
    public partial class AddUpdatedAtToTrader : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "Traders",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "Traders");
        }
    }
}
