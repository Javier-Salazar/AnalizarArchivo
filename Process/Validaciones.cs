using System;
using System.Data;
using System.Text.RegularExpressions;

namespace Process
{
    public class Validaciones
    {
        public SQL.CMD cl1 = new SQL.CMD();
        Ctabla v1;
        Regex Val;
        public string mensaje = "";
        public class Ctabla
        {
            public string mensaje { get; set; }
            public DataTable data { get; set; }
        }
        public string validar(string dato, string tipo, int logitud, string tabla,string col)
        {
            mensaje = null;
            validarLogitud(dato, logitud);
            switch (tipo)
            {
                case "C":
                    if (col == "Tipo")
                    {
                        ValidarTipo(dato, tabla);
                    }
                    else if (col == "Parte" || col == "Clase" || col== "Clave")
                    {
                        ValidarClave(cl1.nombretabla, col, dato, 1);
                    }
                    else if (col == "Origen" || col == "Pais")
                    {
                        ValidarClave("paises", "pais_cve", dato, 0);
                    }
                    else if (col == "HTS Duty" || col == "HTS Expo" || col == "HTS Impo")
                    {
                        ValidarHTS(dato);
                    }
                    else if (col == "Preferencia")
                    {
                        ValidarClave("preferencias", "prf_clave", dato, 0);
                    }
                    break;
                case "D":
                    validarFecha(dato);
                    break;
                case "N":
                    validarNumero(dato, logitud, col);
                    break;
            }
            return mensaje;
        }
        public string validarLogitud(string dato, int logitud)
        {
            mensaje = null;
            if (dato.Equals(null)){
                mensaje = "Error dato vacio";
            }
            else if (dato.Length > logitud){
                mensaje = "Longitud maxima: "+logitud ;
            }
            return mensaje;
        }
        public string validarFecha(string dato)
        {
            mensaje = null;
            try
            {
                DateTime.Parse(dato);
            }catch{
                mensaje = "No cuenta con el formato de fecha correcto";
            }
            return mensaje;
        }
        public string validarNumero(string dato, int logitud, string col)
        {
            mensaje = null;
            Val = new Regex(@"[0-9]{1,9}(\.[0-9]{0,2})?$");
            if (col == "Fraccion" && dato.Length <= 0 || !Val.IsMatch(dato) || dato.Length > logitud)
            {
                mensaje = "No contiene el formato correcto";
            }
            else if (!Val.IsMatch(dato) && dato.Length > logitud){
                mensaje = "No contiene el formato correcto";
            }
            return mensaje;
        }
        public string ValidarTipo(string dato, string tabla)
        {
            mensaje = null;
            switch (tabla)
            {
                case "Customers and Vendors":
                    if (dato != "CS" && dato != "VN" && dato != "CR" && dato != "WH" && dato != "PL")
                    {
                        mensaje = "El dato: " + dato + " no es un codigo valido";
                    }
                    break;
                case "Finished Goods and Components":
                    if (dato != "R" && dato != "A" && dato != "S" && dato != "C")
                    {
                        mensaje = "El dato: " + dato + " no es un codigo valido";
                    }
                    break;
                case "Classes":
                    if (dato != "RM" && dato != "FG" && dato != "ME" && dato != "PK" && dato != "EC" && dato != "TO" && dato != "SP" && dato != "CO" && dato != "SC")
                    {
                        mensaje = "El dato: " + dato + " no es un codigo valido";
                    }
                    break;
            }
            return mensaje;
        }
        public string ValidarHTS(string dato)
        {
            mensaje = null;
            Val = new Regex(@"([0-9]{1,4})(\.[0-9]{0,2})(\.[0-9]{0,4})?$");
            if (!Val.IsMatch(dato) || dato.Length != 12){
                mensaje = "No contiene el formato correcto";
            }
            return mensaje;
        }
        public string ValidarClave(string tabla, string col, string dato, int op)
        {
            mensaje = null;
            if (!cl1.Buscarclave(tabla, col, dato, op))
            {
                if (op != 1){
                    mensaje = "La clave: " + dato + " no es valida para la columna";
                }
            }
            else {
                if (op==1){
                    mensaje = "La clave: " + dato + " ya se encuentra registrada";
                }
            }
            return mensaje;
        }
        public string validarepetidos(DataTable dtAll,string col)
        {
            mensaje = null;
            DataTable dtDistinct = new DataTable();
            dtDistinct = dtAll.DefaultView.ToTable(true, col);
            foreach (DataRow row in dtDistinct.Rows){
                mensaje = mensaje + row[0].ToString() + ", ";
            }
            mensaje = mensaje.Remove(mensaje.Length - 1);
            return mensaje;
        }
        public Ctabla ObtenerConsulta(string v)
        {
            mensaje = null;
            v1 = new Ctabla();
            try
            {
                v1.mensaje = mensaje; v1.data = cl1.MostrarTabla(v);
            }catch (Exception){
                v1.mensaje = "Error"; v1.data.Rows.Clear();
            }
            return v1;
        }
        //public void CargarArchivo(DataTable data)
        //{
        //    cl1.Insertar(data);
        //}
    }
}
