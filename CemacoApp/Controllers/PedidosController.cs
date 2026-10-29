using System.Data;
using CemacoApp.Data;
using CemacoApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
namespace CemacoApp.Controllers;
public sealed class PedidosController(Conexion conexion, ILogger<PedidosController> logger) : Controller
{
    public IActionResult Index()=>View();
    public async Task<IActionResult> Detalle(int id)
    {
        if(id<1)return NotFound();
        try
        {
            const string sql="SELECT p.id_pedido,p.fecha_pedido,p.fecha_estimada,p.direccion_envio,e.nombre,COALESCE(d.transportista,'Sin asignar'),COALESCE(ed.nombre,'Pendiente'),v.id_venta FROM Pedido p JOIN Estado_Pedido e ON e.id_estado_pedido=p.id_estado_pedido JOIN Venta v ON v.id_venta=p.id_venta LEFT JOIN Despacho d ON d.id_despacho=(SELECT MAX(id_despacho) FROM Despacho WHERE id_pedido=p.id_pedido) LEFT JOIN Estado_Despacho ed ON d.id_estado_despacho=ed.id_estado_despacho WHERE p.id_pedido=@id";
            await using var db=conexion.ObtenerConexion();await db.OpenAsync();await using var cmd=new SqlCommand(sql,db);
            cmd.Parameters.Add("@id",SqlDbType.Int).Value=id;await using var r=await cmd.ExecuteReaderAsync();
            if(!await r.ReadAsync())return NotFound();
            var pedido=new Pedido(r.GetInt32(0),r.GetDateTime(1),r.IsDBNull(2)?null:r.GetDateTime(2),r.IsDBNull(3)?"":r.GetString(3),r.GetString(4),r.GetString(5),r.GetString(6),r.GetInt32(7));
            return View(pedido);
        }
        catch(SqlException ex){logger.LogError(ex,"Error consultando pedido");TempData["Error"]="No se pudo consultar el pedido en SQL Server.";return RedirectToAction(nameof(Index));}
    }
}

