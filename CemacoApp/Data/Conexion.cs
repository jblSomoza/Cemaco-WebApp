using Microsoft.Data.SqlClient;
namespace CemacoApp.Data;
public sealed class Conexion(IConfiguration configuration)
{
    public SqlConnection ObtenerConexion() => new(configuration.GetConnectionString("Cemaco")
        ?? throw new InvalidOperationException("Configura ConnectionStrings:Cemaco."));
}
