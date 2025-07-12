using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StudentTracker.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesToOptimizeSearch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Students_CreatedOnUtc",
                table: "AspNetUsers",
                column: "CreatedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Students_Email",
                table: "AspNetUsers",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_Students_FullName",
                table: "AspNetUsers",
                column: "FullName");

            migrationBuilder.CreateIndex(
                name: "IX_Students_Grade_IsActive",
                table: "AspNetUsers",
                columns: new[] { "Grade", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_Students_ParentEmail",
                table: "AspNetUsers",
                column: "ParentEmail");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Students_CreatedOnUtc",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_Students_Email",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_Students_FullName",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_Students_Grade_IsActive",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_Students_ParentEmail",
                table: "AspNetUsers");
        }
    }
}
