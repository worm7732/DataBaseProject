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
    public partial class Open_Database : Form
    {
        public string DB_selected { get; set; }

        public Open_Database(List<String> database_names)
        {
            InitializeComponent();
            foreach (string str in database_names)
            {
                var listViewItem = new ListViewItem(str);
                listView1.Items.Add(listViewItem);
            }


            listView1.FullRowSelect = true;
            this.listView1.DoubleClick += new System.EventHandler(this.listView1_DoubleClick);
            this.DB_selected = "";
        }

        private void listView1_DoubleClick(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                MessageBox.Show("You loaded " + listView1.SelectedItems[0].Text );
                this.DB_selected = listView1.SelectedItems[0].Text;
                this.Close();
            }


        }

     

        private void Open_Database_Load(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
