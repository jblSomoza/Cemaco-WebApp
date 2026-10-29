# Cemaco MVC

Aplicación web ASP.NET Core MVC para catálogo, compra, seguimiento de pedidos, reportes y despacho. Las vistas Razor envían formularios a controladores; estos consultan SQL Server directamente mediante `SqlConnection` y `SqlCommand`. No hay API para el navegador ni patrón repositorio.

## Estructura

- `Controllers/`: navegación y formularios MVC.
- `ViewModels/`: datos mostrados y validación de compra.
- `Services/`: carrito por sesión y reglas de venta.
- `Data/Conexion.cs`: creación de conexiones SQL.
- `Services/VentaService.cs`: validación y transacción de venta.
- `Views/`: interfaz Razor.
- `sql/01_Crear_Cemaco.sql`: esquema y datos iniciales.

## Ejecutar

1. Ejecuta `CemacoApp/sql/01_Crear_Cemaco.sql` en la instancia `SQLEXPRESS` con SQL Server Management Studio o `sqlcmd -S .\SQLEXPRESS -E -i CemacoApp/sql/01_Crear_Cemaco.sql` desde tu sesión de Windows.
2. La cadena predeterminada de `CemacoApp/appsettings.json` usa `.\SQLEXPRESS`, autenticación integrada y la base `Cemaco`. Si tu instancia es distinta, cambia `ConnectionStrings:Cemaco` o usa la variable de entorno `ConnectionStrings__Cemaco`.
3. Ejecuta `dotnet run --project CemacoApp/CemacoApp.csproj` y abre la URL mostrada.

El carrito se conserva en la sesión del navegador. Los precios se vuelven a consultar al guardar la compra. El módulo de administración no tiene autenticación y debe protegerse antes de publicar el sitio en Internet.
