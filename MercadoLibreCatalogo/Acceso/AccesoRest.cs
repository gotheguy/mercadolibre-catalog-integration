using MercadoLibre.SDK.Meta;

namespace MercadoLibreCatalogo.Catalogo
{
    public class AccesoRest
    {
        protected static MeliSite BaseUrl
        {
            get
            {
                return MeliSite.Uruguay;
            }
        }

        protected static string Url
        {
            get
            {
                return "https://api.mercadolibre.com";
            }
        }

        protected static long UserId
        {
            get
            {
                return 0;
            }
        }

        protected static long ClientId
        {
            get
            {
                return 0;
            }
        }

        protected static string ClientSecret
        {
            get
            {
                return "";
            }
        }
    }
}
