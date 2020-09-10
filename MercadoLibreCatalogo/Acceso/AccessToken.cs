using HttpParamsUtility;
using Newtonsoft.Json.Linq;
using System;

namespace MercadoLibreCatalogo.Catalogo
{
    public class AccessToken : AccesoRest
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public string GetToken()
        {
            try
            {
                string token = "";
                var c = Credentials.GetMLInstancia();

                var p = new HttpParams().Add("grant_type", "client_credentials").Add("client_id", ClientId).Add("client_secret", ClientSecret);

                var response = c.PostAsync("oauth/token?" + p);

                var read = response.Result.Content.ReadAsStringAsync().Result;
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(read);

                JObject jObject = JObject.Parse(result.ToString());

                if (response.Result.IsSuccessStatusCode)
                {
                    if (read != "null")
                    {
                        token = (string)jObject.SelectToken("access_token");
                    }
                }
                return token;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }
        }
    }
}
