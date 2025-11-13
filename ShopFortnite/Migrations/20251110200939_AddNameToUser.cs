using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ShopFortnite.Migrations
{
    /// <inheritdoc />
    public partial class AddNameToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Users",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            // Atualizar usuários existentes com um nome padrão baseado no email
            migrationBuilder.Sql(@"
                UPDATE Users
                SET Name = CASE
                    WHEN Email IS NOT NULL AND Email != '' THEN substr(Email, 1, instr(Email, '@') - 1)
                    ELSE 'User'
                END
                WHERE Name IS NULL OR Name = ''
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Users");
        }
    }
}
