using HttpParamsUtility;
using System;
using System.Data.SqlClient;

namespace MercadoLibreCatalogo.Catalogo
{
    public class IntegracionPrecio
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void UpdatePrecio(string cod_articulo, string id, string token)
        {
            try
            {
                Tuple<decimal, string> precios = GetPrecio(cod_articulo);

                var c = Credentials.GetMLInstancia();
                var p = new HttpParams().Add("access_token", token);

                var response = c.PutAsync("/items/" + id, p, new { price = precios.Item1 });

                var read = response.Result.Content.ReadAsStringAsync().Result;  

                if (response.Result.IsSuccessStatusCode)
                {
                    log.Debug(cod_articulo + " - Precio actualizado");
                }
            }
            catch(Exception ex)
            {
                log.Error(cod_articulo + " - " + ex);
            }
        }

        public Tuple<decimal, string> GetPrecio(string cod_articulo)
        {
            DAO sql = new DAO();
            Tuple<decimal, string> precios = null;

            try
            {
                string SqlQuery = "EXEC [FlyDb].[oms].[PreciosMercadoLibre]" + cod_articulo;

                using (SqlConnection conn = new SqlConnection(DAO.Connection()))
                {
                    SqlCommand cmd = new SqlCommand(SqlQuery, conn);

                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        precios = new Tuple<decimal, string>(
                                Convert.ToDecimal(reader[1]),
                                reader[2].ToString()
                            );
                    }
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                log.Error(cod_articulo + " - Error obteniendo el precio del artículo " + ex);
            }
            return precios;
        }
    }
}
