using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESS.Migrations
{
    /// <inheritdoc />
    public partial class AddLeaveFormR1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeftLeaveType");

            migrationBuilder.CreateTable(
                name: "DeftAddLeave",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LeaveType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NumberOfDays = table.Column<int>(type: "int", nullable: true),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeftAddLeave", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeftAddLeave");

            migrationBuilder.CreateTable(
                name: "DeftLeaveType",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmpCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LeaveType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumberOfDays = table.Column<int>(type: "int", nullable: true),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeftLeaveType", x => x.Id);
                });
        }
    }
}
