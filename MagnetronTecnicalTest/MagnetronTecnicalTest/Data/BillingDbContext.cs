using Microsoft.EntityFrameworkCore;

namespace MagnetronTecnicalTest.Data;

public class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options) {}

    public DbSet<Persona> Personas => Set<Persona>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<FacturaEncabezado> Facturas => Set<FacturaEncabezado>();
    public DbSet<FacturaDetalle> FacturaDetalles => Set<FacturaDetalle>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    // Vistas (Keyless)
    public DbSet<VPersonaTotal> PersonaTotales => Set<VPersonaTotal>();
    public DbSet<VPersonaProductoMasCaro> PersonaProductoMasCaro => Set<VPersonaProductoMasCaro>();
    public DbSet<VProductoCantidad> ProductosPorCantidad => Set<VProductoCantidad>();
    public DbSet<VProductoUtilidad> ProductosPorUtilidad => Set<VProductoUtilidad>();
    public DbSet<VProductoMargen> ProductosMargen => Set<VProductoMargen>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // PERSONA
        modelBuilder.Entity<Persona>(e =>
        {
            e.ToTable("persona");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("per_id");
            e.Property(x => x.Nombre).HasColumnName("per_nombre").HasMaxLength(100).IsRequired();
            e.Property(x => x.Apellido).HasColumnName("per_apellido").HasMaxLength(100).IsRequired();
            e.Property(x => x.TipoDocumento).HasColumnName("per_tipodocumento").HasMaxLength(20).IsRequired();
            e.Property(x => x.Documento).HasColumnName("per_documento").HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Documento).HasDatabaseName("idx_persona_documento").IsUnique();
        });

        // PRODUCTO
        modelBuilder.Entity<Producto>(e =>
        {
            e.ToTable("producto");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("prod_id");
            e.Property(x => x.Descripcion).HasColumnName("prod_descripcion").HasMaxLength(200).IsRequired();
            e.Property(x => x.UnidadMedida).HasColumnName("prod_um").HasMaxLength(10).IsRequired();
            e.Property(x => x.Precio).HasColumnName("prod_precio").HasPrecision(18,2).IsRequired();
            e.Property(x => x.Costo).HasColumnName("prod_costo").HasPrecision(18,2).IsRequired();
            e.HasIndex(x => x.Descripcion).HasDatabaseName("idx_producto_descripcion");
        });

        // FACTURA ENCABEZADO
        modelBuilder.Entity<FacturaEncabezado>(e =>
        {
            e.ToTable("fact_encabezado");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("fenc_id");
            e.Property(x => x.Numero).HasColumnName("fenc_numero").HasMaxLength(50).IsRequired();
            e.Property(x => x.Fecha).HasColumnName("fenc_fecha").IsRequired();
            e.Property(x => x.PersonaId).HasColumnName("zper_id").IsRequired();
            e.HasIndex(x => x.PersonaId).HasDatabaseName("idx_fenc_zper");
            e.HasIndex(x => x.Fecha).HasDatabaseName("idx_fenc_fecha");
            e.HasIndex(x => x.Numero).IsUnique();
            e.HasOne(x => x.Persona).WithMany(x => x.Facturas).HasForeignKey(x => x.PersonaId);
        });

        // FACTURA DETALLE
        modelBuilder.Entity<FacturaDetalle>(e =>
        {
            e.ToTable("fact_detalle");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("fdet_id");
            e.Property(x => x.Linea).HasColumnName("fdet_linea").IsRequired();
            e.Property(x => x.Cantidad).HasColumnName("fdet_cantidad").HasPrecision(18,2).IsRequired();
            e.Property(x => x.ProductoId).HasColumnName("zprod_id").IsRequired();
            e.Property(x => x.FacturaId).HasColumnName("zfenc_id").IsRequired();
            e.Property(x => x.UnitPrice).HasColumnName("unit_price").HasPrecision(18,2).IsRequired();
            e.Property(x => x.LineTotal).HasColumnName("line_total").HasPrecision(18,2).ValueGeneratedOnAddOrUpdate();
            e.HasIndex(x => x.ProductoId).HasDatabaseName("idx_fdet_zprod");
            e.HasIndex(x => x.FacturaId).HasDatabaseName("idx_fdet_zfenc");
            e.HasOne(x => x.Factura).WithMany(x => x.Detalles).HasForeignKey(x => x.FacturaId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Producto).WithMany(x => x.Detalles).HasForeignKey(x => x.ProductoId);
            e.HasIndex(x => new { x.FacturaId, x.Linea }).IsUnique();
        });

        // USUARIO (para autenticación simple)
        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("usuario");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("usr_id");
            e.Property(x => x.Username).HasColumnName("usr_username").HasMaxLength(50).IsRequired();
            e.Property(x => x.PasswordHash).HasColumnName("usr_passwordhash").HasMaxLength(200).IsRequired();
            e.Property(x => x.Role).HasColumnName("usr_role").HasMaxLength(20).IsRequired();
            e.HasIndex(x => x.Username).IsUnique();
        });

        // VISTAS ------------------------------------------------------
        modelBuilder.Entity<VPersonaTotal>(e =>
        {
            e.ToView("v_persona_total");
            e.HasNoKey();
            e.Property(x => x.PersonaId).HasColumnName("per_id");
            e.Property(x => x.Nombre).HasColumnName("per_nombre");
            e.Property(x => x.Apellido).HasColumnName("per_apellido");
            e.Property(x => x.TotalFacturado).HasColumnName("total_facturado").HasPrecision(18,2);
        });

        modelBuilder.Entity<VPersonaProductoMasCaro>(e =>
        {
            e.ToView("v_persona_producto_mas_caro");
            e.HasNoKey();
            e.Property(x => x.PersonaId).HasColumnName("per_id");
            e.Property(x => x.Nombre).HasColumnName("per_nombre");
            e.Property(x => x.Apellido).HasColumnName("per_apellido");
            e.Property(x => x.ProductoId).HasColumnName("prod_id");
            e.Property(x => x.ProductoDescripcion).HasColumnName("prod_descripcion");
            e.Property(x => x.ProductoPrecio).HasColumnName("prod_precio").HasPrecision(18,2);
        });

        modelBuilder.Entity<VProductoCantidad>(e =>
        {
            e.ToView("v_productos_por_cantidad");
            e.HasNoKey();
            e.Property(x => x.ProductoId).HasColumnName("prod_id");
            e.Property(x => x.ProductoDescripcion).HasColumnName("prod_descripcion");
            e.Property(x => x.CantidadFacturada).HasColumnName("cantidad_facturada").HasPrecision(18,2);
        });

        modelBuilder.Entity<VProductoUtilidad>(e =>
        {
            e.ToView("v_productos_por_utilidad");
            e.HasNoKey();
            e.Property(x => x.ProductoId).HasColumnName("prod_id");
            e.Property(x => x.ProductoDescripcion).HasColumnName("prod_descripcion");
            e.Property(x => x.UtilidadTotal).HasColumnName("utilidad_total").HasPrecision(18,2);
        });

        modelBuilder.Entity<VProductoMargen>(e =>
        {
            e.ToView("v_productos_margen");
            e.HasNoKey();
            e.Property(x => x.ProductoId).HasColumnName("prod_id");
            e.Property(x => x.ProductoDescripcion).HasColumnName("prod_descripcion");
            e.Property(x => x.Ingresos).HasColumnName("ingresos").HasPrecision(18,2);
            e.Property(x => x.Utilidad).HasColumnName("utilidad").HasPrecision(18,2);
            e.Property(x => x.Margen).HasColumnName("margen").HasPrecision(18,4);
        });
    }
}

public class Persona
{
    public long Id { get; set; }
    public string Nombre { get; set; } = null!;
    public string Apellido { get; set; } = null!;
    public string TipoDocumento { get; set; } = null!;
    public string Documento { get; set; } = null!;
    public ICollection<FacturaEncabezado> Facturas { get; set; } = new List<FacturaEncabezado>();
}

public class Producto
{
    public long Id { get; set; }
    public string Descripcion { get; set; } = null!;
    public string UnidadMedida { get; set; } = null!;
    public decimal Precio { get; set; }
    public decimal Costo { get; set; }
    public ICollection<FacturaDetalle> Detalles { get; set; } = new List<FacturaDetalle>();
}

public class FacturaEncabezado
{
    public long Id { get; set; }
    public string Numero { get; set; } = null!;
    public DateTime Fecha { get; set; }
    public long PersonaId { get; set; }
    public Persona? Persona { get; set; }
    public ICollection<FacturaDetalle> Detalles { get; set; } = new List<FacturaDetalle>();
}

public class FacturaDetalle
{
    public long Id { get; set; }
    public int Linea { get; set; }
    public decimal Cantidad { get; set; }
    public long ProductoId { get; set; }
    public long FacturaId { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public FacturaEncabezado? Factura { get; set; }
    public Producto? Producto { get; set; }
}

public class Usuario
{
    public long Id { get; set; }
    public string Username { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Role { get; set; } = "user";
}

// Keyless entities for views
public class VPersonaTotal
{
    public long PersonaId { get; set; }
    public string Nombre { get; set; } = null!;
    public string Apellido { get; set; } = null!;
    public decimal TotalFacturado { get; set; }
}

public class VPersonaProductoMasCaro
{
    public long PersonaId { get; set; }
    public string Nombre { get; set; } = null!;
    public string Apellido { get; set; } = null!;
    public long ProductoId { get; set; }
    public string ProductoDescripcion { get; set; } = null!;
    public decimal ProductoPrecio { get; set; }
}

public class VProductoCantidad
{
    public long ProductoId { get; set; }
    public string ProductoDescripcion { get; set; } = null!;
    public decimal CantidadFacturada { get; set; }
}

public class VProductoUtilidad
{
    public long ProductoId { get; set; }
    public string ProductoDescripcion { get; set; } = null!;
    public decimal UtilidadTotal { get; set; }
}

public class VProductoMargen
{
    public long ProductoId { get; set; }
    public string ProductoDescripcion { get; set; } = null!;
    public decimal Ingresos { get; set; }
    public decimal Utilidad { get; set; }
    public decimal? Margen { get; set; }
}
