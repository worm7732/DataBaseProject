using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DBProject
{
    public partial class Create_Database : Form
    {
        public string DB_name { get; set; }
        List<String> database_names;
        public Create_Database(List<String> names)
        {
            InitializeComponent();
            this.database_names = names;
            textBox1.Select(0, 0);
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            
            this.DB_name = textBox2.Text;
            
            if (this.DB_name.Length > 0)
            {
                if (!this.DB_name.Contains(".db"))
                {
                    this.DB_name += ".db";
                }
                if (!this.database_names.Contains(this.DB_name))
                { 
                    MessageBox.Show("You created a database with name " + this.DB_name);
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Database " + this.DB_name + " already exist.");
                }
            }
            else
            {
                MessageBox.Show("You did not enter a file name");
            }


        }

        
    }
}
