using CemacoApp.Data;
using CemacoApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.SqlClient;

namespace CemacoApp.Pages.Clientes
{
    public class IndexModel : PageModel
    {
        private readonly Conexion _conexion;

        public List<Cliente> Clientes { get; set; }
            = new List<Cliente>();


        public IndexModel(Conexion conexion)
        {
            _conexion = conexion;
        }


        public void OnGet()
        {
            using SqlConnection conexion =
                _conexion.ObtenerConexion();

            conexion.Open();

            string sql = @"
                SELECT
                    id_cliente,
                    nombre,
                    apellido,
                    correo,
                    telefono
                FROM Clientes_Lopez
                ORDER BY id_cliente;
            ";

            using SqlCommand comando =
                new SqlCommand(sql, conexion);

            using SqlDataReader lector =
                comando.ExecuteReader();

            while (lector.Read())
            {
                Cliente cliente = new Cliente
                {
                    IdCliente =
                        Convert.ToInt32(
                            lector["id_cliente"]
                        ),

                    Nombre =
                        lector["nombre"].ToString()!,

                    Apellido =
                        lector["apellido"].ToString()!,

                    Correo =
                        lector["correo"].ToString()!,

                    Telefono =
                        lector["telefono"].ToString()!
                };

                Clientes.Add(cliente);
            }
        }
    }
}
