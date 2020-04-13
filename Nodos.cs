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
        public string dato;
        public Nodos izq;
        public Nodos der;
        public Nodos()
        {
        }

        public Nodos(string data)
        {
            this.dato = data;
            izq = null;
            der = null;
        }
    }
}
