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
        Parser queryParser = new Parser(); // parser
        Dictionary<string, DB_table> DBtables = new Dictionary<string, DB_table>(); // hashtable for DBtables
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
                string[] tables = strings.Split(' ');
                foreach (string table in tables)
                {
                    if (table.Length > 0)
                    {
                        arg = "/C " + path + "sqlite3 " + path + database + " \"PRAGMA table_info(" + table + ")\" > " + path + "out.txt";
                        //Console.Out.WriteLine(arg);
                        run_command(arg);

                        DBtables.Add(table, new DB_table(table, getResult()));
                        listBox1.Items.Add(table);

                        foreach (string[] s in DBtables[table].attributes.Values)
                        {
                            listBox1.Items.Add("    " + s[0]);
                        }

                     }
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

        //execute query
        private void button1_Click(object sender, EventArgs e)
        {
            if (sql.Length > 0)
            {
                //create file to send to sqlite
                string[] write_to_in = { ".header on", sql};
                System.IO.File.WriteAllLines(@path + "in.txt", write_to_in);
                //send command and run
                string arg = "/C " + path + "sqlite3 " + path + database + " < " + path + "in.txt > " + path + "out.txt";
                run_command(arg);
                //populate result table
                fill_dataGrid(getResult());
             
            }
            else
            {
                Console.Out.WriteLine("\'" + sql + "\' is not a valid sql command");
            }
        }

        private void fill_dataGrid(List<String> result)
        {
            QS.clear();
            if(queryParser.colorMapping.ContainsKey("*"))
            {
                //Console.Out.WriteLine(  "*           1111111111");
                QS.star = true;
            }

            if (result.Count > 0)
            {
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
                            string str = elements[j];
                            dataGridView1.Columns[j].Name = str;
                            
                            if (DB_table.allPK.ContainsKey(str))
                            {
                                QS.pkMap.Add(str, j);
                                //Console.Out.WriteLine(str + "           1111111111");
                            }
                            QS.attMap.Add(str, j);
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
            queryParser.clear();
            queryParser.parse(sql);
            foreach (string word in queryParser.colorMapping.Keys)
            {

                int myPosition = 0;
                while (((myPosition = richTextBox1.Find(word, myPosition, RichTextBoxFinds.None)) != -1))
                {
                    if (myPosition >= 0)
                    {
                        richTextBox1.SelectionStart = myPosition;
                        richTextBox1.SelectionLength = word.Length;
                        string check = queryParser.colorMapping[word];
                        //Console.Out.WriteLine(word + " " + check);
                        if (check == "keyword")
                        {
                            richTextBox1.SelectionColor = Color.Red;
                        }
                        else if (check == "table")
                        {
                            richTextBox1.SelectionColor = Color.Blue;
                        }
                        else if (check == "attribute")
                        {
                            richTextBox1.SelectionColor = Color.Green;
                        }
                        else if (check == "other")
                        {
                            richTextBox1.SelectionColor = Color.Black;
                        }
                    }
                    myPosition += word.Length;
                    Console.Out.WriteLine(myPosition + " " + sql.Length);
                    richTextBox1.SelectionStart = sql.Length;
                    richTextBox1.SelectionLength = 0;
                    richTextBox1.SelectionColor = Color.Black;
                }
            }
            
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
            string newSQL = "SELECT ";
            if (dataGridView1.Visible == false)
            {
                MessageBox.Show("Nothing is selected.");
            }
            else
            {
                if (QS.star)
                {
                    newSQL += "* FROM ";
                }
                if (queryParser.tables.Count == 1)
                {
                    //Console.Out.WriteLine(" one table           1111111111");
                    newSQL += queryParser.tables[0] + " ";
                }
                //setSelectedCells();      
                //whole rows in grid are selected
                if (dataGridView1.SelectedRows.Count > 0)
                {
                    newSQL += "WHERE ";
                    
                    for (int counter = 0; counter < (dataGridView1.SelectedRows.Count); counter++)
                    {
                        int row = dataGridView1.SelectedRows[counter].Index;
                        //primary key is in relation
                        if (QS.pkMap.Count == 1)
                        {
                            string key = "";
                            foreach (string temp in QS.pkMap.Keys)
                            {
                                key = temp;
                            }
                            //newSQL += key + " = " + dataGridView1.Rows[row].DataGridView.Columns[QS.pkMap[key]];
                            newSQL += key + " = " + dataGridView1[QS.pkMap[key],row].Value.ToString();
                        }
                    }
                }
                //items in grid are selected
                else
                {

                }
                Console.Out.WriteLine(newSQL + "           1111111111");
                newSQL += ";";
                sql = newSQL;
                richTextBox1.Text = sql;
                button1_Click(sender, e);

            }
        }


        private void setSelectedCells()
        {
            
            //get selected rows
            //selected_row_index.Clear();
            for (int counter = 0; counter < (dataGridView1.SelectedRows.Count); counter++)
            {
                //Console.Out.WriteLine(dataGridView1.SelectedCells[counter].RowIndex.ToString() + " 1111111111111111111111");
                string temp = dataGridView1.SelectedRows[counter].Index.ToString();
                //Console.Out.WriteLine(temp + "           1111111111");
                //selected_row_index.Add(temp);
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
