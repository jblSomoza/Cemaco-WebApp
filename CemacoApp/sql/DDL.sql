/* =========================================================
   BASE DE DATOS: CEMACO
   PROYECTO FINAL - BASE DE DATOS 1
   MODELO NORMALIZADO A 3FN
   ========================================================= */

CREATE DATABASE Cemaco;
GO

USE Cemaco;
GO


/* =========================================================
   TABLA CLIENTE
   ========================================================= */

CREATE TABLE Cliente
(
    id_cliente INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(50) NOT NULL,
    apellido VARCHAR(50) NOT NULL,
    nit VARCHAR(20),
    direccion VARCHAR(150),
    telefono VARCHAR(20),
    correo VARCHAR(100)
);
GO


/* =========================================================
   TABLA CATEGORIA
   ========================================================= */

CREATE TABLE Categoria
(
    id_categoria INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(50) NOT NULL,
    descripcion VARCHAR(100)
);
GO


/* =========================================================
   TABLA PRODUCTO
   ========================================================= */

CREATE TABLE Producto
(
    id_producto INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(100) NOT NULL,
    descripcion VARCHAR(200),
    precio DECIMAL(10,2) NOT NULL,
    id_categoria INT NOT NULL,

    CONSTRAINT FK_Producto_Categoria
        FOREIGN KEY (id_categoria)
        REFERENCES Categoria(id_categoria)
);
GO


/* =========================================================
   TABLA TIPO_VENTA
   Mejora aplicada para 3FN
   ========================================================= */

CREATE TABLE Tipo_Venta
(
    id_tipo_venta INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(50) NOT NULL,
    descripcion VARCHAR(100)
);
GO


/* =========================================================
   TABLA VENTA
   ========================================================= */

CREATE TABLE Venta
(
    id_venta INT PRIMARY KEY IDENTITY(1,1),
    fecha DATE NOT NULL,
    id_cliente INT NOT NULL,
    id_tipo_venta INT NOT NULL,

    CONSTRAINT FK_Venta_Cliente
        FOREIGN KEY (id_cliente)
        REFERENCES Cliente(id_cliente),

    CONSTRAINT FK_Venta_TipoVenta
        FOREIGN KEY (id_tipo_venta)
        REFERENCES Tipo_Venta(id_tipo_venta)
);
GO


/* =========================================================
   TABLA DETALLE_VENTA
   subtotal fue eliminado por normalización.
   Se obtiene mediante:
   cantidad * precio_unitario
   ========================================================= */

CREATE TABLE Detalle_Venta
(
    id_detalle INT PRIMARY KEY IDENTITY(1,1),
    cantidad INT NOT NULL,
    precio_unitario DECIMAL(10,2) NOT NULL,
    id_venta INT NOT NULL,
    id_producto INT NOT NULL,

    CONSTRAINT FK_DetalleVenta_Venta
        FOREIGN KEY (id_venta)
        REFERENCES Venta(id_venta),

    CONSTRAINT FK_DetalleVenta_Producto
        FOREIGN KEY (id_producto)
        REFERENCES Producto(id_producto)
);
GO


/* =========================================================
   TABLA TIPO_COMPROBANTE
   Mejora aplicada para 3FN
   ========================================================= */

CREATE TABLE Tipo_Comprobante
(
    id_tipo_comprobante INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(50) NOT NULL,
    descripcion VARCHAR(100)
);
GO


/* =========================================================
   TABLA COMPROBANTE
   ========================================================= */

CREATE TABLE Comprobante
(
    id_comprobante INT PRIMARY KEY IDENTITY(1,1),
    numero VARCHAR(50) NOT NULL,
    fecha_emision DATE NOT NULL,
    detalle VARCHAR(200),
    id_venta INT NOT NULL UNIQUE,
    id_tipo_comprobante INT NOT NULL,

    CONSTRAINT FK_Comprobante_Venta
        FOREIGN KEY (id_venta)
        REFERENCES Venta(id_venta),

    CONSTRAINT FK_Comprobante_TipoComprobante
        FOREIGN KEY (id_tipo_comprobante)
        REFERENCES Tipo_Comprobante(id_tipo_comprobante)
);
GO


/* =========================================================
   TABLA METODO_PAGO
   ========================================================= */

CREATE TABLE Metodo_Pago
(
    id_metodo_pago INT PRIMARY KEY IDENTITY(1,1),
    descripcion VARCHAR(100),
    nombre VARCHAR(50) NOT NULL
);
GO


/* =========================================================
   TABLA PAGO
   ========================================================= */

CREATE TABLE Pago
(
    id_pago INT PRIMARY KEY IDENTITY(1,1),
    total DECIMAL(10,2) NOT NULL,
    id_comprobante INT NOT NULL,
    id_metodo_pago INT NOT NULL,

    CONSTRAINT FK_Pago_Comprobante
        FOREIGN KEY (id_comprobante)
        REFERENCES Comprobante(id_comprobante),

    CONSTRAINT FK_Pago_MetodoPago
        FOREIGN KEY (id_metodo_pago)
        REFERENCES Metodo_Pago(id_metodo_pago)
);
GO


/* =========================================================
   TABLA ESTADO_PEDIDO
   ========================================================= */

CREATE TABLE Estado_Pedido
(
    id_estado_pedido INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(50) NOT NULL,
    descripcion VARCHAR(100)
);
GO


/* =========================================================
   TABLA PEDIDO
   ========================================================= */

CREATE TABLE Pedido
(
    id_pedido INT PRIMARY KEY IDENTITY(1,1),
    fecha_pedido DATE NOT NULL,
    fecha_estimada DATE,
    direccion_envio VARCHAR(150),
    observaciones VARCHAR(200),
    id_venta INT NOT NULL,
    id_estado_pedido INT NOT NULL,

    CONSTRAINT FK_Pedido_Venta
        FOREIGN KEY (id_venta)
        REFERENCES Venta(id_venta),

    CONSTRAINT FK_Pedido_EstadoPedido
        FOREIGN KEY (id_estado_pedido)
        REFERENCES Estado_Pedido(id_estado_pedido)
);
GO


/* =========================================================
   TABLA ESTADO_DESPACHO
   ========================================================= */

CREATE TABLE Estado_Despacho
(
    id_estado_despacho INT PRIMARY KEY IDENTITY(1,1),
    nombre VARCHAR(50) NOT NULL,
    descripcion VARCHAR(100)
);
GO


/* =========================================================
   TABLA DESPACHO
   ========================================================= */

CREATE TABLE Despacho
(
    id_despacho INT PRIMARY KEY IDENTITY(1,1),
    fecha_despacho DATE NOT NULL,
    hora TIME,
    transportista VARCHAR(100),
    observaciones VARCHAR(200),
    id_pedido INT NOT NULL,
    id_estado_despacho INT NOT NULL,

    CONSTRAINT FK_Despacho_Pedido
        FOREIGN KEY (id_pedido)
        REFERENCES Pedido(id_pedido),

    CONSTRAINT FK_Despacho_EstadoDespacho
        FOREIGN KEY (id_estado_despacho)
        REFERENCES Estado_Despacho(id_estado_despacho)
);
GO