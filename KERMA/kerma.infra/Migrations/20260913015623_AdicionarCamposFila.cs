using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kerma.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdicionarCamposFila : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // usuario_evento já existe — criada manualmente
            // Apenas adiciona os novos campos na tabela ticket

            migrationBuilder.AddColumn<string>(
                name: "NomeCliente",
                table: "ticket",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "EnviadoFilaEm",
                table: "ticket",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ProntoEm",
                table: "ticket",
                type: "datetime(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "NomeCliente", table: "ticket");
            migrationBuilder.DropColumn(name: "EnviadoFilaEm", table: "ticket");
            migrationBuilder.DropColumn(name: "ProntoEm", table: "ticket");
        }
    }
}