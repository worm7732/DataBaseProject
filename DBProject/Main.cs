using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Controls;
using System.Diagnostics;

namespace DBProject
{
    public partial class Main : Form
    {
        String sql, database,path;
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

        void reset_in()
        {
            string[] reset_file = { };
            System.IO.File.WriteAllLines(@path + "in.txt", reset_file);
        }

        public Main()
        {
            InitializeComponent();
            //set variables
            database = "testDB.db";
            //command = "\"SELECT * FROM COMPANY;\"";
            sql = "";
            path = Directory.GetCurrentDirectory();
            int loc = path.IndexOf("bin");
            if (loc > 0)
            {
                path = path.Substring(0, loc);
            }
            else
            {
                Console.WriteLine("String bin not found: error");
            }
            //TODO: string getDBlist = dir /b /a-d | findstr ".db$"
            Console.Out.WriteLine("reseting in.txt");
            reset_in();

            // Set the column header style.
            //dataGridView1.Dock = DockStyle.Fill;
            //dataGridView1.Parent = splitContainer2.Panel2;
        }

        private void splitContainer2_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (sql.Length > 0)
            {
                Console.Out.WriteLine(sql);
                //run button
                startInfo.FileName = "cmd.exe";
                //create file to send to sqlite
                string[] write_to_in = { ".header on", sql};
                System.IO.File.WriteAllLines(@path + "in.txt", write_to_in);
                //set command to run
                startInfo.Arguments = "/C " + path + "sqlite3 " + path + database + " < " + path + "in.txt > " + path + "out.txt";
                startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                process.StartInfo = startInfo;
                //run command
                process.Start();
                process.WaitForExit();
                //populate result table
                fill_dataGrid(getResult());
                //reset in.txt and string sql
                //reset_in();
                //sql = "";
            }
            else
            {
                Console.Out.WriteLine("\'" + sql + "\' is not a valid sql command");
            }
        }

        private void fill_dataGrid(List<String> result)
        {
            char[] delim = {'|'};
            for (int i = 0; i < result.Count; i++ )
            {
                string[] elements = result.ElementAt(i).Split(delim);
 
                if (i == 0)
                {
                    //setup grid
                    dataGridView1.Rows.Clear();
                    dataGridView1.ColumnCount = elements.Length;
                    dataGridView1.ColumnHeadersVisible = true;
                    DataGridViewCellStyle columnHeaderStyle = new DataGridViewCellStyle();
                    columnHeaderStyle.BackColor = Color.Beige;
                    columnHeaderStyle.Font = new Font("Verdana", 10, FontStyle.Bold);
                    dataGridView1.ColumnHeadersDefaultCellStyle = columnHeaderStyle;
                    for (int j = 0; j < elements.Length; j++)
                    {
                        dataGridView1.Columns[j].Name = elements[j];
                    }
                }
                else
                {
                    dataGridView1.Rows.Add(elements);
                }
                //Console.Out.WriteLine(result.ElementAt(i) + "!!!!!!!!!!!!!!!!!!!!!!!!!");
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

            sql = richTextBox1.Text;
            //Console.Out.WriteLine(sql);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void Main_Load(object sender, EventArgs e)
        {

        }

        private List<String> getResult(){
            String line;
            List < String > content = new List<String>();
            System.IO.StreamReader file = new System.IO.StreamReader(path +"out.txt");
            while ((line = file.ReadLine()) != null)
            {
                content.Add(line);
            }
            file.Close();
            return content;
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }
    }
}
