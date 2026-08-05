using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DungeonMaster.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class StructureElementJsonColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "EdgeAnchor_Side",   table: "StructureElements");
            migrationBuilder.DropColumn(name: "EdgeAnchor_TileCol", table: "StructureElements");
            migrationBuilder.DropColumn(name: "EdgeAnchor_TileRow", table: "StructureElements");
            migrationBuilder.DropColumn(name: "From_Col",          table: "StructureElements");
            migrationBuilder.DropColumn(name: "From_Row",          table: "StructureElements");
            migrationBuilder.DropColumn(name: "To_Col",            table: "StructureElements");
            migrationBuilder.DropColumn(name: "To_Row",            table: "StructureElements");

            migrationBuilder.AddColumn<string>(
                name: "EdgeAnchor",
                table: "StructureElements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "From",
                table: "StructureElements",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "To",
                table: "StructureElements",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "EdgeAnchor", table: "StructureElements");
            migrationBuilder.DropColumn(name: "From",       table: "StructureElements");
            migrationBuilder.DropColumn(name: "To",         table: "StructureElements");

            migrationBuilder.AddColumn<int>(
                name: "From_Col",
                table: "StructureElements",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "From_Row",
                table: "StructureElements",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "To_Col",
                table: "StructureElements",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "To_Row",
                table: "StructureElements",
                type: "INTEGER",
                nullable: true);

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
    }
}
