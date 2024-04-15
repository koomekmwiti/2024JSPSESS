using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESS.Migrations
{
    /// <inheritdoc />
    public partial class ApplicationNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationNo",
                table: "DeftAddLeave",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsUsed",
                table: "DeftAddLeave",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ApplicationNo",
                table: "DeftAddLeave");

            migrationBuilder.DropColumn(
                name: "IsUsed",
                table: "DeftAddLeave");
        }
    }
}
