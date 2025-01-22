using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EminAutoPrime.Migrations
{
    /// <inheritdoc />
    public partial class KullaniciBilgileri : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           

            migrationBuilder.AddColumn<string>(
            name: "KullaniciSoyadi",
            table: "AspNetUsers",
            type: "nvarchar(100)",
            maxLength: 100,
            nullable: true);

            migrationBuilder.CreateTable(
            name: "KullaniciYorumlari",
            columns: table => new
            {
                YorumId = table.Column<int>(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                KullaniciId = table.Column<int>(nullable: false),
                Yorum = table.Column<string>(nullable: true),
                Tarih = table.Column<DateTime>(nullable: false, defaultValueSql: "GETDATE()")
            },
            constraints: table =>
        {
            table.PrimaryKey("PK_KullaniciYorumlari", x => x.YorumId);
            table.ForeignKey(
                name: "FK_KullaniciYorumlari_Kullanicilar",
                column: x => x.KullaniciId,
                principalTable: "Kullanicilar",
                principalColumn: "KullaniciId",
                onDelete: ReferentialAction.Restrict);
        });

            migrationBuilder.CreateIndex(
                name: "IX_KullaniciYorumlari_KullaniciId",
                table: "KullaniciYorumlari",
                column: "KullaniciId");
        }
        

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
            migrationBuilder.DropColumn(
            name: "KullaniciSoyadi",
            table: "AspNetUsers");

            migrationBuilder.DropTable(
            name: "KullaniciYorumlari");
        }
    }
}
