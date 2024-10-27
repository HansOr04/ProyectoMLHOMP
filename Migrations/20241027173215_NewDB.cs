using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProyectoMLHOMP.Migrations
{
    /// <inheritdoc />
    public partial class NewDB : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Review_Apartment_ApartmentId",
                table: "Review");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Apartment",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ImageUrl",
                table: "Apartment",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Apartment_ApartmentId",
                table: "Review",
                column: "ApartmentId",
                principalTable: "Apartment",
                principalColumn: "ApartmentId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Review_Apartment_ApartmentId",
                table: "Review");

            migrationBuilder.DropColumn(
                name: "ImageUrl",
                table: "Apartment");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Apartment",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddForeignKey(
                name: "FK_Review_Apartment_ApartmentId",
                table: "Review",
                column: "ApartmentId",
                principalTable: "Apartment",
                principalColumn: "ApartmentId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
