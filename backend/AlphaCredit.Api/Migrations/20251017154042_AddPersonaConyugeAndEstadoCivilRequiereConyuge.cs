using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AlphaCredit.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddPersonaConyugeAndEstadoCivilRequiereConyuge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "personaconyugeid");

            migrationBuilder.AddColumn<bool>(
                name: "estadocivilrequiereconyuge",
                table: "estadocivil",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "personaconyuge",
                columns: table => new
                {
                    personaconyugeid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personaid = table.Column<long>(type: "bigint", nullable: false),
                    conyugenombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    conyugetelefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    conyugefechacreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    conyugefechamodifica = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personaconyuge", x => x.personaconyugeid);
                    table.ForeignKey(
                        name: "FK_personaconyuge_persona_personaid",
                        column: x => x.personaid,
                        principalTable: "persona",
                        principalColumn: "personaid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_personaconyuge_personaid",
                table: "personaconyuge",
                column: "personaid",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "personaconyuge");

            migrationBuilder.DropColumn(
                name: "estadocivilrequiereconyuge",
                table: "estadocivil");

            migrationBuilder.DropSequence(
                name: "personaconyugeid");
        }
    }
}
