# Cemaco WebApp

Aplicación académica ASP.NET Core MVC para consultar productos, registrar ventas y pedidos, consultar su estado y gestionar reportes y despachos. Usa SQL Server y vistas Razor. El navegador envía formularios HTML a los controladores; **no consume una API**. El pago es un registro de demostración, no un cobro real.

## Requisitos

- .NET SDK **10.0** (`dotnet --version` debe mostrar una versión `10.x`).
- SQL Server con una instancia accesible. La configuración incluida usa **`.\SQLEXPRESS`** y autenticación de Windows.
- Para trabajar en Visual Studio: una instalación que admita proyectos ASP.NET Core con .NET 10 y la carga de trabajo de desarrollo web.
- Para crear la base de datos: SQL Server Management Studio (SSMS) o `sqlcmd`.

Todos los comandos de esta guía se ejecutan desde la raíz del repositorio, donde está `CemacoApp.slnx`.

## 1. Preparar la base de datos

1. Abre SSMS con **Autenticación de Windows** y conéctate a `.\SQLEXPRESS`.
2. Abre y ejecuta `CemacoApp/sql/01_Crear_Cemaco.sql`. El script crea la base `Cemaco`, sus 14 tablas y datos iniciales de catálogo y estados. Contiene instrucciones `GO`, por lo que debe ejecutarse en SSMS o `sqlcmd`, no como un único `SqlCommand` de la aplicación.
3. Comprueba que existen la base y los productos:

   ```sql
   SELECT DB_ID(N'Cemaco') AS IdBase;
   USE Cemaco;
   SELECT COUNT(*) AS Productos FROM dbo.Producto;
   ```

También puedes ejecutar el script desde PowerShell, **con el mismo usuario Windows que tiene acceso a SQL Server**:

```powershell
sqlcmd -S '.\SQLEXPRESS' -E -i '.\CemacoApp\sql\01_Crear_Cemaco.sql'
```

El script está pensado para preparar una base nueva. Si ya tienes una base `Cemaco` con otro esquema o datos importantes, revísala y haz una copia de seguridad antes de ejecutar cambios.

## 2. Configurar la conexión

La cadena está en `CemacoApp/appsettings.json`, bajo `ConnectionStrings:Cemaco`:

```text
Server=.\SQLEXPRESS;Database=Cemaco;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;Connect Timeout=5;
```

- `Server`: nombre de la instancia. Si tu instancia es `SQLSERVER`, cambia a `.\SQLSERVER`.
- `Database`: base que crea el script SQL.
- `Trusted_Connection=True`: la aplicación se autentica como **el usuario Windows que la ejecuta**. Ese usuario debe tener acceso a `Cemaco`.
- `Encrypt=False` y `TrustServerCertificate=True`: configuración local de este prototipo; antes de desplegar en otra red debe configurarse TLS correctamente.
- `Connect Timeout=5`: tiempo máximo inicial de conexión, en segundos.

Puedes sobreescribir la cadena solo en la terminal actual, sin modificar el archivo:

```powershell
$env:ConnectionStrings__Cemaco = 'Server=.\SQLEXPRESS;Database=Cemaco;Trusted_Connection=True;Encrypt=False;TrustServerCertificate=True;Connect Timeout=5;'
```

Ejecuta después `dotnet run` **en esa misma terminal**. Si usas autenticación SQL en vez de Windows, configura usuario y contraseña mediante una variable de entorno o un gestor de secretos local; no guardes contraseñas en `appsettings.json` ni las subas a Git.

`CemacoApp/appsettings.Development.json` contiene ajustes para desarrollo. `CemacoApp/Properties/launchSettings.json` define los perfiles `http` y `https`, sus puertos y `ASPNETCORE_ENVIRONMENT=Development`.

## 3. Limpiar, compilar y ejecutar desde PowerShell

Detén primero la aplicación si está abierta. Desde la raíz del repositorio:

```powershell
dotnet clean .\CemacoApp.slnx
dotnet restore .\CemacoApp.slnx
dotnet build .\CemacoApp.slnx --no-restore
dotnet run --project .\CemacoApp\CemacoApp.csproj --launch-profile http
```

Abre `http://localhost:5078`. Para usar el perfil HTTPS, sustituye `http` por `https` en `--launch-profile` y abre `https://localhost:7290`. Detén la ejecución con **Ctrl+C**.

`dotnet clean` elimina resultados de compilación, pero no borra la base de datos ni el código fuente. Las carpetas `bin/` y `obj/` pueden permanecer vacías o regenerarse al compilar. Normalmente no hace falta borrarlas manualmente.

## 4. Limpiar, compilar y ejecutar desde Visual Studio

1. Abre `CemacoApp.slnx`.
2. Selecciona **CemacoApp** como proyecto de inicio.
3. Detén una depuración anterior si sigue activa.
4. Usa **Compilar > Limpiar solución** y luego **Compilar > Recompilar solución**. Visual Studio restaurará los paquetes si hace falta.
5. Selecciona el perfil **http** o **https** en la barra de ejecución.
6. Pulsa **F5** para depurar o **Ctrl+F5** para ejecutar sin depuración.

Con el perfil `http`, la URL configurada es `http://localhost:5078`; con `https`, `https://localhost:7290`. Si Visual Studio solicita confiar en el certificado local de desarrollo para HTTPS, sigue su asistente. La aplicación usa la identidad Windows con la que se inicia Visual Studio para entrar en SQL Server.

## 5. Comprobar que funciona

1. Abre **Productos**: deben aparecer los artículos iniciales si la conexión SQL funciona.
2. Agrega un producto al **Carrito** y revisa cantidad y total en quetzales.
3. Completa una compra de prueba: debe generarse número de venta, comprobante y pedido. El precio se consulta otra vez en SQL Server al guardar.
4. Usa ese número en **Mi pedido** y verifica el estado.
5. En **Administración**, abre los reportes y, si necesitas probarlo, actualiza el estado o registra un despacho de ese pedido.
6. Visita una ruta inexistente, por ejemplo `/ruta-inexistente`: debe mostrarse la página 404.

Estas acciones de compra y administración **escriben datos** en `Cemaco`; usa una base de prueba. El módulo administrativo aún no tiene autenticación y no debe exponerse en Internet.

## Arquitectura

```text
Navegador
  → formulario o enlace
  → ruta MVC en Program.cs
  → controlador en Controllers/
  → SqlConnection y SqlCommand parametrizado
  → SQL Server
  → ViewModel
  → vista Razor en Views/
  → HTML al navegador
```

Los controladores consultan SQL Server directamente; no hay patrón repositorio. `Conexion` crea la conexión a partir de la configuración. `VentaService` reúne el registro de cliente, venta, detalles, comprobante, pago y pedido en **una transacción**: si una operación falla, se hace `ROLLBACK`. El carrito guarda IDs y cantidades en una sesión en memoria; al reiniciar la aplicación se puede perder. Los precios y el total final se calculan en el servidor.

| Carpeta o archivo | Contenido |
| --- | --- |
| `CemacoApp.slnx` | Solución para Visual Studio y `dotnet`. |
| `CemacoApp/CemacoApp.csproj` | Proyecto web .NET 10, paquetes y exclusiones de archivos heredados. |
| `CemacoApp/Program.cs` | Registro MVC, sesión, archivos estáticos, manejo de errores y ruta principal. |
| `CemacoApp/Controllers/` | `Home`, `Catalogo`, `Carrito`, `Pedidos` y `Admin`: navegación, formularios y consultas SQL. |
| `CemacoApp/Data/` | `Conexion.cs`: obtiene la cadena y crea `SqlConnection`. |
| `CemacoApp/Services/` | `VentaService.cs`: transacción de compra; `CarritoSession.cs`: lectura y escritura del carrito en sesión. |
| `CemacoApp/ViewModels/` | Datos para las vistas y reglas de validación del formulario de compra. |
| `CemacoApp/Views/` | Vistas Razor por controlador, diseño compartido y página 404. |
| `CemacoApp/wwwroot/css/` | Estilos de la interfaz; `wwwroot/` contiene archivos públicos. |
| `CemacoApp/sql/` | Script de esquema y datos iniciales de `Cemaco`. |
| `CemacoApp/Properties/` | Perfiles locales de ejecución (`launchSettings.json`). |
| `presentaciones/` | PPTX y material de exposición; no es necesario para ejecutar la aplicación. |

`Pages/`, `Repositories/`, `Models/Cliente.cs`, `wwwroot/lib/` y `wwwroot/js/site.js` son restos de versiones anteriores. El `.csproj` excluye los archivos de Razor Pages, repositorios y librerías estáticas antiguas; no forman parte del flujo MVC actual. `bin/`, `obj/`, `.vs/` y `.keys/` son archivos generados o locales y están en `.gitignore`.

## Errores frecuentes

| Síntoma | Posible causa y solución |
| --- | --- |
| `dotnet` no se reconoce o falla por `net10.0` | Instala el SDK .NET 10 y comprueba `dotnet --version`. Abre la solución con una versión de Visual Studio compatible con ese SDK. |
| Error al restaurar paquetes (`NU1301`, fuente no disponible) | Comprueba la conexión a NuGet o la configuración de fuentes; después ejecuta `dotnet restore .\CemacoApp.slnx`. |
| `MSB3021` o `MSB3027`: no se puede copiar `CemacoApp.dll` o `.exe` | Hay una ejecución abierta usando el archivo. Detén F5, cierra la consola donde corre la app y vuelve a ejecutar `dotnet clean` y `dotnet build`. |
| No se puede abrir `Cemaco` o aparece `Invalid object name 'Producto'` | La base no existe o el script no se ejecutó completo en la instancia configurada. Revisa la instancia en SSMS y vuelve al paso 1. |
| Error de red o instancia de SQL Server | Comprueba `Server=.\SQLEXPRESS` y que el servicio `SQL Server (SQLEXPRESS)` esté iniciado. En PowerShell puedes consultar `Get-Service 'MSSQL$SQLEXPRESS'`. |
| `Login failed` o `No se puede generar contexto SSPI` | La identidad Windows que ejecuta la web no logra autenticarse. Prueba SSMS con esa misma cuenta y revisa sus permisos en la instancia y en `Cemaco`. En este entorno aislado la app corre como `codexsandboxoffline`, que no hereda la sesión Windows del usuario. |
| Aviso de certificado HTTPS local | Usa el perfil `http` para una prueba local o confía en el certificado de desarrollo desde Visual Studio. No desactives la validación TLS para un despliegue real. |
| Advertencia `Failed to determine the https port for redirect` | La aplicación se inició solo por HTTP y no se indicó un puerto HTTPS de destino. Para probar HTTPS usa `--launch-profile https`; en pruebas locales con el perfil `http`, esta advertencia puede aparecer y no implica un fallo de SQL. |
| El catálogo muestra “No se pudo consultar la base Cemaco” | Revisa el error detallado en la consola de la aplicación: puede ser instancia, autenticación, base o tablas. |
| HTTP 404 | La ruta o el registro solicitado no existe. La página 404 es el comportamiento esperado para esos casos. |
| HTTP 400 al enviar un POST manual | Los formularios requieren token antifalsificación. Envía el formulario desde la interfaz web. |
| `git fetch` falla con `protocol 'git remote add origin https' is not supported` | `origin` tiene un comando completo como URL. Corrígelo con `git remote set-url origin https://github.com/jblSomoza/Cemaco-WebApp.git`. |

## Límites actuales

El pago es simulado; no hay inventario, inicio de sesión administrativo ni integración con transportistas. Para un despliegue real harían falta autenticación, autorización, TLS correcto, persistencia de sesión y pruebas completas con una base SQL de prueba.
