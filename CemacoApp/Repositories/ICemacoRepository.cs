using CemacoApp.ViewModels;
namespace CemacoApp.Repositories;
public interface ICemacoRepository
{
    Task<List<Producto>> Productos();
    Task<Producto?> Producto(int id);
    Task<List<Opcion>> Opciones(string tipo);
    Task<Pedido?> Pedido(int id);
    Task<List<VentaReporte>> Ventas();
    Task<List<CategoriaReporte>> Categorias();
    Task<ResultadoVenta> CrearVenta(CompraForm form, IReadOnlyList<ItemVenta> items);
    Task<bool> CambiarEstado(int pedidoId, int estadoId);
    Task<int?> RegistrarDespacho(int pedidoId, string transportista, int estadoId);
}
