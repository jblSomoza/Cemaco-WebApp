USE Cemaco;
GO


/* =========================================================
   1. MOSTRAR TODOS LOS CLIENTES
   ========================================================= */

SELECT *
FROM Cliente;
GO


/* =========================================================
   2. MOSTRAR TODAS LAS CATEGORIAS
   ========================================================= */

SELECT *
FROM Categoria;
GO


/* =========================================================
   3. MOSTRAR PRODUCTOS CON SU CATEGORIA
   ========================================================= */

SELECT
    P.id_producto,
    P.nombre AS producto,
    P.descripcion,
    P.precio,
    C.nombre AS categoria
FROM Producto AS P
INNER JOIN Categoria AS C
    ON P.id_categoria = C.id_categoria;
GO


/* =========================================================
   4. MOSTRAR VENTAS CON CLIENTE Y TIPO DE VENTA
   ========================================================= */

SELECT
    V.id_venta,
    V.fecha,
    C.nombre + ' ' + C.apellido AS cliente,
    C.nit,
    TV.nombre AS tipo_venta
FROM Venta AS V
INNER JOIN Cliente AS C
    ON V.id_cliente = C.id_cliente
INNER JOIN Tipo_Venta AS TV
    ON V.id_tipo_venta = TV.id_tipo_venta;
GO


/* =========================================================
   5. MOSTRAR DETALLE DE LAS VENTAS
   subtotal se calcula porque ya no se almacena
   ========================================================= */

SELECT
    DV.id_detalle,
    DV.id_venta,
    P.nombre AS producto,
    DV.cantidad,
    DV.precio_unitario,
    (DV.cantidad * DV.precio_unitario) AS subtotal
FROM Detalle_Venta AS DV
INNER JOIN Producto AS P
    ON DV.id_producto = P.id_producto;
GO


/* =========================================================
   6. MOSTRAR COMPROBANTES Y SU TIPO
   ========================================================= */

SELECT
    C.id_comprobante,
    C.numero,
    C.fecha_emision,
    TC.nombre AS tipo_comprobante,
    C.detalle,
    C.id_venta
FROM Comprobante AS C
INNER JOIN Tipo_Comprobante AS TC
    ON C.id_tipo_comprobante = TC.id_tipo_comprobante;
GO


/* =========================================================
   7. MOSTRAR PAGOS CON METODO DE PAGO
   ========================================================= */

SELECT
    P.id_pago,
    P.total,
    MP.nombre AS metodo_pago,
    MP.descripcion,
    P.id_comprobante
FROM Pago AS P
INNER JOIN Metodo_Pago AS MP
    ON P.id_metodo_pago = MP.id_metodo_pago;
GO


/* =========================================================
   8. MOSTRAR PEDIDOS CON CLIENTE Y ESTADO
   ========================================================= */

SELECT
    PE.id_pedido,
    PE.fecha_pedido,
    PE.fecha_estimada,
    PE.direccion_envio,
    C.nombre + ' ' + C.apellido AS cliente,
    EP.nombre AS estado_pedido
FROM Pedido AS PE
INNER JOIN Venta AS V
    ON PE.id_venta = V.id_venta
INNER JOIN Cliente AS C
    ON V.id_cliente = C.id_cliente
INNER JOIN Estado_Pedido AS EP
    ON PE.id_estado_pedido = EP.id_estado_pedido;
GO


/* =========================================================
   9. MOSTRAR DESPACHOS Y SU ESTADO
   ========================================================= */

SELECT
    D.id_despacho,
    D.fecha_despacho,
    D.hora,
    D.transportista,
    D.observaciones,
    ED.nombre AS estado_despacho,
    D.id_pedido
FROM Despacho AS D
INNER JOIN Estado_Despacho AS ED
    ON D.id_estado_despacho = ED.id_estado_despacho;
GO


/* =========================================================
   10. CONSULTA COMPLETA DE UNA VENTA
   ========================================================= */

SELECT
    V.id_venta,
    V.fecha,
    C.nombre + ' ' + C.apellido AS cliente,
    TV.nombre AS tipo_venta,
    PR.nombre AS producto,
    DV.cantidad,
    DV.precio_unitario,
    (DV.cantidad * DV.precio_unitario) AS subtotal
FROM Venta AS V
INNER JOIN Cliente AS C
    ON V.id_cliente = C.id_cliente
INNER JOIN Tipo_Venta AS TV
    ON V.id_tipo_venta = TV.id_tipo_venta
INNER JOIN Detalle_Venta AS DV
    ON V.id_venta = DV.id_venta
INNER JOIN Producto AS PR
    ON DV.id_producto = PR.id_producto
ORDER BY V.id_venta;
GO


/* =========================================================
   11. TOTAL CALCULADO DE CADA VENTA
   ========================================================= */

SELECT
    V.id_venta,
    V.fecha,
    C.nombre + ' ' + C.apellido AS cliente,
    SUM(DV.cantidad * DV.precio_unitario) AS total_venta
FROM Venta AS V
INNER JOIN Cliente AS C
    ON V.id_cliente = C.id_cliente
INNER JOIN Detalle_Venta AS DV
    ON V.id_venta = DV.id_venta
GROUP BY
    V.id_venta,
    V.fecha,
    C.nombre,
    C.apellido;
GO


/* =========================================================
   12. CONSULTA GENERAL DEL PROCESO
       VENTA -> COMPROBANTE -> PAGO -> PEDIDO -> DESPACHO
   ========================================================= */

SELECT
    V.id_venta,
    V.fecha AS fecha_venta,

    C.nombre + ' ' + C.apellido AS cliente,

    TV.nombre AS tipo_venta,

    CO.numero AS comprobante,
    TC.nombre AS tipo_comprobante,

    PA.total AS total_pagado,
    MP.nombre AS metodo_pago,

    PE.id_pedido,
    EP.nombre AS estado_pedido,

    D.fecha_despacho,
    D.transportista,
    ED.nombre AS estado_despacho

FROM Venta AS V

INNER JOIN Cliente AS C
    ON V.id_cliente = C.id_cliente

INNER JOIN Tipo_Venta AS TV
    ON V.id_tipo_venta = TV.id_tipo_venta

INNER JOIN Comprobante AS CO
    ON V.id_venta = CO.id_venta

INNER JOIN Tipo_Comprobante AS TC
    ON CO.id_tipo_comprobante = TC.id_tipo_comprobante

INNER JOIN Pago AS PA
    ON CO.id_comprobante = PA.id_comprobante

INNER JOIN Metodo_Pago AS MP
    ON PA.id_metodo_pago = MP.id_metodo_pago

INNER JOIN Pedido AS PE
    ON V.id_venta = PE.id_venta

INNER JOIN Estado_Pedido AS EP
    ON PE.id_estado_pedido = EP.id_estado_pedido

INNER JOIN Despacho AS D
    ON PE.id_pedido = D.id_pedido

INNER JOIN Estado_Despacho AS ED
    ON D.id_estado_despacho = ED.id_estado_despacho

ORDER BY V.id_venta;
GO