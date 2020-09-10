using FluentFTP;
using MercadoLibre.SDK;
using System.Net;

namespace MercadoLibreCatalogo.Catalogo
{
    public class Credentials : AccesoRest
    {
        private static MeliApiService mlinstancia;
        private static FtpClient ftpinstancia;

        private Credentials() { }

        public static MeliApiService GetMLInstancia()
        {
            if (mlinstancia == null)
            {
                mlinstancia = new MeliApiService
                {
                    Credentials = new MeliCredentials(BaseUrl, ClientId, ClientSecret)
                };
            }
            return mlinstancia;
        }

        public static FtpClient GetFTPInstancia()
        {
            if (ftpinstancia == null)
            {
                ftpinstancia =  new FtpClient(FtpUrl)
                {
                    Credentials = new NetworkCredential(FtpUser, FtpPassword)
                };
            }
            return ftpinstancia;
        }
    }
}
