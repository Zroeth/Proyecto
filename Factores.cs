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

     }
