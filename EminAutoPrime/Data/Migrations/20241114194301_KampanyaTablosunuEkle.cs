using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EminAutoPrime.Data.Migrations
{
    /// <inheritdoc />
    public partial class KampanyaTablosunuEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Kampanyalar",
                columns: table => new
                {
                    KampanyaID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KampanyaBasligi = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    KampanyaAciklamasi = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    BaslangicTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BitisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GorselYolu = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Kampanyalar", x => x.KampanyaID);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Kampanyalar");
        }
    }
}
