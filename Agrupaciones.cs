using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Beta
{
    //clase que revisa los signos de agrupacion y los terminos importantes del documento 
    class Agrupaciones
    {
        //regresa un bool para expresar que los simbolos estan balanceados, o no
        internal Boolean EstaBalanceadoQm(char[] exp)
        {
            Stack<char> st = new Stack<char>();
            for (int i = 0; i < exp.Length; i++)
            {
                if (exp[i] == '{' || exp[i] == '(' || exp[i] == '[' || exp[i] == '\'')
                {
                    st.Push(exp[i]);
                }
                if (exp[i] == '}' || exp[i] == ')' || exp[i] == ']' || exp[i] == '\'')
                {
                    if (st.Count == 0)
                    {

                        return false;

                    }
                    else if (!SeEncontroParejaQm(st.Pop(), exp[i]))
                    {
                        return false;
                    }
                }
            }
            if (st.Count == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //para emparejar los simbolos que se han encontrado abiertos
        internal Boolean SeEncontroParejaQm(char character1, char character2)
        {
            if (character1 == '(' && character2 == ')')
            {
                return true;

            }
            else if (character1 == '{' && character2 == '}')
            {
                return true;
            }
            else if (character1 == '[' && character2 == ']')
            {
                return true;
            }
            else if (character1 == '\'' && character2 == '\'')
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //determina la distancia entre las expresiones regulares para determinar cuantas lineas hay entre una er y la otra
        internal int Distancia(string[] lineas, int i, Regex terminales)
        {

            for (int j = i; j < lineas.Length; j++)
            {
                Match match2 = terminales.Match(lineas[j]);
                if (match2.Success)
                {
                    return j;
                }
            }

            return lineas.Length;
        }

        //omitir los simbolos que se leen por linea ya que no estan balanceados
        internal string[] Omitir(string[] linea, int pos)
        {
            linea[pos] = linea[pos].Replace("'(''*'", " ");
            linea[pos] = linea[pos].Replace("'*'')'", " ");
            linea[pos] = linea[pos].Replace("'{'", " ");
            linea[pos] = linea[pos].Replace("'}'", " ");
            linea[pos] = linea[pos].Replace("'('", " ");
            linea[pos] = linea[pos].Replace("')'", " ");
            linea[pos] = linea[pos].Replace("'['", " ");
            linea[pos] = linea[pos].Replace("']'", " ");
            linea[pos] = linea[pos].Replace("{", " ");
            linea[pos] = linea[pos].Replace("}", " ");
            return linea;
        }
        //partes que no pueden faltar en un documento
        internal string Revisar(string[] linea, int pos, string capturada)
        {
            string[] rev = linea;
            linea[pos] = linea[pos].Replace(" ", "");
            linea[pos] = linea[pos].Replace("   ", "");
            linea[pos] = linea[pos].Replace(" ", "");

            if (rev[pos].Contains("TOKENS"))
            {
                capturada += "TOKENS";

            }
            if (rev[pos].Contains("ACTIONS"))
            {
                capturada += "ACTIONS";
            }
            if (rev[pos].Contains("RESERVADAS()"))
            {
                capturada += "RESERVADAS()";
            }
            if (rev[pos].Contains("ERROR"))
            {
                capturada += "ERROR";
            }
            return capturada;
        }
    }


}

