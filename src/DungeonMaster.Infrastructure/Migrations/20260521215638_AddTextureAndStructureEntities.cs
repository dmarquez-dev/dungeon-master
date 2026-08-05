using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DungeonMaster.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTextureAndStructureEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StructureElements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AreaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    From_Col = table.Column<int>(type: "INTEGER", nullable: false),
                    From_Row = table.Column<int>(type: "INTEGER", nullable: false),
                    To_Col = table.Column<int>(type: "INTEGER", nullable: false),
                    To_Row = table.Column<int>(type: "INTEGER", nullable: false),
                    Kind = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StructureElements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_StructureElements_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TileTextures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AreaId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Position_Col = table.Column<int>(type: "INTEGER", nullable: false),
                    Position_Row = table.Column<int>(type: "INTEGER", nullable: false),
                    Kind = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    LastModified = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TileTextures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TileTextures_Areas_AreaId",
                        column: x => x.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StructureElements_AreaId",
                table: "StructureElements",
                column: "AreaId");

            migrationBuilder.CreateIndex(
                name: "IX_TileTextures_AreaId",
                table: "TileTextures",
                column: "AreaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StructureElements");

            migrationBuilder.DropTable(
                name: "TileTextures");
        }
    }
}
