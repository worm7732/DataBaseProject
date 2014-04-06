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
        QueryState QS = new QueryState();
        List<String> possibleQueries= new List<String>();
        //List<String> current_columns = new List<String>();
        Dictionary<int, HashSet<int>> selectedCells = new Dictionary<int, HashSet<int>>();
        //HashSet<int> selected = new HashSet<int>();
        
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
                            
                            if (DB_table.allPK.ContainsKey(str.ToLower()))
                            {
                                QS.pkMap.Add(str, j);
                                //Console.Out.WriteLine(str + "           1111111111");
                            }
                            if (QS.attMap.ContainsKey(str))
                            {
                                QS.attMap.Add(str, j);
                                //Console.Out.WriteLine(str + "           1111111111");
                            }
                            
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
            int save = richTextBox1.SelectionStart;
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
                    if (myPosition == sql.Length)
                    {
                        break;
                    }
                    //Console.Out.WriteLine(myPosition + " " + sql.Length);
                    richTextBox1.SelectionStart = save;
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
            possibleQueries.Clear();
            string newSQL = "SELECT ";
            if (dataGridView1.Visible == false)
            {
                MessageBox.Show("Nothing is selected.");
            }
            else
            {
                //whole rows in grid are selected
                //Console.Out.WriteLine(dataGridView1.SelectedRows.Count);
                if (dataGridView1.SelectedRows.Count > 0)
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

                    newSQL += "WHERE ";
                    
                    for (int counter = 0; counter < (dataGridView1.SelectedRows.Count); counter++)
                    {
                        //Console.Out.WriteLine(counter + "      1111111)");
                        if (counter > 0)
                        {
                            newSQL += "or ";
                        }
                        int row = dataGridView1.SelectedRows[counter].Index;
                        //primary key is in relation
                       // Console.Out.WriteLine(QS.pkMap.Count + "      pkmapcount)");
                        if (QS.pkMap.Count == 1)
                        {
                            string key = "";
                            foreach (string temp in QS.pkMap.Keys)
                            {
                                
                                key = temp;
                            }
                            //newSQL += key + " = " + dataGridView1.Rows[row].DataGridView.Columns[QS.pkMap[key]];
                            newSQL += key + " = " + dataGridView1[QS.pkMap[key],row].Value.ToString() + " ";
                    
                        }
                    }
                    
                }
                //items in grid are selected
                else
                {
                    setSelectedCells();
                    if (checkCells())
                    {
                        bool notfirst = false;
                        string currentTable = "";
                        int pkInt = -1;
                        string pkString = "";
                        foreach (int i in selectedCells.ElementAt(0).Value)
                        {
                            //Console.Out.WriteLine(dataGridView1.Columns[i].Name);
                            if (notfirst)
                            {
                                newSQL += ", ";
                            }
                            newSQL += dataGridView1.Columns[i].Name + " ";
                            notfirst = true;
                            if (DB_table.allPK.ContainsKey(dataGridView1.Columns[i].Name.ToLower()))
                            {
                                pkString = dataGridView1.Columns[i].Name.ToLower();
                                pkInt = i;
                            }
                        }
                        newSQL += "FROM ";
                        if (queryParser.tables.Count == 1)
                        {
                            //Console.Out.WriteLine(" one table           1111111111");
                            currentTable = queryParser.tables[0];
                            newSQL += currentTable + " ";
                        }
                        newSQL += "WHERE ";
                        //pk is in relation
                        if (pkInt > -1)
                        {
                            notfirst = false;
                            foreach(int row in selectedCells.Keys)
                            {
                                if (notfirst)
                                {
                                    newSQL += " or ";
                                }
                                newSQL += pkString + " = " + dataGridView1[pkInt, row].Value.ToString() + " ";
                                notfirst = true;
                            }
                        }
                            //need to find pk value
                        else
                        {
                            //get choices of tables
                            List<String> table_choices = new List<String>();
                            foreach(DB_table table in DBtables.Values)
                            {
                                bool choice = true;
                                foreach (int col in selectedCells.ElementAt(0).Value)
                                {
                                    if (!table.attributes.ContainsKey(dataGridView1.Columns[col].Name.ToLower()))
                                    {
                                        choice = false;
                                    }
                                }
                                if (choice)
                                {
                                    table_choices.Add(table.table_name);
                                }
                            }

                            Console.Out.WriteLine("Number of table choices = " + table_choices.Count);
                            // get pk of selected rows
                            if (table_choices.Count == 1)
                            {
                                Console.Out.WriteLine("Table choices = " + table_choices[0]);
                                pkString = DBtables[table_choices[0]].primary_keys[0];
                                bool needOR = false;
                                string extraSQL = "SELECT " + pkString + " FROM " + table_choices[0] + " WHERE ";
                                string where = "";
                                foreach (int row in selectedCells.Keys)
                                {
                                    if (needOR)
                                    {
                                        where += " or ";
                                    }
                                    where += "(";
                                    notfirst = false;
                                    foreach (int col in selectedCells[row])
                                    {
                                        if (notfirst)
                                        {
                                            where += " and ";
                                        }
                                        //newSQL += pkString + " = " + DBtables[table_choices[0]].attributes[pkString][0] + " ";
                                        where += dataGridView1.Columns[col].Name.ToLower() + " = ";
                                        Console.Out.WriteLine(DBtables[table_choices[0]].attributes[dataGridView1.Columns[col].Name.ToLower()][1]);
                                        if (DBtables[table_choices[0]].attributes[dataGridView1.Columns[col].Name.ToLower()][1] == "text")
                                        {
                                            where += "\"" + dataGridView1[col, row].Value.ToString() + "\"";
                                        }
                                        else
                                        {
                                            where += dataGridView1[col, row].Value.ToString();
                                        }
                                        notfirst = true;
                                    }
                                    where += ")";
                                    needOR = true;
                                }
                                //newSQL += where;
                                extraSQL += where.Replace("\"","\\\"") + ";";
                                string arg = "/C " + path + "sqlite3 " + path + database + " \"" + extraSQL + "\" > " + path + "out.txt";
                                //Console.Out.WriteLine(arg);
                                run_command(arg);
                                string finalSQL = newSQL + pkString + " = ";
                                
                                notfirst = false;
                                foreach (string str in getResult())
                                {
                                    if (notfirst)
                                    {
                                        finalSQL += " or ";
                                    }
                                    notfirst = true;
                                    finalSQL += str;
                                }
                                Console.Out.WriteLine(finalSQL);
                                newSQL += where;
                                newSQL = finalSQL;
                            }
                        }

                    }
                    else
                    {
                        MessageBox.Show("Invalid selection: Please try again.");
                    }
                }
                Console.Out.WriteLine(newSQL + "           new sql!");
                newSQL += ";";
                possibleQueries.Add(newSQL);
                sql = newSQL;
                richTextBox1.Text = sql;
                button1_Click(sender, e);
            }
        }


        private void setSelectedCells()
        {
            selectedCells.Clear();
            for (int row = 0; row < dataGridView1.RowCount - 1; row++)
            {
                HashSet<int> add = new HashSet<int>();
                for (int col = 0; col < dataGridView1.ColumnCount - 1; col++)
                {
                   
                    //Console.Out.WriteLine(dataGridView1[col, row].Value.ToString());
                    if (dataGridView1[col, row].Selected == true)
                    {
                        add.Add(col);
                    }
                }
                if (add.Count > 0)
                {
                    selectedCells.Add(row, add);
                }
            }

            //checkCells();

        }

        private bool checkCells()
        {
            HashSet<int> check = new HashSet<int>();
            bool first = true;
            foreach (int row in selectedCells.Keys)
            {
                Console.Out.Write("row: " + row + "    columns: ");
                foreach (int col in selectedCells[row])
                {
                    if (first)
                    {
                        check.Add(col);
                    }
                    else
                    {
                        if (!check.Contains(col))
                        {
                            return false;
                        }
                    }
                    Console.Out.Write(col + " ");
                }
                Console.Out.WriteLine();
                first = false;
            }
            return true;
        }

    }
}
