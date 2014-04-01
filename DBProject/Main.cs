using System;
using System.IO;
using System.Collections.Generic;
using System.Collections;
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
        String sql, database, path;
        List<String> database_names;// = new List<String>(); // names of databases
        bool is_there_a_DB = false;
        List<String> table_names = new List<String>(); //table names in database
        Dictionary<string, DB_table> tables = new Dictionary<string, DB_table>(); // hashtable for tables
        List<String> selected_row_index = new List<String>();
        List<String> selected_column_index = new List<String>();
        List<String> current_columns = new List<String>();
        QueryState QS = new QueryState();

        private void reset_in_out()
        {
            string[] reset_file = { };
            System.IO.File.WriteAllLines(@path + "in.txt", reset_file);
            System.IO.File.WriteAllLines(@path + "out.txt", reset_file);
        }

        private void find_DB_files()
        {
            string arg = "/C " + path + "getDB.bat";
            run_command(arg);
            database_names = getResult();
            
        }

        private void load_DB()
        {
            string arg = "/C " + path + "sqlite3 " + path + database + " .tables > " + path + "out.txt";
            run_command(arg);
            foreach(string strings in getResult()){
                string[] strs = strings.Split(' ');
                foreach (string s in strs)
                {
                    if (s.Length > 0)
                    {
                        //Console.Out.WriteLine(s + " 1111111");
                        table_names.Add(s);
                    }
                }
            }
            
           
            foreach (string str in table_names)
            {

                //Console.Out.WriteLine(str + " !!!!!!!!!!!!!!!!!!");
                arg = "/C " + path + "sqlite3 " + path + database + " \"PRAGMA table_info(" + str + ")\" > " + path + "out.txt";
                //Console.Out.WriteLine(arg);
                run_command(arg);

                tables.Add(str, new DB_table(str, getResult()));
                listBox1.Items.Add(str);

                foreach (string[] s in tables[str].attributes.Values)
                {
                    listBox1.Items.Add("    " + s[0]);
                }

            }
        }

        public Main()
        {
            InitializeComponent();
            //set variables
            //get path
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
            //clear in.txt and out.txt
            reset_in_out();
            // get all db files
            find_DB_files();
            //set default DB
            if (database_names.Count > 0)
            {
                database = database_names.ElementAt(0);
                textBox1.Text = "Current database is " + database;
                is_there_a_DB = true;
            }
            else
            {
                textBox1.Text = "There are no databases available!!!!!!!!!!!!";
            }
            
            Size size = TextRenderer.MeasureText(textBox1.Text, textBox1.Font);
            textBox1.Width = size.Width;
            textBox1.Height = size.Height;
            load_DB();
            //set sql command to null
            sql = "";
            //listBox1.Items.Add("COMPANY");
            

        }

        private void splitContainer2_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void run_command(string arg)
        {
            //set command to run
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = arg;
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            process.StartInfo = startInfo;
            //run command
            process.Start();
            process.WaitForExit();
        }

        private void setQS()
        {
            QS.table.Add("COMPANY");
            QS.star = true;
            QS.select = true;
            QS.join = false;
            QS.where = false;
        }

        //execute query
        private void button1_Click(object sender, EventArgs e)
        {
            if (sql.Length > 0)
            {
                //Console.Out.WriteLine(sql);
                //run button
                //create file to send to sqlite
                string[] write_to_in = { ".header on", sql};
                System.IO.File.WriteAllLines(@path + "in.txt", write_to_in);
                //send command and run
                string arg = "/C " + path + "sqlite3 " + path + database + " < " + path + "in.txt > " + path + "out.txt";
                run_command(arg);
                //populate result table
                //setQS();
                fill_dataGrid(getResult());
                //TODO: reset in.txt and string sql
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


            if (result.Count > 0)
            {
                current_columns.Clear();
                char[] delim = { '|' };
                for (int i = 0; i < result.Count; i++)
                {
                    string[] elements = result.ElementAt(i).Split(delim);

                    if (i == 0)
                    {
                        //setup grid
                        dataGridView1.Rows.Clear();
                        dataGridView1.ColumnCount = elements.Length;
                        dataGridView1.Visible = true;
                        dataGridView1.ColumnHeadersVisible = true;
                        DataGridViewCellStyle columnHeaderStyle = new DataGridViewCellStyle();
                        columnHeaderStyle.BackColor = Color.Beige;
                        columnHeaderStyle.Font = new Font("Verdana", 10, FontStyle.Bold);
                        dataGridView1.ColumnHeadersDefaultCellStyle = columnHeaderStyle;
                        for (int j = 0; j < elements.Length; j++)
                        {
                            dataGridView1.Columns[j].Name = elements[j];
                            //Console.Out.WriteLine(elements[j] + "  11111111111111111");
                            //QS.column.Add(elements[j]);
                            //foreach (string s in QS.table)
                            //{
                            //    if (tables[s].primary_keys.Contains(elements[j]))
                            //    {
                            //        QS.pk.Add(elements[j]);
                            //    }
                            //}
                        }
                    }
                    else
                    {
                        dataGridView1.Rows.Add(elements);
                    }
                    //Console.Out.WriteLine(result.ElementAt(i) + "!!!!!!!!!!!!!!!!!!!!!!!!!");
                }
            }
            else
            {
                dataGridView1.Rows.Clear();
                dataGridView1.Visible = false;
                MessageBox.Show("This query returned no results");
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

        private void openToolStripMenuItem1_Click(object sender, EventArgs e)
        {

            using (Open_Database form = new Open_Database(database_names))
            {
                form.ShowDialog();
                if (form.DB_selected.Length > 0)
                {
                    database = form.DB_selected;
                    textBox1.Text = "Current database is " + database;
                    is_there_a_DB = true;
                    Size size = TextRenderer.MeasureText(textBox1.Text, textBox1.Font);
                    textBox1.Width = size.Width;
                    textBox1.Height = size.Height;
                    dataGridView1.Rows.Clear();
                    dataGridView1.Visible = false;
                }

            }


            
        }

        private void createDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (Create_Database form = new Create_Database(database_names))
            {
                form.ShowDialog();
                database = form.DB_name;
                textBox1.Text = "Current database is " + database;
                is_there_a_DB = true;
                is_there_a_DB = true;
                Size size = TextRenderer.MeasureText(textBox1.Text, textBox1.Font);
                textBox1.Width = size.Width;
                textBox1.Height = size.Height;
                dataGridView1.Rows.Clear();
                dataGridView1.Visible = false;
                string arg = "/C " + path + "sqlite3 " + path + database + " < " + path + "makeDB.txt";
                run_command(arg);
                database_names.Add(database);
            }
        }

        //reverse engineer
        private void button2_Click(object sender, EventArgs e)
        {
            if (dataGridView1.Visible == false)
            {
                MessageBox.Show("Nothing is selected.");
            }
            else
            {
                setSelectedCells();      
                //whole rows in grid are selected
                if (selected_row_index.Count > 0)
                {
                    foreach (string s in selected_row_index)
                    {
                        
                    }
                }
                //items in grid are selected
                else
                {

                }
            }
        }


        private void setSelectedCells()
        {
            
            Console.Out.WriteLine("Selection changed!!!!!!!!!!!!!!!!!!!!!!");
           
            //get current columns and pk's and tables
            for (int counter = 0; counter < (dataGridView1.Columns.Count); counter++)
            {
                //Console.Out.WriteLine(dataGridView1.Columns[counter].Name.ToString() + " 1111111111111111111111");
                string temp = dataGridView1.Columns[counter].Name.ToString();
                QS.column.Add(temp);
                if (DB_table.allPK.ContainsKey(temp))
                {
                    QS.pk.Add(temp);
                    QS.table.Add(DB_table.allPK[temp]);
                    Console.Out.WriteLine("pk = " + temp);
                    Console.Out.WriteLine("teble = " + DB_table.allPK[temp]);
                }
            }

            //check if its a star or not
            foreach(string str in QS.table)
            {
                if (tables[str].attributes.Count == QS.column.Count)
                {
                    QS.star = true;
                    foreach(string s in QS.column)
                    {
                        if (!tables[str].attributes.ContainsKey(s))
                        {
                            QS.star = false;
                        }
                    }
                }
            }
            Console.Out.WriteLine(QS.star);

            //get selected rows
            selected_row_index.Clear();
            for (int counter = 0; counter < (dataGridView1.SelectedRows.Count); counter++)
            {
                //Console.Out.WriteLine(dataGridView1.SelectedCells[counter].RowIndex.ToString() + " 1111111111111111111111");
                string temp = dataGridView1.SelectedCells[counter].RowIndex.ToString();
                Console.Out.WriteLine(temp + "           1111111111");
                selected_row_index.Add(temp);
            }


            //probably not needed
            for (int counter = 0; counter < (dataGridView1.SelectedColumns.Count); counter++)
            {
                //Console.Out.WriteLine(dataGridView1.SelectedCells[counter].RowIndex.ToString() + " 1111111111111111111111");
                selected_column_index.Add(dataGridView1.SelectedCells[counter].RowIndex.ToString());
            }
            

        }

    }
}
