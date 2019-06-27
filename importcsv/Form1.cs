using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Data.SqlClient;
using Microsoft.VisualBasic.FileIO;


namespace importcsv
{
    public partial class Form1 : Form
    {

        string ConnStr = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=C:\USERS\HQ\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\CSV DATA.MDF;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

        public static string activeTable;
        public Form1()
        {
            InitializeComponent();
        }

        public void Button1_Click(object sender, EventArgs e)
        {

            OpenFileDialog ofd = new OpenFileDialog();
            ofd.DefaultExt = ".csv";
            ofd.Filter = "CSV files (*.csv)|*.csv";
            ofd.ShowDialog();

            txtFileName.Text = ofd.FileName;
        }

        private void Button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        String generateRandomString(int length)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            var stringChars = new char[length];
            var random = new Random();

            for (int i = 0; i < length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            return new String(stringChars);
        }

        String createTable(string[] headers, SqlConnection conn)
        {
            List<string> columns = new List<string>();
            foreach (string header in headers)
            {
                columns.Add(string.Format("{0} NVARCHAR(MAX)", header));
            }

            List<string> goodNames = new List<String>();



            String tableName = generateRandomString(10);
            activeTable = tableName;
            String query = string.Format("CREATE TABLE {0}({1});", tableName, string.Join(",", columns));
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.ExecuteNonQuery();
            return tableName;
        }

        void insertColumns(string tableName, string[] headers, string[] columns, SqlConnection connection)
        {
            if (headers.Length != columns.Length)
            {
                throw new System.ArgumentException("Length of headers must be the same as columns length");
            }

            List<string> headersWithAtSign = new List<string>(headers).Select(header => "@" + header).ToList();

            String query = string.Format("INSERT INTO {0} ({1}) VALUES ({2});", tableName, string.Join(",", headers), string.Join(",", headersWithAtSign));
            SqlCommand cmd = new SqlCommand(query, connection);
            for (int i = 0; i < columns.Length; i++)
            {
                cmd.Parameters.AddWithValue("@" + headers[i], columns[i]);
            }

            cmd.ExecuteNonQuery();


        }

        void ReadAndImport()
        {
            using (TextFieldParser tfp = new TextFieldParser(txtFileName.Text))
            {
                if (tfp.EndOfData)
                {
                    Console.WriteLine("File is empty");
                    return;
                }

                using (SqlConnection conn = new SqlConnection(@"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=C:\USERS\HQ\APPDATA\LOCAL\MICROSOFT\MICROSOFT SQL SERVER LOCAL DB\INSTANCES\MSSQLLOCALDB\CSV DATA.MDF;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False"))
                {
                    conn.Open();

                    //SqlCommand cmd = new SqlCommand("CREATE TABLE test(First_Name NVARCHAR(MAX), Birth_Date NVARCHAR(MAX));", conn);
                    //cmd.ExecuteNonQuery();
                    tfp.TextFieldType = FieldType.Delimited;
                    tfp.SetDelimiters(";");
                    List<string[]> temp = new List<string[]>();

                    // NEW
                    int headersCount = tfp.ReadFields().Length;
                    string[] headers = Enumerable.Range(0, headersCount).Select(i => "column_" + i).ToArray();
                    string tableName = createTable(headers, conn);
                    //

                    while (!tfp.EndOfData)
                    {
                        string[] fields = tfp.ReadFields();
                        temp.Add(fields);
                        insertColumns(tableName, headers, fields, conn); // NEW

                    }
                }
            }
        }
        private void BtnImport_Click(object sender, EventArgs e)
        {
            ReadAndImport();
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            FillTable();
        }

        // Показать таблицу
        private void FillTable()
        {
            string SqlText = string.Format("SELECT * FROM {0};", activeTable);
            SqlDataAdapter da = new SqlDataAdapter(SqlText, ConnStr);
            DataSet ds = new DataSet();
            da.Fill(ds, activeTable);
            dataGridView1.DataSource = ds.Tables[activeTable].DefaultView;
        }
        private void dataGridView1_Click(object sender, EventArgs e)
        {
            FillTable();
        }

        private void PrintDocument1_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            Bitmap bmp = new Bitmap(dataGridView1.Size.Width+10, dataGridView1.Size.Height+10);
            dataGridView1.DrawToBitmap(bmp, dataGridView1.Bounds);
            e.Graphics.DrawImage(bmp, 0, 0);
        }

        private void Button1_Click_1(object sender, EventArgs e)
        {
            printDocument1.DefaultPageSettings.Landscape = true;
            printDocument1.Print();
        }
    }
}