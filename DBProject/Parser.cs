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


    class Parser
    {
        private HashSet<string> keywords = new HashSet<string>();
        //public List<List<string>> colorMapping = new List<List<string>>();
        public  Dictionary<string, string> colorMapping = new Dictionary<string, string>();

        public Parser()
        {
            keywords.Add("select");
            keywords.Add("from");
            keywords.Add("where");
        }

        public void parse(string sql)
        {
            colorMapping.Clear();
            string temp = "";
            
            foreach (char ch in sql)
            {
                if (ch == ' ' || ch == '\n' || ch == ',' || ch == ';')
                {
                    string add = "";
                    if (keywords.Contains(temp.ToLower()))
                    {
                        add = "keyword";
                    }
                    else if (DB_table.allTables.Contains(temp.ToLower()))
                    {
                        add = "table";
                    }
                    else if (DB_table.allAttributes.Contains(temp.ToLower()))
                    {
                       add = "attribute";
                    }
                    else
                    {
                        add = "other";
                    }

                    colorMapping.Add(temp, add);
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
