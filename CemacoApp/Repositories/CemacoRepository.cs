using System.Data;
using CemacoApp.Data;
using CemacoApp.ViewModels;
using Microsoft.Data.SqlClient;
namespace CemacoApp.Repositories;
public sealed class CemacoRepository(Conexion conexion) : ICemacoRepository
{
    public async Task<List<Producto>> Productos()
    {
        await using var db = conexion.ObtenerConexion(); await db.OpenAsync();
        const string sql = "SELECT p.id_producto,p.nombre,p.descripcion,p.precio,c.nombre FROM Producto p JOIN Categoria c ON c.id_categoria=p.id_categoria ORDER BY p.id_producto";
        await using var cmd = new SqlCommand(sql, db); await using var r = await cmd.ExecuteReaderAsync();
        var rows = new List<Producto>();
        while (await r.ReadAsync()) rows.Add(new(r.GetInt32(0),r.GetString(1),r.IsDBNull(2)?"":r.GetString(2),r.GetDecimal(3),r.GetString(4)));
        return rows;
    public async Task<Producto?> Producto(int id) => (await Productos()).FirstOrDefault(p => p.Id == id);
    }
    public async Task<List<Opcion>> Opciones(string tipo)
    {
        var sql = tipo switch {
            "venta" => "SELECT id_tipo_venta,nombre FROM Tipo_Venta ORDER BY id_tipo_venta",
            "pago" => "SELECT id_metodo_pago,nombre FROM Metodo_Pago ORDER BY id_metodo_pago",
            "comprobante" => "SELECT id_tipo_comprobante,nombre FROM Tipo_Comprobante ORDER BY id_tipo_comprobante",
            _ => throw new ArgumentOutOfRangeException(nameof(tipo))
        };
        await using var db = conexion.ObtenerConexion(); await db.OpenAsync();
        await using var cmd = new SqlCommand(sql, db); await using var r = await cmd.ExecuteReaderAsync();
        var rows = new List<Opcion>(); while (await r.ReadAsync()) rows.Add(new(r.GetInt32(0),r.GetString(1))); return rows;
    }
    public async Task<Pedido?> Pedido(int id)
    {
        const string sql = "SELECT p.id_pedido,p.fecha_pedido,p.fecha_estimada,p.direccion_envio,e.nombre,COALESCE(d.transportista,'Sin asignar'),COALESCE(ed.nombre,'Pendiente'),v.id_venta FROM Pedido p JOIN Estado_Pedido e ON e.id_estado_pedido=p.id_estado_pedido JOIN Venta v ON v.id_venta=p.id_venta LEFT JOIN Despacho d ON d.id_despacho=(SELECT MAX(id_despacho) FROM Despacho WHERE id_pedido=p.id_pedido) LEFT JOIN Estado_Despacho ed ON d.id_estado_despacho=ed.id_estado_despacho WHERE p.id_pedido=@id";
        await using var db = conexion.ObtenerConexion(); await db.OpenAsync(); await using var cmd = new SqlCommand(sql, db);
        cmd.Parameters.Add("@id",SqlDbType.Int).Value=id; await using var r = await cmd.ExecuteReaderAsync();
        return await r.ReadAsync() ? new(r.GetInt32(0),r.GetDateTime(1),r.IsDBNull(2)?null:r.GetDateTime(2),r.IsDBNull(3)?"":r.GetString(3),r.GetString(4),r.GetString(5),r.GetString(6),r.GetInt32(7)) : null;
    }
    public async Task<List<VentaReporte>> Ventas()
    {
        const string sql = "SELECT v.id_venta,v.fecha,c.nombre+' '+c.apellido,tv.nombre,p.nombre,cat.nombre,dv.cantidad,dv.precio_unitario,dv.cantidad*dv.precio_unitario,pe.id_pedido,ep.nombre,COALESCE(ed.nombre,'Sin despacho') FROM Venta v JOIN Cliente c ON c.id_cliente=v.id_cliente JOIN Tipo_Venta tv ON tv.id_tipo_venta=v.id_tipo_venta JOIN Detalle_Venta dv ON dv.id_venta=v.id_venta JOIN Producto p ON p.id_producto=dv.id_producto JOIN Categoria cat ON cat.id_categoria=p.id_categoria LEFT JOIN Pedido pe ON pe.id_venta=v.id_venta LEFT JOIN Estado_Pedido ep ON ep.id_estado_pedido=pe.id_estado_pedido LEFT JOIN Despacho des ON des.id_despacho=(SELECT MAX(id_despacho) FROM Despacho WHERE id_pedido=pe.id_pedido) LEFT JOIN Estado_Despacho ed ON ed.id_estado_despacho=des.id_estado_despacho ORDER BY v.id_venta DESC";
        await using var db=conexion.ObtenerConexion(); await db.OpenAsync(); await using var cmd=new SqlCommand(sql,db); await using var r=await cmd.ExecuteReaderAsync();
        var rows=new List<VentaReporte>(); while(await r.ReadAsync()) rows.Add(new(r.GetInt32(0),r.GetDateTime(1),r.GetString(2),r.GetString(3),r.GetString(4),r.GetString(5),r.GetInt32(6),r.GetDecimal(7),r.GetDecimal(8),r.IsDBNull(9)?null:r.GetInt32(9),r.IsDBNull(10)?"Sin pedido":r.GetString(10),r.GetString(11))); return rows;
    }
    public async Task<List<CategoriaReporte>> Categorias()
    {
        const string sql="SELECT cat.nombre,ep.nombre,COUNT(DISTINCT v.id_venta) FROM Venta v JOIN Detalle_Venta dv ON dv.id_venta=v.id_venta JOIN Producto p ON p.id_producto=dv.id_producto JOIN Categoria cat ON cat.id_categoria=p.id_categoria JOIN Pedido pe ON pe.id_venta=v.id_venta JOIN Estado_Pedido ep ON ep.id_estado_pedido=pe.id_estado_pedido GROUP BY cat.nombre,ep.nombre ORDER BY cat.nombre,ep.nombre";
        await using var db=conexion.ObtenerConexion(); await db.OpenAsync(); await using var cmd=new SqlCommand(sql,db); await using var r=await cmd.ExecuteReaderAsync();
        var rows=new List<CategoriaReporte>(); while(await r.ReadAsync()) rows.Add(new(r.GetString(0),r.GetString(1),r.GetInt32(2))); return rows;
    }
    public async Task<ResultadoVenta> CrearVenta(CompraForm form, IReadOnlyList<ItemVenta> items)
    {
        await using var db=conexion.ObtenerConexion(); await db.OpenAsync(); await using var tx=(SqlTransaction)await db.BeginTransactionAsync();
        try {
            async Task<int> Id(string sql, params (string,object?)[] ps) { await using var c=new SqlCommand(sql,db,tx); foreach(var (k,v) in ps)c.Parameters.AddWithValue(k,v??DBNull.Value); return Convert.ToInt32(await c.ExecuteScalarAsync()); }
            async Task Write(string sql,params (string,object?)[] ps) { await using var c=new SqlCommand(sql,db,tx); foreach(var(k,v) in ps)c.Parameters.AddWithValue(k,v??DBNull.Value); await c.ExecuteNonQueryAsync(); }
            var cliente=await Id("INSERT INTO Cliente(nombre,apellido,nit,direccion,telefono,correo) OUTPUT INSERTED.id_cliente VALUES(@n,@a,@nit,@d,@t,@c)",("@n",form.Nombre.Trim()),("@a",form.Apellido.Trim()),("@nit",form.Nit),("@d",form.Direccion.Trim()),("@t",form.Telefono),("@c",form.Correo));
            var venta=await Id("INSERT INTO Venta(fecha,id_cliente,id_tipo_venta) OUTPUT INSERTED.id_venta VALUES(CONVERT(date,GETDATE()),@c,@t)",("@c",cliente),("@t",form.TipoVentaId));
            decimal total=0;
            foreach(var item in items) {
                await using var cmd=new SqlCommand("SELECT precio FROM Producto WHERE id_producto=@id",db,tx); cmd.Parameters.Add("@id",SqlDbType.Int).Value=item.ProductoId;
                var value=await cmd.ExecuteScalarAsync(); if(value is null)throw new ArgumentException("Un producto ya no existe.");
                var precio=Convert.ToDecimal(value); total+=precio*item.Cantidad;
                await Write("INSERT INTO Detalle_Venta(cantidad,precio_unitario,id_venta,id_producto) VALUES(@q,@p,@v,@i)",("@q",item.Cantidad),("@p",precio),("@v",venta),("@i",item.ProductoId));
            }
            if(total<=0)throw new ArgumentException("El total debe ser mayor que cero.");
            var comprobante=await Id("INSERT INTO Comprobante(numero,fecha_emision,detalle,id_venta,id_tipo_comprobante) OUTPUT INSERTED.id_comprobante VALUES(CONCAT('FAC-',@v),CONVERT(date,GETDATE()),'Compra web',@v,@t)",("@v",venta),("@t",form.TipoComprobanteId));
            await Write("INSERT INTO Pago(total,id_comprobante,id_metodo_pago) VALUES(@total,@c,@m)",("@total",total),("@c",comprobante),("@m",form.MetodoPagoId));
            var pedido=await Id("INSERT INTO Pedido(fecha_pedido,fecha_estimada,direccion_envio,observaciones,id_venta,id_estado_pedido) OUTPUT INSERTED.id_pedido VALUES(CONVERT(date,GETDATE()),DATEADD(day,3,CONVERT(date,GETDATE())),@d,@o,@v,1)",("@d",form.Direccion.Trim()),("@o",form.Observaciones),("@v",venta));
            await tx.CommitAsync(); return new(venta,pedido,total,"FAC-"+venta);
        } catch { await tx.RollbackAsync(); throw; }
    }
    public async Task<bool> CambiarEstado(int pedidoId,int estadoId)
    {
        await using var db=conexion.ObtenerConexion(); await db.OpenAsync(); await using var cmd=new SqlCommand("UPDATE Pedido SET id_estado_pedido=@e WHERE id_pedido=@id",db);
        cmd.Parameters.Add("@e",SqlDbType.Int).Value=estadoId;cmd.Parameters.Add("@id",SqlDbType.Int).Value=pedidoId; return await cmd.ExecuteNonQueryAsync()>0;
    }
    public async Task<int?> RegistrarDespacho(int pedidoId,string transportista,int estadoId)
    {
        await using var db=conexion.ObtenerConexion();await db.OpenAsync();await using var cmd=new SqlCommand("INSERT INTO Despacho(fecha_despacho,hora,transportista,id_pedido,id_estado_despacho) OUTPUT INSERTED.id_despacho VALUES(CONVERT(date,GETDATE()),CONVERT(time,GETDATE()),@t,@id,@e)",db);
        cmd.Parameters.Add("@t",SqlDbType.VarChar,100).Value=transportista.Trim();cmd.Parameters.Add("@id",SqlDbType.Int).Value=pedidoId;cmd.Parameters.Add("@e",SqlDbType.Int).Value=estadoId;
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }
}
