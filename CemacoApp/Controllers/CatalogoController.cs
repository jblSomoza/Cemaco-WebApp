using CemacoApp.Data;
using CemacoApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
namespace CemacoApp.Controllers;
public sealed class CatalogoController(Conexion conexion, ILogger<CatalogoController> logger) : Controller
{
    public async Task<IActionResult> Index(string? buscar, string? categoria)
    {
        try
        {
            await using var db=conexion.ObtenerConexion(); await db.OpenAsync();
            const string sql="SELECT p.id_producto,p.nombre,p.descripcion,p.precio,c.nombre FROM Producto p JOIN Categoria c ON c.id_categoria=p.id_categoria ORDER BY p.id_producto";
            await using var cmd=new SqlCommand(sql,db); await using var reader=await cmd.ExecuteReaderAsync();
            var productos=new List<Producto>();
            while(await reader.ReadAsync()) productos.Add(new(reader.GetInt32(0),reader.GetString(1),reader.IsDBNull(2)?"":reader.GetString(2),reader.GetDecimal(3),reader.GetString(4)));
            ViewBag.Categorias=productos.Select(p=>p.Categoria).Distinct().OrderBy(c=>c).ToList();
            ViewBag.Buscar=buscar;ViewBag.Categoria=categoria;
            if(!string.IsNullOrWhiteSpace(buscar)) productos=productos.Where(p=>(p.Nombre+" "+p.Descripcion+" "+p.Categoria).Contains(buscar,StringComparison.OrdinalIgnoreCase)).ToList();
            if(!string.IsNullOrWhiteSpace(categoria)) productos=productos.Where(p=>p.Categoria==categoria).ToList();
            return View(productos);
        }
        catch(SqlException ex)
        {
            logger.LogError(ex,"Error cargando catálogo");
            ViewBag.Error="No se pudo consultar la base Cemaco. Revisa la instancia y ejecuta el script SQL.";
            return View(new List<Producto>());
        }
    }
}
