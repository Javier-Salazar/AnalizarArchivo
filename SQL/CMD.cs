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
        public bool flag = false;

        public string ObtenerTabla(string v)
        {
            switch (v){
                case "Customers and Vendors":
                    nombretabla = "VENDCUST";
                    break;
                case "Classes":
                    nombretabla = "CLASES";
                    break;
                case "Finished Goods and Components":
                    nombretabla = "ITEMMASTER";
                    break;
                case "Bill of Materials":
                    nombretabla = "BOM";
                    break;
            }
            return nombretabla;
        }
        public DataTable MostrarTabla(string v)
        {
            data = new DataTable();
            con.Open();
            com = new SqlCommand("SELECT clyd_col_name,clyd_col_len,clyd_col_dtype FROM cfglayoutdef WHERE clyd_tabla = '" + ObtenerTabla(v) + "' AND clyd_erp = 'IMPLEMENTACION'", con);
            sda = new SqlDataAdapter(com);
            sda.Fill(data);
            con.Close();
            creartabla(data);
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
            flag = false;
            con.Open();
            com = new SqlCommand("SELECT * from " + v1 + " where " + v2 + " = '" + v3 + "'", con);
            leer = com.ExecuteReader();
            if (!leer.Read())
            {
                con.Close();
                if(op == 1) { Insertar(nombretabla, v2, v3); }
                flag = true;
            }
            con.Close();
            return flag;
        }
    }
}
