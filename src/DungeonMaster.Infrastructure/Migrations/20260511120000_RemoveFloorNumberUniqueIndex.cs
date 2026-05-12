using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DungeonMaster.Infrastructure.Migrations
{
    public partial class RemoveFloorNumberUniqueIndex : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Floors_DungeonId_Number",
                table: "Floors");

            migrationBuilder.CreateIndex(
                name: "IX_Floors_DungeonId_Number",
                table: "Floors",
                columns: new[] { "DungeonId", "Number" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Floors_DungeonId_Number",
                table: "Floors");

            migrationBuilder.CreateIndex(
                name: "IX_Floors_DungeonId_Number",
                table: "Floors",
                columns: new[] { "DungeonId", "Number" },
                unique: true);
        }
    }
}
