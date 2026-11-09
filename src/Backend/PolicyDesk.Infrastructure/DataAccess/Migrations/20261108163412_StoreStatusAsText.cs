using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolicyDesk.Infrastructure.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class StoreStatusAsText : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE policies
                    ALTER COLUMN status TYPE character varying(20)
                    USING CASE status
                        WHEN 1 THEN 'Active'
                        WHEN 2 THEN 'Cancelled'
                    END;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE claims
                    ALTER COLUMN status TYPE character varying(20)
                    USING CASE status
                        WHEN 1 THEN 'Pending'
                        WHEN 2 THEN 'Approved'
                        WHEN 3 THEN 'InAnalysis'
                        WHEN 4 THEN 'Denied'
                        WHEN 5 THEN 'Paid'
                    END;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE policies
                    ALTER COLUMN status TYPE integer
                    USING CASE status
                        WHEN 'Active' THEN 1
                        WHEN 'Cancelled' THEN 2
                    END;
                """);

            migrationBuilder.Sql("""
                ALTER TABLE claims
                    ALTER COLUMN status TYPE integer
                    USING CASE status
                        WHEN 'Pending' THEN 1
                        WHEN 'Approved' THEN 2
                        WHEN 'InAnalysis' THEN 3
                        WHEN 'Denied' THEN 4
                        WHEN 'Paid' THEN 5
                    END;
                """);
        }
    }
}
