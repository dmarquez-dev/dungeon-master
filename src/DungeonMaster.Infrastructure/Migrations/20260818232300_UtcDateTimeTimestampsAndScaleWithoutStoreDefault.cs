using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DungeonMaster.Infrastructure.Migrations
{
    /// <summary>
    ///     Covers the two model changes this session makes.
    ///     <para>
    ///         MASTER-029. <c>Entity.CreatedAt</c> and <c>Entity.LastModified</c> changed from
    ///         <see cref="System.DateTimeOffset"/> to <see cref="System.DateTime"/> (UTC). Both CLR types land in
    ///         SQLite as TEXT, so the scaffolder emits no schema operation for them -- the substance of the change
    ///         is the stored value, and it is rewritten by hand below.
    ///     </para>
    ///     <para>
    ///         Observed formats, measured against the EF Core 10.0.11 SQLite provider rather than assumed. A
    ///         <c>DateTimeOffset</c> at UTC is stored as <c>2026-08-18 14:05:06.7891234+00:00</c> (33 characters);
    ///         the same instant as a <c>DateTime</c> is stored as <c>2026-08-18 14:05:06.7891234</c> (27
    ///         characters). The two forms are identical apart from the six-character offset suffix, so
    ///         <c>Up</c> strips those six characters and <c>Down</c> appends them.
    ///     </para>
    ///     <para>
    ///         <c>Down</c> reconstructing <c>+00:00</c> is lossless <em>for this data set specifically</em>: every
    ///         value in these eight columns originated from <c>DateTimeOffset.UtcNow</c> in
    ///         <c>Entity</c>'s property initialisers and <c>Touch()</c>, so the offset was always exactly zero and
    ///         nothing but a constant is being recovered. That reasoning does not generalise -- a bare
    ///         <c>DateTime</c> carries no offset, so a column that had ever held a non-UTC
    ///         <c>DateTimeOffset</c> could not be reversed this way.
    ///     </para>
    ///     <para>
    ///         Both directions are guarded by a <c>LIKE</c> test on the suffix and are therefore idempotent, and
    ///         a row already in the target shape is left untouched.
    ///     </para>
    ///     <para>
    ///         MASTER-075 / INFRA-008. The <c>Scale</c> store default is dropped. It made the property
    ///         store-generated on add, so the CLR default <c>0f</c> could never have been sent; the backfill it
    ///         existed for has been applied and the value now travels as ordinary data. See
    ///         <c>docs/persistence-contract.md</c>.
    ///     </para>
    /// </summary>
    public partial class UtcDateTimeTimestampsAndScaleWithoutStoreDefault : Migration
    {
        /// <summary>
        ///     The four tables whose <c>CreatedAt</c> and <c>LastModified</c> columns are rewritten -- eight
        ///     columns in total.
        /// </summary>
        private static readonly string[] TimestampTables =
        [
            "Dungeons",
            "Floors",
            "Areas",
            "PlacedObjects",
        ];

        /// <summary>
        ///     The offset suffix a UTC <c>DateTimeOffset</c> is stored with, and its length in characters.
        /// </summary>
        private const string UtcOffsetSuffix = "+00:00";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<float>(
                name: "Scale",
                table: "PlacedObjects",
                type: "REAL",
                nullable: false,
                oldClrType: typeof(float),
                oldType: "REAL",
                oldDefaultValue: 1f);

            foreach (var table in TimestampTables)
            {
                migrationBuilder.Sql(
                    $"""
                    UPDATE "{table}"
                    SET "CreatedAt" = CASE
                            WHEN "CreatedAt" LIKE '%{UtcOffsetSuffix}'
                            THEN substr("CreatedAt", 1, length("CreatedAt") - {UtcOffsetSuffix.Length})
                            ELSE "CreatedAt"
                        END,
                        "LastModified" = CASE
                            WHEN "LastModified" LIKE '%{UtcOffsetSuffix}'
                            THEN substr("LastModified", 1, length("LastModified") - {UtcOffsetSuffix.Length})
                            ELSE "LastModified"
                        END;
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var table in TimestampTables)
            {
                migrationBuilder.Sql(
                    $"""
                    UPDATE "{table}"
                    SET "CreatedAt" = CASE
                            WHEN "CreatedAt" LIKE '%{UtcOffsetSuffix}'
                            THEN "CreatedAt"
                            ELSE "CreatedAt" || '{UtcOffsetSuffix}'
                        END,
                        "LastModified" = CASE
                            WHEN "LastModified" LIKE '%{UtcOffsetSuffix}'
                            THEN "LastModified"
                            ELSE "LastModified" || '{UtcOffsetSuffix}'
                        END;
                    """);
            }

            migrationBuilder.AlterColumn<float>(
                name: "Scale",
                table: "PlacedObjects",
                type: "REAL",
                nullable: false,
                defaultValue: 1f,
                oldClrType: typeof(float),
                oldType: "REAL");
        }
    }
}
