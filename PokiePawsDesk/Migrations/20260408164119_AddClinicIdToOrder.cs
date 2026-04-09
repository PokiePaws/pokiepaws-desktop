using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PokiePawsDesk.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicIdToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ClinicId",
                table: "Orders",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClinicId",
                table: "Orders");
        }
    }
}
