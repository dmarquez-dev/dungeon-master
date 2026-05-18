using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DungeonMaster.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlacedObjects : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlacedObjects",
                columns: table =>
                    new
                    {
                        Id = table.Column<Guid>(type: "TEXT", nullable: false),
                        AreaId = table.Column<Guid>(type: "TEXT", nullable: false),
                        Kind = table.Column<string>(type: "TEXT", nullable: false),
                        Position_Col = table.Column<int>(type: "INTEGER", nullable: false),
                        Position_Row = table.Column<int>(type: "INTEGER", nullable: false),
                        Anchor = table.Column<string>(type: "TEXT", nullable: false),
                        Rotation = table.Column<string>(type: "TEXT", nullable: false),
                        CreatedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                        LastModified = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                    },
                constraints: table =>
                {
                    table.PrimaryKey(
                        "PK_PlacedObjects",
                        placedObject =>
                            placedObject.Id);

                    table.ForeignKey(
                        name: "FK_PlacedObjects_Areas_AreaId",
                        column: placedObject =>
                            placedObject.AreaId,
                        principalTable: "Areas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlacedObjects_AreaId",
                table: "PlacedObjects",
                column: "AreaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlacedObjects");
        }
    }
}
