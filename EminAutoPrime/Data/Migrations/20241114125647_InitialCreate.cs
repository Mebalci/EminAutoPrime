using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EminAutoPrime.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EminAutoAraclar",
                columns: table => new
                {
                    AracId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Marka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Model = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Yil = table.Column<int>(type: "int", nullable: false),
                    Plaka = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SahipAdi = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EminAutoAraclar", x => x.AracId);
                });

            migrationBuilder.CreateTable(
                name: "EminAutoServisler",
                columns: table => new
                {
                    ServisId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AracId = table.Column<int>(type: "int", nullable: false),
                    YapilanIslemler = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    GirisTarihi = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CikisTarihi = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Tamamlandi = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EminAutoServisler", x => x.ServisId);
                    table.ForeignKey(
                        name: "FK_EminAutoServisler_EminAutoAraclar_AracId",
                        column: x => x.AracId,
                        principalTable: "EminAutoAraclar",
                        principalColumn: "AracId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EminAutoServisler_AracId",
                table: "EminAutoServisler",
                column: "AracId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EminAutoServisler");

            migrationBuilder.DropTable(
                name: "EminAutoAraclar");
        }
    }
}
