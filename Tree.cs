using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beta
{

    //clase arbol
    class Tree
    {


        public Nodos Root;
        private string Nodes;
        public void Add(string nuevoNodo)
        {

            Nodos tempParent = new Nodos();
            int comparador;


            Nodos nodoTemporal = Root;

            while (nodoTemporal != null)
            {
                tempParent = nodoTemporal;
                //se creo la funcion comparar para determinar el peso que tiene la palabra
                comparador = Comparar(nuevoNodo, nodoTemporal.dato);

                //en caso de que se encuentre un termino repetido se sale
                if (nuevoNodo == nodoTemporal.dato)
                {
                    return;
                }

                
                else if (comparador == -1)
                {

                    nodoTemporal = nodoTemporal.izq;

                }

                else
                {
                    nodoTemporal = nodoTemporal.der;
                }
            }


            Nodos nodoagregar = new Nodos(nuevoNodo);
            comparador = Comparar(nuevoNodo, tempParent.dato);
            if (Root == null)
            {
                Root = nodoagregar;

            }
            else if (comparador == -1)
            {
                tempParent.izq = nodoagregar;
            }
            else
            {
                tempParent.der = nodoagregar;
            }

        }

        //palabras a identificar
        private static readonly string[] Valores = new[] {
        "ERROR","+","ACTIONS","-","RESERVADAS()","*","TOKENS","|","SETS",};

        //comparar
        public int Comparar(string left, string right)
        {
            return Array.IndexOf(Valores, right).CompareTo(Array.IndexOf(Valores, left));
        }
        //ordenar
        public void InOrder()
        {
            Nodes = "";
            InOrderString(Root);
        }

        private void InOrderString(Nodos TraverseNode)
        {
            if (TraverseNode == null)
                return;
            InOrderString(TraverseNode.izq);
            Visit(TraverseNode);
            InOrderString(TraverseNode.der);
        }
        private void Visit(Nodos TraverseNode)
        {
            Nodes += TraverseNode.dato + " ";
        }

        public string ObtenerNodos()
        {
            return Nodes;
        }
        public virtual void BorrarArbol(Nodos node)
        {
           
            Root = null;
        }
    }
}
