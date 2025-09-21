using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ChefEye.Domain.Migrations
{
    /// <inheritdoc />
    public partial class UpdateModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "MenuItems",
                columns: new[] { "Id", "Description", "Name", "Price" },
                values: new object[,]
                {
                    { new Guid("1cedff26-8134-4c53-b22a-7f3e61abb594"), null, "Coffe", 3.5m },
                    { new Guid("98758b15-218d-4711-a0dd-f7a87e80197f"), null, "Pancake", 4.5m },
                    { new Guid("e71ffcd6-01c2-48e9-8a11-b9e8ce2e9aef"), null, "Pasta", 13m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("1cedff26-8134-4c53-b22a-7f3e61abb594"));

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("98758b15-218d-4711-a0dd-f7a87e80197f"));

            migrationBuilder.DeleteData(
                table: "MenuItems",
                keyColumn: "Id",
                keyValue: new Guid("e71ffcd6-01c2-48e9-8a11-b9e8ce2e9aef"));
        }
    }
}
