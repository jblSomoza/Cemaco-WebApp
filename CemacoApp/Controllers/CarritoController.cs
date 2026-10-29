using System.Data;
using CemacoApp.Data;
using CemacoApp.Services;
using CemacoApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
namespace CemacoApp.Controllers;
public sealed class CarritoController(Conexion conexion,VentaService venta,ILogger<CarritoController> logger) : Controller
{
    private async Task<CompraViewModel> Model(CompraForm? form=null)
    {
        await using var db=conexion.ObtenerConexion();await db.OpenAsync();
        var productos=new List<Producto>();
        await using(var cmd=new SqlCommand("SELECT p.id_producto,p.nombre,p.descripcion,p.precio,c.nombre FROM Producto p JOIN Categoria c ON c.id_categoria=p.id_categoria ORDER BY p.id_producto",db))
        await using(var r=await cmd.ExecuteReaderAsync()) while(await r.ReadAsync()) productos.Add(new(r.GetInt32(0),r.GetString(1),r.IsDBNull(2)?"":r.GetString(2),r.GetDecimal(3),r.GetString(4)));
        async Task<List<Opcion>> Opciones(string sql)
        {
            await using var cmd=new SqlCommand(sql,db);await using var r=await cmd.ExecuteReaderAsync();var rows=new List<Opcion>();
            while(await r.ReadAsync())rows.Add(new(r.GetInt32(0),r.GetString(1)));return rows;
        }
        var items=CarritoSession.Leer(HttpContext.Session);
        return new CompraViewModel { Form=form??new(),Lineas=items.Join(productos,i=>i.ProductoId,p=>p.Id,(i,p)=>new LineaCarrito(p,i.Cantidad)).ToList(),TiposVenta=await Opciones("SELECT id_tipo_venta,nombre FROM Tipo_Venta ORDER BY id_tipo_venta"),MetodosPago=await Opciones("SELECT id_metodo_pago,nombre FROM Metodo_Pago ORDER BY id_metodo_pago"),TiposComprobante=await Opciones("SELECT id_tipo_comprobante,nombre FROM Tipo_Comprobante ORDER BY id_tipo_comprobante") };
    }
    public async Task<IActionResult> Index()
    {
        try{return View(await Model());}
        catch(SqlException ex){logger.LogError(ex,"Error cargando carrito");TempData["Error"]="No se pudo conectar a SQL Server.";return RedirectToAction("Index","Catalogo");}
    }
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Agregar(int productoId)
    {
        if(productoId<1)return NotFound();
        await using var db=conexion.ObtenerConexion();await db.OpenAsync();await using var cmd=new SqlCommand("SELECT COUNT(1) FROM Producto WHERE id_producto=@id",db);
        cmd.Parameters.Add("@id",SqlDbType.Int).Value=productoId;if(Convert.ToInt32(await cmd.ExecuteScalarAsync())==0)return NotFound();
        var items=CarritoSession.Leer(HttpContext.Session);var index=items.FindIndex(x=>x.ProductoId==productoId);
        if(index>=0){if(items[index].Cantidad<999)items[index]=items[index] with {Cantidad=items[index].Cantidad+1};}
        else if(items.Count<50)items.Add(new(productoId,1));
        CarritoSession.Guardar(HttpContext.Session,items);return RedirectToAction(nameof(Index));
    }
    [HttpPost,ValidateAntiForgeryToken]
    public IActionResult Actualizar(int productoId,int cantidad)
    {
        var items=CarritoSession.Leer(HttpContext.Session);var index=items.FindIndex(x=>x.ProductoId==productoId);
        if(index<0)return NotFound();
        if(cantidad<=0)items.RemoveAt(index);else items[index]=items[index] with {Cantidad=Math.Min(cantidad,999)};
        CarritoSession.Guardar(HttpContext.Session,items);return RedirectToAction(nameof(Index));
    }
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Comprar(CompraForm form)
    {
        if(!ModelState.IsValid)return View("Index",await Model(form));
        try{var result=await venta.Registrar(form,CarritoSession.Leer(HttpContext.Session));CarritoSession.Limpiar(HttpContext.Session);return View("Confirmacion",result);}
        catch(ArgumentException ex){ModelState.AddModelError("",ex.Message);return View("Index",await Model(form));}
        catch(SqlException ex){logger.LogError(ex,"Error registrando compra");ModelState.AddModelError("","No se pudo guardar la compra. Revisa los datos y la conexión.");return View("Index",await Model(form));}
    }
}


