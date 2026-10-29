using System.Data;
using CemacoApp.Data;
using CemacoApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
namespace CemacoApp.Controllers;
public sealed class AdminController(Conexion conexion, ILogger<AdminController> logger) : Controller
{
    public IActionResult Index()=>View();
    public async Task<IActionResult> Ventas()
    {
        try
        {
            const string sql="SELECT v.id_venta,v.fecha,c.nombre+' '+c.apellido,tv.nombre,p.nombre,cat.nombre,dv.cantidad,dv.precio_unitario,dv.cantidad*dv.precio_unitario,pe.id_pedido,ep.nombre,COALESCE(ed.nombre,'Sin despacho') FROM Venta v JOIN Cliente c ON c.id_cliente=v.id_cliente JOIN Tipo_Venta tv ON tv.id_tipo_venta=v.id_tipo_venta JOIN Detalle_Venta dv ON dv.id_venta=v.id_venta JOIN Producto p ON p.id_producto=dv.id_producto JOIN Categoria cat ON cat.id_categoria=p.id_categoria LEFT JOIN Pedido pe ON pe.id_venta=v.id_venta LEFT JOIN Estado_Pedido ep ON ep.id_estado_pedido=pe.id_estado_pedido LEFT JOIN Despacho des ON des.id_despacho=(SELECT MAX(id_despacho) FROM Despacho WHERE id_pedido=pe.id_pedido) LEFT JOIN Estado_Despacho ed ON ed.id_estado_despacho=des.id_estado_despacho ORDER BY v.id_venta DESC";
            await using var db=conexion.ObtenerConexion();await db.OpenAsync();await using var cmd=new SqlCommand(sql,db);await using var r=await cmd.ExecuteReaderAsync();
            var rows=new List<VentaReporte>();while(await r.ReadAsync())rows.Add(new(r.GetInt32(0),r.GetDateTime(1),r.GetString(2),r.GetString(3),r.GetString(4),r.GetString(5),r.GetInt32(6),r.GetDecimal(7),r.GetDecimal(8),r.IsDBNull(9)?null:r.GetInt32(9),r.IsDBNull(10)?"Sin pedido":r.GetString(10),r.GetString(11)));
            return View(rows);
        }
        catch(SqlException ex){logger.LogError(ex,"Error cargando ventas");ViewBag.Error="No se pudo consultar SQL Server.";return View(new List<VentaReporte>());}
    }
    public async Task<IActionResult> Categorias()
    {
        try
        {
            const string sql="SELECT cat.nombre,ep.nombre,COUNT(DISTINCT v.id_venta) FROM Venta v JOIN Detalle_Venta dv ON dv.id_venta=v.id_venta JOIN Producto p ON p.id_producto=dv.id_producto JOIN Categoria cat ON cat.id_categoria=p.id_categoria JOIN Pedido pe ON pe.id_venta=v.id_venta JOIN Estado_Pedido ep ON ep.id_estado_pedido=pe.id_estado_pedido GROUP BY cat.nombre,ep.nombre ORDER BY cat.nombre,ep.nombre";
            await using var db=conexion.ObtenerConexion();await db.OpenAsync();await using var cmd=new SqlCommand(sql,db);await using var r=await cmd.ExecuteReaderAsync();
            var rows=new List<CategoriaReporte>();while(await r.ReadAsync())rows.Add(new(r.GetString(0),r.GetString(1),r.GetInt32(2)));return View(rows);
        }
        catch(SqlException ex){logger.LogError(ex,"Error cargando categorías");ViewBag.Error="No se pudo consultar SQL Server.";return View(new List<CategoriaReporte>());}
    }
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Estado(int pedidoId,int estadoId)
    {
        if(pedidoId<1||estadoId is <1 or >3)TempData["Error"]="Datos inválidos.";
        else try
        {
            await using var db=conexion.ObtenerConexion();await db.OpenAsync();await using var cmd=new SqlCommand("UPDATE Pedido SET id_estado_pedido=@e WHERE id_pedido=@id",db);
            cmd.Parameters.Add("@e",SqlDbType.Int).Value=estadoId;cmd.Parameters.Add("@id",SqlDbType.Int).Value=pedidoId;
            var actualizado=await cmd.ExecuteNonQueryAsync()>0;
            if(!actualizado)return NotFound();
            TempData["Mensaje"]="Estado actualizado.";
        }
        catch(SqlException ex){logger.LogError(ex,"Error cambiando estado");TempData["Error"]="No se pudo actualizar el pedido.";}
        return RedirectToAction(nameof(Index));
    }
    [HttpPost,ValidateAntiForgeryToken]
    public async Task<IActionResult> Despacho(int pedidoId,string transportista,int estadoId)
    {
        if(pedidoId<1||estadoId is <1 or >3||string.IsNullOrWhiteSpace(transportista)||transportista.Length>100)TempData["Error"]="Datos inválidos.";
        else try
        {
            await using var db=conexion.ObtenerConexion();await db.OpenAsync();
            await using var cmd=new SqlCommand("INSERT INTO Despacho(fecha_despacho,hora,transportista,id_pedido,id_estado_despacho) OUTPUT INSERTED.id_despacho VALUES(CONVERT(date,GETDATE()),CONVERT(time,GETDATE()),@t,@id,@e)",db);
            cmd.Parameters.Add("@t",SqlDbType.VarChar,100).Value=transportista.Trim();cmd.Parameters.Add("@id",SqlDbType.Int).Value=pedidoId;cmd.Parameters.Add("@e",SqlDbType.Int).Value=estadoId;
            var despachoId=Convert.ToInt32(await cmd.ExecuteScalarAsync());TempData["Mensaje"]=$"Despacho #{despachoId} registrado.";
        }
        catch(SqlException ex){logger.LogError(ex,"Error registrando despacho");TempData["Error"]="No se pudo registrar el despacho. Verifica que el pedido exista.";}
        return RedirectToAction(nameof(Index));
    }
}

