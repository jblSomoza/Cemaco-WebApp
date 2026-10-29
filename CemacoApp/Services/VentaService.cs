using System.Data;
using CemacoApp.Data;
using CemacoApp.ViewModels;
using Microsoft.Data.SqlClient;
namespace CemacoApp.Services;
public sealed class VentaService(Conexion conexion)
{
    public async Task<ResultadoVenta> Registrar(CompraForm form, IReadOnlyList<ItemVenta> items)
    {
        if(items.Count is < 1 or > 50 || items.Any(x=>x.Cantidad is < 1 or > 999) || items.Select(x=>x.ProductoId).Distinct().Count()!=items.Count) throw new ArgumentException("El carrito contiene cantidades inválidas.");
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

}
