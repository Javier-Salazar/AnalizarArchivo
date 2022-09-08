using System;
using System.Data;
using System.Data.SqlClient;


namespace SQL
{
    public class CMD
    {
        SqlDataReader leer;
        SqlCommand com;
        SqlDataAdapter sda;
        DataTable data;
        SqlConnection con = new SqlConnection("Data Source=AHERNANDEZ;Initial Catalog=GlobalTradeSMPFS;Persist Security Info=True;User ID=Prisma; password=Prisma@1");
        public string mensaje, nombretabla, sql;

        public string ObtenerTabla(string v)
        {
            nombretabla = "";
            if (v == "Customers and Vendors")
            {
                nombretabla = "VENDCUST";
            }
            else if (v == "Classes")
            {
                nombretabla = "CLASES";
            }
            else if (v == "Finished Goods and Components")
            {
                nombretabla = "ITEMMASTER";
            }
            else if (v == "Bill of Materials")
            {
                nombretabla = "BOM";
            }
            return nombretabla;
        }
        public DataTable MostrarTabla(string v)
        {
            try
            {
                data = new DataTable();
                con.Open();
                com = new SqlCommand("SELECT clyd_col_name,clyd_col_len,clyd_col_dtype FROM cfglayoutdef WHERE clyd_tabla = '" + ObtenerTabla(v) + "' AND clyd_erp = 'IMPLEMENTACION'", con);
                sda = new SqlDataAdapter(com);
                sda.Fill(data);
                con.Close();
                creartabla(data);
            }
            catch (Exception er)
            {
                mensaje = "mensaje de conexión a la base de datos. " + er.Message;
            }
            return data;
        }
        public void creartabla(DataTable data)
        {
            con.Open();
            nombretabla = "Temp" + nombretabla;
            com = new SqlCommand("IF OBJECT_ID('" + nombretabla + "') IS NOT NULL " +
            "BEGIN " +
            "DROP TABLE " + nombretabla + "; " +
            "END", con);
            com.ExecuteNonQuery();
            string sql = "Create Table " + nombretabla + " ( ";
            foreach (DataRow item in data.Rows)
            {
                sql = sql + item["clyd_col_name"].ToString().Replace(" ", "") + " VARCHAR(MAX) NULL, ";
            }
            sql = sql.Remove(sql.Length - 2);
            com = new SqlCommand(sql + " )", con);
            com.ExecuteNonQuery();
            con.Close();
        }
        public void Insertar( string tabla,string col, string clave)
        {
            sql = "INSERT INTO " + tabla + " ("+col+") VALUES ('" + clave + "' )";
            con.Open();
            com = new SqlCommand(sql, con);
            com.ExecuteNonQuery();
            con.Close();
        }
        public bool Buscarclave(string v1, string v2, string v3, int op)
        {
            try
            {

                con.Open();
                com = new SqlCommand("SELECT * from " + v1 + " where " + v2 + " = '" + v3 + "'", con);
                leer = com.ExecuteReader();
                if (leer.Read())
                {
                    con.Close();
                    return true;
                }
                else
                {
                    con.Close();
                    if (op == 1)
                    {
                        Insertar(nombretabla,v2,v3);
                    }
                    return false;
                }
            }
            catch (Exception)
            {
                con.Close();
                return false;
            }
        }
    }
}
