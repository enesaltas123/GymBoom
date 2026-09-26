using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GymBoom.Migrations
{
    /// <inheritdoc />
    public partial class FixUserIdShadowProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM \"UserSubscriptions\";");
            
            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptions_Users_UserId1",
                table: "UserSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_UserSubscriptions_UserId1",
                table: "UserSubscriptions");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "UserSubscriptions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserSubscriptions");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "UserSubscriptions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_UserId",
                table: "UserSubscriptions",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscriptions_Users_UserId",
                table: "UserSubscriptions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserSubscriptions_Users_UserId",
                table: "UserSubscriptions");

            migrationBuilder.DropIndex(
                name: "IX_UserSubscriptions_UserId",
                table: "UserSubscriptions");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "UserSubscriptions");

            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "UserSubscriptions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UserId1",
                table: "UserSubscriptions",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSubscriptions_UserId1",
                table: "UserSubscriptions",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_UserSubscriptions_Users_UserId1",
                table: "UserSubscriptions",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}