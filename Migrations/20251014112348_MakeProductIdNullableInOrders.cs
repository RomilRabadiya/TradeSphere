using Microsoft.EntityFrameworkCore.Migrations;

namespace TradeSphere3.Migrations
{
    public partial class MakeProductIdNullableInOrders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_Traders_TraderId",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_TraderId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "TraderId",
                table: "Feedbacks");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "Orders",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers",
                column: "Email",
                unique: true,
                filter: "[Email] IS NOT NULL");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<int>(
                name: "ProductId",
                table: "Orders",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TraderId",
                table: "Feedbacks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_TraderId",
                table: "Feedbacks",
                column: "TraderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_Traders_TraderId",
                table: "Feedbacks",
                column: "TraderId",
                principalTable: "Traders",
                principalColumn: "TraderId");
        }
    }
}
