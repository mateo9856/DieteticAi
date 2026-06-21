using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DietAI.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddKeycloakUsers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Diets_Id",
                table: "Diets");

            migrationBuilder.CreateTable(
                name: "KeycloakUsers",
                columns: table => new
                {
                    UniqueId = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeycloakUsers", x => x.UniqueId);
                });

            migrationBuilder.InsertData(
                table: "KeycloakUsers",
                columns: new[] { "UniqueId", "Email" },
                values: new object[] { "019eea6e-aef4-7546-88e2-c4acdbc71872", "test@test.com" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "KeycloakUsers");

            migrationBuilder.CreateIndex(
                name: "IX_Diets_Id",
                table: "Diets",
                column: "Id");
        }
    }
}
