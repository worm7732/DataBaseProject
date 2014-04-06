using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DBProject
{

    class DB_table
    {

        public static Dictionary<string, string> allPK = new Dictionary<string, string>(); // pk -> tname
        public static HashSet<string> allAttributes = new HashSet<string>();
        public static HashSet<string> allTables = new HashSet<string>();

        public string table_name = "";   //name of table
        public List<String> primary_keys = new List<String>(); //list of pks
        public int pk_count = 0; // pk count
        public Dictionary<string, string[]> attributes = new Dictionary<string, string[]>(); // hash table of attributes 0:name, 1:type, 2:null 3:pk

        public DB_table(string name, List<String> columns)
        {
            allAttributes.Add("*");
            table_name = name;
            allTables.Add(name.ToLower());
            pk_count = 0;
            //Console.Out.WriteLine(name + " ???????");
            foreach (string str in columns)
            {
                if (str.Length > 0)
                {
                    string[] attr = str.Split('|');
                    string[] fin = { attr[1].ToLower(), attr[2].ToLower(), attr[3].ToLower(), attr[5].ToLower() };
                    
                        if (fin[3] == "1")
                        {
                            if (!allPK.ContainsKey(fin[0]))
                            {
                                primary_keys.Add(fin[0].ToLower());
                                pk_count++;
                                attributes.Add(fin[0].ToLower(), fin);
                                allPK.Add(fin[0].ToLower(), name);
                                allAttributes.Add(fin[0].ToLower());
                            }else
                            {
                                MessageBox.Show("Multiple Primary Keys with Same name. Table will not be loaded");
                            }
                        }else{
                            allAttributes.Add(fin[0].ToLower());
                            attributes.Add(fin[0].ToLower(), fin);
                        }
                    }
                   
                }
            //Console.Out.WriteLine("number of pk = " + pk_count);
            //Console.Out.WriteLine("number of attr = " + attributes.Count);
        }

    }

}
