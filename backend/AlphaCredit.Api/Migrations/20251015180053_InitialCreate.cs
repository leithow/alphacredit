using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AlphaCredit.Api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "actividadcnbsid");

            migrationBuilder.CreateSequence(
                name: "actividadeconomicaid");

            migrationBuilder.CreateSequence(
                name: "bancoid");

            migrationBuilder.CreateSequence(
                name: "componenteprestamoid");

            migrationBuilder.CreateSequence(
                name: "cuentabancariaid");

            migrationBuilder.CreateSequence(
                name: "destinocreditoid");

            migrationBuilder.CreateSequence(
                name: "empresaid");

            migrationBuilder.CreateSequence(
                name: "estadocivilid");

            migrationBuilder.CreateSequence(
                name: "estadocomponenteid");

            migrationBuilder.CreateSequence(
                name: "estadogarantiaid");

            migrationBuilder.CreateSequence(
                name: "estadoprestamoid");

            migrationBuilder.CreateSequence(
                name: "fechasistemaid");

            migrationBuilder.CreateSequence(
                name: "fondoid");

            migrationBuilder.CreateSequence(
                name: "fondomovimientoid");

            migrationBuilder.CreateSequence(
                name: "formapagoid");

            migrationBuilder.CreateSequence(
                name: "frecuenciapagoid");

            migrationBuilder.CreateSequence(
                name: "garantiaid");

            migrationBuilder.CreateSequence(
                name: "monedaid");

            migrationBuilder.CreateSequence(
                name: "movimientoprestamoid");

            migrationBuilder.CreateSequence(
                name: "operadortelefonoid");

            migrationBuilder.CreateSequence(
                name: "paisid");

            migrationBuilder.CreateSequence(
                name: "parametrossistemaid");

            migrationBuilder.CreateSequence(
                name: "personaactividadid");

            migrationBuilder.CreateSequence(
                name: "personaid");

            migrationBuilder.CreateSequence(
                name: "personareferenciaid");

            migrationBuilder.CreateSequence(
                name: "personatelefonosid");

            migrationBuilder.CreateSequence(
                name: "prestamocomponenteid");

            migrationBuilder.CreateSequence(
                name: "prestamogarantiaid");

            migrationBuilder.CreateSequence(
                name: "prestamoid");

            migrationBuilder.CreateSequence(
                name: "sexoid");

            migrationBuilder.CreateSequence(
                name: "sucursalid");

            migrationBuilder.CreateSequence(
                name: "tipocuentaid");

            migrationBuilder.CreateSequence(
                name: "tipofondoid");

            migrationBuilder.CreateSequence(
                name: "tipogarantiaid");

            migrationBuilder.CreateSequence(
                name: "tipoidentificacionid");

            migrationBuilder.CreateSequence(
                name: "transaccionid");

            migrationBuilder.CreateSequence(
                name: "ubicaciongeograficaid");

            migrationBuilder.CreateTable(
                name: "actividadcnbs",
                columns: table => new
                {
                    actividadcnbsid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    actividadcnbscodigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    actividadcnbsnombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    actividadcnbsdescripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    actividadcnbsestaactiva = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_actividadcnbs", x => x.actividadcnbsid);
                });

            migrationBuilder.CreateTable(
                name: "banco",
                columns: table => new
                {
                    bancoid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    banconombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    bancocodigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    bancoestaactivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_banco", x => x.bancoid);
                });

            migrationBuilder.CreateTable(
                name: "componenteprestamo",
                columns: table => new
                {
                    componenteprestamoid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    componenteprestamonombre = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    componenteprestamodescripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    componenteprestamotipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    componenteprestamoestaactivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_componenteprestamo", x => x.componenteprestamoid);
                });

            migrationBuilder.CreateTable(
                name: "destinocredito",
                columns: table => new
                {
                    destinocreditoid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    destinocreditonombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    destinocreditodescripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    destinocreditoestaactivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_destinocredito", x => x.destinocreditoid);
                });

            migrationBuilder.CreateTable(
                name: "empresa",
                columns: table => new
                {
                    empresaid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    empresanombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    empresartn = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    empresadireccion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    empresatelefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    empresaemail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    empresaestaactiva = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_empresa", x => x.empresaid);
                });

            migrationBuilder.CreateTable(
                name: "estadocivil",
                columns: table => new
                {
                    estadocivilid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    estadocivilnombre = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    estadocivildescripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    estadocivilestaactivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estadocivil", x => x.estadocivilid);
                });

            migrationBuilder.CreateTable(
                name: "estadocomponente",
                columns: table => new
                {
                    estadocomponenteid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    estadocomponentenombre = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    estadocomponentedescripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    estadocomponenteestaactivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estadocomponente", x => x.estadocomponenteid);
                });

            migrationBuilder.CreateTable(
                name: "estadogarantia",
                columns: table => new
                {
                    estadogarantiaid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    estadogarantianombre = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    estadogarantiadescripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    estadogarantiaestaactiva = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estadogarantia", x => x.estadogarantiaid);
                });

            migrationBuilder.CreateTable(
                name: "estadoprestamo",
                columns: table => new
                {
                    estadoprestamoid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    estadoprestamonombre = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    estadoprestamodescripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    estadoprestamoestaactivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_estadoprestamo", x => x.estadoprestamoid);
                });

            migrationBuilder.CreateTable(
                name: "fechasistema",
                columns: table => new
                {
                    fechasistemaid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fechasistemafecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fechasistemaestaactiva = table.Column<bool>(type: "boolean", nullable: false),
                    fechasistemausercrea = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    fechasistemafechacreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fechasistema", x => x.fechasistemaid);
                });

            migrationBuilder.CreateTable(
                name: "formapago",
                columns: table => new
                {
                    formapagoid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    formapaganombre = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    formapagodescripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    formapagoestaactiva = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_formapago", x => x.formapagoid);
                });

            migrationBuilder.CreateTable(
                name: "frecuenciapago",
                columns: table => new
                {
                    frecuenciapagoid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    frecuenciapaganombre = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    frecuenciapagodias = table.Column<short>(type: "smallint", nullable: false),
                    frecuenciapagoestaactiva = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_frecuenciapago", x => x.frecuenciapagoid);
                });

            migrationBuilder.CreateTable(
                name: "moneda",
                columns: table => new
                {
                    monedaid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    monedanombre = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    monedacodigo = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    monedasimbolo = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    monedaestaactiva = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_moneda", x => x.monedaid);
                });

            migrationBuilder.CreateTable(
                name: "operadortelefono",
                columns: table => new
                {
                    operadortelefonoid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    operadortelefonombre = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    operadortelefonoestaactivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_operadortelefono", x => x.operadortelefonoid);
                });

            migrationBuilder.CreateTable(
                name: "pais",
                columns: table => new
                {
                    paisid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    paisnombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    paiscodigo = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: true),
                    paisestaactivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pais", x => x.paisid);
                });

            migrationBuilder.CreateTable(
                name: "parametrossistema",
                columns: table => new
                {
                    parametrosistemaid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    parametrosistemallave = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    parametrossistemavalor = table.Column<string>(type: "text", nullable: false),
                    parametrossistemadescripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    parametrossistematipodato = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    parametrossistemaestaactivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_parametrossistema", x => x.parametrosistemaid);
                });

            migrationBuilder.CreateTable(
                name: "sexo",
                columns: table => new
                {
                    sexoid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    sexonombre = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    sexodescripcion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    sexoestaactivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sexo", x => x.sexoid);
                });

            migrationBuilder.CreateTable(
                name: "tipocuenta",
                columns: table => new
                {
                    tipocuentaid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipocuentanombre = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    tipocuentadescripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    tipocuentaestaactiva = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipocuenta", x => x.tipocuentaid);
                });

            migrationBuilder.CreateTable(
                name: "tipofondo",
                columns: table => new
                {
                    tipofondoid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipofondonombre = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    tipofondodescripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    tipofondoestaactivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipofondo", x => x.tipofondoid);
                });

            migrationBuilder.CreateTable(
                name: "tipogarantia",
                columns: table => new
                {
                    tipogarantiaid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipogarantianombre = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    tipogarantiadescripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    tipogarantiaestaactiva = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipogarantia", x => x.tipogarantiaid);
                });

            migrationBuilder.CreateTable(
                name: "tipoidentificacion",
                columns: table => new
                {
                    tipoidentificacionid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipoidentificacionnombre = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    tipoidentificaciondescripcion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    tipoidentificacionestaactivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tipoidentificacion", x => x.tipoidentificacionid);
                });

            migrationBuilder.CreateTable(
                name: "actividadeconomica",
                columns: table => new
                {
                    actividadeconomicaid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    actividadcnbsid = table.Column<long>(type: "bigint", nullable: true),
                    actividadeconomicanombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    actividadeconomicadescripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    actividadeconomicaestaactiva = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_actividadeconomica", x => x.actividadeconomicaid);
                    table.ForeignKey(
                        name: "FK_actividadeconomica_actividadcnbs_actividadcnbsid",
                        column: x => x.actividadcnbsid,
                        principalTable: "actividadcnbs",
                        principalColumn: "actividadcnbsid");
                });

            migrationBuilder.CreateTable(
                name: "sucursal",
                columns: table => new
                {
                    sucursalid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    empresaid = table.Column<long>(type: "bigint", nullable: false),
                    sucursalnombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    sucursaldireccion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    sucursaltelefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    sucursalestaactiva = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sucursal", x => x.sucursalid);
                    table.ForeignKey(
                        name: "FK_sucursal_empresa_empresaid",
                        column: x => x.empresaid,
                        principalTable: "empresa",
                        principalColumn: "empresaid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ubicaciongeografica",
                columns: table => new
                {
                    ubicaciongeograficaid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    paisid = table.Column<long>(type: "bigint", nullable: true),
                    ubicaciongeograficapadre = table.Column<long>(type: "bigint", nullable: true),
                    ubicaciongeograficanombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ubicaciongeograficanivel = table.Column<short>(type: "smallint", nullable: false),
                    ubicaciongeograficacodigo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    ubicaciongeograficaestaactiva = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ubicaciongeografica", x => x.ubicaciongeograficaid);
                    table.ForeignKey(
                        name: "FK_ubicaciongeografica_pais_paisid",
                        column: x => x.paisid,
                        principalTable: "pais",
                        principalColumn: "paisid");
                    table.ForeignKey(
                        name: "FK_ubicaciongeografica_ubicaciongeografica_ubicaciongeografica~",
                        column: x => x.ubicaciongeograficapadre,
                        principalTable: "ubicaciongeografica",
                        principalColumn: "ubicaciongeograficaid");
                });

            migrationBuilder.CreateTable(
                name: "cuentabancaria",
                columns: table => new
                {
                    cuentabancariaid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bancoid = table.Column<long>(type: "bigint", nullable: false),
                    tipocuentaid = table.Column<long>(type: "bigint", nullable: false),
                    monedaid = table.Column<long>(type: "bigint", nullable: false),
                    cuentabancarianumero = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    cuentabancarianombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    cuentabancariasaldo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    cuentabancariaestaactiva = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cuentabancaria", x => x.cuentabancariaid);
                    table.ForeignKey(
                        name: "FK_cuentabancaria_banco_bancoid",
                        column: x => x.bancoid,
                        principalTable: "banco",
                        principalColumn: "bancoid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cuentabancaria_moneda_monedaid",
                        column: x => x.monedaid,
                        principalTable: "moneda",
                        principalColumn: "monedaid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_cuentabancaria_tipocuenta_tipocuentaid",
                        column: x => x.tipocuentaid,
                        principalTable: "tipocuenta",
                        principalColumn: "tipocuentaid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fondo",
                columns: table => new
                {
                    fondoid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tipofondoid = table.Column<long>(type: "bigint", nullable: false),
                    monedaid = table.Column<long>(type: "bigint", nullable: false),
                    fondonombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    fondodescripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    fondosaldoinicial = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    fondosaldoactual = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    fondoestaactivo = table.Column<bool>(type: "boolean", nullable: false),
                    fondousercrea = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    fondousermodifica = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    fondofechacreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fondofechamodifica = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fondo", x => x.fondoid);
                    table.ForeignKey(
                        name: "FK_fondo_moneda_monedaid",
                        column: x => x.monedaid,
                        principalTable: "moneda",
                        principalColumn: "monedaid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_fondo_tipofondo_tipofondoid",
                        column: x => x.tipofondoid,
                        principalTable: "tipofondo",
                        principalColumn: "tipofondoid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "persona",
                columns: table => new
                {
                    personaid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personaprimernombre = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    personasegundonombre = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    personaprimerapellido = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    personasegundoapellido = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    tipoidentificacionid = table.Column<long>(type: "bigint", nullable: false),
                    personaidentificacion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    personafechanacimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    personaesnatural = table.Column<bool>(type: "boolean", nullable: false),
                    personaesempleado = table.Column<bool>(type: "boolean", nullable: false),
                    personaescliente = table.Column<bool>(type: "boolean", nullable: false),
                    personaesproveedor = table.Column<bool>(type: "boolean", nullable: false),
                    estadocivilid = table.Column<long>(type: "bigint", nullable: true),
                    personadireccion = table.Column<string>(type: "character varying(1024)", maxLength: 1024, nullable: false),
                    personageolocalizacion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    personaemail = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    personaestaactiva = table.Column<bool>(type: "boolean", nullable: false),
                    personausercrea = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    personausermodifica = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    personafechacreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    personafechamodifica = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    sexoid = table.Column<long>(type: "bigint", nullable: true),
                    personanombrecompleto = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_persona", x => x.personaid);
                    table.ForeignKey(
                        name: "FK_persona_estadocivil_estadocivilid",
                        column: x => x.estadocivilid,
                        principalTable: "estadocivil",
                        principalColumn: "estadocivilid");
                    table.ForeignKey(
                        name: "FK_persona_sexo_sexoid",
                        column: x => x.sexoid,
                        principalTable: "sexo",
                        principalColumn: "sexoid");
                    table.ForeignKey(
                        name: "FK_persona_tipoidentificacion_tipoidentificacionid",
                        column: x => x.tipoidentificacionid,
                        principalTable: "tipoidentificacion",
                        principalColumn: "tipoidentificacionid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transaccion",
                columns: table => new
                {
                    transaccionid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    cuentabancariaid = table.Column<long>(type: "bigint", nullable: true),
                    transaccionnumero = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    transacciontipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    transaccionfecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    transaccionmonto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    transaccionconcepto = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    transaccionreferencia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    transaccionusercrea = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    transaccionfechacreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaccion", x => x.transaccionid);
                    table.ForeignKey(
                        name: "FK_transaccion_cuentabancaria_cuentabancariaid",
                        column: x => x.cuentabancariaid,
                        principalTable: "cuentabancaria",
                        principalColumn: "cuentabancariaid");
                });

            migrationBuilder.CreateTable(
                name: "garantia",
                columns: table => new
                {
                    garantiaid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personaid = table.Column<long>(type: "bigint", nullable: false),
                    tipogarantiaid = table.Column<long>(type: "bigint", nullable: false),
                    estadogarantiaid = table.Column<long>(type: "bigint", nullable: false),
                    garantiadescripcion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    garantiavalorcomercial = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    garantiavalorrealizable = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    garantiaobservaciones = table.Column<string>(type: "text", nullable: true),
                    garantiausercrea = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    garantiausermodifica = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    garantiafechacreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    garantiafechamodifica = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_garantia", x => x.garantiaid);
                    table.ForeignKey(
                        name: "FK_garantia_estadogarantia_estadogarantiaid",
                        column: x => x.estadogarantiaid,
                        principalTable: "estadogarantia",
                        principalColumn: "estadogarantiaid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_garantia_persona_personaid",
                        column: x => x.personaid,
                        principalTable: "persona",
                        principalColumn: "personaid");
                    table.ForeignKey(
                        name: "FK_garantia_tipogarantia_tipogarantiaid",
                        column: x => x.tipogarantiaid,
                        principalTable: "tipogarantia",
                        principalColumn: "tipogarantiaid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "personaactividad",
                columns: table => new
                {
                    personaactividadid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personaid = table.Column<long>(type: "bigint", nullable: false),
                    actividadeconomicaid = table.Column<long>(type: "bigint", nullable: false),
                    personaactividadingresomensual = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    personaactividadestaactiva = table.Column<bool>(type: "boolean", nullable: false),
                    personaactividadfechainicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    personaactividadesprincipal = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personaactividad", x => x.personaactividadid);
                    table.ForeignKey(
                        name: "FK_personaactividad_actividadeconomica_actividadeconomicaid",
                        column: x => x.actividadeconomicaid,
                        principalTable: "actividadeconomica",
                        principalColumn: "actividadeconomicaid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_personaactividad_persona_personaid",
                        column: x => x.personaid,
                        principalTable: "persona",
                        principalColumn: "personaid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "personareferencia",
                columns: table => new
                {
                    personareferenciaid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personaid = table.Column<long>(type: "bigint", nullable: false),
                    personareferencianombre = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    personareferenciatelefono = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    personareferenciadireccion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    personareferenciaparentesco = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    personareferenciaestaactiva = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personareferencia", x => x.personareferenciaid);
                    table.ForeignKey(
                        name: "FK_personareferencia_persona_personaid",
                        column: x => x.personaid,
                        principalTable: "persona",
                        principalColumn: "personaid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "personatelefono",
                columns: table => new
                {
                    personatelefonoid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personaid = table.Column<long>(type: "bigint", nullable: false),
                    operadortelefonoid = table.Column<long>(type: "bigint", nullable: false),
                    personatelefonumero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    personatelefonoesprincipal = table.Column<bool>(type: "boolean", nullable: false),
                    personatelefonoestaactivo = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_personatelefono", x => x.personatelefonoid);
                    table.ForeignKey(
                        name: "FK_personatelefono_operadortelefono_operadortelefonoid",
                        column: x => x.operadortelefonoid,
                        principalTable: "operadortelefono",
                        principalColumn: "operadortelefonoid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_personatelefono_persona_personaid",
                        column: x => x.personaid,
                        principalTable: "persona",
                        principalColumn: "personaid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prestamo",
                columns: table => new
                {
                    prestamoid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    personaid = table.Column<long>(type: "bigint", nullable: false),
                    sucursalid = table.Column<long>(type: "bigint", nullable: false),
                    monedaid = table.Column<long>(type: "bigint", nullable: false),
                    estadoprestamoid = table.Column<long>(type: "bigint", nullable: false),
                    destinocreditoid = table.Column<long>(type: "bigint", nullable: true),
                    frecuenciapagoid = table.Column<long>(type: "bigint", nullable: false),
                    prestamonumero = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    prestamomonto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    prestamotasainteres = table.Column<decimal>(type: "numeric(5,2)", nullable: false),
                    prestamoplazo = table.Column<int>(type: "integer", nullable: false),
                    prestamofechaaprobacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    prestamofechadesembolso = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    prestamofechavencimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    prestamosaldocapital = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    prestamosaldointeres = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    prestamosaldomora = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    prestamoobservaciones = table.Column<string>(type: "text", nullable: true),
                    prestamousercrea = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    prestamousermodifica = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    prestamofechacreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    prestamofechamodifica = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prestamo", x => x.prestamoid);
                    table.ForeignKey(
                        name: "FK_prestamo_destinocredito_destinocreditoid",
                        column: x => x.destinocreditoid,
                        principalTable: "destinocredito",
                        principalColumn: "destinocreditoid");
                    table.ForeignKey(
                        name: "FK_prestamo_estadoprestamo_estadoprestamoid",
                        column: x => x.estadoprestamoid,
                        principalTable: "estadoprestamo",
                        principalColumn: "estadoprestamoid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_prestamo_frecuenciapago_frecuenciapagoid",
                        column: x => x.frecuenciapagoid,
                        principalTable: "frecuenciapago",
                        principalColumn: "frecuenciapagoid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_prestamo_moneda_monedaid",
                        column: x => x.monedaid,
                        principalTable: "moneda",
                        principalColumn: "monedaid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_prestamo_persona_personaid",
                        column: x => x.personaid,
                        principalTable: "persona",
                        principalColumn: "personaid");
                    table.ForeignKey(
                        name: "FK_prestamo_sucursal_sucursalid",
                        column: x => x.sucursalid,
                        principalTable: "sucursal",
                        principalColumn: "sucursalid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "fondomovimiento",
                columns: table => new
                {
                    fondomovimientoid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    fondoid = table.Column<long>(type: "bigint", nullable: false),
                    transaccionid = table.Column<long>(type: "bigint", nullable: true),
                    fondomovimientotipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    fondomovimientofecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    fondomovimientomonto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    fondomovimientoconcepto = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    fondomovimientousercrea = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    fondomovimientofechacreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_fondomovimiento", x => x.fondomovimientoid);
                    table.ForeignKey(
                        name: "FK_fondomovimiento_fondo_fondoid",
                        column: x => x.fondoid,
                        principalTable: "fondo",
                        principalColumn: "fondoid");
                    table.ForeignKey(
                        name: "FK_fondomovimiento_transaccion_transaccionid",
                        column: x => x.transaccionid,
                        principalTable: "transaccion",
                        principalColumn: "transaccionid");
                });

            migrationBuilder.CreateTable(
                name: "movimientoprestamo",
                columns: table => new
                {
                    movimientoprestamoid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    prestamoid = table.Column<long>(type: "bigint", nullable: false),
                    transaccionid = table.Column<long>(type: "bigint", nullable: true),
                    formapagoid = table.Column<long>(type: "bigint", nullable: true),
                    movimientoprestamotipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    movimientoprestamofecha = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    movimientoprestamomontocapital = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    movimientoprestamomontointeres = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    movimientoprestamomontomora = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    movimientoprestamomontootros = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    movimientoprestamomontototal = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    movimientoprestamoobservaciones = table.Column<string>(type: "text", nullable: true),
                    movimientoprestamousercrea = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                    movimientoprestamofechacreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_movimientoprestamo", x => x.movimientoprestamoid);
                    table.ForeignKey(
                        name: "FK_movimientoprestamo_formapago_formapagoid",
                        column: x => x.formapagoid,
                        principalTable: "formapago",
                        principalColumn: "formapagoid");
                    table.ForeignKey(
                        name: "FK_movimientoprestamo_prestamo_prestamoid",
                        column: x => x.prestamoid,
                        principalTable: "prestamo",
                        principalColumn: "prestamoid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_movimientoprestamo_transaccion_transaccionid",
                        column: x => x.transaccionid,
                        principalTable: "transaccion",
                        principalColumn: "transaccionid");
                });

            migrationBuilder.CreateTable(
                name: "prestamocomponente",
                columns: table => new
                {
                    prestamocomponenteid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    prestamoid = table.Column<long>(type: "bigint", nullable: false),
                    componenteprestamoid = table.Column<long>(type: "bigint", nullable: false),
                    estadocomponenteid = table.Column<long>(type: "bigint", nullable: false),
                    prestamocomponentemonto = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    prestamocomponentefechavencimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    prestamocomponentesaldo = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    prestamocomponentenumerocuota = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prestamocomponente", x => x.prestamocomponenteid);
                    table.ForeignKey(
                        name: "FK_prestamocomponente_componenteprestamo_componenteprestamoid",
                        column: x => x.componenteprestamoid,
                        principalTable: "componenteprestamo",
                        principalColumn: "componenteprestamoid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_prestamocomponente_estadocomponente_estadocomponenteid",
                        column: x => x.estadocomponenteid,
                        principalTable: "estadocomponente",
                        principalColumn: "estadocomponenteid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_prestamocomponente_prestamo_prestamoid",
                        column: x => x.prestamoid,
                        principalTable: "prestamo",
                        principalColumn: "prestamoid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "prestamogarantia",
                columns: table => new
                {
                    prestamogarantiaid = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    prestamoid = table.Column<long>(type: "bigint", nullable: false),
                    garantiaid = table.Column<long>(type: "bigint", nullable: false),
                    prestamogarantiafechaasignacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    prestamogarantiaestaactiva = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_prestamogarantia", x => x.prestamogarantiaid);
                    table.ForeignKey(
                        name: "FK_prestamogarantia_garantia_garantiaid",
                        column: x => x.garantiaid,
                        principalTable: "garantia",
                        principalColumn: "garantiaid",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_prestamogarantia_prestamo_prestamoid",
                        column: x => x.prestamoid,
                        principalTable: "prestamo",
                        principalColumn: "prestamoid",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "uactividadcnbs",
                table: "actividadcnbs",
                column: "actividadcnbscodigo",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_actividadeconomica_actividadcnbsid",
                table: "actividadeconomica",
                column: "actividadcnbsid");

            migrationBuilder.CreateIndex(
                name: "IX_cuentabancaria_bancoid",
                table: "cuentabancaria",
                column: "bancoid");

            migrationBuilder.CreateIndex(
                name: "IX_cuentabancaria_monedaid",
                table: "cuentabancaria",
                column: "monedaid");

            migrationBuilder.CreateIndex(
                name: "IX_cuentabancaria_tipocuentaid",
                table: "cuentabancaria",
                column: "tipocuentaid");

            migrationBuilder.CreateIndex(
                name: "ufechasistemaf",
                table: "fechasistema",
                column: "fechasistemafecha",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_fondo_monedaid",
                table: "fondo",
                column: "monedaid");

            migrationBuilder.CreateIndex(
                name: "IX_fondo_tipofondoid",
                table: "fondo",
                column: "tipofondoid");

            migrationBuilder.CreateIndex(
                name: "IX_fondomovimiento_fondoid",
                table: "fondomovimiento",
                column: "fondoid");

            migrationBuilder.CreateIndex(
                name: "IX_fondomovimiento_transaccionid",
                table: "fondomovimiento",
                column: "transaccionid");

            migrationBuilder.CreateIndex(
                name: "IX_garantia_estadogarantiaid",
                table: "garantia",
                column: "estadogarantiaid");

            migrationBuilder.CreateIndex(
                name: "IX_garantia_personaid",
                table: "garantia",
                column: "personaid");

            migrationBuilder.CreateIndex(
                name: "IX_garantia_tipogarantiaid",
                table: "garantia",
                column: "tipogarantiaid");

            migrationBuilder.CreateIndex(
                name: "IX_movimientoprestamo_formapagoid",
                table: "movimientoprestamo",
                column: "formapagoid");

            migrationBuilder.CreateIndex(
                name: "IX_movimientoprestamo_prestamoid",
                table: "movimientoprestamo",
                column: "prestamoid");

            migrationBuilder.CreateIndex(
                name: "IX_movimientoprestamo_transaccionid",
                table: "movimientoprestamo",
                column: "transaccionid");

            migrationBuilder.CreateIndex(
                name: "IX_persona_estadocivilid",
                table: "persona",
                column: "estadocivilid");

            migrationBuilder.CreateIndex(
                name: "IX_persona_sexoid",
                table: "persona",
                column: "sexoid");

            migrationBuilder.CreateIndex(
                name: "IX_persona_tipoidentificacionid",
                table: "persona",
                column: "tipoidentificacionid");

            migrationBuilder.CreateIndex(
                name: "upersona",
                table: "persona",
                column: "personaidentificacion",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_personaactividad_actividadeconomicaid",
                table: "personaactividad",
                column: "actividadeconomicaid");

            migrationBuilder.CreateIndex(
                name: "IX_personaactividad_personaid",
                table: "personaactividad",
                column: "personaid");

            migrationBuilder.CreateIndex(
                name: "IX_personareferencia_personaid",
                table: "personareferencia",
                column: "personaid");

            migrationBuilder.CreateIndex(
                name: "IX_personatelefono_operadortelefonoid",
                table: "personatelefono",
                column: "operadortelefonoid");

            migrationBuilder.CreateIndex(
                name: "IX_personatelefono_personaid",
                table: "personatelefono",
                column: "personaid");

            migrationBuilder.CreateIndex(
                name: "IX_prestamo_destinocreditoid",
                table: "prestamo",
                column: "destinocreditoid");

            migrationBuilder.CreateIndex(
                name: "IX_prestamo_estadoprestamoid",
                table: "prestamo",
                column: "estadoprestamoid");

            migrationBuilder.CreateIndex(
                name: "IX_prestamo_frecuenciapagoid",
                table: "prestamo",
                column: "frecuenciapagoid");

            migrationBuilder.CreateIndex(
                name: "IX_prestamo_monedaid",
                table: "prestamo",
                column: "monedaid");

            migrationBuilder.CreateIndex(
                name: "IX_prestamo_personaid",
                table: "prestamo",
                column: "personaid");

            migrationBuilder.CreateIndex(
                name: "IX_prestamo_sucursalid",
                table: "prestamo",
                column: "sucursalid");

            migrationBuilder.CreateIndex(
                name: "IX_prestamocomponente_componenteprestamoid",
                table: "prestamocomponente",
                column: "componenteprestamoid");

            migrationBuilder.CreateIndex(
                name: "IX_prestamocomponente_estadocomponenteid",
                table: "prestamocomponente",
                column: "estadocomponenteid");

            migrationBuilder.CreateIndex(
                name: "IX_prestamocomponente_prestamoid",
                table: "prestamocomponente",
                column: "prestamoid");

            migrationBuilder.CreateIndex(
                name: "IX_prestamogarantia_garantiaid",
                table: "prestamogarantia",
                column: "garantiaid");

            migrationBuilder.CreateIndex(
                name: "IX_prestamogarantia_prestamoid",
                table: "prestamogarantia",
                column: "prestamoid");

            migrationBuilder.CreateIndex(
                name: "IX_sucursal_empresaid",
                table: "sucursal",
                column: "empresaid");

            migrationBuilder.CreateIndex(
                name: "IX_transaccion_cuentabancariaid",
                table: "transaccion",
                column: "cuentabancariaid");

            migrationBuilder.CreateIndex(
                name: "IX_ubicaciongeografica_paisid",
                table: "ubicaciongeografica",
                column: "paisid");

            migrationBuilder.CreateIndex(
                name: "IX_ubicaciongeografica_ubicaciongeograficapadre",
                table: "ubicaciongeografica",
                column: "ubicaciongeograficapadre");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "fechasistema");

            migrationBuilder.DropTable(
                name: "fondomovimiento");

            migrationBuilder.DropTable(
                name: "movimientoprestamo");

            migrationBuilder.DropTable(
                name: "parametrossistema");

            migrationBuilder.DropTable(
                name: "personaactividad");

            migrationBuilder.DropTable(
                name: "personareferencia");

            migrationBuilder.DropTable(
                name: "personatelefono");

            migrationBuilder.DropTable(
                name: "prestamocomponente");

            migrationBuilder.DropTable(
                name: "prestamogarantia");

            migrationBuilder.DropTable(
                name: "ubicaciongeografica");

            migrationBuilder.DropTable(
                name: "fondo");

            migrationBuilder.DropTable(
                name: "formapago");

            migrationBuilder.DropTable(
                name: "transaccion");

            migrationBuilder.DropTable(
                name: "actividadeconomica");

            migrationBuilder.DropTable(
                name: "operadortelefono");

            migrationBuilder.DropTable(
                name: "componenteprestamo");

            migrationBuilder.DropTable(
                name: "estadocomponente");

            migrationBuilder.DropTable(
                name: "garantia");

            migrationBuilder.DropTable(
                name: "prestamo");

            migrationBuilder.DropTable(
                name: "pais");

            migrationBuilder.DropTable(
                name: "tipofondo");

            migrationBuilder.DropTable(
                name: "cuentabancaria");

            migrationBuilder.DropTable(
                name: "actividadcnbs");

            migrationBuilder.DropTable(
                name: "estadogarantia");

            migrationBuilder.DropTable(
                name: "tipogarantia");

            migrationBuilder.DropTable(
                name: "destinocredito");

            migrationBuilder.DropTable(
                name: "estadoprestamo");

            migrationBuilder.DropTable(
                name: "frecuenciapago");

            migrationBuilder.DropTable(
                name: "persona");

            migrationBuilder.DropTable(
                name: "sucursal");

            migrationBuilder.DropTable(
                name: "banco");

            migrationBuilder.DropTable(
                name: "moneda");

            migrationBuilder.DropTable(
                name: "tipocuenta");

            migrationBuilder.DropTable(
                name: "estadocivil");

            migrationBuilder.DropTable(
                name: "sexo");

            migrationBuilder.DropTable(
                name: "tipoidentificacion");

            migrationBuilder.DropTable(
                name: "empresa");

            migrationBuilder.DropSequence(
                name: "actividadcnbsid");

            migrationBuilder.DropSequence(
                name: "actividadeconomicaid");

            migrationBuilder.DropSequence(
                name: "bancoid");

            migrationBuilder.DropSequence(
                name: "componenteprestamoid");

            migrationBuilder.DropSequence(
                name: "cuentabancariaid");

            migrationBuilder.DropSequence(
                name: "destinocreditoid");

            migrationBuilder.DropSequence(
                name: "empresaid");

            migrationBuilder.DropSequence(
                name: "estadocivilid");

            migrationBuilder.DropSequence(
                name: "estadocomponenteid");

            migrationBuilder.DropSequence(
                name: "estadogarantiaid");

            migrationBuilder.DropSequence(
                name: "estadoprestamoid");

            migrationBuilder.DropSequence(
                name: "fechasistemaid");

            migrationBuilder.DropSequence(
                name: "fondoid");

            migrationBuilder.DropSequence(
                name: "fondomovimientoid");

            migrationBuilder.DropSequence(
                name: "formapagoid");

            migrationBuilder.DropSequence(
                name: "frecuenciapagoid");

            migrationBuilder.DropSequence(
                name: "garantiaid");

            migrationBuilder.DropSequence(
                name: "monedaid");

            migrationBuilder.DropSequence(
                name: "movimientoprestamoid");

            migrationBuilder.DropSequence(
                name: "operadortelefonoid");

            migrationBuilder.DropSequence(
                name: "paisid");

            migrationBuilder.DropSequence(
                name: "parametrossistemaid");

            migrationBuilder.DropSequence(
                name: "personaactividadid");

            migrationBuilder.DropSequence(
                name: "personaid");

            migrationBuilder.DropSequence(
                name: "personareferenciaid");

            migrationBuilder.DropSequence(
                name: "personatelefonosid");

            migrationBuilder.DropSequence(
                name: "prestamocomponenteid");

            migrationBuilder.DropSequence(
                name: "prestamogarantiaid");

            migrationBuilder.DropSequence(
                name: "prestamoid");

            migrationBuilder.DropSequence(
                name: "sexoid");

            migrationBuilder.DropSequence(
                name: "sucursalid");

            migrationBuilder.DropSequence(
                name: "tipocuentaid");

            migrationBuilder.DropSequence(
                name: "tipofondoid");

            migrationBuilder.DropSequence(
                name: "tipogarantiaid");

            migrationBuilder.DropSequence(
                name: "tipoidentificacionid");

            migrationBuilder.DropSequence(
                name: "transaccionid");

            migrationBuilder.DropSequence(
                name: "ubicaciongeograficaid");
        }
    }
}
