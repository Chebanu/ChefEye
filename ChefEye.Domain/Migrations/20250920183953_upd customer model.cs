using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChefEye.Domain.Migrations
{
    /// <inheritdoc />
    public partial class Updcustomermodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Phone",
                table: "Customers",
                newName: "PhoneNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PhoneNumber",
                table: "Customers",
                newName: "Phone");
        }
    }
}
