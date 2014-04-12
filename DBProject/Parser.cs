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


    public class Parser
    {
        private HashSet<string> keywords = new HashSet<string>();
        public List<string> tables = new List<string>();
       // public List<string> words = new List<string>();
        public  Dictionary<string, string> colorMapping = new Dictionary<string, string>();

        public Parser()
        {
            keywords.Add("select");
            keywords.Add("from");
            keywords.Add("where");
            keywords.Add("insert");
            keywords.Add("into");
            keywords.Add("values");
            keywords.Add("or");
            keywords.Add("and");
            keywords.Add("min");
            keywords.Add("max");
            keywords.Add(">");
            keywords.Add(">=");
            keywords.Add("<=");
            keywords.Add("between");
            keywords.Add("avg");
            keywords.Add("count");
            keywords.Add("sum");
            keywords.Add("inner");
            keywords.Add("join");
            keywords.Add("on");
        }

        public void clear()
        {
            
            tables.Clear();
            colorMapping.Clear();
        }

        public void parse(string sql)
        {
            //colorMapping.Clear();
            string temp = "";
            clear();
            
            foreach (char ch in sql)
            {
                if ((ch == ' ' || ch == '\n' || ch == ',' || ch == ';' || ch == '(' || ch == ')' || ch == '.'))
                {
                    string add = "";
                    if (keywords.Contains(temp.ToLower()))
                    {
                        add = "keyword";
                    }
                    else if (DB_table.allTables.Contains(temp.ToLower()))
                    {
                        add = "table";
                        tables.Add(temp);
                    }
                    else if (DB_table.allAttributes.Contains(temp.ToLower()))
                    {
                       add = "attribute";
                    }
                    else
                    {
                        add = "other";
                    }
                    //Console.Out.WriteLine(temp + "    " + add);
                    if (temp.Length > 0 && !colorMapping.Keys.Contains(temp))
                    {
                        colorMapping.Add(temp, add);
                    }
                    temp = "";
                }
                else
                {
                    temp += ch;
                }
            }

        }
    }
}
