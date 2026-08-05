using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DungeonMaster.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEdgeCenterAnchor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "To_Row",
                table: "StructureElements",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "To_Col",
                table: "StructureElements",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "From_Row",
                table: "StructureElements",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "From_Col",
                table: "StructureElements",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<string>(
                name: "EdgeAnchor_Side",
                table: "StructureElements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EdgeAnchor_TileCol",
                table: "StructureElements",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EdgeAnchor_TileRow",
                table: "StructureElements",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EdgeAnchor_Side",
                table: "StructureElements");

            migrationBuilder.DropColumn(
                name: "EdgeAnchor_TileCol",
                table: "StructureElements");

            migrationBuilder.DropColumn(
                name: "EdgeAnchor_TileRow",
                table: "StructureElements");

            migrationBuilder.AlterColumn<int>(
                name: "To_Row",
                table: "StructureElements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "To_Col",
                table: "StructureElements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "From_Row",
                table: "StructureElements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "From_Col",
                table: "StructureElements",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);
        }
    }
}
