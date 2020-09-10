using MercadoLibreCatalogo.Catalogo;
using MercadoLibreCatalogo.Modelo;
using System;
using System.Collections;
using System.Collections.Generic;

namespace MercadoLibreCatalogo
{
    public class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static int linea = 1;
        private static int total = 1;

        public static void Main(string[] args)
        {
            try
            {
                AccessToken Token = new AccessToken();
                IntegracionCatalogo IntegracionCatalogo = new IntegracionCatalogo();
                IntegracionPrecio IntegracionPrecio = new IntegracionPrecio();
                IntegracionStock IntegracionStock = new IntegracionStock();
                IntegracionImagen IntegracionImagen = new IntegracionImagen();
                IntegracionCategorias IntegracionCategorias = new IntegracionCategorias();

                const int storeId = 330;
                const string modoCompra = "buy_it_now";
                const string tipoPublicacion = "gold_special";
                const string condicion = "new";
                string token = Token.GetToken();

                Dictionary<string, string> objects = new Dictionary<string, string>();
                IEnumerable<Articulo> articulos = IntegracionCatalogo.GetArticulos();

                log.Debug("Integración de Catálogo MercadoLibre iniciada");

                foreach(var obj in articulos)
                {
                    Tuple<decimal, string> precio = IntegracionPrecio.GetPrecio(obj.CodArticulo);
                    int stock = IntegracionStock.GetStock(obj.CodArticulo);
                    bool checkImagen = IntegracionImagen.CheckImagen(obj.CodArticulo);

                    if (checkImagen)
                    {
                        if(stock > 0)
                        {
                            if (precio != null)
                            {
                                string categoria = IntegracionCategorias.GetCategoria(obj, token, precio.Item1);

                                objects["seller_custom_field"] = obj.CodArticulo;
                                objects["title"] = obj.NomAlternativo;
                                objects["description"] = obj.DescArticulo;
                                objects["category_id"] = categoria;
                                objects["official_store_id"] = storeId.ToString();
                                objects["price"] = precio.Item1.ToString();
                                objects["currency_id"] = precio.Item2;
                                objects["available_quantity"] = stock.ToString();
                                objects["buying_mode"] = modoCompra;
                                objects["listing_type_id"] = tipoPublicacion;
                                objects["video_id"] = obj.WebLinkVideo;
                                objects["condition"] = condicion;
                                objects["venta_web"] = obj.VentaWeb;
                                objects["web_viñetas"] = obj.WebViñetas;
                                objects["web_publicado"] = obj.WebPublicado;
                                objects["descripcion_web"] = obj.DescripcionWeb;

                                IntegracionCatalogo.CrearCatalogo(objects, token);
                                log.Debug(obj.CodArticulo + " - Cantidad: " + linea++ + "/" + total);
                            }
                            else
                            {
                                log.Debug(obj.CodArticulo + " - El artículo no tiene precio");
                            }
                        }
                        else
                        {
                            log.Debug(obj.CodArticulo + " - El artículo no tiene stock");
                        }
                    }
                    else
                    {
                        log.Debug(obj.CodArticulo + " - El artículo no tiene imagen");
                    }
                    total++;
                }
                log.Debug("Integración de Catálogo MercadoLibre finalizada");
            }
            catch(Exception ex)
            {
                log.Error(ex);
            }
        }
    }
}
