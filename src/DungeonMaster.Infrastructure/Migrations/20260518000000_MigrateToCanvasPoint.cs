using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DungeonMaster.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MigrateToCanvasPoint : Migration
    {
        private const float TileSize = 40f;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: add new float columns (nullable so existing rows can be populated first)
            migrationBuilder.AddColumn<float>(
                name: "Position_X",
                table: "PlacedObjects",
                type: "REAL",
                nullable: true);

            migrationBuilder.AddColumn<float>(
                name: "Position_Y",
                table: "PlacedObjects",
                type: "REAL",
                nullable: true);

            // Step 2: populate using area-relative pixel coordinates.
            // Position_X = (old_Col - area.Origin_Col) * TileSize + TileSize/2
            // The + TileSize/2 converts "top-left tile corner" to "center of that tile",
            // matching what the old Center-anchor draw code produced.
            migrationBuilder.Sql($@"
                UPDATE PlacedObjects
                SET Position_X = (Position_Col - (SELECT Origin_Col FROM Areas WHERE Areas.Id = PlacedObjects.AreaId)) * {TileSize} + {TileSize / 2f},
                    Position_Y = (Position_Row - (SELECT Origin_Row FROM Areas WHERE Areas.Id = PlacedObjects.AreaId)) * {TileSize} + {TileSize / 2f}
            ");

            // Step 3: make the new columns NOT NULL now that all rows are populated.
            // SQLite doesn't support ALTER COLUMN directly; EF Core handles it via table rebuild.
            migrationBuilder.AlterColumn<float>(
                name: "Position_X",
                table: "PlacedObjects",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "REAL",
                oldNullable: true);

            migrationBuilder.AlterColumn<float>(
                name: "Position_Y",
                table: "PlacedObjects",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "REAL",
                oldNullable: true);

            // Step 4: drop obsolete columns (EF Core rebuilds the table for SQLite).
            migrationBuilder.DropColumn(
                name: "Position_Col",
                table: "PlacedObjects");

            migrationBuilder.DropColumn(
                name: "Position_Row",
                table: "PlacedObjects");

            migrationBuilder.DropColumn(
                name: "Anchor",
                table: "PlacedObjects");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Position_Col",
                table: "PlacedObjects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Position_Row",
                table: "PlacedObjects",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Anchor",
                table: "PlacedObjects",
                type: "TEXT",
                nullable: false,
                defaultValue: "Center");

            // Reverse: convert area-relative float center back to absolute integer tile position.
            migrationBuilder.Sql($@"
                UPDATE PlacedObjects
                SET Position_Col = CAST((Position_X - {TileSize / 2f}) / {TileSize} AS INTEGER) + (SELECT Origin_Col FROM Areas WHERE Areas.Id = PlacedObjects.AreaId),
                    Position_Row = CAST((Position_Y - {TileSize / 2f}) / {TileSize} AS INTEGER) + (SELECT Origin_Row FROM Areas WHERE Areas.Id = PlacedObjects.AreaId)
            ");

            migrationBuilder.DropColumn(
                name: "Position_X",
                table: "PlacedObjects");

            migrationBuilder.DropColumn(
                name: "Position_Y",
                table: "PlacedObjects");
        }
    }
}
