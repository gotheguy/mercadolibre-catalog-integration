using Dapper;
using HttpParamsUtility;
using MercadoLibreCatalogo.Modelo;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace MercadoLibreCatalogo.Catalogo
{
    public class IntegracionCatalogo : AccesoRest
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public IEnumerable<Articulo> GetArticulos()
        {
            DAO sql = new DAO();
            IEnumerable<Articulo> articulos;

            using (IDbConnection db = new SqlConnection(DAO.Connection()))
            {
                articulos = db.Query<Articulo>("[FlyDb].[oms].[CatalogoMercadoLibre]",
                commandType: CommandType.StoredProcedure);
            }
            return articulos;
        }

        public void CrearCatalogo(Dictionary<string, string> objects, string token)
        {
            try
            {
                IntegracionPrecio precio = new IntegracionPrecio();
                IntegracionStock stock = new IntegracionStock();
                IntegracionImagen imagen = new IntegracionImagen();
                dynamic s = new JObject();

                HttpClient client = new HttpClient();
                HttpResponseMessage response = new HttpResponseMessage();

                client.BaseAddress = new Uri(Url);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                response = client.GetAsync("/users/" + UserId + "/items/search?sku=" + objects["seller_custom_field"] + "&access_token=" + token).Result;

                var read = response.Content.ReadAsStringAsync().Result;
                var result = Newtonsoft.Json.JsonConvert.DeserializeObject(read);

                JObject jObject = JObject.Parse(result.ToString());

                if (jObject.SelectToken("results[0]") == null)
                {
                    if (objects["venta_web"].Equals("S") && objects["web_publicado"].Equals("S"))
                    {

                        Console.WriteLine("Descripcion " + objects["description"] + objects["web_viñetas"]
                                                + objects["descripcion_web"]);

                        dynamic body = new JObject();
                        body.title = objects["title"] + " - Mosca";
                        dynamic description = new JObject();
                        body.description = description;

                        if(objects["description"] != null && objects["web_viñetas"] != null)
                        {
                            description.plain_text = objects["description"].Replace("<br/>", "\n") + objects["web_viñetas"].Replace("<br/>", "\n")
                                                                         + objects["descripcion_web"].Replace("<br/>", "\n");
                        }
                        else if(objects["description"] == null && objects["web_viñetas"] != null)
                        {
                            description.plain_text = objects["web_viñetas"].Replace("<br/>", "\n") + objects["descripcion_web"].Replace("<br/>", "\n");
                        }
                        else if(objects["web_viñetas"] == null && objects["description"] != null)
                        {
                            description.plain_text = objects["description"].Replace("<br/>", "\n") + objects["descripcion_web"].Replace("<br/>", "\n");
                        }
                        else
                        {
                            description.plain_text = objects["descripcion_web"].Replace("<br/>", "\n");
                        }

                        body.category_id = objects["category_id"];
                        body.price = objects["price"].Replace(",",".");
                        body.currency_id = objects["currency_id"];
                        body.official_store_id = objects["official_store_id"];
                        body.available_quantity = objects["available_quantity"];
                        body.buying_mode = objects["buying_mode"];
                        body.listing_type_id = objects["listing_type_id"];
                        body.video_id = objects["video_id"];
                        body.condition = objects["condition"];
                        body.seller_custom_field = objects["seller_custom_field"];

                        JArray attributes = new JArray();
                        dynamic attribute = new JObject();
                        attribute.id = "ITEM_CONDITION";
                        attribute.name = "Condición del ítem";
                        attribute.value_id = "2230284";
                        attribute.value_name = "Nuevo";
                        attribute.value_struct = null;
                        attribute.attribute_group_id = "";
                        attribute.attribute_group_name = "";

                        attributes.Add(attribute);
                        body.attributes = attributes;

                        JArray tags = new JArray
                        {
                            "immediate_payment",
                            "good_quality_picture",
                            "good_quality_thumbnail"
                        };
                        body.tags = tags;

                        JArray pictures = new JArray();
                        ArrayList listaImagenes = imagen.GetImagenUrl(objects["seller_custom_field"], token);

                        for (int i = 0; i < listaImagenes.Count; i++)
                        {
                            dynamic picture = new JObject();
                            picture.source = listaImagenes[i];
                            pictures.Add(picture);
                        }
                        body.pictures = pictures;

                        response = client.PostAsync("/items?access_token=" + token, new StringContent(body.ToString(), Encoding.UTF8, "application/json")).Result;

                        if (response.IsSuccessStatusCode)
                        {
                            log.Debug(objects["seller_custom_field"] + " - Artículo creado");
                        }
                    }
                    else
                    {
                        log.Debug(objects["seller_custom_field"] + " - Artículo no cumple los requisitos de Venta Web y Publicación");
                    }
                }
                else
                {
                    if (objects["venta_web"].Equals("S") && objects["web_publicado"].Equals("S"))
                    {
                        precio.UpdatePrecio(objects["seller_custom_field"], jObject.SelectToken("results[0]").ToString(), token);
                        stock.UpdateStock(objects["seller_custom_field"], jObject.SelectToken("results[0]").ToString(), token);

                        string ImgId = imagen.PublicarImagen(objects["seller_custom_field"], token);
                        imagen.AsociarImagen(jObject.SelectToken("results[0]").ToString(), ImgId, token);

                        s.status = "active";
                        response = client.PutAsync("/items/" + jObject.SelectToken("results[0]") + "?access_token=" + token, new StringContent(s.ToString(), Encoding.UTF8, "application/json")).Result;
                        log.Debug(objects["seller_custom_field"] + " - Artículo actualizado");
                    } 
                    else
                    {
                        s.status = "paused";
                        response = client.PutAsync("/items/" + jObject.SelectToken("results[0]") + "?access_token=" + token, new StringContent(s.ToString(), Encoding.UTF8, "application/json")).Result;
                        log.Debug(objects["seller_custom_field"] + " - Artículo desactivado");
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
    }
}
