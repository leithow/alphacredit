using Microsoft.EntityFrameworkCore;
using AlphaCredit.Api.Models;

namespace AlphaCredit.Api.Data;

public class AlphaCreditDbContext : DbContext
{
    public AlphaCreditDbContext(DbContextOptions<AlphaCreditDbContext> options) : base(options)
    {
    }

    // Catálogos básicos
    public DbSet<Sexo> Sexos { get; set; }
    public DbSet<EstadoCivil> EstadosCiviles { get; set; }
    public DbSet<TipoIdentificacion> TiposIdentificacion { get; set; }
    public DbSet<OperadorTelefono> OperadoresTelefono { get; set; }
    public DbSet<Moneda> Monedas { get; set; }
    public DbSet<FrecuenciaPago> FrecuenciasPago { get; set; }
    public DbSet<FormaPago> FormasPago { get; set; }
    public DbSet<DestinoCredito> DestinosCredito { get; set; }
    public DbSet<EstadoPrestamo> EstadosPrestamo { get; set; }
    public DbSet<TipoGarantia> TiposGarantia { get; set; }
    public DbSet<EstadoGarantia> EstadosGarantia { get; set; }
    public DbSet<ComponentePrestamo> ComponentesPrestamo { get; set; }
    public DbSet<EstadoComponente> EstadosComponente { get; set; }
    public DbSet<TipoCuenta> TiposCuenta { get; set; }
    public DbSet<TipoFondo> TiposFondo { get; set; }

    // Entidades geográficas
    public DbSet<Pais> Paises { get; set; }
    public DbSet<UbicacionGeografica> UbicacionesGeograficas { get; set; }

    // Actividades económicas
    public DbSet<ActividadCnbs> ActividadesCnbs { get; set; }
    public DbSet<ActividadEconomica> ActividadesEconomicas { get; set; }

    // Personas
    public DbSet<Persona> Personas { get; set; }
    public DbSet<PersonaTelefono> PersonaTelefonos { get; set; }
    public DbSet<PersonaActividad> PersonaActividades { get; set; }
    public DbSet<PersonaReferencia> PersonaReferencias { get; set; }
    public DbSet<PersonaConyuge> PersonasConyuges { get; set; }

    // Organizaciones
    public DbSet<Empresa> Empresas { get; set; }
    public DbSet<Sucursal> Sucursales { get; set; }
    public DbSet<Banco> Bancos { get; set; }

    // Entidades bancarias
    public DbSet<CuentaBancaria> CuentasBancarias { get; set; }
    public DbSet<Transaccion> Transacciones { get; set; }

    // Préstamos
    public DbSet<Prestamo> Prestamos { get; set; }
    public DbSet<PrestamoComponente> PrestamosComponentes { get; set; }
    public DbSet<MovimientoPrestamo> MovimientosPrestamo { get; set; }

    // Garantías
    public DbSet<Garantia> Garantias { get; set; }
    public DbSet<PrestamoGarantia> PrestamosGarantias { get; set; }

    // Fondos
    public DbSet<Fondo> Fondos { get; set; }
    public DbSet<FondoMovimiento> FondosMovimientos { get; set; }

    // Sistema
    public DbSet<ParametrosSistema> ParametrosSistema { get; set; }
    public DbSet<FechaSistema> FechasSistema { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuración de índices únicos
        modelBuilder.Entity<Persona>()
            .HasIndex(p => p.PersonaIdentificacion)
            .IsUnique()
            .HasDatabaseName("upersona");

        modelBuilder.Entity<ActividadCnbs>()
            .HasIndex(a => a.ActividadCnbsCodigo)
            .IsUnique()
            .HasDatabaseName("uactividadcnbs");

        modelBuilder.Entity<FechaSistema>()
            .HasIndex(f => f.FechaSistemaFecha)
            .IsUnique()
            .HasDatabaseName("ufechasistemaf");

        // Configuración de relaciones auto-referenciadas
        modelBuilder.Entity<UbicacionGeografica>()
            .HasOne(u => u.UbicacionPadre)
            .WithMany(u => u.UbicacionesHijas)
            .HasForeignKey(u => u.UbicacionGeograficaPadre)
            .OnDelete(DeleteBehavior.NoAction);

        // Configuración de relaciones de FondoMovimiento
        modelBuilder.Entity<FondoMovimiento>()
            .HasOne(fm => fm.Fondo)
            .WithMany(f => f.FondoMovimientos)
            .HasForeignKey(fm => fm.FondoId)
            .OnDelete(DeleteBehavior.NoAction);

        // Configuración para evitar ciclos en las relaciones
        modelBuilder.Entity<Prestamo>()
            .HasOne(p => p.Persona)
            .WithMany(per => per.Prestamos)
            .HasForeignKey(p => p.PersonaId)
            .OnDelete(DeleteBehavior.NoAction);

        modelBuilder.Entity<Garantia>()
            .HasOne(g => g.Persona)
            .WithMany(p => p.Garantias)
            .HasForeignKey(g => g.PersonaId)
            .OnDelete(DeleteBehavior.NoAction);

        // Configuración de secuencias (si PostgreSQL usa secuencias)
        modelBuilder.HasSequence<long>("actividadcnbsid");
        modelBuilder.HasSequence<long>("actividadeconomicaid");
        modelBuilder.HasSequence<long>("bancoid");
        modelBuilder.HasSequence<long>("componenteprestamoid");
        modelBuilder.HasSequence<long>("cuentabancariaid");
        modelBuilder.HasSequence<long>("destinocreditoid");
        modelBuilder.HasSequence<long>("empresaid");
        modelBuilder.HasSequence<long>("estadocivilid");
        modelBuilder.HasSequence<long>("estadocomponenteid");
        modelBuilder.HasSequence<long>("estadogarantiaid");
        modelBuilder.HasSequence<long>("estadoprestamoid");
        modelBuilder.HasSequence<long>("fechasistemaid");
        modelBuilder.HasSequence<long>("fondoid");
        modelBuilder.HasSequence<long>("fondomovimientoid");
        modelBuilder.HasSequence<long>("formapagoid");
        modelBuilder.HasSequence<long>("frecuenciapagoid");
        modelBuilder.HasSequence<long>("garantiaid");
        modelBuilder.HasSequence<long>("monedaid");
        modelBuilder.HasSequence<long>("movimientoprestamoid");
        modelBuilder.HasSequence<long>("operadortelefonoid");
        modelBuilder.HasSequence<long>("paisid");
        modelBuilder.HasSequence<long>("parametrossistemaid");
        modelBuilder.HasSequence<long>("personaid");
        modelBuilder.HasSequence<long>("personaactividadid");
        modelBuilder.HasSequence<long>("personareferenciaid");
        modelBuilder.HasSequence<long>("personatelefonosid");
        modelBuilder.HasSequence<long>("personaconyugeid");
        modelBuilder.HasSequence<long>("prestamoid");
        modelBuilder.HasSequence<long>("prestamocomponenteid");
        modelBuilder.HasSequence<long>("prestamogarantiaid");
        modelBuilder.HasSequence<long>("sexoid");
        modelBuilder.HasSequence<long>("sucursalid");
        modelBuilder.HasSequence<long>("tipocuentaid");
        modelBuilder.HasSequence<long>("tipofondoid");
        modelBuilder.HasSequence<long>("tipogarantiaid");
        modelBuilder.HasSequence<long>("tipoidentificacionid");
        modelBuilder.HasSequence<long>("transaccionid");
        modelBuilder.HasSequence<long>("ubicaciongeograficaid");
    }
}
