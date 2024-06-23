using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoucmentManagmentSys.Migrations
{
    /// <inheritdoc />
    public partial class DocumentTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DocumentTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TemplateElements",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FixedTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DefaultData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    DocumentTemplateId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateElements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TemplateElements_DocumentTemplates_DocumentTemplateId",
                        column: x => x.DocumentTemplateId,
                        principalTable: "DocumentTemplates",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateElements_DocumentTemplateId",
                table: "TemplateElements",
                column: "DocumentTemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TemplateElements");

            migrationBuilder.DropTable(
                name: "DocumentTemplates");
        }
    }
}
