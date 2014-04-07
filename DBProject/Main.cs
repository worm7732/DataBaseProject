﻿using System;
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
        //HashSet<int> selected = new HashSet<int>();

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
                fill_dataGrid(getResult(), grid_view, qs, qParser);

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
            }
            sql2 = richTextBox2.Text;
           // Console.Out.WriteLine(sql2.Length + " " + sql2.Length);
            if (sql2.Length > 0)
            {
                //Console.Out.WriteLine("running right side");
                run_query(sql2, dataGridView2, QS2, queryParser2);
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
                grid_view.Rows.Clear();
                grid_view.Visible = false;
                MessageBox.Show("This query returned no results");
            }
        }

        private string changedText(object sender, EventArgs e, System.Windows.Forms.RichTextBox rtb, Parser qParser)
        {
            string SQL = "";
            int save = rtb.SelectionStart;
            SQL = rtb.Text;
            qParser.clear();
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
            Console.Out.WriteLine(qParser.tables.Count);
            if (qParser.tables.Count == 1)
            {
                currentTable = qParser.tables[0];
            }
            //build attr list
            Console.Out.WriteLine(currentTable + "       look here!");
            //Console.Out.WriteLine(DBtables[currentTable].attributes.Count + "       look here" );
            if (DBtables[currentTable].attributes.Count == sCells.ElementAt(0).Value.Count)
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
            newSQL += tablePART + " FROM " + currentTable + " WHERE ";
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
                possibleQ.Add(newSQL + ";");
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
                        foreach (int col in sCells[row])
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
                    newSQL += where + ";";
                    possibleQ.Add(newSQL);
                    newSQL = finalSQL + ";";
                    possibleQ.Add(newSQL);
                    pkQUERY = finalSQL; 
                }
            }
        }

        private void pickQuery(List<String> possibleQ, System.Windows.Forms.RichTextBox rtb, System.Windows.Forms.DataGridView grid_view, Parser qParser, QueryState qs)
        {
            Console.Out.WriteLine(possibleQ.Count + " = number of  possible!!");
            foreach (string str in possibleQ)
            {
                Console.Out.WriteLine(str + "           possible!!");
            }
            string SQL = possibleQ[possibleQ.Count - 1];
            Console.Out.WriteLine(SQL + "           chosen!!");
            rtb.Text = SQL;
            //button1_Click(sender, e);
            if (SQL.Length > 0)
            {
                //Console.Out.WriteLine("running left side");
                run_query(SQL, grid_view, qs, qParser);
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
                //sql = newSQL;
                rtb.Text = newSQL;
                //button1_Click(sender, e);
                if (newSQL.Length > 0)
                {
                    Console.Out.WriteLine("running left side");
                    run_query(newSQL, grid_view, qs, qParser);
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
            Console.Out.WriteLine(newSQL + "      between");
            Console.Out.WriteLine(pkQUERY + "      PK");
            Console.Out.WriteLine(currentTable + "    tab;e");
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
            string attempt = "SELECT " + add + " FROM " + currentTable + " WHERE ";
            
            foreach (string str in arithmatic.Keys)
            {
                //fin = attempt + str + " > " + arithmatic[str][0];
                checkTotal(attempt + str + " > " + arithmatic[str][0] + ";", total, answer);
                checkTotal(attempt + str + " != " + arithmatic[str][0] + ";", total, answer);
                checkTotal(attempt + str + " != " + arithmatic[str][1] + ";", total, answer);
                //checkTotal(attempt + str + " >= " + arithmatic[str][0] + ";", total, answer);
                checkTotal(attempt + str + " = " + arithmatic[str][0] + ";", total, answer);
                checkTotal(attempt + str + " = " + arithmatic[str][1] + ";", total, answer);
                checkTotal(attempt + str + " < " + arithmatic[str][1] + ";", total, answer);
                checkTotal(attempt + str + " BETWEEN " + arithmatic[str][0] + " and " + arithmatic[str][1] + ";", total, answer);
                checkTotal(attempt + str + " = (SELECT MAX(" + str + ") FROM " + currentTable + ");", total, answer);
                checkTotal(attempt + str + " = (SELECT MIN(" + str + ") FROM " + currentTable + ");", total, answer);
                checkTotal(attempt + str + " > (SELECT AVG(" + str + ") FROM " + currentTable + ");", total, answer);
                checkTotal(attempt + str + " < (SELECT AVG(" + str + ") FROM " + currentTable + ");", total, answer);
                checkTotal(attempt + str + " >= (SELECT AVG(" + str + ") FROM " + currentTable + ");", total, answer);
                checkTotal(attempt + str + " <= (SELECT AVG(" + str + ") FROM " + currentTable + ");", total, answer);
                Console.Out.WriteLine(attempt + str + " <= (SELECT AVG(" + str + ") FROM " + currentTable + ");");
            }
        }

        private void checkTotal(string check, int total, List<string> answer)
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
                    Console.Out.WriteLine(check + "produced " + myAnswer.Count + " results!  " + total);
                }
            }
        }

        private void reversePortion(List<String> possibleQ, System.Windows.Forms.DataGridView grid_view, Dictionary<int,
            HashSet<int>> sCells, Parser qParser, System.Windows.Forms.RichTextBox rtb,QueryState qs)
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
                    Console.Out.WriteLine("valid selection!!!!");
                    primaryKeyOptions(qParser, sCells, grid_view, possibleQ);
                    BETWEEN_entry(qParser, sCells, grid_view, possibleQ);
                    pickQuery(possibleQ, rtb,grid_view,qParser,qs);
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
            reversePortion(possibleQueries, dataGridView1, selectedCells, queryParser, richTextBox1, QS);
            //Console.Out.WriteLine("grid 2!!!!");
            reversePortion(possibleQueries2, dataGridView2, selectedCells2, queryParser2, richTextBox2, QS2);
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

      

    }
}
