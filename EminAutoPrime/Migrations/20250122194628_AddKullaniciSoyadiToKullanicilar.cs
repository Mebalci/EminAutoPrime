using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EminAutoPrime.Migrations
{
    /// <inheritdoc />
    public partial class AddKullaniciSoyadiToKullanicilar : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            
                     
            migrationBuilder.AddColumn<string>(
                name: "KullaniciSoyadi",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "KullaniciYorumlari",
                columns: table => new
                {
                    YorumId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    YorumMetni = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Puan = table.Column<int>(type: "int", nullable: false),
                    OlusturulmaTarihi = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KullaniciYorumlari", x => x.YorumId);
                    table.ForeignKey(
                        name: "FK_KullaniciYorumlari_AspNetUsers_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });
            

           
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {                                
            migrationBuilder.DropTable(
                name: "KullaniciYorumlari");     

            migrationBuilder.DropColumn(
                name: "KullaniciSoyadi",
                table: "AspNetUsers");

           
        }
    }
}
