using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoucmentManagmentSys.Data.Migrations
{
    /// <inheritdoc />
    public partial class docsupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Content = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => new { x.Id, x.FileName });
                });


            migrationBuilder.DropTable(
                name: "Documents");


        }
    }
}
