using Microsoft.EntityFrameworkCore.Migrations;

public partial class ModifyDocumentIndex : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Remove the existing index on the 'name' column
        migrationBuilder.DropIndex(
            name: "IX_Documents_name",
            table: "Documents");

        // Create a new composite index on 'name' and 'code' columns
        migrationBuilder.CreateIndex(
            name: "IX_Documents_name_code",
            table: "Documents",
            columns: new[] { "name", "code" },
            unique: false);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Recreate the original index on the 'name' column
        migrationBuilder.CreateIndex(
            name: "IX_Documents_name",
            table: "Documents",
            column: "name",
            unique: true);

        // Remove the composite index
        migrationBuilder.DropIndex(
            name: "IX_Documents_name_code",
            table: "Documents");
    }
}