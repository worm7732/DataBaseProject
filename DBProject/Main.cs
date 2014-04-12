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
        bool join = false, joinOP = false;
        String sql, sql2, database, path;
        List<String> database_names;// = new List<String>(); // names of databases
        Parser queryParser = new Parser(); // parser
        Parser queryParser2 = new Parser(); // parser
        Dictionary<string, DB_table> DBtables = new Dictionary<string, DB_table>(); // hashtable for DBtables
        QueryState QS = new QueryState();
        QueryState QS2 = new QueryState();
        List<String> possibleQueries= new List<String>();
        List<String> possibleQueries2 = new List<String>();
        //List<String> current_columns = new List<String>();
        Dictionary<int, HashSet<int>> selectedCells = new Dictionary<int, HashSet<int>>();
        Dictionary<int, HashSet<int>> selectedCells2 = new Dictionary<int, HashSet<int>>();
        Dictionary<string, long> queryTime = new Dictionary<string, long>();
        //HashSet<int> selected = new HashSet<int>();
        string previousQuery = "";

        int totalsize = 0;
        string currentTable = "";
        string pkString = "";
        int pkInt = 0;
        string pkQUERY = "";

        private void reset_in_out()
        {
            string[] reset_file = { };
            System.IO.File.WriteAllLines(@path + "in.txt", reset_file);
            System.IO.File.WriteAllLines(@path + "out.txt", reset_file);
        }

        private void find_DB_files()
        {
            string arg = "/C " + path + "getDB.bat";
            //Console.Out.WriteLine(arg);
            run_command(arg);
            database_names = getResult();
            
        }

        private void load_DB()
        {
            string arg = "/C " + path + "sqlite3 " + path + database + " .tables > " + path + "out.txt";
            //Console.Out.WriteLine(arg);
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
            sql2 = "";
            //dataGridView2.Visible = false;
            //richTextBox2.Visible = false;
            splitContainer2.SplitterDistance = splitContainer1.Right;
            splitContainer4.Visible = false ;
            button3.Visible = false;
            

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

        public void run_query(string SQLquery, System.Windows.Forms.DataGridView grid_view, QueryState qs, Parser qParser)
        {
            if (SQLquery.Length > 0)
            {
                //create file to send to sqlite
                string[] write_to_in = { ".header on", SQLquery };
                System.IO.File.WriteAllLines(@path + "in.txt", write_to_in);
                //send command and run
                string arg = "/C " + path + "sqlite3 " + path + database + " < " + path + "in.txt > " + path + "out.txt";
                run_command(arg);
                //populate result table
                //fill_dataGrid(getResult(), grid_view, qs, qParser);

            }
            else
            {
                Console.Out.WriteLine("\'" + SQLquery + "\' is not a valid sql command");
            }
        }

        //execute query
        private void button1_Click(object sender, EventArgs e)
        {
           
            sql = richTextBox1.Text;
            if (sql.Length > 0)
            {
                //Console.Out.WriteLine("running left side");
                run_query(sql, dataGridView1, QS, queryParser);
                fill_dataGrid(getResult(), dataGridView1, QS, queryParser);
                previousQuery = sql;
            }
            sql2 = richTextBox2.Text;
           // Console.Out.WriteLine(sql2.Length + " " + sql2.Length);
            if (sql2.Length > 0 && join)
            {
                //Console.Out.WriteLine("running right side");
                run_query(sql2, dataGridView2, QS2, queryParser2);
                fill_dataGrid(getResult(), dataGridView2, QS2, queryParser2);
                previousQuery = sql2;
            }
            
        }

        private void fill_dataGrid(List<String> result, System.Windows.Forms.DataGridView grid_view, QueryState qs, Parser qParser)
        {
            qs.clear();
            if(qParser.colorMapping.ContainsKey("*"))
            {
                //Console.Out.WriteLine(  "*           1111111111");
                qs.star = true;
            }

            if (result.Count > 0)
            {
                char[] delim = { '|' };
                grid_view.AllowUserToAddRows = true;
                totalsize = result.Count;
               // Console.Out.WriteLine("selection changed      " + totalsize);
                for (int i = 0; i < result.Count; i++)
                {
                    string[] elements = result.ElementAt(i).Split(delim);

                    if (i == 0)
                    {
                        //setup grid
                        grid_view.Rows.Clear();
                        grid_view.ColumnCount = elements.Length;
                        grid_view.Visible = true;
                        grid_view.ColumnHeadersVisible = true;
                        DataGridViewCellStyle columnHeaderStyle = new DataGridViewCellStyle();
                        columnHeaderStyle.BackColor = Color.Beige;
                        columnHeaderStyle.Font = new Font("Verdana", 10, FontStyle.Bold);
                        grid_view.ColumnHeadersDefaultCellStyle = columnHeaderStyle;
                        for (int j = 0; j < elements.Length; j++)
                        {
                            string str = elements[j];
                            grid_view.Columns[j].Name = str;
                            
                            if (DB_table.allPK.ContainsKey(str.ToLower()))
                            {
                                qs.pkMap.Add(str, j);
                                //Console.Out.WriteLine(str + "           1111111111");
                            }
                            if (qs.attMap.ContainsKey(str))
                            {
                                qs.attMap.Add(str, j);
                                //Console.Out.WriteLine(str + "           1111111111");
                            }
                            
                        }
                    }
                    else
                    {
                        grid_view.Rows.Add(elements);
                    }
                    //Console.Out.WriteLine(result.ElementAt(i) + "!!!!!!!!!!!!!!!!!!!!!!!!!");
                }
            }
            else
            {
                //grid_view.Visible = false;
                //grid_view.AllowUserToAddRows = true;
                grid_view.Rows.Clear();
                grid_view.ColumnCount = 0;
                MessageBox.Show("No result was returned.");
                
            }
        }

        private string fillSQLOption(List<String> possibleQ, System.Windows.Forms.DataGridView grid_view)
        {
            //setup grid
            grid_view.AllowUserToAddRows = false;
            
            grid_view.Rows.Clear();
            grid_view.ColumnCount = 2;
            //grid_view.Columns[0].au;
            grid_view.Visible = true;
            grid_view.ColumnHeadersVisible = true;
            DataGridViewCellStyle columnHeaderStyle = new DataGridViewCellStyle();
            columnHeaderStyle.BackColor = Color.Beige;
            columnHeaderStyle.Font = new Font("Verdana", 10, FontStyle.Bold);
            grid_view.ColumnHeadersDefaultCellStyle = columnHeaderStyle;
            grid_view.Columns[0].Name = "Query Options";
            grid_view.Columns[1].Name = "Query Time (sec)";
            float fin = 10000;
            string result = "";
            foreach (string str in possibleQ)
            {
                string[] add = {"",""};
                if(queryTime.Keys.Contains(str)){
                    add[0] = str;
                    add[1] = queryTime[str].ToString();
                }else{
                    add[0] = str;
                    string arg = "/C " + path + "sqlite3 " + path + database + " \"" + str + "\" > " + path + "out.txt";
                    //DateTime dt1 = new DateTime(DateTime.MaxValue.Ticks);
                    //Console.Out.Write(str + "    ");
                    DateTime dt1 = DateTime.Now;
                    float t1 = dt1.Second + (float)((float)dt1.Millisecond/1000);
                    //Console.Out.Write(t1 + "    ");
                    run_command(arg);
                    //Console.Out.WriteLine(getResult().Count + "    SIZE  ");
                    //Console.Out.WriteLine(arg);
                    dt1 = DateTime.Now;
                    float t2 = dt1.Second + (float)((float)dt1.Millisecond/1000);
                    //Console.Out.WriteLine((t2 - t1).ToString() + "    " + str);
                    add[1] = (t2 - t1).ToString();

                    if ((t2 - t1) < fin)
                    {
                        fin = (t2 - t1);
                        result = str;
                    }
                    if ((t2 - t1) == fin)
                    {
                        if (str.Length < result.Length)
                        {
                            result = str;
                        }
                    }
                }
                grid_view.Rows.Add(add);
            }
            grid_view.Sort(grid_view.Columns[1], ListSortDirection.Ascending);
            grid_view.AutoResizeColumns();
            //Console.Out.WriteLine(grid_view[0, 1].Value.ToString());

            return result;
        }

        private string changedText(object sender, EventArgs e, System.Windows.Forms.RichTextBox rtb, Parser qParser)
        {
            string SQL = "";
            int save = rtb.SelectionStart;
            SQL = rtb.Text;
           // qParser.clear();
            qParser.parse(SQL);
            foreach (string word in qParser.colorMapping.Keys)
            {

                int myPosition = 0;
                while (((myPosition = rtb.Find(word, myPosition, RichTextBoxFinds.None)) != -1))
                {
                    if (myPosition >= 0)
                    {
                        rtb.SelectionStart = myPosition;
                        rtb.SelectionLength = word.Length;
                        string check = qParser.colorMapping[word];
                        //Console.Out.WriteLine(word + " " + check);
                        if (check == "keyword")
                        {
                            rtb.SelectionColor = Color.Red;
                        }
                        else if (check == "table")
                        {
                            rtb.SelectionColor = Color.Blue;
                        }
                        else if (check == "attribute")
                        {
                            rtb.SelectionColor = Color.Green;
                        }
                        else if (check == "other")
                        {
                            rtb.SelectionColor = Color.Black;
                        }
                    }
                    myPosition += word.Length;
                    if (myPosition == SQL.Length)
                    {
                        break;
                    }
                    //Console.Out.WriteLine(myPosition + " " + SQL.Length);
                    rtb.SelectionStart = save;
                    rtb.SelectionLength = 0;
                    rtb.SelectionColor = Color.Black;
                }
            }
            return SQL;
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

        private string sqlStart(Parser qParser, Dictionary<int, HashSet<int>> sCells, System.Windows.Forms.DataGridView grid_view)
        {
            string newSQL = "SELECT ";
            bool notfirst = false;
            string tablePART = "";
            currentTable = "";
            //determine table
            Console.Out.WriteLine(qParser.tables.Count + "        look here for error" );
            if (qParser.tables.Count == 1)
            {
                currentTable = qParser.tables[0];
            }
            //build attr list
           // Console.Out.WriteLine(sCells.ElementAt(0).Value.Count + "       look here!    " + currentTable);
            //Console.Out.WriteLine(DBtables[currentTable].attributes.Count + "       look here" );
            int tableCount = 0;
            if (DBtables.Keys.Contains(currentTable))
            {
                tableCount = DBtables[currentTable].attributes.Count;
            }
            if (tableCount == sCells.ElementAt(0).Value.Count)
            {
                tablePART = " * ";
            }
            else
            {
                foreach (int i in sCells.ElementAt(0).Value)
                {
                    //Console.Out.WriteLine(dataGridView1.Columns[i].Name);
                    if (notfirst)
                    {
                        tablePART += ", ";
                    }
                    tablePART += grid_view.Columns[i].Name + " ";
                    notfirst = true;
                    //Console.Out.WriteLine(grid_view.Columns[i].Name + "      !!!!!!!!!!!");
                    if (DB_table.allPK.ContainsKey(grid_view.Columns[i].Name.ToLower()))
                    {
                        pkString = grid_view.Columns[i].Name.ToLower();
                        pkInt = i;
                    }
                }
            }

            if (qParser.colorMapping.Keys.Contains("join"))
            {
                newSQL += tablePART + " FROM (" + previousQuery + ") WHERE ";
            }
            else
            {
                newSQL += tablePART + " FROM " + currentTable + " WHERE ";
            }
            return newSQL;
        }

        private void primaryKeyOptions(Parser qParser, Dictionary<int, HashSet<int>> sCells,
            System.Windows.Forms.DataGridView grid_view, List<String> possibleQ)
        {
            pkString = "";
            currentTable = "";
            pkInt = -1;
            string newSQL = sqlStart(qParser,sCells,grid_view);
            bool notfirst = false;
            
            //pk is in relation
            if (pkInt > -1)
            {
                notfirst = false;
                foreach (int row in sCells.Keys)
                {
                    if (notfirst)
                    {
                        newSQL += " or ";
                    }
                    newSQL += pkString + " = " + grid_view[pkInt, row].Value.ToString() + " ";
                    notfirst = true;
                }
                possibleQ.Add(newSQL.Trim() + ";");
                pkQUERY = newSQL + ";";
            }
            //need to find pk value
            else
            {
                //get choices of tables
                List<String> table_choices = new List<String>();
                foreach (DB_table table in DBtables.Values)
                {
                    bool choice = true;
                    foreach (int col in sCells.ElementAt(0).Value)
                    {
                        if (!table.attributes.ContainsKey(grid_view.Columns[col].Name.ToLower()))
                        {
                            choice = false;
                        }
                    }
                    if (choice)
                    {
                        table_choices.Add(table.table_name);
                    }
                }

                // get pk of selected rows
                if (table_choices.Count == 1)
                {
                    pkString = DBtables[table_choices[0]].primary_keys[0];
                    bool needOR = false;
                    //where clause
                    string where = "";
                    foreach (int row in sCells.Keys)
                    {
                        if (needOR)
                        {
                            where += " or ";
                        }
                        where += "(";
                        notfirst = false;
                        for (int col = 0; col < grid_view.ColumnCount; col ++ )
                        {
                            if (notfirst)
                            {
                                where += " and ";
                            }
                            where += grid_view.Columns[col].Name.ToLower() + " = ";
                            string type = DBtables[table_choices[0]].attributes[grid_view.Columns[col].Name.ToLower()][1];
                            //Console.Out.WriteLine(type);
                            if (type != "int")// || type = )
                            {
                                where += "\"" + grid_view[col, row].Value.ToString() + "\"";
                            }
                            else
                            {
                                where += grid_view[col, row].Value.ToString();
                            }
                            notfirst = true;
                        }
                        where += ")";
                        needOR = true;
                    }

                    string extraSQL = "SELECT " + pkString + " FROM " + table_choices[0] + " WHERE ";
                    extraSQL += where.Replace("\"", "\\\"") + ";";
                    string arg = "/C " + path + "sqlite3 " + path + database + " \"" + extraSQL + "\" > " + path + "out.txt";
                    //Console.Out.WriteLine(arg);
                    run_command(arg);
                    string finalSQL = newSQL;

                    notfirst = false;
                    foreach (string str in getResult())
                    {
                        if (notfirst)
                        {
                            finalSQL += " or ";
                        }
                        notfirst = true;
                        finalSQL += pkString + " = " + str;
                    }
                    // Console.Out.WriteLine(finalSQL);
                    newSQL += where.Trim() + ";";
                    possibleQ.Add(newSQL);
                    newSQL = finalSQL.Trim() + ";";
                    possibleQ.Add(newSQL);
                    pkQUERY = finalSQL; 
                }
            }
        }


        private bool insertEntry( Parser qParser, Dictionary<int, HashSet<int>> sCells,
            System.Windows.Forms.DataGridView grid_view, System.Windows.Forms.RichTextBox rtb, QueryState qs)
        {
            //determine table
            string currentTable = "";
            string newSQL = "";
            if (qParser.tables.Count == 1)
            {
                currentTable = qParser.tables[0];
            }
            else
            {
                MessageBox.Show("Can't insert becuase multiple tables are being accessed.");
            }
            //build attr list
            if (DBtables[currentTable].attributes.Count == sCells.ElementAt(0).Value.Count)
            {
                //bool valid = true;
                newSQL = "INSERT INTO " + currentTable + " (";
                bool notfirst = false;
                for (int i = 0; i < grid_view.ColumnCount; i++)
                {
                    //Console.Out.WriteLine(grid_view.Columns[i].Name);
                    if(notfirst)
                    {
                        newSQL += ",";
                    }
                    newSQL += grid_view.Columns[i].Name;
                    notfirst = true;
                }
                newSQL += ")\nVALUES (";
                notfirst = false;
                Console.Out.WriteLine(newSQL);
                for (int i = 0; i < grid_view.ColumnCount; i++)
                {
                    if (notfirst)
                    {
                        newSQL += ",";
                    }
                    //string temp = grid_view[i, grid_view.RowCount - 1].Value.ToString();
                    if (grid_view[i, grid_view.RowCount - 1].Value != null)
                    {
                        string type = DBtables[currentTable].attributes[grid_view.Columns[i].Name.ToLower()][1];
                        Console.Out.WriteLine(type + "      sadfkhjsaedfuih sadfhs dfisdh ");
                        if (type != "int")
                        {
                            newSQL += "\'" + grid_view[i, grid_view.RowCount - 1].Value.ToString() +"\'";
                        }
                        else
                        {
                            newSQL += grid_view[i, grid_view.RowCount - 1].Value.ToString();
                        }
                        //newSQL += grid_view[i, grid_view.RowCount - 1].Value.ToString();
                        notfirst = true;
                    }
                    else
                    {
                        
                        Console.Out.WriteLine("notthing there");
                        MessageBox.Show("Can't insert becuase of null value.");
                        return false;
                    }
                    
                }
                newSQL += ");";
                Console.Out.WriteLine(newSQL);
                rtb.Text = newSQL;
                if (newSQL.Length > 0)
                {
                    Console.Out.WriteLine("running left side");
                    run_query(newSQL, grid_view, qs, qParser);
                    fill_dataGrid(getResult(), grid_view, qs, qParser);
                    previousQuery = newSQL;
                }
            }
            else
            {
                MessageBox.Show("Can't insert becuase not all attributes are present.");
                return false;
            }
            return true;
        }

        private void BETWEEN_entry(Parser qParser, Dictionary<int, HashSet<int>> sCells,
            System.Windows.Forms.DataGridView grid_view, List<String> possibleQ)
        {
            pkString = "";
            currentTable = "";
            pkInt = -1;
            string newSQL = sqlStart(qParser, sCells, grid_view);
            //Console.Out.WriteLine(newSQL + "      between");
            //Console.Out.WriteLine(pkQUERY + "      PK");
           // Console.Out.WriteLine(currentTable + "    table");
            string add = "";
            bool notfirst = false;
            string end = pkQUERY.Substring(pkQUERY.IndexOf("WHERE") + 5);
            Dictionary<string, List<string>> arithmatic = new Dictionary<string, List<string>>();
            //Console.Out.WriteLine(run);
            string arg = "/C " + path + "sqlite3 " + path + database + " \"" + pkQUERY + "\" > " + path + "out.txt";
            run_command(arg);
            List<string> answer = getResult();
            int total = answer.Count;
            string run = "", min = "", max = "";
            foreach(string[] str in DBtables[currentTable].attributes.Values){
                if (str[1] == "int" || str[1] == "real")
                {
                    List<string> maxMIN = new List<string>();
                    if (notfirst)
                    {
                        add += " , ";
                    }
                    notfirst = true;
                    add += str[0];
                     run = "SELECT min(" + str[0] + ") FROM " + currentTable + " WHERE " + end;
                    //Console.Out.WriteLine(run);
                     arg = "/C " + path + "sqlite3 " + path + database + " \"" + run + "\" > " + path + "out.txt";
                    run_command(arg);
                    min = getResult()[0];
                    run = "SELECT max(" + str[0] + ") FROM " + currentTable + " WHERE " + end;
                    //Console.Out.WriteLine(run);
                    arg = "/C " + path + "sqlite3 " + path + database + " \"" + run + "\" > " + path + "out.txt";
                    run_command(arg);
                    max = getResult()[0];
                    //Console.Out.WriteLine(min + " = min   " + max + "   = max");
                    maxMIN.Add(min);
                    maxMIN.Add(max);
                    //string finalSQL = newSQL;
                    arithmatic.Add(str[0], maxMIN);

                }
            }

            string attempt = pkQUERY.Substring(0, pkQUERY.IndexOf("WHERE") + 5) + " ";
            
            foreach (string str in arithmatic.Keys)
            {
                //fin = attempt + str + " > " + arithmatic[str][0];
                checkTotal(attempt + str + " > " + arithmatic[str][0] + ";", total, answer, possibleQ);
                checkTotal(attempt + str + " != " + arithmatic[str][0] + ";", total, answer, possibleQ);
                checkTotal(attempt + str + " != " + arithmatic[str][1] + ";", total, answer, possibleQ);
                //checkTotal(attempt + str + " >= " + arithmatic[str][0] + ";", total, answer);
                checkTotal(attempt + str + " = " + arithmatic[str][0] + ";", total, answer, possibleQ);
                checkTotal(attempt + str + " = " + arithmatic[str][1] + ";", total, answer, possibleQ);
                checkTotal(attempt + str + " < " + arithmatic[str][1] + ";", total, answer, possibleQ);
                checkTotal(attempt + str + " BETWEEN " + arithmatic[str][0] + " and " + arithmatic[str][1] + ";", total, answer, possibleQ);
                checkTotal(attempt + str + " = (SELECT MAX(" + str + ") FROM " + currentTable + ");", total, answer, possibleQ);
                checkTotal(attempt + str + " = (SELECT MIN(" + str + ") FROM " + currentTable + ");", total, answer, possibleQ);
                checkTotal(attempt + str + " > (SELECT AVG(" + str + ") FROM " + currentTable + ");", total, answer, possibleQ);
                checkTotal(attempt + str + " < (SELECT AVG(" + str + ") FROM " + currentTable + ");", total, answer, possibleQ);
                checkTotal(attempt + str + " >= (SELECT AVG(" + str + ") FROM " + currentTable + ");", total, answer, possibleQ);
                checkTotal(attempt + str + " <= (SELECT AVG(" + str + ") FROM " + currentTable + ");", total, answer, possibleQ);
                //Console.Out.WriteLine(attempt + str + " <= (SELECT AVG(" + str + ") FROM " + currentTable + ");");
                
            }
        }

        private void checkTotal(string check, int total, List<string> answer, List<String> possibleQ)
        {
            string arg = "/C " + path + "sqlite3 " + path + database + " \"" + check + "\" > " + path + "out.txt";
            run_command(arg);
            List<string> myAnswer= getResult();
            if (myAnswer.Count == total)
            {
                bool correct = true;
                foreach(string str in myAnswer){
                    if(!answer.Contains(str)){
                        correct = false;
                    }
                }
                if (correct)
                {
                    //Console.Out.WriteLine(check + "        is correct");
                    if (!possibleQ.Contains(check))
                    {
                        possibleQ.Add(check);
                    }
                    
                }
            }
        }

        private void reversePortion(List<String> possibleQ, System.Windows.Forms.DataGridView grid_view, Dictionary<int,
            HashSet<int>> sCells, Parser qParser, System.Windows.Forms.RichTextBox rtb, QueryState qs, System.Windows.Forms.DataGridView SQLgrid)
        {
            possibleQ.Clear();
            setSelectedCells(grid_view, sCells);
            if (sCells.Count < 1)
            {
                //MessageBox.Show("Nothing is selected.");
                
            }
            else
            {
                //setSelectedCells(grid_view, sCells);
                string check = checkCells(grid_view, sCells);
                if (check == "Valid Selection")
                {
                   // Console.Out.WriteLine("valid selection!!!!");
                    primaryKeyOptions(qParser, sCells, grid_view, possibleQ);
                    
                    if (!joinOP)
                    {
                        BETWEEN_entry(qParser, sCells, grid_view, possibleQ);
                        string str = fillSQLOption(possibleQ, SQLgrid);
                        rtb.Text = str;
                        run_query(str, grid_view, qs, qParser);
                        fill_dataGrid(getResult(), grid_view, qs, qParser);
                        previousQuery = str;
                        
                    }
                     
                }
                else if (check == "Invalid Selection")
                {
                    MessageBox.Show("Invalid selection: Please try again.");
                }
                else if (check == "Invalid Insert")
                {
                    MessageBox.Show("Invalid Insert. Last row can only be chosen by itself.");
                }
                else if (check == "Valid Insert")
                {
                    //MessageBox.Show("Invalid Insert. Last row can only be chosen by itself.");
                    Console.Out.WriteLine("insertion!!!!");
                    insertEntry(qParser, sCells,grid_view,rtb, qs);
                }

            }
        }
        //reverse engineer
        private void button2_Click(object sender, EventArgs e)
        {

            //Console.Out.WriteLine("grid 1!!!!");
            reversePortion(possibleQueries, dataGridView1, selectedCells, queryParser, richTextBox1, QS, dataGridView3);
            //Console.Out.WriteLine("grid 2!!!!");
            reversePortion(possibleQueries2, dataGridView2, selectedCells2, queryParser2, richTextBox2, QS2, dataGridView4);
        }


        private void setSelectedCells(System.Windows.Forms.DataGridView grid_view, Dictionary<int, HashSet<int>> sCells)
        {
            if (grid_view.RowCount > totalsize)
            {
                grid_view.AllowUserToAddRows = false;
            }
            sCells.Clear();
            for (int row = 0; row < grid_view.RowCount; row++)
            {
                HashSet<int> add = new HashSet<int>();
                for (int col = 0; col < grid_view.ColumnCount; col++)
                {
                   
                    //Console.Out.WriteLine(grid_view[col, row].Value.ToString());
                   // Console.Out.WriteLine(grid_view[col, row].Selected.ToString());
                    if (grid_view[col, row].Selected == true)
                    {
                        //Console.Out.WriteLine(col + "    " + row);
                        add.Add(col);
                    }
                }
                if (add.Count > 0)
                {
                    sCells.Add(row, add);
                }
            }

            //checkCells();

        }

        private string checkCells(System.Windows.Forms.DataGridView grid_view, Dictionary<int, HashSet<int>> sCells)
        {
            HashSet<int> check = new HashSet<int>();
            check.Clear();
            bool first = true;
            foreach (int row in sCells.Keys)
            {
               // Console.Out.Write("row: " + row + "    columns: ");
                foreach (int col in sCells[row])
                {
                    if (first)
                    {
                        check.Add(col);
                    }
                    else
                    {
                        if (!check.Contains(col))
                        {
                            return "Invalid Selection";
                        }
                    }
                    //Console.Out.Write(col + " ");
                }
                //Console.Out.WriteLine();
                first = false;
            }
           // Console.Out.WriteLine(grid_view.RowCount - 1 + "      number of selected rows");
            if (sCells.Keys.Contains(grid_view.RowCount - 1))
            {
               // Console.Out.WriteLine(check.Count + "      number of selected columns");
                if (sCells.Count > 1)
                {
                    return "Invalid Insert";
                }
                else
                {
                    return "Valid Insert";
                }
            }
            return "Valid Selection";
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick_1(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellContentClick_2(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void splitContainer4_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void richTextBox1_TextChanged_1(object sender, EventArgs e)
        {
            sql = changedText(sender, e, richTextBox1, queryParser);
        }

        private void richTextBox2_TextChanged(object sender, EventArgs e)
        {
            sql2 = changedText(sender, e, richTextBox2, queryParser2);
        }

        private void joinOptionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            join = !join;
            if (join)
            {
                splitContainer2.SplitterDistance = splitContainer1.Right/2;
                splitContainer4.Visible = true;
                button3.Visible = true;
            }
            else
            {
                splitContainer2.SplitterDistance = splitContainer1.Right;
                splitContainer4.Visible = false;
                button3.Visible = false;
            }
        }

        private void generateJOIN(Dictionary<string, List<string>> t1, Dictionary<string, List<string>> t2, object sender, EventArgs e)
        {
            string newSQL = "SELECT ";
            bool notfirst = false;
            string attList = "";
            //HashSet<int> temp = selectedCells.First().Value;
            foreach (int col in selectedCells.First().Value)
            {
                if (notfirst)
                {
                    attList += ", ";
                }
                notfirst = true;
                attList += dataGridView1.Columns[col].Name.ToString();
            }
            foreach (int col in selectedCells2.First().Value)
            {
                if (notfirst)
                {
                    attList += ", ";
                }
                notfirst = true;
                attList += dataGridView2.Columns[col].Name.ToString();
            }
            newSQL += attList + " FROM " + queryParser.tables[0] + " INNER JOIN " + queryParser2.tables[0] + " ON ";
            //Console.Out.WriteLine(newSQL + "      ?????????");
            bool add = true;
            foreach (string str1 in t1.Keys)
            {
                //Console.Out.WriteLine(str1 + "      first att");
                List<string> check1 = t1[str1];
                
                foreach (string str2 in t2.Keys)
                {
                    add = true;
                    //Console.Out.WriteLine(str2 + "      att2");
                    List<string> check2 = t2[str2];
                    foreach (string val in check1)
                    {
                        if(!check2.Contains(val))
                        {
                            add = false;
                        }
                    }
                    if (add)
                    {
                        newSQL += queryParser.tables[0] + "." + str1 + " = " + queryParser2.tables[0] + "." + str2;
                    }
                }

            }
            newSQL = newSQL.Trim() + ";";
            //Console.Out.WriteLine(newSQL + "      ?????????");
            possibleQueries.Clear();
            joinOptionToolStripMenuItem_Click(sender, e);
            richTextBox1.Text = newSQL;
            button1_Click(sender, e);

        }



        private void button3_Click(object sender, EventArgs e)
        {

            joinOP = true;
            if (dataGridView1.RowCount == 0 || dataGridView2.RowCount == 0)
            {
                MessageBox.Show("One of the resulting tables is unpopulated.");
            }
            else
            {
                Console.Out.WriteLine("possible valid join!!!");
                //check correct numer of rows
                button2_Click(sender, e);
                char[] delim = { '|' };
                //get left table
                string query1 = "Select * " + possibleQueries[0].Substring(possibleQueries[0].IndexOf("FROM"));
                Console.Out.WriteLine(query1);
                string arg = "/C " + path + "sqlite3 " + path + database + " \"" + query1 + "\" > " + path + "out.txt";
                run_query(query1, dataGridView1, QS, queryParser);
                List<string> table1 = getResult();
                Dictionary<string, List<string>> Table1Values = new Dictionary<string, List<string>>();
                Dictionary<string, List<string>> Table2Values = new Dictionary<string, List<string>>();
                List<string[]> temp = new List<string[]>();
                foreach (string str in table1)
                {
                   string[] elements = str.Split(delim);
                   temp.Add(elements);
                }
                string attName = "";
                for (int col = 0; col < temp[0].Length; col++)
                {
                    List<string> add = new List<string>();
                    for (int row = 0; row < temp.Count; row++)
                    {
                        if (row == 0)
                        {
                            attName = temp[row][col];
                        }
                        else
                        {
                            add.Add(temp[row][col]);
                        }
                        
                    }
                    //Console.Out.WriteLine();
                    //Console.Out.Write(attName + " ");
                    Table1Values.Add(attName, add);
                    //foreach (string str in Table1Values[attName])
                    //{
                    //    Console.Out.Write(str + " ");
                    //}
                    //Console.Out.WriteLine();
                }

                //get right table
                string query2 = "Select * " + possibleQueries2[0].Substring(possibleQueries2[0].IndexOf("FROM"));
                arg = "/C " + path + "sqlite3 " + path + database + " \"" + query2 + "\" > " + path + "out.txt";
               // run_command(arg);
                run_query(query2, dataGridView2, QS2, queryParser2);
                List<string> table2 = getResult();
                temp.Clear();
                foreach (string str in table2)
                {
                    string[] elements = str.Split(delim);
                    temp.Add(elements);
                }
                for (int col = 0; col < temp[0].Length; col++)
                {
                    List<string> add = new List<string>();
                    for (int row = 0; row < temp.Count; row++)
                    {
                        if (row == 0)
                        {
                            attName = temp[row][col];
                        }
                        else
                        {
                            add.Add(temp[row][col]);
                        }

                    }
                    //Console.Out.WriteLine();
                    //Console.Out.Write(attName + " ");
                    Table2Values.Add(attName, add);
                    //foreach (string str in Table2Values[attName])
                    //{
                    //    Console.Out.Write(str + " ");
                    //}
                    //Console.Out.WriteLine();
                }
                generateJOIN(Table1Values, Table2Values,sender, e);
            }
            
            joinOP = false;
        }
    }
}
