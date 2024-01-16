using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOT.TCPListner.Migrations
{
    /// <inheritdoc />
    public partial class addjsoncolumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OriginalJson",
                table: "AVLsData",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalJson",
                table: "AVLsData");
        }
    }
}
