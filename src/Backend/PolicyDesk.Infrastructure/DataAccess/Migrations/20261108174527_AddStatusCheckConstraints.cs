using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolicyDesk.Infrastructure.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddStatusCheckConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_policies_status",
                table: "policies",
                sql: "status IN ('Active', 'Cancelled')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_claims_status",
                table: "claims",
                sql: "status IN ('Pending', 'InAnalysis', 'Approved', 'Denied', 'Paid')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_policies_status",
                table: "policies");

            migrationBuilder.DropCheckConstraint(
                name: "CK_claims_status",
                table: "claims");
        }
    }
}
