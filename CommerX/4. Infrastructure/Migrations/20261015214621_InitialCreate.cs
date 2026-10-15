using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommerX.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName_Value = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName_Value = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Document_Value = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false),
                    Email_Value = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: false),
                    Phone_Value = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address_Value = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    BirthDate_Value = table.Column<DateOnly>(type: "date", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModifiedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Document_Value",
                table: "Customers",
                column: "Document_Value",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Email_Value",
                table: "Customers",
                column: "Email_Value",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
