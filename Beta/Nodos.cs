using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beta
{

    //nodos arbol 
    class Nodos
    {
        public string m_Data;
        public Nodos Left;
        public Nodos Right;
        public Nodos()
        {
        }


        public Nodos(string data)
        {
            this.m_Data = data;
            Left = null;
            Right = null;
        }
    }
}
