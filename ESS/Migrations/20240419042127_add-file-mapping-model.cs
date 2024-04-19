using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ESS.Migrations
{
    /// <inheritdoc />
    public partial class addfilemappingmodel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FileMappings",
                columns: table => new
                {
                    FileId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FileMappings", x => x.FileId);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FileMappings");
        }
    }
}
