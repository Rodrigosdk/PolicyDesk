using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PolicyDesk.Infrastructure.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedActiveColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "active",
                table: "policies");

            migrationBuilder.DropColumn(
                name: "active",
                table: "claims");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // As colunas removidas valiam sempre true; o rollback deve recriá-las assim
            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "policies",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "active",
                table: "claims",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }
    }
}
