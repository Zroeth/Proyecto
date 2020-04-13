using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beta
{
    class Factores
    {
        public ArrayList Dividir(string cadenanueva)
        {
            ArrayList arreglo = new ArrayList();
            string linea = "";
            char[] caracteres = cadenanueva.ToCharArray();
            for (int i = 0; i < caracteres.Length; i++)
            {
                if (caracteres[i] == '·' || caracteres[i] == '(' || caracteres[i] == '|' || caracteres[i] == ')' || caracteres[i] == '*' || caracteres[i] == '+' || caracteres[i] == '?')
                {
                    if (linea != "")
                    {
                        arreglo.Add(linea);
                        arreglo.Add(caracteres[i]);
                        linea = "";
                    }
                    else
                    {
                        arreglo.Add(caracteres[i]);
                    }
                }
                else
                {
                    if (i == caracteres.Length - 1)
                    {
                        linea += caracteres[i];
                        arreglo.Add(linea);
                    }
                    else
                    {
                        linea += caracteres[i];
                    }
                }
            }
            return arreglo;
        }

        public ArrayList Recorrer(ArrayList entrada)
        {
            Stack<char> Operadores = new Stack<char>();
            ArrayList LSufija = new ArrayList();

            for (int i = 0; i < entrada.Count; i++)
            {
                if (entrada[i].ToString() != "·" && entrada[i].ToString() != "(" && entrada[i].ToString() != "|" && entrada[i].ToString() != ")" && entrada[i].ToString() != "*" && entrada[i].ToString() != "+" && entrada[i].ToString() != "?")
                {
                    LSufija.Add(entrada[i]);
                }
                else if (entrada[i].ToString() == "(")
                {
                    Operadores.Push(Convert.ToChar(entrada[i]));
                }
                else if (entrada[i].ToString() == ")")
                {
                    char SimboloTope = Operadores.Pop();
                    while (SimboloTope != '(')
                    {
                        LSufija.Add(SimboloTope);
                        SimboloTope = Operadores.Pop();
                    }
                }
                else
                {
                    while ((Operadores.Count != 0) && (Precedencia(Operadores.Peek()) >= Precedencia(Convert.ToChar(entrada[i]))))
                    {
                        LSufija.Add(Operadores.Pop());
                    }
                    Operadores.Push(Convert.ToChar(entrada[i]));
                }
            }
            while (Operadores.Count != 0)
            {
                LSufija.Add(Operadores.Pop());
            }

            return LSufija;
        }
        public int Precedencia(char simbolo)
        {
            int Orden = 0;
            switch (simbolo)
            {
                case '*': Orden = 4; break;
                case '+': Orden = 4; break;
                case '?': Orden = 4; break;
                case '·': Orden = 3; break;
                case '|': Orden = 2; break;
                case '(': Orden = 1; break;
            }
            return Orden;
        }



    }
}
