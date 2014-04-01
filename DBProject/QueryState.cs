using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBProject
{
    class QueryState
    {
        public List<string> pk = new List<string>();
        public List<string> table = new List<string>();
        public List<string> column = new List<string>();
        public bool join = false;
        public bool star = false;
        public bool select = false;
        public bool where = false;
        
        public QueryState()
        {

        }
        public void clear()
        {
            pk.Clear();
            table.Clear();
            column.Clear();
            join = false;
            star = false;
            select = false;
            where = false;
        }
    }
}
