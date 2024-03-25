using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoucmentManagmentSys.Migrations
{
    /// <inheritdoc />
    public partial class addReasonColumntoMail : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Reason",
                table: "Documents");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Reason",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: true);
        }
    }
}
