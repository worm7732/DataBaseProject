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

namespace DBProject
{
    public partial class Main : Form
    {
        String sql, database,command,path,preferences;
        System.Diagnostics.Process process = new System.Diagnostics.Process();
        System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();

        public Main()
        {
            InitializeComponent();
            database = "testDB.db";
            command = "\"SELECT * FROM COMPANY;\"";
            preferences = "";// "\".header on\"";
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
        }

        private void splitContainer2_Panel2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            //run button
            startInfo.FileName = "cmd.exe";
            //startInfo.Arguments = "/K " + path + "runSQL " + database + " " + preferences + " " + command + " " + path; 
            //startInfo.Arguments = "/K " + path + "runSQL " + database + " " + path;
           // String input = "/K " + path + "sqlite3 " + path + database + " < " + path + "in.txt > " + path + "out.txt";
            //Console.Out.WriteLine(input);
            startInfo.Arguments = "/K " + path + "sqlite3 " + path + database + " < "+ path + "in.txt > " + path +"out.txt";

            process.StartInfo = startInfo;
            process.Start();
            List<String> content = getResult();
            foreach( string str in content){
                Console.Out.WriteLine(str + "!!!!!!!!!!!!!!!!!!!!!!!!!");
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
    }
}
