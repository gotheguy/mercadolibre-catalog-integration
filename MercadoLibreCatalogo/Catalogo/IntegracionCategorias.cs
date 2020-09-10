using HttpParamsUtility;
using MercadoLibreCatalogo.Modelo;
using Newtonsoft.Json.Linq;
using System;

namespace MercadoLibreCatalogo.Catalogo
{
    public class IntegracionCategorias : AccesoRest
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string GetCategoria(Articulo articulo, string token, decimal precio)
        {
            string categoria = "";

            try
            {
                var c = Credentials.GetMLInstancia();
                var p = new HttpParams().Add("title", articulo.NomAlternativo).Add("price", Convert.ToInt32(precio)).Add("seller_id", ClientId);

                var response = c.GetAsync("/sites/MLU/category_predictor/predict", p);

                var read = response.Result.Content.ReadAsStringAsync().Result;
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(read);

                JObject jObject = JObject.Parse(result.ToString());

                if (response.Result.IsSuccessStatusCode)
                {
                    categoria = (string)jObject.SelectToken("id");
                    log.Debug(articulo.CodArticulo + " - Categoria agregada " + categoria);
                }
            }
            catch (Exception ex)
            {
                log.Error(articulo.CodArticulo + " - " + ex);
            }
            return categoria;
        }
    }
}
