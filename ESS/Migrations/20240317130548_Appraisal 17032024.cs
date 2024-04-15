using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESS.Migrations
{
    /// <inheritdoc />
    public partial class Appraisal17032024 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeftAddAppraisal",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmpCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Appraisal_No = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Appraisal_Period = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AppraisalType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsUsed = table.Column<bool>(type: "bit", nullable: false),
                    Created = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeftAddAppraisal", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DeftAddAppraisal");
        }
    }
}
