USE Cemaco;
GO

/* =========================================================
   1. CLIENTES
   ========================================================= */

INSERT INTO Cliente
(nombre, apellido, nit, direccion, telefono, correo)
VALUES
('Juan', 'Perez', '1234567-8', 'Zona 1, Guatemala', '55551234', 'juan@gmail.com'),
('Maria', 'Lopez', '8765432-1', 'Zona 10, Guatemala', '55556789', 'maria@gmail.com'),
('Carlos', 'Ramirez', '4567891-2', 'Mixco, Guatemala', '55559876', 'carlos@gmail.com');
GO


/* =========================================================
   2. CATEGORIAS
   ========================================================= */

INSERT INTO Categoria
(nombre, descripcion)
VALUES
('Herramientas', 'Productos para construccion y reparacion'),
('Hogar', 'Articulos para el hogar'),
('Jardineria', 'Productos para jardin');
GO


/* =========================================================
   3. PRODUCTOS
   ========================================================= */

INSERT INTO Producto
(nombre, descripcion, precio, id_categoria)
VALUES
('Martillo', 'Martillo de acero', 75.00, 1),
('Sarten', 'Sarten antiadherente', 150.00, 2),
('Manguera', 'Manguera para jardin', 125.00, 3);
GO


/* =========================================================
   4. TIPOS DE VENTA
   ========================================================= */

INSERT INTO Tipo_Venta
(nombre, descripcion)
VALUES
('Contado', 'Venta pagada al momento'),
('Credito', 'Venta con pago posterior'),
('En linea', 'Venta realizada por medio digital');
GO


/* =========================================================
   5. VENTAS
   ========================================================= */

INSERT INTO Venta
(fecha, id_cliente, id_tipo_venta)
VALUES
('2026-10-01', 1, 1),
('2026-10-02', 2, 1),
('2026-10-03', 3, 2);
GO


/* =========================================================
   6. DETALLE DE VENTA
   ========================================================= */

INSERT INTO Detalle_Venta
(cantidad, precio_unitario, id_venta, id_producto)
VALUES
(2, 75.00, 1, 1),
(1, 150.00, 2, 2),
(2, 125.00, 3, 3);
GO


/* =========================================================
   7. TIPOS DE COMPROBANTE
   ========================================================= */

INSERT INTO Tipo_Comprobante
(nombre, descripcion)
VALUES
('Factura', 'Documento fiscal de venta'),
('Recibo', 'Comprobante simple de pago'),
('Nota de credito', 'Documento para ajuste de una venta');
GO


/* =========================================================
   8. COMPROBANTES
   ========================================================= */

INSERT INTO Comprobante
(numero, fecha_emision, detalle, id_venta, id_tipo_comprobante)
VALUES
('FAC-001', '2026-10-01', 'Compra de herramientas', 1, 1),
('FAC-002', '2026-10-02', 'Compra de articulos para el hogar', 2, 1),
('FAC-003', '2026-10-03', 'Compra de productos de jardineria', 3, 1);
GO


/* =========================================================
   9. METODOS DE PAGO
   ========================================================= */

INSERT INTO Metodo_Pago
(descripcion, nombre)
VALUES
('Pago en efectivo', 'Efectivo'),
('Pago con tarjeta', 'Tarjeta'),
('Pago por transferencia bancaria', 'Transferencia');
GO


/* =========================================================
   10. PAGOS
   ========================================================= */

INSERT INTO Pago
(total, id_comprobante, id_metodo_pago)
VALUES
(150.00, 1, 1),
(150.00, 2, 2),
(250.00, 3, 3);
GO


/* =========================================================
   11. ESTADOS DE PEDIDO
   ========================================================= */

INSERT INTO Estado_Pedido
(nombre, descripcion)
VALUES
('Pendiente', 'Pedido pendiente de preparacion'),
('En proceso', 'Pedido en preparacion'),
('Completado', 'Pedido preparado y finalizado');
GO


/* =========================================================
   12. PEDIDOS
   ========================================================= */

INSERT INTO Pedido
(fecha_pedido, fecha_estimada, direccion_envio,
 observaciones, id_venta, id_estado_pedido)
VALUES
('2026-10-01', '2026-10-04', 'Zona 1, Guatemala', 'Entrega en casa', 1, 1),
('2026-10-02', '2026-10-05', 'Zona 10, Guatemala', 'Entrega por la tarde', 2, 2),
('2026-10-03', '2026-10-06', 'Mixco, Guatemala', 'Llamar antes de entregar', 3, 3);
GO


/* =========================================================
   13. ESTADOS DE DESPACHO
   ========================================================= */

INSERT INTO Estado_Despacho
(nombre, descripcion)
VALUES
('Pendiente', 'Despacho pendiente'),
('En camino', 'Producto en transporte'),
('Entregado', 'Producto entregado al cliente');
GO


/* =========================================================
   14. DESPACHOS
   ========================================================= */

INSERT INTO Despacho
(fecha_despacho, hora, transportista,
 observaciones, id_pedido, id_estado_despacho)
VALUES
('2026-10-04', '09:00:00', 'Transportes Lopez', 'Salida de bodega', 1, 1),
('2026-10-05', '11:30:00', 'Transportes Express', 'En ruta de entrega', 2, 2),
('2026-10-06', '14:00:00', 'Transportes GT', 'Entrega realizada', 3, 3);
GO