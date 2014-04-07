using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DBProject
{
    public class QueryState
    {
        public  Dictionary<string, int> pkMap = new Dictionary<string, int>();
        public Dictionary<string, int> attMap = new Dictionary<string, int>();
        public List<string> tables = new List<string>();
        
        public bool join = false;
        public bool star = false;
        public bool select = false;
        public bool where = false;
        
        public QueryState()
        {

        }
        public void clear()
        {
            pkMap.Clear();
            tables.Clear();
            attMap.Clear();
            join = false;
            star = false;
            select = false;
            where = false;
        }
    }
}
