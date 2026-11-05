using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolicyDesk.Infrastructure.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class NormalizePhonesToE164 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE policyholders
                SET phone = CASE
                    WHEN btrim(phone) LIKE '+%' THEN '+' || regexp_replace(phone, '[^0-9]', '', 'g')
                    ELSE '+55' || regexp_replace(phone, '[^0-9]', '', 'g')
                END
                WHERE phone !~ '^\+[1-9][0-9]{6,14}$'
                  AND (
                      (btrim(phone) LIKE '+%' AND regexp_replace(phone, '[^0-9]', '', 'g') ~ '^[1-9][0-9]{6,14}$')
                      OR (btrim(phone) NOT LIKE '+%' AND regexp_replace(phone, '[^0-9]', '', 'g') ~ '^[0-9]{10,11}$')
                  );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
