using System.Configuration;

namespace MercadoLibreCatalogo.Catalogo
{
    public class DAO
    {
        public static string Connection()
        {
            string ConnectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;

            return ConnectionString;
        }
    }
}
