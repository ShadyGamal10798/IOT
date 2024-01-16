using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IOT.TCPListner.Migrations
{
    /// <inheritdoc />
    public partial class adddeviceIntegration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AVLsData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IMEI = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ListeningDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CodecID = table.Column<int>(type: "int", nullable: false),
                    DataCount = table.Column<int>(type: "int", nullable: false),
                    Longitude = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Latitiude = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Altitude = table.Column<float>(type: "real", nullable: false),
                    Satellites = table.Column<int>(type: "int", nullable: false),
                    Angle = table.Column<float>(type: "real", nullable: false),
                    Speed = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AVLsData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CommandTransactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SendingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Command = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IMEI = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommandTransactions", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AVLsData");

            migrationBuilder.DropTable(
                name: "CommandTransactions");
        }
    }
}
