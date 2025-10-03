using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TradeSphere3.Migrations
{
    public partial class AddTradersTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsTrader",
                table: "AspNetUsers");

            migrationBuilder.CreateTable(
                name: "Traders",
                columns: table => new
                {
                    TraderId = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(maxLength: 200, nullable: false),
                    CIN = table.Column<string>(maxLength: 50, nullable: false),
                    GSTNo = table.Column<string>(maxLength: 50, nullable: false),
                    ISO = table.Column<string>(maxLength: 50, nullable: false),
                    Country = table.Column<string>(maxLength: 100, nullable: false),
                    State = table.Column<string>(maxLength: 100, nullable: false),
                    City = table.Column<string>(maxLength: 100, nullable: false),
                    Address = table.Column<string>(maxLength: 300, nullable: false),
                    Email = table.Column<string>(nullable: false),
                    Phone = table.Column<string>(nullable: false),
                    TradeRole = table.Column<string>(maxLength: 20, nullable: false),
                    RegistrationDate = table.Column<DateTime>(nullable: false),
                    Turnover = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    TrustScore = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Traders", x => x.TraderId);
                    table.ForeignKey(
                        name: "FK_Traders_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Traders_UserId",
                table: "Traders",
                column: "UserId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Traders");

            migrationBuilder.AddColumn<bool>(
                name: "IsTrader",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
