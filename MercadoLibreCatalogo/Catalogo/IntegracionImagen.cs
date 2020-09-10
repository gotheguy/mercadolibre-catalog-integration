using HttpParamsUtility;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace MercadoLibreCatalogo.Catalogo
{
    public class IntegracionImagen : AccesoRest
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public int ImgUrl { get; private set; }

        public bool CheckImagen(string cod_articulo)
        {
            bool exist = false;
            try
            {
                var f = Credentials.GetFTPInstancia();

                int sku = Int32.Parse(cod_articulo);
                exist = f.FileExists("/" + (sku % 10).ToString() + "/" + sku.ToString() + ".jpg") || f.FileExists("/" + (sku % 10).ToString() + "/" + sku.ToString() + ".JPG");
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            return exist;
        }

        public bool ImagenExiste(string ImgId, string token)
        {
            bool existe = false;
            try
            {
                var c = Credentials.GetMLInstancia();
                var p = new HttpParams().Add("access_token", token);

                var response = c.GetAsync("/pictures/" + ImgId, p).Result;

                if (response.IsSuccessStatusCode)
                {
                    existe = true;
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            return existe;
        }

        public string PublicarImagen(string cod_articulo, string token)
        {
            string imgId = "";
            try
            {
                var c = Credentials.GetMLInstancia();
                var f = Credentials.GetFTPInstancia();
                var p = new HttpParams().Add("access_token", token);

                int order = 1;
                int idSku = Int32.Parse(cod_articulo);
                int directory = idSku % 10;
                f.SetWorkingDirectory("/" + directory);

                foreach (var images in f.GetListing())
                {
                    if (images.Name.StartsWith(cod_articulo + "-") || images.Name.StartsWith(cod_articulo + "."))
                    {
                        var response = c.PostAsync("/pictures", p, new { source = ImgUrl + directory + "/" + images.Name }).Result;
                        var read = response.Content.ReadAsStringAsync().Result;
                        var result = Newtonsoft.Json.JsonConvert.DeserializeObject(read);

                        JObject jObject = JObject.Parse(result.ToString());

                        if (response.IsSuccessStatusCode)
                        {
                            imgId = (string)jObject.SelectToken("id");
                        }
                        order++;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            return imgId;
        }

        public ArrayList GetImagenUrl(string cod_articulo, string token)
        {
            ArrayList listaImagenes = new ArrayList();
            string imagen = "";
            try
            {
                var f = Credentials.GetFTPInstancia();

                int idSku = Int32.Parse(cod_articulo);
                int directory = idSku % 10;
                f.SetWorkingDirectory("/" + directory);

                foreach (var images in f.GetListing())
                {
                    if (images.Name.StartsWith(cod_articulo + "-") || images.Name.StartsWith(cod_articulo + "."))
                    {
                        if (listaImagenes.Count <= 11)
                        {
                            imagen = ImgUrl + directory + "/" + images.Name;
                            listaImagenes.Add(imagen);
                            log.Debug(images.Name + " - Imagen agregada");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            return listaImagenes;
        }

        public void AsociarImagen(string MLId, string imgId, string token)
        {
            HttpClient client = new HttpClient();
            HttpResponseMessage response = new HttpResponseMessage();

            client.BaseAddress = new Uri(Url);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            if(!ImagenExiste(imgId, token))
            {
                dynamic image = new JObject();
                image.id = imgId;

                response = client.PostAsync("/items/" + MLId + "/pictures?access_token=" + token, new StringContent(image.ToString(), Encoding.UTF8, "application/json")).Result;

                if(response.IsSuccessStatusCode)
                {
                    log.Debug(imgId + " - Imagen agregada");
                }
            }
        }
    }
}