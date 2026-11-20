using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DungeonMaster.Infrastructure.Migrations
{
    /// <summary>
    ///     Rewrites the stored <c>ObjectRotation</c> values after its members were renamed.
    ///     <para>
    ///         MASTER-130 / CORE-024. <c>ObjectRotation.CW90</c>, <c>CW180</c> and <c>CW270</c> were renamed to
    ///         <c>Clockwise90</c>, <c>Clockwise180</c> and <c>Clockwise270</c>. The enum is persisted by member name
    ///         (<c>PlacedObjectConfiguration</c> maps it with <c>HasConversion&lt;string&gt;()</c>), so the column is
    ///         TEXT either way and the scaffolder emits no schema operation -- the substance of the change is the
    ///         stored value, and it is rewritten by hand below. This migration changes data only.
    ///     </para>
    ///     <para>
    ///         <c>Up</c> rewrites <c>PlacedObjects.Rotation</c> from <c>'CW90'</c> to <c>'Clockwise90'</c>, from
    ///         <c>'CW180'</c> to <c>'Clockwise180'</c> and from <c>'CW270'</c> to <c>'Clockwise270'</c>. <c>Down</c>
    ///         is the exact inverse. No other table or column is read or written.
    ///     </para>
    ///     <para>
    ///         <c>'None'</c> and any value outside the direction's three source strings are left untouched, so both
    ///         directions are idempotent and a row already in the target form is left as it is.
    ///     </para>
    ///     <para>
    ///         <c>Down</c> writes strings the renamed enum cannot materialise. It restores the previous data for the
    ///         previous code, which is what reverting a data migration behind a renamed enum means.
    ///     </para>
    /// </summary>
    public partial class RenameObjectRotationClockwiseMembers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "PlacedObjects"
                SET "Rotation" = CASE "Rotation"
                        WHEN 'CW90' THEN 'Clockwise90'
                        WHEN 'CW180' THEN 'Clockwise180'
                        WHEN 'CW270' THEN 'Clockwise270'
                        ELSE "Rotation"
                    END
                WHERE "Rotation" IN ('CW90', 'CW180', 'CW270');
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                UPDATE "PlacedObjects"
                SET "Rotation" = CASE "Rotation"
                        WHEN 'Clockwise90' THEN 'CW90'
                        WHEN 'Clockwise180' THEN 'CW180'
                        WHEN 'Clockwise270' THEN 'CW270'
                        ELSE "Rotation"
                    END
                WHERE "Rotation" IN ('Clockwise90', 'Clockwise180', 'Clockwise270');
                """);
        }
    }
}
