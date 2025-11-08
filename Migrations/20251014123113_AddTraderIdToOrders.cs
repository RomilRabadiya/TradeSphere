using Microsoft.EntityFrameworkCore.Migrations;

namespace TradeSphere3.Migrations
{
    public partial class AddTraderIdToOrders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TraderId",
                table: "Orders",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_TraderId",
                table: "Orders",
                column: "TraderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Traders_TraderId",
                table: "Orders",
                column: "TraderId",
                principalTable: "Traders",
                principalColumn: "TraderId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Traders_TraderId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_TraderId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "TraderId",
                table: "Orders");
        }
    }
}
