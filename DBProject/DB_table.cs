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

        public string table_name = "";   //name of table
        public List<String> primary_keys = new List<String>(); //list of pks
        public int pk_count = 0; // pk count
        public Dictionary<string, string[]> attributes = new Dictionary<string, string[]>(); // hash table of attributes 0:name, 1:type, 2:null 3:pk

        public DB_table(string name, List<String> columns)
        {
            table_name = name; 
            pk_count = 0;
            //Console.Out.WriteLine(name + " ???????");
            foreach (string str in columns)
            {
                if (str.Length > 0)
                {
                    string[] attr = str.Split('|');
                    string[] fin = { attr[1], attr[2], attr[3], attr[5] };
                    
                        if (fin[3] == "1")
                        {
                            if (!allPK.ContainsKey(fin[0]))
                            {
                                primary_keys.Add(fin[0]);
                                pk_count++;
                                attributes.Add(fin[0], fin);
                                allPK.Add(fin[0], name);
                            }else
                            {
                                MessageBox.Show("Multiple Primary Keys with Same name. Table will not be loaded");
                            }
                        }else{
                        
                            attributes.Add(fin[0], fin);
                        }
                    }
                   
                }
            //Console.Out.WriteLine("number of pk = " + pk_count);
            //Console.Out.WriteLine("number of attr = " + attributes.Count);
        }

    }

}
