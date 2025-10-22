using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AlphaCredit.Api.Migrations
{
    /// <inheritdoc />
    public partial class database : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "prestamogarantiamontocomprometido",
                table: "prestamogarantia",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<long>(
                name: "fondoid",
                table: "prestamo",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "garantiadocumento",
                columns: table => new
                {
                    garantiadocumentoid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    garantiaid = table.Column<long>(type: "bigint", nullable: false),
                    garantiadocumentopath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    garantiadocumentotipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    garantiadocumentofechacreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    garantiadocumentofechamodifica = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_garantiadocumento", x => x.garantiadocumentoid);
                    table.ForeignKey(
                        name: "FK_garantiadocumento_garantia_garantiaid",
                        column: x => x.garantiaid,
                        principalTable: "garantia",
                        principalColumn: "garantiaid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "personadocumento",
                columns: table => new
                {
                    personadocumentoid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personaid = table.Column<long>(type: "bigint", nullable: false),
                    personadocumentopath = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    personadocumentotipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    personadocumentofechacreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    personadocumentofechamodifica = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personadocumento", x => x.personadocumentoid);
                    table.ForeignKey(
                        name: "FK_personadocumento_persona_personaid",
                        column: x => x.personaid,
                        principalTable: "persona",
                        principalColumn: "personaid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prestamo_documento",
                columns: table => new
                {
                    prestamo_documentoid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    prestamoid = table.Column<long>(type: "bigint", nullable: false),
                    prestamo_documento_tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    prestamo_documento_ruta = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    prestamo_documento_hash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    prestamo_documento_veces_impreso = table.Column<int>(type: "integer", nullable: false),
                    prestamo_documento_fecha_primera_impresion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    prestamo_documento_fecha_ultima_impresion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    prestamo_documento_user_crea = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    prestamo_documento_fecha_creacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prestamo_documento", x => x.prestamo_documentoid);
                    table.ForeignKey(
                        name: "FK_prestamo_documento_prestamo_prestamoid",
                        column: x => x.prestamoid,
                        principalTable: "prestamo",
                        principalColumn: "prestamoid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prestamo_documento_impresion",
                columns: table => new
                {
                    prestamo_documento_impresionid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    prestamo_documentoid = table.Column<long>(type: "bigint", nullable: false),
                    prestamo_documento_impresion_fecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    prestamo_documento_impresion_usuario = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    prestamo_documento_impresion_ip = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    prestamo_documento_impresion_observaciones = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prestamo_documento_impresion", x => x.prestamo_documento_impresionid);
                    table.ForeignKey(
                        name: "FK_prestamo_documento_impresion_prestamo_documento_prestamo_do~",
                        column: x => x.prestamo_documentoid,
                        principalTable: "prestamo_documento",
                        principalColumn: "prestamo_documentoid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_prestamo_fondoid",
                table: "prestamo",
                column: "fondoid");

            migrationBuilder.CreateIndex(
                name: "IX_garantiadocumento_garantiaid",
                table: "garantiadocumento",
                column: "garantiaid");

            migrationBuilder.CreateIndex(
                name: "IX_personadocumento_personaid",
                table: "personadocumento",
                column: "personaid");

            migrationBuilder.CreateIndex(
                name: "IX_prestamo_documento_prestamoid",
                table: "prestamo_documento",
                column: "prestamoid");

            migrationBuilder.CreateIndex(
                name: "IX_prestamo_documento_impresion_prestamo_documentoid",
                table: "prestamo_documento_impresion",
                column: "prestamo_documentoid");

            migrationBuilder.AddForeignKey(
                name: "FK_prestamo_fondo_fondoid",
                table: "prestamo",
                column: "fondoid",
                principalTable: "fondo",
                principalColumn: "fondoid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_prestamo_fondo_fondoid",
                table: "prestamo");

            migrationBuilder.DropTable(
                name: "garantiadocumento");

            migrationBuilder.DropTable(
                name: "personadocumento");

            migrationBuilder.DropTable(
                name: "prestamo_documento_impresion");

            migrationBuilder.DropTable(
                name: "prestamo_documento");

            migrationBuilder.DropIndex(
                name: "IX_prestamo_fondoid",
                table: "prestamo");

            migrationBuilder.DropColumn(
                name: "prestamogarantiamontocomprometido",
                table: "prestamogarantia");

            migrationBuilder.DropColumn(
                name: "fondoid",
                table: "prestamo");
        }
    }
}
