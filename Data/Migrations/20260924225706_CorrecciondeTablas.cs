using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Veterinaria.Data.Migrations
{
    /// <inheritdoc />
    public partial class CorrecciondeTablas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Citas_Mascotas_MascotaId",
                table: "Citas");

            migrationBuilder.DropForeignKey(
                name: "FK_Mascotas_AspNetUsers_UsuarioId",
                table: "Mascotas");

            migrationBuilder.AddColumn<string>(
                name: "UsuarioId",
                table: "Citas",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("UPDATE c SET c.UsuarioId = m.UsuarioId FROM Citas c INNER JOIN Mascotas m ON c.MascotaId = m.Id");

            migrationBuilder.CreateIndex(
                name: "IX_Citas_UsuarioId",
                table: "Citas",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Citas_AspNetUsers_UsuarioId",
                table: "Citas",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Citas_Mascotas_MascotaId",
                table: "Citas",
                column: "MascotaId",
                principalTable: "Mascotas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Mascotas_AspNetUsers_UsuarioId",
                table: "Mascotas",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Citas_AspNetUsers_UsuarioId",
                table: "Citas");

            migrationBuilder.DropForeignKey(
                name: "FK_Citas_Mascotas_MascotaId",
                table: "Citas");

            migrationBuilder.DropForeignKey(
                name: "FK_Mascotas_AspNetUsers_UsuarioId",
                table: "Mascotas");

            migrationBuilder.DropIndex(
                name: "IX_Citas_UsuarioId",
                table: "Citas");

            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Citas");

            migrationBuilder.AddForeignKey(
                name: "FK_Citas_Mascotas_MascotaId",
                table: "Citas",
                column: "MascotaId",
                principalTable: "Mascotas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Mascotas_AspNetUsers_UsuarioId",
                table: "Mascotas",
                column: "UsuarioId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
