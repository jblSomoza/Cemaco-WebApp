using System.Text.Json;
using CemacoApp.ViewModels;
namespace CemacoApp.Services;
public static class CarritoSession
{
    private const string Key="CemacoCarrito";
    public static List<ItemVenta> Leer(ISession session) => JsonSerializer.Deserialize<List<ItemVenta>>(session.GetString(Key)??"[]")??[];
    public static void Guardar(ISession session,List<ItemVenta> items) => session.SetString(Key,JsonSerializer.Serialize(items));
    public static void Limpiar(ISession session) => session.Remove(Key);
}
