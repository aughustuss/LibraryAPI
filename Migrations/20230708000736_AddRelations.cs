using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LibraryAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_books_users_UserId",
                table: "books");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "books",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "UserFKId",
                table: "books",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_books_UserFKId",
                table: "books",
                column: "UserFKId");

            migrationBuilder.AddForeignKey(
                name: "FK_books_users_UserFKId",
                table: "books",
                column: "UserFKId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_books_users_UserId",
                table: "books",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_books_users_UserFKId",
                table: "books");

            migrationBuilder.DropForeignKey(
                name: "FK_books_users_UserId",
                table: "books");

            migrationBuilder.DropIndex(
                name: "IX_books_UserFKId",
                table: "books");

            migrationBuilder.DropColumn(
                name: "UserFKId",
                table: "books");

            migrationBuilder.AlterColumn<int>(
                name: "UserId",
                table: "books",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_books_users_UserId",
                table: "books",
                column: "UserId",
                principalTable: "users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
