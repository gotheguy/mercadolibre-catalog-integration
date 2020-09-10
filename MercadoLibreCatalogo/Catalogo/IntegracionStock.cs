using HttpParamsUtility;
using System;
using System.Data.SqlClient;

namespace MercadoLibreCatalogo.Catalogo
{
    public class IntegracionStock
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public void UpdateStock(string cod_articulo, string id, string token)
        {
            try
            {  
                int stock = GetStock(cod_articulo);

                var c = Credentials.GetMLInstancia();

                var p = new HttpParams().Add("access_token", token);
                var response = c.PutAsync("/items/" + id, p, new { available_quantity = stock });

                var read = response.Result.Content.ReadAsStringAsync().Result;

                if (response.Result.IsSuccessStatusCode)
                {
                    log.Debug(cod_articulo + " - Stock actualizado");
                }
            }
            catch(Exception ex)
            {
                log.Error(cod_articulo + " - " + ex);
            }
        }

        public int GetStock(string cod_articulo)
        {
            DAO sql = new DAO();
            string SqlQuery = "EXEC [FlyDb].[oms].[StockEcommerceCompleto]" + cod_articulo;
            int stock = 0;

            using (SqlConnection conn = new SqlConnection(DAO.Connection()))
            {
                SqlCommand cmd = new SqlCommand(SqlQuery, conn);
                try
                {
                    conn.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        stock = Convert.ToInt32(reader[1]);
                    }
                    conn.Close();
                }
                catch (Exception ex)
                {
                    log.Error(cod_articulo + " - " + ex);
                    stock = -1;
                }
            }
            return stock;
        }
    }
}
