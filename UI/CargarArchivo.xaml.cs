using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using ExcelDataReader;
using FastMember;
using Microsoft.Win32;
using Process;
using SpreadsheetLight;

namespace UI
{
    public class Filtros
    {
        public string Name { get; set; }

        public bool ischeck { get; set; }
    }
    public class Errores
    {
        public string ID { get; set; }
        public int Renglon { get; set; }
        public string Columna { get; set; }
        public string Error { get; set; }
    }
    public partial class MainWindow : Window
    {
        public Process.Validaciones cl1 = new Validaciones();
        public string Strpath, mensaje, nombretabla, Colname;
        public int reglon = 1, col = 0,select;
        public DataTable dt, Excel;
        private List<DataTable> ListaDatos;
        private List<Filtros> filtro;
        private List<Errores> Tablaerrores;
        private IExcelDataReader reader;
        private OpenFileDialog openFileDialog;
        private ExcelDataSetConfiguration conf;
        private SaveFileDialog saveFile;
        private SLDocument oSLDocument;
        private DataTable dtDistinct;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ccbarchivos_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            select = ccbarchivos.SelectedIndex - 1;
        }

        private void btnfiltrar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Colname = (sender as Button).DataContext.ToString();
                popexcel.PlacementTarget = sender as Button;
                filtro = new List<Filtros>();
                dtDistinct = Excel.DefaultView.ToTable(true, Colname);
                foreach (DataRow item in dtDistinct.Rows)
                {
                    filtro.Add(new Filtros() { Name = item[0].ToString(), ischeck = false });
                }
                lbfilter.ItemsSource = filtro;
                popexcel.IsOpen = true;
            }
            catch (Exception er)
            {
                MessageBox.Show("Error "+er.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void btnrealizarfiltro_Click(object sender, RoutedEventArgs e)
        {
            string caption = "";
            foreach (Filtros item in filtro)
            {
                if (item.ischeck)
                {
                    caption += item.Name;
                }
            }
            tabla.ItemsSource = null;
            buscar(caption, Colname);
        }

        private void btntabla_Click(object sender, RoutedEventArgs e)
        {
            openFileDialog = new OpenFileDialog();
            openFileDialog.InitialDirectory = @"Desktop";
            openFileDialog.Filter = "Text files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true)
            {
                Strpath = openFileDialog.FileName;
                try
                {
                    using (var stream = File.Open(Strpath, FileMode.Open, FileAccess.Read)){
                        reader = ExcelDataReader.ExcelReaderFactory.CreateReader(stream);
                        conf = new ExcelDataSetConfiguration{
                            ConfigureDataTable = _ => new ExcelDataTableConfiguration{
                                UseHeaderRow = false
                            }
                        };
                        ccbarchivos.Items.Clear();
                        ccbarchivos.Items.Add("Seleccionar hoja Excel");
                        ListaDatos = new List<DataTable>();
                        for (int i = 0; i < reader.AsDataSet(conf).Tables.Count; i++){
                            ListaDatos.Add(reader.AsDataSet(conf).Tables[i]);
                            ccbarchivos.Items.Add(reader.AsDataSet(conf).Tables[i].TableName);
                        }
                        ccbarchivos.SelectedIndex = 0;
                    }
                }
                catch (Exception er)
                {
                    MessageBox.Show("Ocurrio un error " + er.Message);
                }
            }
        }

        private void btncargar_Click(object sender, RoutedEventArgs e)
        {
            if (ccbtabla.SelectedIndex != 0){
                seleccion();
                _ = MessageBox.Show("Terminado....", "Process", MessageBoxButton.OK, MessageBoxImage.Information);
                CrearArchivo(Excel);
            }
            else{
                _ = MessageBox.Show("Selecciona el tipo de archivo que se desea Cargar", "Error", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        public void seleccion()
        {
            tabla.ItemsSource = Validartabla(ListaDatos[select]);
            tabla.Visibility = Visibility.Visible;
        }

        public List<Errores> Validartabla(DataTable data)
        {
            Tablaerrores = new List<Errores>();
            nombretabla = (ccbtabla.SelectedItem as ComboBoxItem).Content as string;
            dt = new DataTable(); dt = cl1.ObtenerConsulta(nombretabla).data;
            try
            {
                reglon = 1;
                foreach (DataRow row in data.Rows)
                {
                    col = 0;
                    foreach (object item in row.ItemArray)
                    {
                        mensaje = cl1.validar(item.ToString(), dt.Rows[col]["clyd_col_dtype"].ToString(), Convert.ToInt32(dt.Rows[col]["clyd_col_len"].ToString()), nombretabla, dt.Rows[col][0].ToString());
                        if (mensaje != null)
                        {
                            Tablaerrores.Add(new Errores() { ID= nombretabla, Renglon=reglon, Columna= dt.Rows[col][0].ToString(), Error=mensaje });
                        }
                        col++;
                    }
                    reglon++;
                }
                Excel = new DataTable();
                using (ObjectReader reader = ObjectReader.Create(Tablaerrores, "ID", "Renglon", "Columna", "Error"))
                {
                    Excel.Load(reader);
                }
                return Tablaerrores;
            }
            catch (Exception er)
            {
                _ = MessageBox.Show("Error " + er.Message);
                return Tablaerrores;
            }
        }

        public void CrearArchivo(DataTable data)
        {
            saveFile = new SaveFileDialog
            {
                Filter = "Name of file (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                Title = "Save an Excel File"
            };
            saveFile.ShowDialog();
            if (saveFile.FileName != "")
            {
                Strpath = saveFile.FileName; string hoja = ListaDatos[select].TableName;
                oSLDocument = new SLDocument();
                oSLDocument.ImportDataTable(1, 1, data, true);
                oSLDocument.SaveAs(Strpath);
                System.Diagnostics.Process.Start(Strpath);
            }
        }

        public void buscar(string name, string colname)
        {
            
            switch (colname)
            {
                case "ID":
                    IEnumerable<Errores> sql1 = from Datos in Tablaerrores where Datos.ID == name select Datos; tabla.ItemsSource = sql1;
                    break;
                case "Renglon":
                    IEnumerable<Errores> sql2 = from Datos in Tablaerrores where Datos.Renglon.ToString() == name select Datos; tabla.ItemsSource = sql2;
                    break;
                case "Columna":
                    IEnumerable<Errores> sql3 = from Datos in Tablaerrores where Datos.Columna == name select Datos; tabla.ItemsSource = sql3;
                    break;
                case "Error":
                    IEnumerable<Errores> sql4 = from Datos in Tablaerrores where Datos.Error == name select Datos; tabla.ItemsSource = sql4;
                    break;
                default:
                    break;
            }

        }
    }
}
