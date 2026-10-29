using System.ComponentModel.DataAnnotations;
namespace CemacoApp.ViewModels;
public record Producto(int Id, string Nombre, string Descripcion, decimal Precio, string Categoria);
public record Opcion(int Id, string Nombre);
public record LineaCarrito(Producto Producto, int Cantidad) { public decimal Subtotal => Producto.Precio * Cantidad; }
public record Pedido(int Id, DateTime Fecha, DateTime? Estimada, string Direccion, string Estado, string Transportista, string Despacho, int VentaId);
public record VentaReporte(int Venta, DateTime Fecha, string Cliente, string Tipo, string Producto, string Categoria, int Cantidad, decimal Precio, decimal Subtotal, int? Pedido, string Estado, string Despacho);
public record CategoriaReporte(string Categoria, string Estado, int Ventas);
public record ResultadoVenta(int VentaId, int PedidoId, decimal Total, string Comprobante);
public record ItemVenta(int ProductoId, int Cantidad);
public class CompraForm
{
    [Required, StringLength(50)] public string Nombre { get; set; } = "";
    [Required, StringLength(50)] public string Apellido { get; set; } = "";
    [StringLength(20)] public string? Nit { get; set; }
    [StringLength(20)] public string? Telefono { get; set; }
    [EmailAddress, StringLength(100)] public string? Correo { get; set; }
    [Required, StringLength(150)] public string Direccion { get; set; } = "";
    [StringLength(200)] public string? Observaciones { get; set; }
    [Range(1, int.MaxValue)] public int TipoVentaId { get; set; }
    [Range(1, int.MaxValue)] public int MetodoPagoId { get; set; }
    [Range(1, int.MaxValue)] public int TipoComprobanteId { get; set; }
}
public class CompraViewModel
{
    public CompraForm Form { get; set; } = new();
    public List<LineaCarrito> Lineas { get; set; } = [];
    public List<Opcion> TiposVenta { get; set; } = [];
    public List<Opcion> MetodosPago { get; set; } = [];
    public List<Opcion> TiposComprobante { get; set; } = [];
    public decimal Total => Lineas.Sum(x => x.Subtotal);
}
